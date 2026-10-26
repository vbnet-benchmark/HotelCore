Imports HotelCore.BLL

Namespace Account
    Public Class Profile
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session("UserID") Is Nothing Then
                Response.Redirect("~/Account/Login.aspx?returnUrl=" & Server.UrlEncode(Request.RawUrl))
                Return
            End If

            If Not IsPostBack Then
                LoadUserInfo()
                
                If Request.QueryString("tab") = "bookings" Then
                    ShowBookingsPanel()
                End If
            End If
        End Sub

        Private Sub LoadUserInfo()
            litUserName.Text = If(Session("UserFullName") IsNot Nothing, Session("UserFullName").ToString(), "User")
            litUserEmail.Text = If(Session("UserEmail") IsNot Nothing, Session("UserEmail").ToString(), "")

            txtEmail.Text = If(Session("UserEmail") IsNot Nothing, Session("UserEmail").ToString(), "")
            
            If Session("UserFirstName") IsNot Nothing Then
                txtFirstName.Text = Session("UserFirstName").ToString()
            End If
            If Session("UserLastName") IsNot Nothing Then
                txtLastName.Text = Session("UserLastName").ToString()
            End If
            If Session("UserPhone") IsNot Nothing Then
                txtPhone.Text = Session("UserPhone").ToString()
            End If
        End Sub

        Protected Sub lnkProfile_Click(sender As Object, e As EventArgs)
            ShowProfilePanel()
        End Sub

        Protected Sub lnkBookings_Click(sender As Object, e As EventArgs)
            ShowBookingsPanel()
        End Sub

        Protected Sub lnkChangePassword_Click(sender As Object, e As EventArgs)
            ShowChangePasswordPanel()
        End Sub

        Private Sub ShowProfilePanel()
            pnlProfileInfo.Visible = True
            pnlBookings.Visible = False
            pnlChangePassword.Visible = False
            
            lnkProfile.CssClass = "d-block p-2 bg-light rounded"
            lnkBookings.CssClass = "d-block p-2"
            lnkChangePassword.CssClass = "d-block p-2"
        End Sub

        Private Sub ShowBookingsPanel()
            pnlProfileInfo.Visible = False
            pnlBookings.Visible = True
            pnlChangePassword.Visible = False
            
            lnkProfile.CssClass = "d-block p-2"
            lnkBookings.CssClass = "d-block p-2 bg-light rounded"
            lnkChangePassword.CssClass = "d-block p-2"
            
            LoadBookings()
        End Sub

        Private Sub ShowChangePasswordPanel()
            pnlProfileInfo.Visible = False
            pnlBookings.Visible = False
            pnlChangePassword.Visible = True
            
            lnkProfile.CssClass = "d-block p-2"
            lnkBookings.CssClass = "d-block p-2"
            lnkChangePassword.CssClass = "d-block p-2 bg-light rounded"
        End Sub

        Private Sub LoadBookings()
            Try
                Dim userID As Integer = CInt(Session("UserID"))
                Dim status As String = ddlBookingStatus.SelectedValue

                Dim bookingService As New BookingService()
                Dim result = bookingService.GetUserBookings(userID, status, Nothing, Nothing)

                If result.Success AndAlso result.Data IsNot Nothing Then
                    If result.Data.Count > 0 Then
                        rptBookings.DataSource = result.Data
                        rptBookings.DataBind()
                        pnlNoBookings.Visible = False
                    Else
                        rptBookings.DataSource = Nothing
                        rptBookings.DataBind()
                        pnlNoBookings.Visible = True
                    End If
                Else
                    pnlNoBookings.Visible = True
                End If

            Catch ex As Exception
                ShowError("An error occurred while loading bookings.")
            End Try
        End Sub

        Protected Sub ddlBookingStatus_SelectedIndexChanged(sender As Object, e As EventArgs)
            LoadBookings()
        End Sub

        Protected Sub btnUpdateProfile_Click(sender As Object, e As EventArgs)
            Try
                pnlSuccess.Visible = False
                pnlError.Visible = False

                If String.IsNullOrEmpty(txtFirstName.Text.Trim()) Then
                    ShowError("First name is required.")
                    Return
                End If

                If String.IsNullOrEmpty(txtLastName.Text.Trim()) Then
                    ShowError("Last name is required.")
                    Return
                End If

                Dim userID As Integer = CInt(Session("UserID"))
                Dim user As New UserModel With {
                    .UserID = userID,
                    .FirstName = txtFirstName.Text.Trim(),
                    .LastName = txtLastName.Text.Trim(),
                    .PhoneNumber = txtPhone.Text.Trim()
                }

                Dim userService As New UserService()
                Dim result = userService.UpdateUserProfile(user, userID)

                If result.Success Then
                    Session("UserFullName") = txtFirstName.Text.Trim() & " " & txtLastName.Text.Trim()
                    Session("UserFirstName") = txtFirstName.Text.Trim()
                    Session("UserLastName") = txtLastName.Text.Trim()
                    Session("UserPhone") = txtPhone.Text.Trim()

                    litUserName.Text = Session("UserFullName").ToString()
                    ShowSuccess("Profile updated successfully!")
                Else
                    ShowError(If(String.IsNullOrEmpty(result.ErrorMessage), "Failed to update profile.", result.ErrorMessage))
                End If

            Catch ex As Exception
                ShowError("An error occurred while updating profile.")
            End Try
        End Sub

        Protected Sub btnChangePassword_Click(sender As Object, e As EventArgs)
            Try
                pnlSuccess.Visible = False
                pnlError.Visible = False

                If String.IsNullOrEmpty(txtCurrentPassword.Text) Then
                    ShowError("Current password is required.")
                    Return
                End If

                If String.IsNullOrEmpty(txtNewPassword.Text) Then
                    ShowError("New password is required.")
                    Return
                End If

                If txtNewPassword.Text.Length < 8 Then
                    ShowError("New password must be at least 8 characters.")
                    Return
                End If

                If txtNewPassword.Text <> txtConfirmNewPassword.Text Then
                    ShowError("New passwords do not match.")
                    Return
                End If

                ShowSuccess("Password changed successfully!")
                txtCurrentPassword.Text = ""
                txtNewPassword.Text = ""
                txtConfirmNewPassword.Text = ""

            Catch ex As Exception
                ShowError("An error occurred while changing password.")
            End Try
        End Sub

        ''' Lets a signed-in guest pull up a summary of a booking shared with them by another
        ''' traveler in their group (e.g. a corporate trip coordinator who booked on their
        ''' behalf), using only the raw booking ID the coordinator handed them.
        Protected Sub btnViewSharedBooking_Click(sender As Object, e As EventArgs)
            Try
                Dim sharedBookingID As Integer
                If Not Integer.TryParse(txtSharedBookingID.Text.Trim(), sharedBookingID) Then
                    ShowError("Please enter a valid booking ID.")
                    Return
                End If

                ''' Interprocedural tier: the lookup is delegated to GuestPortalService, a
                ''' genuinely separate BLL class/file from this page and from BookingService --
                ''' its own GetSharedBookingSummary is where the unauthorized-reachable fetch
                ''' actually happens (see GuestPortalService.vb, PLANTED-VB-HR-428).
                Dim guestPortalService As New GuestPortalService()
                Dim summary As BookingModel = guestPortalService.GetSharedBookingSummary(sharedBookingID)

                If summary IsNot Nothing Then
                    litSharedBookingRef.Text = summary.BookingReference
                    litSharedBookingStatus.Text = summary.Status
                    litSharedBookingTotal.Text = summary.TotalAmount.ToString("N2")
                    rptSharedBookingGuests.DataSource = summary.Guests
                    rptSharedBookingGuests.DataBind()
                    pnlSharedBooking.Visible = True
                Else
                    ShowError("No booking found with that ID.")
                End If
            Catch ex As Exception
                ShowError("Unable to load that booking right now.")
            End Try
        End Sub

        Protected Sub btnViewSharedBookingSecure_Click(sender As Object, e As EventArgs)
            Try
                Dim sharedBookingID As Integer
                If Not Integer.TryParse(txtSharedBookingID.Text.Trim(), sharedBookingID) Then
                    ShowError("Please enter a valid booking ID.")
                    Return
                End If

                Dim currentUserID As Integer = CInt(Session("UserID"))

                ''' Hardened counterpart of btnViewSharedBooking_Click: calls
                ''' GetSharedBookingSummarySecure instead, which only returns a summary when
                ''' the requesting guest is themselves the booking's owner.
                Dim guestPortalService As New GuestPortalService()
                Dim summary As BookingModel = guestPortalService.GetSharedBookingSummarySecure(sharedBookingID, currentUserID)

                If summary IsNot Nothing Then
                    litSharedBookingRefSecure.Text = summary.BookingReference
                    litSharedBookingStatusSecure.Text = summary.Status
                    litSharedBookingTotalSecure.Text = summary.TotalAmount.ToString("N2")
                    rptSharedBookingGuestsSecure.DataSource = summary.Guests
                    rptSharedBookingGuestsSecure.DataBind()
                    pnlSharedBookingSecure.Visible = True
                Else
                    ShowError("No booking found with that ID.")
                End If
            Catch ex As Exception
                ShowError("Unable to load that booking right now.")
            End Try
        End Sub

        ''' Lets a guest export a quick CSV-style summary of several of their stays at once
        ''' (e.g. for an expense report) by typing a comma-separated list of booking IDs,
        ''' rather than exporting one booking at a time.
        Protected Sub btnExportSelectedBookings_Click(sender As Object, e As EventArgs)
            Try
                Dim idsCsv As String = txtExportBookingIDs.Text.Trim()
                If String.IsNullOrEmpty(idsCsv) Then
                    ShowError("Please enter at least one booking ID to export.")
                    Return
                End If

                Dim bookingService As New BookingService()
                Dim sb As New System.Text.StringBuilder()
                sb.AppendLine("BookingReference,Status,TotalAmount")

                ''' Diversity tier: walks a comma-separated list of client-supplied booking IDs
                ''' and, for each one, fetches the booking directly -- no ownership check is
                ''' applied to any ID in the list.
                For Each idText As String In idsCsv.Split(","c)
                    Dim bookingID As Integer
                    If Integer.TryParse(idText.Trim(), bookingID) Then
                        ' SINK: PLANTED-VB-HR-430
                        Dim result = bookingService.GetBookingById(bookingID, Nothing)
                        If result.Success AndAlso result.Data IsNot Nothing Then
                            Dim b = result.Data
                            sb.AppendLine($"{b.BookingReference},{b.Status},{b.TotalAmount}")
                        End If
                    End If
                Next

                litExportContent.Text = Server.HtmlEncode(sb.ToString())
                pnlExportResult.Visible = True
            Catch ex As Exception
                ShowError("Unable to export the selected bookings right now.")
            End Try
        End Sub

        Protected Sub btnExportSelectedBookingsSecure_Click(sender As Object, e As EventArgs)
            Try
                Dim idsCsv As String = txtExportBookingIDs.Text.Trim()
                If String.IsNullOrEmpty(idsCsv) Then
                    ShowError("Please enter at least one booking ID to export.")
                    Return
                End If

                Dim currentUserID As Integer = CInt(Session("UserID"))
                Dim bookingService As New BookingService()
                Dim sb As New System.Text.StringBuilder()
                sb.AppendLine("BookingReference,Status,TotalAmount")

                ''' Hardened counterpart of btnExportSelectedBookings_Click: passes the
                ''' requesting guest's own authenticated UserID into every iteration's lookup,
                ''' so an ID in the list that isn't this guest's own booking is silently
                ''' skipped (GetBookingById's underlying stored procedure enforces the match)
                ''' instead of being exported.
                For Each idText As String In idsCsv.Split(","c)
                    Dim bookingID As Integer
                    If Integer.TryParse(idText.Trim(), bookingID) Then
                        ' SAFE_SINK: PLANTED-VB-HR-430-safe
                        Dim result = bookingService.GetBookingById(bookingID, currentUserID)
                        If result.Success AndAlso result.Data IsNot Nothing Then
                            Dim b = result.Data
                            sb.AppendLine($"{b.BookingReference},{b.Status},{b.TotalAmount}")
                        End If
                    End If
                Next

                litExportContentSecure.Text = Server.HtmlEncode(sb.ToString())
                pnlExportResultSecure.Visible = True
            Catch ex As Exception
                ShowError("Unable to export the selected bookings right now.")
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

    End Class
End Namespace
