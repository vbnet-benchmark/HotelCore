Imports System.Data
Imports System.Net.Mail
Imports HotelCore.BLL

Namespace Account
    Public Class BookingDetails
        Inherits System.Web.UI.Page

        Private _bookingID As Integer = 0
        Private _bookingStatus As String = ""

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session("UserID") Is Nothing Then
                Response.Redirect("~/Account/Login.aspx?returnUrl=" & Server.UrlEncode(Request.RawUrl))
                Return
            End If

            If Not Integer.TryParse(Request.QueryString("id"), _bookingID) Then
                ShowError("Invalid booking ID.")
                Return
            End If

            If Not IsPostBack Then
                LoadBookingDetails()
            End If
        End Sub

        Private Sub LoadBookingDetails()
            Try
                Dim currentUserID As Integer = CInt(Session("UserID"))

                Dim bookingService As New BookingService()
                Dim result = bookingService.GetBookingById(_bookingID, currentUserID)

                If result.Success AndAlso result.Data IsNot Nothing Then
                    Dim booking = result.Data

                    _bookingStatus = booking.Status
                    ViewState("BookingStatus") = booking.Status

                    litBookingRef.Text = booking.BookingReference
                    litRoomType.Text = booking.RoomType
                    litRoomNumber.Text = booking.RoomNumber
                    litCheckIn.Text = booking.StartDateTime.ToShortDateString()
                    litCheckOut.Text = booking.EndDateTime.ToShortDateString()
                    litGuests.Text = booking.GuestCount.ToString()
                    litTotal.Text = booking.TotalAmount.ToString("N2")
                    litStatus.Text = booking.Status
                    litBookingDate.Text = booking.CreatedDate.ToShortDateString()

                    If booking.Guests IsNot Nothing AndAlso booking.Guests.Count > 0 Then
                        pnlGuests.Visible = True
                        rptGuests.DataSource = booking.Guests
                        rptGuests.DataBind()
                    End If

                    If booking.Payments IsNot Nothing AndAlso booking.Payments.Count > 0 Then
                        pnlPayments.Visible = True
                        rptPayments.DataSource = booking.Payments
                        rptPayments.DataBind()
                    End If

                    If booking.Status = "Cancelled" Then
                        pnlCancellationInfo.Visible = True
                        pnlCanCancel.Visible = False
                        litCancelledDate.Text = If(booking.CancelledDate.HasValue, booking.CancelledDate.Value.ToShortDateString(), "N/A")
                        litCancellationReason.Text = If(String.IsNullOrEmpty(booking.CancellationReason), "No reason provided", booking.CancellationReason)
                    Else
                        Dim canCancel As Boolean = (booking.Status = "Pending" OrElse booking.Status = "Confirmed") AndAlso booking.StartDateTime > DateTime.Now.AddHours(24)
                        pnlCanCancel.Visible = canCancel
                    End If
                Else
                    ShowError(If(String.IsNullOrEmpty(result.ErrorMessage), "Booking not found.", result.ErrorMessage))
                End If

            Catch ex As Exception
                ShowError("An error occurred while loading booking details.")
            End Try
        End Sub

        Protected Sub btnCancelBooking_Click(sender As Object, e As EventArgs)
            pnlBookingDetails.Visible = False
            pnlCancelForm.Visible = True
        End Sub

        Protected Sub btnCancelCancelation_Click(sender As Object, e As EventArgs)
            pnlBookingDetails.Visible = True
            pnlCancelForm.Visible = False
        End Sub

        Protected Sub btnConfirmCancel_Click(sender As Object, e As EventArgs)
            Try
                Dim reason As String = txtCancellationReason.Text.Trim()

                If String.IsNullOrEmpty(reason) OrElse reason.Length < 10 Then
                    ShowError("Cancellation reason must be at least 10 characters.")
                    Return
                End If

                Dim currentUserID As Integer = CInt(Session("UserID"))

                Dim bookingService As New BookingService()
                Dim result = bookingService.CancelBooking(_bookingID, reason, currentUserID)

                If result.Success Then
                    ShowSuccess("Booking cancelled successfully.")
                    pnlCancelForm.Visible = False
                    pnlBookingDetails.Visible = True
                    LoadBookingDetails()
                Else
                    ShowError(If(String.IsNullOrEmpty(result.ErrorMessage), "Failed to cancel booking.", result.ErrorMessage))
                End If

            Catch ex As Exception
                ShowError("An error occurred while cancelling the booking.")
            End Try
        End Sub

        Protected Function GetStatusBadgeClass() As String
            Dim status As String = If(ViewState("BookingStatus") IsNot Nothing, ViewState("BookingStatus").ToString(), "")
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

        Protected Sub btnResendConfirmation_Click(sender As Object, e As EventArgs)
            Try
                Dim recipient As String = txtResendEmail.Text.Trim()
                If String.IsNullOrEmpty(recipient) Then
                    ShowError("Please enter an email address.")
                    Return
                End If
                SendConfirmationCopy(litBookingRef.Text, recipient)
                ShowSuccess("Confirmation re-sent.")
            Catch ex As Exception
                ShowError("Unable to resend the confirmation right now.")
            End Try
        End Sub

        ''' One-hop private helper: builds and sends the confirmation copy. Called directly from
        ''' btnResendConfirmation_Click above -- the indirect-tier counterpart of
        ''' Confirmation.aspx.vb's direct-tier btnEmailConfirmation_Click.
        Private Sub SendConfirmationCopy(bookingReference As String, recipientRaw As String)
            Using msg As New MailMessage()
                msg.From = New MailAddress("bookings@hotelcore.com", "HotelCore Reservations")
                msg.Subject = "Your HotelCore Booking Confirmation - " & bookingReference
                msg.Body = "Here is a copy of your booking confirmation. Reference: " & bookingReference
                ' SINK: PLANTED-VB-HR-297
                msg.To.Add(recipientRaw)
                Using client As New SmtpClient("mail.hotelcore-internal.local")
                    client.Send(msg)
                End Using
            End Using
        End Sub

        Protected Sub btnResendConfirmationSecure_Click(sender As Object, e As EventArgs)
            Try
                Dim recipient As String = txtResendEmail.Text.Trim()
                If String.IsNullOrEmpty(recipient) Then
                    ShowError("Please enter an email address.")
                    Return
                End If
                SendConfirmationCopySecure(litBookingRef.Text, recipient)
                ShowSuccess("Confirmation re-sent.")
            Catch ex As Exception
                ShowError("Unable to re-send the confirmation right now.")
            End Try
        End Sub

        ''' Hardened counterpart of SendConfirmationCopy: the recipient is validated via the
        ''' single-address MailAddress constructor (rejecting a comma/semicolon-smuggled extra
        ''' recipient) before the already-validated address object -- never the raw string -- is
        ''' added to the message.
        Private Sub SendConfirmationCopySecure(bookingReference As String, recipientRaw As String)
            Dim validAddress As MailAddress
            Try
                validAddress = New MailAddress(recipientRaw)
            Catch ex As FormatException
                Throw New Exception("Please enter a single, valid email address.")
            End Try

            Using msg As New MailMessage()
                msg.From = New MailAddress("bookings@hotelcore.com", "HotelCore Reservations")
                msg.Subject = "Your HotelCore Booking Confirmation - " & bookingReference
                msg.Body = "Here is a copy of your booking confirmation. Reference: " & bookingReference
                ' SAFE_SINK: PLANTED-VB-HR-297-safe
                msg.To.Add(validAddress)
                Using client As New SmtpClient("mail.hotelcore-internal.local")
                    client.Send(msg)
                End Using
            End Using
        End Sub

        ''' Sends a booking-update notification to either the guest or internal staff,
        ''' resolving the concrete INotificationSender implementation to use via a genuine
        ''' interface dispatch -- see HotelCore.BLL.Common.BookingNotificationService
        ''' (NotificationSenderFactory / GuestEmailNotifier / StaffEmailNotifier). The dropdown
        ''' selection is a plain "who is this for" routing choice, not a trust decision.
        Protected Sub btnSendNotification_Click(sender As Object, e As EventArgs)
            Try
                Dim recipient As String = txtNotifyEmail.Text.Trim()
                If String.IsNullOrEmpty(recipient) Then
                    ShowError("Please enter an email address to notify.")
                    Return
                End If

                Dim notifier As INotificationSender = NotificationSenderFactory.GetSender(ddlNotifyTarget.SelectedValue)
                notifier.Send(litBookingRef.Text, recipient)

                ShowSuccess("Notification sent.")
            Catch ex As Exception
                ShowError("Unable to send the notification right now.")
            End Try
        End Sub

        ''' Lets a group organizer who holds several travelers' booking IDs (e.g. a corporate
        ''' trip coordinator) pull up another booking's guest roster from this same account
        ''' page, without needing to be logged in as that booking's own guest.
        Protected Sub btnLookupGroupRoster_Click(sender As Object, e As EventArgs)
            Try
                Dim groupBookingID As Integer
                If Not Integer.TryParse(txtGroupBookingID.Text.Trim(), groupBookingID) Then
                    ShowError("Please enter a valid booking ID.")
                    Return
                End If

                LoadGroupGuestRoster(groupBookingID)
            Catch ex As Exception
                ShowError("Unable to load the guest roster right now.")
            End Try
        End Sub

        ''' Indirect tier: the actual lookup lives in this private helper, one hop away from
        ''' the public click handler above. It fetches the full booking (including its guest
        ''' roster -- names, email, age category) for an arbitrary client-supplied booking ID,
        ''' with no check that the requesting guest owns that booking.
        Private Sub LoadGroupGuestRoster(groupBookingID As Integer)
            Dim bookingService As New BookingService()
            ' SINK: PLANTED-VB-HR-427
            Dim result = bookingService.GetBookingById(groupBookingID, Nothing)

            If result.Success AndAlso result.Data IsNot Nothing Then
                rptGroupRoster.DataSource = result.Data.Guests
                rptGroupRoster.DataBind()
                pnlGroupRoster.Visible = True
            Else
                ShowError("No booking found with that ID.")
            End If
        End Sub

        Protected Sub btnLookupGroupRosterSecure_Click(sender As Object, e As EventArgs)
            Try
                Dim groupBookingID As Integer
                If Not Integer.TryParse(txtGroupBookingID.Text.Trim(), groupBookingID) Then
                    ShowError("Please enter a valid booking ID.")
                    Return
                End If

                Dim currentUserID As Integer = CInt(Session("UserID"))
                LoadGroupGuestRosterSecure(groupBookingID, currentUserID)
            Catch ex As Exception
                ShowError("Unable to load the guest roster right now.")
            End Try
        End Sub

        ''' Hardened counterpart of LoadGroupGuestRoster: passes the requesting guest's own
        ''' authenticated UserID through to the lookup, so the underlying stored procedure's
        ''' own ownership filter (the same one Page_Load/LoadBookingDetails already relies on
        ''' for this page's main booking panel) actually applies here too.
        Private Sub LoadGroupGuestRosterSecure(groupBookingID As Integer, currentUserID As Integer)
            Dim bookingService As New BookingService()
            ' SAFE_SINK: PLANTED-VB-HR-427-safe
            Dim result = bookingService.GetBookingById(groupBookingID, currentUserID)

            If result.Success AndAlso result.Data IsNot Nothing Then
                rptGroupRosterSecure.DataSource = result.Data.Guests
                rptGroupRosterSecure.DataBind()
                pnlGroupRosterSecure.Visible = True
            Else
                ShowError("No booking found with that ID.")
            End If
        End Sub

        ''' Lets a guest pull up the receipt for a specific payment on their account by its raw
        ''' PaymentID (e.g. copied from a bank/card statement line referencing the payment
        ''' reference) without re-navigating through the booking it belongs to.
        Protected Sub btnViewPaymentReceipt_Click(sender As Object, e As EventArgs)
            Try
                Dim paymentID As Integer
                If Not Integer.TryParse(txtReceiptPaymentID.Text.Trim(), paymentID) Then
                    ShowError("Please enter a valid payment ID.")
                    Return
                End If

                Dim paymentService As New PaymentService()
                ' SINK: PLANTED-VB-HR-429
                Dim row As DataRow = paymentService.GetPaymentReceiptById(paymentID)

                If row IsNot Nothing Then
                    litReceiptReference.Text = row("PaymentReference").ToString()
                    litReceiptMethod.Text = row("PaymentMethod").ToString()
                    litReceiptAmount.Text = CDec(row("Amount")).ToString("N2")
                    litReceiptStatus.Text = row("Status").ToString()
                    litReceiptBookingRef.Text = row("BookingReference").ToString()
                    pnlReceipt.Visible = True
                Else
                    ShowError("No payment found with that ID.")
                End If
            Catch ex As Exception
                ShowError("Unable to load the payment receipt right now.")
            End Try
        End Sub

        Protected Sub btnViewPaymentReceiptSecure_Click(sender As Object, e As EventArgs)
            Try
                Dim paymentID As Integer
                If Not Integer.TryParse(txtReceiptPaymentID.Text.Trim(), paymentID) Then
                    ShowError("Please enter a valid payment ID.")
                    Return
                End If

                Dim currentUserID As Integer = CInt(Session("UserID"))

                ''' Hardened counterpart of btnViewPaymentReceipt_Click: dispatches to the
                ''' genuine polymorphic ownership-accessor hierarchy (OwnedRecordAccessorBase /
                ''' PaymentOwnedRecordAccessor) to confirm the requesting guest owns the
                ''' booking this payment belongs to -- resolved by joining through Payments to
                ''' Bookings, since Payments has no UserID column of its own -- before the
                ''' receipt is ever fetched for display.
                Dim accessor = OwnedRecordAccessorFactory.GetAccessor("Payment")
                If Not accessor.IsOwnedBy(paymentID, currentUserID) Then
                    ShowError("No payment found with that ID.")
                    Return
                End If

                Dim paymentService As New PaymentService()
                ' SAFE_SINK: PLANTED-VB-HR-429-safe
                Dim row As DataRow = paymentService.GetPaymentReceiptById(paymentID)

                If row IsNot Nothing Then
                    litReceiptReferenceSecure.Text = row("PaymentReference").ToString()
                    litReceiptMethodSecure.Text = row("PaymentMethod").ToString()
                    litReceiptAmountSecure.Text = CDec(row("Amount")).ToString("N2")
                    litReceiptStatusSecure.Text = row("Status").ToString()
                    litReceiptBookingRefSecure.Text = row("BookingReference").ToString()
                    pnlReceiptSecure.Visible = True
                Else
                    ShowError("No payment found with that ID.")
                End If
            Catch ex As Exception
                ShowError("Unable to load the payment receipt right now.")
            End Try
        End Sub

    End Class
End Namespace
