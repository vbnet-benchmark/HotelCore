Imports HotelCore.BLL

Namespace Account
    Public Class ForgotPassword
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        End Sub

        Protected Sub btnRequestReset_Click(sender As Object, e As EventArgs)
            Try
                pnlError.Visible = False
                pnlSuccess.Visible = False

                Dim email As String = txtEmail.Text.Trim()
                If String.IsNullOrEmpty(email) Then
                    ShowError("Please enter your email address.")
                    Return
                End If

                ' Compute a reset token and stash it for the (not-yet-built) confirmation
                ' step; a real implementation would email a link carrying this value instead
                ' of a generic success message.
                Dim userService As New UserService()
                Dim resetToken As String = userService.GeneratePasswordResetToken(email)

                Session("PasswordResetToken") = resetToken
                Session("PasswordResetEmail") = email

                ShowSuccess("If an account exists for that email, a password reset link has been sent.")

            Catch ex As Exception
                ShowError("An error occurred while processing your request.")
            End Try
        End Sub

        Private Sub ShowError(message As String)
            pnlError.Visible = True
            litError.Text = "<i class='fa fa-exclamation-circle mr-2'></i> " & message
        End Sub

        Private Sub ShowSuccess(message As String)
            pnlSuccess.Visible = True
            litSuccess.Text = "<i class='fa fa-check-circle mr-2'></i> " & message
        End Sub

    End Class
End Namespace
