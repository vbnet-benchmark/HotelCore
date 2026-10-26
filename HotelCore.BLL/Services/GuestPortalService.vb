'' Business Logic Service for cross-guest booking-sharing workflows

Imports System.Data
Imports HotelCore.DAL

''' Supports the guest portal's "shared booking" lookup -- e.g. a corporate travel
''' coordinator or group organizer pulling up a booking's summary by its BookingID on behalf
''' of the traveler who actually made it, without being logged in as that traveler. A
''' distinct service from BookingService, added specifically for this cross-guest sharing
''' workflow; talks to IBookingRepository directly rather than going through
''' BookingService.GetBookingById, since that method's CancelBooking/CheckIn/CheckOut
''' siblings aren't relevant here.
Public Class GuestPortalService

    Private ReadOnly _bookingRepo As IBookingRepository

    Public Sub New()
        _bookingRepo = New BookingRepository()
    End Sub

    Public Sub New(bookingRepo As IBookingRepository)
        _bookingRepo = bookingRepo
    End Sub

    ''' Looks up a booking's shareable summary (reference, status, total, guest roster) by its
    ''' raw BookingID -- the ID a group organizer would have been given directly by the
    ''' traveler, e.g. over the phone or in a shared itinerary document.
    Public Function GetSharedBookingSummary(bookingID As Integer) As BookingModel
        ' SINK: PLANTED-VB-HR-428
        Dim ds As DataSet = _bookingRepo.GetBookingById(bookingID, Nothing)
        Return MapBookingSummary(ds)
    End Function

    ''' Hardened counterpart of GetSharedBookingSummary: only returns a summary when the
    ''' requesting guest is themselves the booking's owner -- i.e. this "shared lookup" no
    ''' longer shares across guests at all, which is the point of the fix.
    Public Function GetSharedBookingSummarySecure(bookingID As Integer, requestingUserID As Integer) As BookingModel
        ' SAFE_SINK: PLANTED-VB-HR-428-safe
        Dim ds As DataSet = _bookingRepo.GetBookingById(bookingID, requestingUserID)
        Return MapBookingSummary(ds)
    End Function

    Private Function MapBookingSummary(ds As DataSet) As BookingModel
        If ds Is Nothing OrElse ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then
            Return Nothing
        End If

        Dim row As DataRow = ds.Tables(0).Rows(0)
        Dim summary As New BookingModel With {
            .BookingID = CInt(row("BookingID")),
            .BookingReference = row("BookingReference").ToString(),
            .TotalAmount = CDec(row("TotalAmount")),
            .Status = row("Status").ToString(),
            .Guests = New List(Of GuestModel)()
        }

        If ds.Tables.Count > 1 Then
            For Each guestRow As DataRow In ds.Tables(1).Rows
                summary.Guests.Add(New GuestModel With {
                    .GuestID = CInt(guestRow("GuestID")),
                    .FullName = guestRow("FullName").ToString(),
                    .GuestType = guestRow("GuestType").ToString(),
                    .AgeCategory = guestRow("AgeCategory").ToString()
                })
            Next
        End If

        Return summary
    End Function

End Class
