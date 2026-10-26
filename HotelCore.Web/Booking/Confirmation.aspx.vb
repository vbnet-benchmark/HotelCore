Imports System.Net.Mail
Imports HotelCore.BLL

Namespace Booking
    Public Class Confirmation
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Not IsPostBack Then
                LoadBookingDetails()
            End If
        End Sub

        Private Sub LoadBookingDetails()
            Try
                Dim bookingID As Integer
                If Not Integer.TryParse(Request.QueryString("id"), bookingID) Then
                    ShowError("Invalid booking ID.")
                    Return
                End If

                Dim currentUserID As Integer? = Nothing
                If Session("UserID") IsNot Nothing Then
                    currentUserID = CInt(Session("UserID"))
                End If

                Dim bookingService As New BookingService()
                Dim result = bookingService.GetBookingById(bookingID, currentUserID)

                If result.Success AndAlso result.Data IsNot Nothing Then
                    Dim booking = result.Data

                    litBookingRef.Text = booking.BookingReference
                    litRoomType.Text = booking.RoomType
                    litRoomNumber.Text = booking.RoomNumber
                    litCheckIn.Text = booking.StartDateTime.ToShortDateString()
                    litCheckOut.Text = booking.EndDateTime.ToShortDateString()
                    litGuests.Text = booking.GuestCount.ToString()
                    litStatus.Text = booking.Status
                    litTotal.Text = booking.TotalAmount.ToString("N2")

                    If booking.Guests IsNot Nothing AndAlso booking.Guests.Count > 0 Then
                        pnlGuests.Visible = True
                        rptGuests.DataSource = booking.Guests
                        rptGuests.DataBind()
                    End If
                Else
                    ShowError(If(String.IsNullOrEmpty(result.ErrorMessage), "Booking not found.", result.ErrorMessage))
                End If

            Catch ex As Exception
                ShowError("An error occurred while loading booking details.")
            End Try
        End Sub

        Private Sub ShowError(message As String)
            pnlError.Visible = True
            pnlConfirmation.Visible = False
            litError.Text = "<i class='fa fa-exclamation-circle mr-2'></i> " & message
        End Sub

        ''' Emails a copy of this confirmation to whatever address the guest typed in, straight
        ''' from the postback -- no validation beyond what System.Net.Mail itself enforces.
        Protected Sub btnEmailConfirmation_Click(sender As Object, e As EventArgs)
            Try
                Dim shareEmail As String = txtShareEmail.Text.Trim()
                If String.IsNullOrEmpty(shareEmail) Then
                    ShowEmailStatus("Please enter an email address to share this confirmation with.")
                    Return
                End If

                Using msg As New MailMessage()
                    msg.From = New MailAddress("bookings@hotelcore.com", "HotelCore Reservations")
                    msg.Subject = "Your HotelCore Booking Confirmation - " & litBookingRef.Text
                    msg.Body = "Here is a copy of your booking confirmation. Reference: " & litBookingRef.Text
                    ' SINK: PLANTED-VB-HR-296
                    msg.To.Add(shareEmail)
                    Using client As New SmtpClient("mail.hotelcore-internal.local")
                        client.Send(msg)
                    End Using
                End Using

                ShowEmailStatus("A copy of your confirmation has been emailed.")

            Catch ex As Exception
                ShowEmailStatus("Unable to send the confirmation email right now.")
            End Try
        End Sub

        ''' Hardened counterpart of btnEmailConfirmation_Click: the typed address is validated
        ''' via the single-address MailAddress constructor (rejecting a comma/semicolon-smuggled
        ''' extra recipient) before the already-validated address object -- never the raw string
        ''' -- is added to the message.
        Protected Sub btnEmailConfirmationSecure_Click(sender As Object, e As EventArgs)
            Try
                Dim shareEmail As String = txtShareEmail.Text.Trim()
                If String.IsNullOrEmpty(shareEmail) Then
                    ShowEmailStatus("Please enter an email address to share this confirmation with.")
                    Return
                End If

                Dim validAddress As MailAddress
                Try
                    validAddress = New MailAddress(shareEmail)
                Catch ex As FormatException
                    ShowEmailStatus("Please enter a single, valid email address.")
                    Return
                End Try

                Using msg As New MailMessage()
                    msg.From = New MailAddress("bookings@hotelcore.com", "HotelCore Reservations")
                    msg.Subject = "Your HotelCore Booking Confirmation - " & litBookingRef.Text
                    msg.Body = "Here is a copy of your booking confirmation. Reference: " & litBookingRef.Text
                    ' SAFE_SINK: PLANTED-VB-HR-296-safe
                    msg.To.Add(validAddress)
                    Using client As New SmtpClient("mail.hotelcore-internal.local")
                        client.Send(msg)
                    End Using
                End Using

                ShowEmailStatus("A copy of your confirmation has been emailed.")

            Catch ex As Exception
                ShowEmailStatus("Unable to send the confirmation email right now.")
            End Try
        End Sub

        Private Sub ShowEmailStatus(message As String)
            pnlEmailStatus.Visible = True
            litEmailStatus.Text = message
        End Sub

    End Class
End Namespace
