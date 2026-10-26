Imports System.Net.Mail

''' Genuine interface-based dispatch for sending a booking-related notification email to
''' either the guest (a free-text address the guest themselves typed in) or an internal staff
''' distribution address. NotificationSenderFactory resolves which concrete sender to use
''' purely from a client-visible "who is this notification for" selector -- a routing choice,
''' not a trust or security decision (mirrors this project's own established framing for a
''' request-visible dispatch condition with no trust claim, e.g. CWE-328's/CWE-209's dispatch
''' conditions elsewhere in this codebase).
Public Interface INotificationSender
    Sub Send(bookingReference As String, recipientRaw As String)
End Interface

''' Sends directly to whatever address was supplied, with no validation beyond whatever
''' System.Net.Mail itself enforces at parse time.
Public Class GuestEmailNotifier
    Implements INotificationSender

    Public Sub Send(bookingReference As String, recipientRaw As String) Implements INotificationSender.Send
        Using msg As New MailMessage()
            msg.From = New MailAddress("notifications@hotelcore.com", "HotelCore Notifications")
            msg.Subject = "HotelCore Booking Update - " & bookingReference
            msg.Body = "There is an update on your booking " & bookingReference & "."
            ' SINK: PLANTED-VB-HR-299
            msg.To.Add(recipientRaw)
            Using client As New SmtpClient("mail.hotelcore-internal.local")
                client.Send(msg)
            End Using
        End Using
    End Sub
End Class

''' Sends to an internal staff address. The address is validated via the single-address
''' MailAddress constructor before being added, so a smuggled extra recipient (e.g. a
''' comma-separated value) is rejected rather than silently split into two recipients.
Public Class StaffEmailNotifier
    Implements INotificationSender

    Public Sub Send(bookingReference As String, recipientRaw As String) Implements INotificationSender.Send
        Dim validAddress As MailAddress
        Try
            validAddress = New MailAddress(recipientRaw)
        Catch ex As FormatException
            Throw New Exception("Please enter a single, valid staff email address.")
        End Try

        Using msg As New MailMessage()
            msg.From = New MailAddress("notifications@hotelcore.com", "HotelCore Notifications")
            msg.Subject = "HotelCore Booking Update - " & bookingReference
            msg.Body = "There is an update on booking " & bookingReference & "."
            ' SAFE_SINK: PLANTED-VB-HR-299-safe
            msg.To.Add(validAddress)
            Using client As New SmtpClient("mail.hotelcore-internal.local")
                client.Send(msg)
            End Using
        End Using
    End Sub
End Class

''' Resolves which concrete INotificationSender to use. notifyTarget is a plain
''' "who is this notification for" routing selector, not a trust or security decision.
Public Module NotificationSenderFactory

    Public Function GetSender(notifyTarget As String) As INotificationSender
        If String.Equals(notifyTarget, "Staff", StringComparison.OrdinalIgnoreCase) Then
            Return New StaffEmailNotifier()
        Else
            Return New GuestEmailNotifier()
        End If
    End Function

End Module
