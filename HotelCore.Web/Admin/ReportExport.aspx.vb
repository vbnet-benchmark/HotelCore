Imports HotelCore.BLL

Namespace Admin
    Public Class ReportExport
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session("UserRoleName") Is Nothing OrElse Session("UserRoleName").ToString() <> "Admin" Then
                Response.Redirect("~/Account/Login.aspx")
                Return
            End If
        End Sub

        Protected Sub btnExport_Click(sender As Object, e As EventArgs)
            Try
                pnlError.Visible = False
                pnlSuccess.Visible = False

                Dim currentUserID As Integer = CInt(Session("UserID"))
                Dim fileName As String = txtExportFileName.Text.Trim()
                Dim exportDir As String = Server.MapPath("~/App_Data/Exports")
                If Not System.IO.Directory.Exists(exportDir) Then System.IO.Directory.CreateDirectory(exportDir)

                Dim exportPath As String = System.IO.Path.Combine(exportDir, fileName & ".csv")

                Dim bookingService As New BookingService()
                Dim result = bookingService.ExportBookingReport(currentUserID, exportPath)

                If result.Success Then
                    ShowSuccess($"Report exported to {result.Data}")
                ElseIf chkShowTechnicalDetails.Checked Then
                    pnlError.Visible = True
                    ' SINK: PLANTED-VB-HR-143
                    litError.Text = "<i class='fa fa-exclamation-circle mr-2'></i> " & result.ErrorMessage
                Else
                    pnlError.Visible = True
                    ' SAFE_SINK: PLANTED-VB-HR-143-safe
                    litError.Text = "<i class='fa fa-exclamation-circle mr-2'></i> Report export failed. Please verify the export location and try again."
                End If

            Catch ex As Exception
                ShowError("An error occurred while exporting the report.")
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
