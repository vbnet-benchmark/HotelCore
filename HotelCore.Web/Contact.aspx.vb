Namespace HotelCore.Web
    Public Class Contact
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        End Sub

        Protected Sub btnSubmit_Click(sender As Object, e As EventArgs)
            Try
                pnlError.Visible = False
                pnlSuccess.Visible = False

                If String.IsNullOrEmpty(txtName.Text.Trim()) Then
                    ShowError("Please enter your name.")
                    Return
                End If

                If String.IsNullOrEmpty(txtEmail.Text.Trim()) Then
                    ShowError("Please enter your email.")
                    Return
                End If

                If String.IsNullOrEmpty(txtMessage.Text.Trim()) Then
                    ShowError("Please enter your message.")
                    Return
                End If

                ' Record every contact-form submission to the activity log so the front-desk
                ' team can follow up even if the notification email bounces. A visitor who is
                ' already signed in gets the hardened logging path below -- see
                ' LogSubmissionSecure for why.
                If Session("UserID") IsNot Nothing Then
                    LogSubmissionSecure(txtName.Text.Trim(), txtEmail.Text.Trim(), txtMessage.Text.Trim())
                Else
                    LogSubmission(txtName.Text.Trim(), txtEmail.Text.Trim(), txtMessage.Text.Trim())
                End If

                pnlSuccess.Visible = True
                pnlForm.Visible = False

            Catch ex As Exception
                ShowError("An error occurred. Please try again.")
            End Try
        End Sub

        ''' Appends one line to the contact-activity log for an anonymous (not-signed-in)
        ''' visitor's submission. Builds the line piece by piece from the raw form fields.
        Private Sub LogSubmission(name As String, email As String, message As String)
            Dim logDir As String = Server.MapPath("~/App_Data")
            If Not System.IO.Directory.Exists(logDir) Then System.IO.Directory.CreateDirectory(logDir)

            Dim line As String = "[" & DateTime.UtcNow.ToString() & "] CONTACT_SUBMITTED - Name: " & name
            line &= ", Email: " & email
            line &= ", Message: " & message
            ' SINK: PLANTED-VB-HR-92
            System.IO.File.AppendAllText(Server.MapPath("~/App_Data/contact-activity.log"), line & Environment.NewLine)
        End Sub

        ''' Hardened counterpart of LogSubmission, used once the visitor is already
        ''' authenticated: every field is stripped of embedded CR/LF before it's written, so a
        ''' crafted message/name can't forge an extra log line.
        Private Sub LogSubmissionSecure(name As String, email As String, message As String)
            Dim logDir As String = Server.MapPath("~/App_Data")
            If Not System.IO.Directory.Exists(logDir) Then System.IO.Directory.CreateDirectory(logDir)

            Dim safeName As String = name.Replace(vbCr, " ").Replace(vbLf, " ")
            Dim safeEmail As String = email.Replace(vbCr, " ").Replace(vbLf, " ")
            Dim safeMessage As String = message.Replace(vbCr, " ").Replace(vbLf, " ")

            Dim line As String = "[" & DateTime.UtcNow.ToString() & "] CONTACT_SUBMITTED - Name: " & safeName
            line &= ", Email: " & safeEmail
            line &= ", Message: " & safeMessage
            ' SAFE_SINK: PLANTED-VB-HR-92-safe
            System.IO.File.AppendAllText(Server.MapPath("~/App_Data/contact-activity.log"), line & Environment.NewLine)
        End Sub

        Private Sub ShowError(message As String)
            pnlError.Visible = True
            litError.Text = "<i class='fa fa-exclamation-circle mr-2'></i> " & message
        End Sub

    End Class
End Namespace
