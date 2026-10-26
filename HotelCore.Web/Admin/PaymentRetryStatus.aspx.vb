Namespace Admin
    ''' Shared landing page for HotelCore.Web.Admin.PaymentRetry's two retry actions. Which
    ''' branch below actually fires depends entirely on which of PaymentRetry's two real
    ''' buttons (Gateway A vs Gateway B) the admin clicked on the previous request -- see
    ''' PaymentRetry.aspx.vb's RetryAndRedirect.
    Public Class PaymentRetryStatus
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session("UserRoleName") Is Nothing OrElse Session("UserRoleName").ToString() <> "Admin" Then
                Response.Redirect("~/Account/Login.aspx")
                Return
            End If

            Dim succeeded As Object = Session("PaymentRetrySuccess")
            If succeeded IsNot Nothing AndAlso CBool(succeeded) Then
                litStatusMessage.Text = "Payment retried successfully."
                Return
            End If

            Dim detail As String = TryCast(Session("PaymentRetryDetail"), String)
            If Not String.IsNullOrEmpty(detail) Then
                ' SINK: PLANTED-VB-HR-144
                litStatusMessage.Text = "Payment retry failed. Technical detail: " & Server.HtmlEncode(detail)
            Else
                ' SAFE_SINK: PLANTED-VB-HR-144-safe
                litStatusMessage.Text = "Payment retry failed. Our team has been notified; please try again shortly."
            End If
        End Sub

    End Class
End Namespace
