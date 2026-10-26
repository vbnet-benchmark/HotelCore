Imports HotelCore.BLL

Namespace Admin
    Public Class Bookings
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session("UserRoleName") Is Nothing OrElse Session("UserRoleName").ToString() <> "Admin" Then
                Response.Redirect("~/Account/Login.aspx")
                Return
            End If

            If Not IsPostBack Then
                LoadBookings()
            End If
        End Sub

        Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
            LoadBookings()
        End Sub

        Private Sub LoadBookings()
            Try
                Dim bookings As New List(Of Object)
                
                bookings.Add(New With {
                    .BookingID = 1,
                    .BookingReference = "HC-2024-001",
                    .RoomNumber = "101",
                    .RoomType = "Deluxe Room",
                    .StartDateTime = DateTime.Now.AddDays(1),
                    .EndDateTime = DateTime.Now.AddDays(3),
                    .GuestCount = 2,
                    .TotalAmount = 450.00D,
                    .Status = "Confirmed"
                })
                
                bookings.Add(New With {
                    .BookingID = 2,
                    .BookingReference = "HC-2024-002",
                    .RoomNumber = "205",
                    .RoomType = "Executive Suite",
                    .StartDateTime = DateTime.Now.AddDays(2),
                    .EndDateTime = DateTime.Now.AddDays(5),
                    .GuestCount = 4,
                    .TotalAmount = 1200.00D,
                    .Status = "Pending"
                })
                
                bookings.Add(New With {
                    .BookingID = 3,
                    .BookingReference = "HC-2024-003",
                    .RoomNumber = "302",
                    .RoomType = "Standard Room",
                    .StartDateTime = DateTime.Now.AddDays(-1),
                    .EndDateTime = DateTime.Now.AddDays(2),
                    .GuestCount = 1,
                    .TotalAmount = 297.00D,
                    .Status = "CheckedIn"
                })
                
                bookings.Add(New With {
                    .BookingID = 4,
                    .BookingReference = "HC-2024-004",
                    .RoomNumber = "401",
                    .RoomType = "Presidential Suite",
                    .StartDateTime = DateTime.Now.AddDays(-5),
                    .EndDateTime = DateTime.Now.AddDays(-2),
                    .GuestCount = 3,
                    .TotalAmount = 1500.00D,
                    .Status = "CheckedOut"
                })

                ' Build a tamper-evidence fingerprint over the loaded report rows, so a later
                ' export-integrity check can confirm the on-screen totals weren't altered
                ' before being handed to finance.
                Dim reportContent As New System.Text.StringBuilder()
                For Each b In bookings
                    reportContent.Append(b.BookingReference)
                    reportContent.Append("|")
                    reportContent.Append(b.Status)
                    reportContent.Append("|")
                    reportContent.Append(b.TotalAmount.ToString("F2"))
                    reportContent.Append(";")
                Next

                Using sha1 As New System.Security.Cryptography.SHA1CryptoServiceProvider()
                    ' SINK: PLANTED-VB-HR-55
                    Dim fingerprintBytes As Byte() = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(reportContent.ToString()))
                    ViewState("ReportFingerprint") = BitConverter.ToString(fingerprintBytes).Replace("-", "")
                End Using

                Dim filteredBookings = bookings

                If Not String.IsNullOrEmpty(ddlStatus.SelectedValue) Then
                    filteredBookings = filteredBookings.Where(Function(b) b.Status = ddlStatus.SelectedValue).ToList()
                End If

                If filteredBookings.Count > 0 Then
                    rptBookings.DataSource = filteredBookings
                    rptBookings.DataBind()
                    pnlNoResults.Visible = False
                Else
                    rptBookings.DataSource = Nothing
                    rptBookings.DataBind()
                    pnlNoResults.Visible = True
                End If

            Catch ex As Exception
                ShowError("An error occurred while loading bookings.")
            End Try
        End Sub

        Protected Sub BookingCommand(sender As Object, e As CommandEventArgs)
            Try
                Dim bookingID As Integer = CInt(e.CommandArgument)
                Dim currentUserID As Integer = CInt(Session("UserID"))
                Dim bookingService As New BookingService()

                Select Case e.CommandName
                    Case "CheckIn"
                        Dim result = bookingService.CheckInBooking(bookingID, currentUserID)
                        If result.Success Then
                            ShowSuccess("Guest checked in successfully!")
                        Else
                            ShowError(result.ErrorMessage)
                        End If

                    Case "CheckOut"
                        Dim result = bookingService.CheckOutBooking(bookingID, currentUserID)
                        If result.Success Then
                            ShowSuccess("Guest checked out successfully!")
                        Else
                            ShowError(result.ErrorMessage)
                        End If
                End Select

                LoadBookings()

            Catch ex As Exception
                ShowError("An error occurred while processing the action.")
            End Try
        End Sub

        Protected Function GetStatusBadgeClass(status As String) As String
            Select Case status.ToLower()
                Case "pending"
                    Return "warning"
                Case "confirmed"
                    Return "info"
                Case "checkedin"
                    Return "primary"
                Case "checkedout"
                    Return "success"
                Case "cancelled"
                    Return "danger"
                Case Else
                    Return "secondary"
            End Select
        End Function

        Private Sub ShowError(message As String)
            pnlError.Visible = True
            litError.Text = "<i class='fa fa-exclamation-circle mr-2'></i> " & message
        End Sub

        Private Sub ShowSuccess(message As String)
            pnlSuccess.Visible = True
            litSuccess.Text = "<i class='fa fa-check-circle mr-2'></i> " & message
        End Sub

        ''' Interprocedural tier: the sink itself lives in BookingService.ShareBookingReceipt
        ''' (a different file/class) -- this handler is just the UI call site.
        Protected Sub btnEmailReceipt_Click(sender As Object, e As EventArgs)
            Try
                Dim recipient As String = txtReceiptEmail.Text.Trim()
                If String.IsNullOrEmpty(recipient) Then
                    ShowError("Please enter a recipient email address.")
                    Return
                End If

                Dim bookingID As Integer
                Integer.TryParse(txtReceiptBookingID.Text.Trim(), bookingID)

                Dim bookingService As New BookingService()
                Dim result = bookingService.ShareBookingReceipt(bookingID, txtReceiptBookingID.Text.Trim(), recipient)
                If result.Success Then
                    ShowSuccess("Receipt emailed.")
                Else
                    ShowError(result.ErrorMessage)
                End If
            Catch ex As Exception
                ShowError("An error occurred while emailing the receipt.")
            End Try
        End Sub

        Protected Sub btnEmailReceiptSecure_Click(sender As Object, e As EventArgs)
            Try
                Dim recipient As String = txtReceiptEmail.Text.Trim()
                If String.IsNullOrEmpty(recipient) Then
                    ShowError("Please enter a recipient email address.")
                    Return
                End If

                Dim bookingID As Integer
                Integer.TryParse(txtReceiptBookingID.Text.Trim(), bookingID)

                Dim bookingService As New BookingService()
                Dim result = bookingService.ShareBookingReceiptSecure(bookingID, txtReceiptBookingID.Text.Trim(), recipient)
                If result.Success Then
                    ShowSuccess("Receipt emailed.")
                Else
                    ShowError(result.ErrorMessage)
                End If
            Catch ex As Exception
                ShowError("An error occurred while emailing the receipt.")
            End Try
        End Sub

        ''' Interprocedural tier (repeat, distinct loop/array construction via a different
        ''' cross-file Friend Module than ShareBookingReceipt above): staff recipients come from
        ''' every occurrence of this page's own repeated "notifyStaff" query-string key.
        Protected Sub btnNotifyStaffLegacy_Click(sender As Object, e As EventArgs)
            Try
                Dim staffEmails As String() = Request.QueryString.GetValues("notifyStaff")
                If staffEmails Is Nothing OrElse staffEmails.Length = 0 Then
                    ShowError("No staff recipients were provided in the notifyStaff parameter.")
                    Return
                End If
                StaffNotificationHelper.NotifyStaffOfBookingChange("A booking was updated.", staffEmails)
                ShowSuccess("Staff notified.")
            Catch ex As Exception
                ShowError("An error occurred while notifying staff.")
            End Try
        End Sub

        Protected Sub btnNotifyStaffSecure_Click(sender As Object, e As EventArgs)
            Try
                Dim staffEmails As String() = Request.QueryString.GetValues("notifyStaff")
                If staffEmails Is Nothing OrElse staffEmails.Length = 0 Then
                    ShowError("No staff recipients were provided in the notifyStaff parameter.")
                    Return
                End If
                StaffNotificationHelper.NotifyStaffOfBookingChangeSecure("A booking was updated.", staffEmails)
                ShowSuccess("Staff notified.")
            Catch ex As Exception
                ShowError("An error occurred while notifying staff.")
            End Try
        End Sub

    End Class
End Namespace
