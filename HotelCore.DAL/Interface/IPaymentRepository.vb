Imports System.Data

Public Interface IPaymentRepository
    Function CreatePayment(bookingID As Integer, paymentMethod As String, amount As Decimal,
                          processorTransactionID As String, currentUserID As Integer,
                          ByRef errorCode As Integer, ByRef errorMessage As String) As Integer

    Function ProcessRefund(paymentID As Integer, refundAmount As Decimal, currentUserID As Integer,
                          ByRef errorCode As Integer, ByRef errorMessage As String) As Boolean

    ''' Looks up a single payment's receipt detail (amount, method, status, processor
    ''' reference) together with its parent booking's reference and owning UserID, joined in
    ''' one round trip. Takes only the raw PaymentID -- callers are responsible for confirming
    ''' the requesting guest actually owns the resulting booking before showing it to them.
    Function GetPaymentReceiptById(paymentID As Integer) As DataRow
End Interface
