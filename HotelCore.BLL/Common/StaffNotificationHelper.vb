Imports System.Net.Mail

''' Cross-file helper (a distinct module from BookingNotificationService.vb/BookingService.vb)
''' for CC'ing a list of internal staff addresses on a booking-change event. The staff list is
''' supplied by the caller as an array (e.g. every occurrence of a repeated
''' Request.QueryString key), so the number of recipients varies per call. Public (not Friend):
''' this module lives in HotelCore.BLL and is called from HotelCore.Web, a separate assembly
''' with no InternalsVisibleTo between them, so a Friend-scoped member would not be accessible
''' there.
Public Module StaffNotificationHelper

    Public Sub NotifyStaffOfBookingChange(eventDescription As String, staffEmailsRaw As String())
        Using msg As New MailMessage()
            msg.From = New MailAddress("alerts@hotelcore.com", "HotelCore Alerts")
            msg.Subject = "HotelCore Staff Alert"
            msg.Body = eventDescription
            For Each rawAddress As String In staffEmailsRaw
                ' SINK: PLANTED-VB-HR-300
                msg.CC.Add(rawAddress)
            Next
            Using client As New SmtpClient("mail.hotelcore-internal.local")
                client.Send(msg)
            End Using
        End Using
    End Sub

    ''' Hardened counterpart: each staff address is individually validated via the
    ''' single-address MailAddress constructor before being added; an invalid/malformed entry
    ''' (including one carrying a smuggled extra comma-separated address) is skipped rather
    ''' than letting the whole notification fail.
    Public Sub NotifyStaffOfBookingChangeSecure(eventDescription As String, staffEmailsRaw As String())
        Using msg As New MailMessage()
            msg.From = New MailAddress("alerts@hotelcore.com", "HotelCore Alerts")
            msg.Subject = "HotelCore Staff Alert"
            msg.Body = eventDescription
            For Each rawAddress As String In staffEmailsRaw
                Try
                    Dim validAddress As New MailAddress(rawAddress)
                    ' SAFE_SINK: PLANTED-VB-HR-300-safe
                    msg.CC.Add(validAddress)
                Catch ex As FormatException
                    ' Skip a malformed/multi-address entry rather than failing the whole batch.
                End Try
            Next
            Using client As New SmtpClient("mail.hotelcore-internal.local")
                client.Send(msg)
            End Using
        End Using
    End Sub

End Module
