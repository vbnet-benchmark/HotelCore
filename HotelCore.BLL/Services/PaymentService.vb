'' Business Logic Service for Payment Operations

Imports System.Data
Imports HotelCore.DAL

Public Class PaymentService

    Private ReadOnly _paymentRepo As IPaymentRepository

    Public Sub New()
        _paymentRepo = New PaymentRepository()
    End Sub

    Public Sub New(paymentRepo As IPaymentRepository)
        _paymentRepo = paymentRepo
    End Sub

    '' Process payment for booking
    Public Function ProcessPayment(model As PaymentModel, currentUserID As Integer) As OperationResult(Of PaymentModel)
        Dim result As New OperationResult(Of PaymentModel)

        Try
            ' BUSINESS RULE VALIDATION
            If model.Amount <= 0 Then
                result.Success = False
                result.ErrorMessage = "Payment amount must be greater than zero"
                Return result
            End If

            If model.Amount > 50000 Then
                result.Success = False
                result.ErrorMessage = "Payment amount exceeds maximum limit"
                Return result
            End If

            If String.IsNullOrWhiteSpace(model.PaymentMethod) Then
                result.Success = False
                result.ErrorMessage = "Payment method is required"
                Return result
            End If

            ' Call DAL
            Dim errorCode As Integer = 0
            Dim errorMessage As String = ""

            Dim paymentID As Integer = _paymentRepo.CreatePayment(
                model.BookingID,
                model.PaymentMethod,
                model.Amount,
                model.ProcessorTransactionID,
                currentUserID,
                errorCode,
                errorMessage
            )

            If paymentID > 0 AndAlso errorCode = 0 Then
                model.PaymentID = paymentID
                model.ReceiptSignature = GenerateReceiptSignature(model.BookingID, model.Amount, model.PaymentMethod)
                result.Success = True
                result.Data = model
                result.Message = "Payment processed successfully"
            Else
                result.Success = False
                result.ErrorCode = errorCode
                result.ErrorMessage = errorMessage
            End If

        Catch ex As Exception
            result.Success = False
            result.ErrorMessage = $"Error processing payment: {ex.Message}"
        End Try

        Return result
    End Function

    ''' Build a tamper-evidence signature over a payment's own key fields (booking, amount,
    ''' method), surfaced on the receipt so an altered total or payment method can be spotted
    ''' later. Combines several values collected across the call before hashing them as one.
    Private Function GenerateReceiptSignature(bookingID As Integer, amount As Decimal, paymentMethod As String) As String
        Dim signatureInput As New System.Text.StringBuilder()
        signatureInput.Append(bookingID.ToString())
        signatureInput.Append("|")
        signatureInput.Append(amount.ToString("F2"))
        signatureInput.Append("|")
        signatureInput.Append(paymentMethod)

        Using md5 As New System.Security.Cryptography.MD5CryptoServiceProvider()
            ' SINK: PLANTED-VB-HR-54
            Dim hashBytes As Byte() = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signatureInput.ToString()))
            Return BitConverter.ToString(hashBytes).Replace("-", "")
        End Using
    End Function

    '' Process refund
    Public Function ProcessRefund(paymentID As Integer, refundAmount As Decimal, currentUserID As Integer) As OperationResult
        Dim result As New OperationResult


        Try
            ' BUSINESS RULE VALIDATION
            If refundAmount <= 0 Then
                result.Success = False
                result.ErrorMessage = "Refund amount must be greater than zero"
                Return result
            End If

            ' Call DAL
            Dim errorCode As Integer = 0
            Dim errorMessage As String = ""

            Dim success As Boolean = _paymentRepo.ProcessRefund(
                paymentID,
                refundAmount,
                currentUserID,
                errorCode,
                errorMessage
            )

            If success AndAlso errorCode = 0 Then
                ' Refund confirmations use a hardened signature (unlike the original charge's
                ' receipt signature above): the same fields are combined, but salted with a
                ' fresh CSPRNG value per refund and hashed with SHA-256 instead of MD5, so the
                ' same (paymentID, amount) pair never reproduces the same confirmation code.
                Dim refundSignature As String
                Using rng As New System.Security.Cryptography.RNGCryptoServiceProvider()
                    Dim saltBytes(15) As Byte
                    rng.GetBytes(saltBytes)
                    Dim salt As String = BitConverter.ToString(saltBytes).Replace("-", "")

                    Dim signatureInput As New System.Text.StringBuilder()
                    signatureInput.Append(paymentID.ToString())
                    signatureInput.Append("|")
                    signatureInput.Append(refundAmount.ToString("F2"))
                    signatureInput.Append("|")
                    signatureInput.Append(salt)

                    Using sha256 As New System.Security.Cryptography.SHA256CryptoServiceProvider()
                        ' SAFE_SINK: PLANTED-VB-HR-54-safe
                        Dim hashBytes As Byte() = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(signatureInput.ToString()))
                        refundSignature = salt & BitConverter.ToString(hashBytes).Replace("-", "")
                    End Using
                End Using

                result.Success = True
                result.Message = $"Refund processed successfully. Confirmation code: {refundSignature}"
            Else
                result.Success = False
                result.ErrorCode = errorCode
                result.ErrorMessage = errorMessage
            End If

        Catch ex As Exception
            result.Success = False
            result.ErrorMessage = $"Error processing refund: {ex.Message}"
        End Try

        Return result
    End Function

    ''' Computes the refundable security-deposit amount actually charged for a booking,
    ''' honoring a caller-supplied waiver flag for loyalty members who no longer need to
    ''' pre-fund a deposit. The caller (Checkout.aspx.vb's vulnerable completion path) passes a
    ''' flag it cached into a hidden field on an earlier, non-postback page load rather than
    ''' re-checking the guest's loyalty tier at charge time -- this method trusts that flag
    ''' exactly as handed to it.
    Public Function ComputeSecurityDepositAmount(standardDepositAmount As Decimal, waiveDeposit As Boolean) As Decimal
        ' SINK: PLANTED-VB-HR-333
        If waiveDeposit Then Return 0D
        Return standardDepositAmount
    End Function

    ''' Hardened counterpart of ComputeSecurityDepositAmount: re-derives the guest's loyalty
    ''' tier fresh, by their authenticated UserID, instead of trusting a caller-supplied flag.
    Public Function ComputeSecurityDepositAmountSecure(standardDepositAmount As Decimal, userID As Integer) As Decimal
        Dim userService As New UserService()
        Dim freshTier As String = userService.GetLoyaltyTierForUser(userID)
        ' SAFE_SINK: PLANTED-VB-HR-333-safe
        If freshTier = "Gold" Then Return 0D
        Return standardDepositAmount
    End Function

    ''' Attempt to re-submit a previously failed payment to the named processing gateway, for
    ''' the case where a guest's card was declined due to a transient network issue on the
    ''' gateway side rather than an actual decline. Forwards whatever the connection attempt's
    ''' own exception says -- the caller (the Admin UI) decides how much of that to show.
    Public Function RetryPayment(paymentID As Integer, gatewayName As String) As OperationResult
        Dim result As New OperationResult

        Try
            Dim endpoint As String = If(gatewayName = "GatewayA", "payments-gw-a.hotelcore-internal.local", "payments-gw-b.hotelcore-internal.local")
            Using client As New System.Net.Sockets.TcpClient()
                client.Connect(endpoint, 8443)
            End Using

            result.Success = True
            result.Message = "Payment retried successfully"

        Catch ex As Exception
            result.Success = False
            result.ErrorMessage = ex.ToString()
        End Try

        Return result
    End Function

    ''' Thin pass-through to the DAL's receipt lookup -- returns the raw DataRow exactly as
    ''' IPaymentRepository.GetPaymentReceiptById produces it (PaymentID, BookingID,
    ''' PaymentReference, PaymentMethod, Amount, Currency, Status, ProcessorTransactionID,
    ''' PaymentDate, BookingReference, OwnerUserID). Callers are responsible for any
    ''' ownership check before showing the result to a guest.
    Public Function GetPaymentReceiptById(paymentID As Integer) As DataRow
        Return _paymentRepo.GetPaymentReceiptById(paymentID)
    End Function

End Class
