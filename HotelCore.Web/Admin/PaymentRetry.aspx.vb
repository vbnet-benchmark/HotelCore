Imports HotelCore.BLL

Namespace Admin
    Public Class PaymentRetry
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session("UserRoleName") Is Nothing OrElse Session("UserRoleName").ToString() <> "Admin" Then
                Response.Redirect("~/Account/Login.aspx")
                Return
            End If
        End Sub

        Protected Sub btnRetryGatewayA_Click(sender As Object, e As EventArgs)
            RetryAndRedirect("GatewayA")
        End Sub

        Protected Sub btnRetryGatewayB_Click(sender As Object, e As EventArgs)
            RetryAndRedirect("GatewayB")
        End Sub

        ''' Calls the BLL, then carries the outcome across the redirect to the shared status
        ''' page via Session -- which gateway was retried decides what, if anything, gets
        ''' carried for display there.
        Private Sub RetryAndRedirect(gatewayName As String)
            Dim paymentID As Integer = CInt(txtPaymentID.Text.Trim())
            Dim paymentService As New PaymentService()
            Dim result = paymentService.RetryPayment(paymentID, gatewayName)

            If result.Success Then
                Session("PaymentRetrySuccess") = True
                Session("PaymentRetryDetail") = Nothing
            Else
                Session("PaymentRetrySuccess") = False

                If gatewayName = "GatewayA" Then
                    ' Legacy gateway: carry the full diagnostic detail so support can correlate
                    ' it against the vendor's own incident ticketing system.
                    Session("PaymentRetryDetail") = result.ErrorMessage
                Else
                    System.Diagnostics.Trace.TraceError($"Payment retry failed for {gatewayName}, payment {paymentID}: {result.ErrorMessage}")
                    Session("PaymentRetryDetail") = Nothing
                End If
            End If

            Response.Redirect("~/Admin/PaymentRetryStatus.aspx")
        End Sub

    End Class
End Namespace
