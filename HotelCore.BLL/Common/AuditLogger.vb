'' Shared audit-trail helper for business-significant booking events (new bookings, guest
'' roster changes). This class library has no HttpContext/Server.MapPath of its own -- the log
'' directory is resolved relative to the hosting application's base directory instead, which
'' (when this assembly is loaded by HotelCore.Web, as it always is in production) resolves to
'' the Web project's own App_Data folder.
Public Class AuditLogger

    Private Shared ReadOnly _logDir As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data")

    ''' Records a single booking-creation audit event. `guestNote` is caller-supplied, free-text
    ''' context (the guest's own special-requests text) and is written exactly as given.
    Public Shared Sub LogBookingEvent(bookingID As Integer, guestNote As String)
        EnsureLogDir()
        Dim line As String = String.Format("[{0:O}] BOOKING_CREATED - BookingID: {1} - Notes: {2}", DateTime.UtcNow, bookingID, guestNote)
        ' SINK: PLANTED-VB-HR-93
        System.IO.File.AppendAllText(System.IO.Path.Combine(_logDir, "audit-trail.log"), line & Environment.NewLine)
    End Sub

    ''' Hardened counterpart of LogBookingEvent: strips embedded CR/LF from the free-text note
    ''' before it is persisted, so it can't forge an additional audit-trail line.
    Public Shared Sub LogBookingEventSecure(bookingID As Integer, guestNote As String)
        EnsureLogDir()
        Dim safeNote As String = If(guestNote, "").Replace(vbCr, " ").Replace(vbLf, " ")
        Dim line As String = String.Format("[{0:O}] BOOKING_CREATED - BookingID: {1} - Notes: {2}", DateTime.UtcNow, bookingID, safeNote)
        ' SAFE_SINK: PLANTED-VB-HR-93-safe
        System.IO.File.AppendAllText(System.IO.Path.Combine(_logDir, "audit-trail.log"), line & Environment.NewLine)
    End Sub

    ''' Records one line per additional guest added to a booking's roster, so front-desk can
    ''' reconcile the roster against ID checks at check-in. Called once per guest, in a loop, by
    ''' BookingService.CreateBooking.
    Public Shared Sub LogGuestRosterEntry(bookingID As Integer, guestFullName As String)
        EnsureLogDir()
        Dim line As String = String.Format("[{0:O}] GUEST_ADDED - BookingID: {1} - Name: {2}", DateTime.UtcNow, bookingID, guestFullName)
        ' SINK: PLANTED-VB-HR-95
        System.IO.File.AppendAllText(System.IO.Path.Combine(_logDir, "audit-trail.log"), line & Environment.NewLine)
    End Sub

    ''' Hardened counterpart of LogGuestRosterEntry: strips embedded CR/LF from the guest name
    ''' before it is persisted.
    Public Shared Sub LogGuestRosterEntrySecure(bookingID As Integer, guestFullName As String)
        EnsureLogDir()
        Dim safeName As String = If(guestFullName, "").Replace(vbCr, " ").Replace(vbLf, " ")
        Dim line As String = String.Format("[{0:O}] GUEST_ADDED - BookingID: {1} - Name: {2}", DateTime.UtcNow, bookingID, safeName)
        ' SAFE_SINK: PLANTED-VB-HR-95-safe
        System.IO.File.AppendAllText(System.IO.Path.Combine(_logDir, "audit-trail.log"), line & Environment.NewLine)
    End Sub

    Private Shared Sub EnsureLogDir()
        If Not System.IO.Directory.Exists(_logDir) Then System.IO.Directory.CreateDirectory(_logDir)
    End Sub

End Class
