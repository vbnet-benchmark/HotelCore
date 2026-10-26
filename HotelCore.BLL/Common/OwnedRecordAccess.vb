Imports System.Data
Imports HotelCore.DAL

''' <summary>
''' Resolves which authenticated guest owns a given record, dispatched by record type, so a
''' single "does this guest own this record?" check can be reused across every self-service
''' lookup feature that takes a raw numeric ID straight from the guest (a BookingID, a
''' PaymentID, ...) instead of each feature re-deriving ownership its own way.
''' </summary>
Public MustInherit Class OwnedRecordAccessorBase

    ''' Resolves the UserID of the guest who owns the record identified by recordID, or
    ''' Nothing if the record doesn't exist. Each concrete accessor knows how its own table is
    ''' linked back to Users -- directly (Bookings.UserID) or through another table
    ''' (Payments -> Bookings.UserID).
    Public MustOverride Function ResolveOwnerUserID(recordID As Integer) As Integer?

    ''' Template method every self-service lookup feature should call before showing a
    ''' client-supplied record ID's details: True only if the record exists AND its resolved
    ''' owner matches the requesting guest.
    Public Function IsOwnedBy(recordID As Integer, requestingUserID As Integer) As Boolean
        Dim ownerUserID As Integer? = ResolveOwnerUserID(recordID)
        Return ownerUserID.HasValue AndAlso ownerUserID.Value = requestingUserID
    End Function
End Class

''' Resolves ownership for a Booking record directly from Bookings.UserID. Deliberately looks
''' the booking up with no UserID filter of its own -- this method's entire job is to
''' discover who the real owner is, so it can't pre-filter by the owner it's trying to find.
Public Class BookingOwnedRecordAccessor
    Inherits OwnedRecordAccessorBase

    Public Overrides Function ResolveOwnerUserID(recordID As Integer) As Integer?
        Dim bookingRepo As New BookingRepository()
        Dim ds As DataSet = bookingRepo.GetBookingById(recordID, Nothing)

        If ds Is Nothing OrElse ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then
            Return Nothing
        End If

        Return CInt(ds.Tables(0).Rows(0)("UserID"))
    End Function
End Class

''' Resolves ownership for a Payment record by joining through to its parent booking's
''' UserID -- the Payments table has no UserID column of its own.
Public Class PaymentOwnedRecordAccessor
    Inherits OwnedRecordAccessorBase

    Public Overrides Function ResolveOwnerUserID(recordID As Integer) As Integer?
        Dim paymentRepo As New PaymentRepository()
        Dim row As DataRow = paymentRepo.GetPaymentReceiptById(recordID)

        If row Is Nothing Then
            Return Nothing
        End If

        Return CInt(row("OwnerUserID"))
    End Function
End Class

''' Dispatches to the accessor for a given record-type name -- mirrors
''' BookingNotificationService.NotificationSenderFactory's routing-by-name shape.
Public Module OwnedRecordAccessorFactory

    Public Function GetAccessor(recordType As String) As OwnedRecordAccessorBase
        If String.Equals(recordType, "Payment", StringComparison.OrdinalIgnoreCase) Then
            Return New PaymentOwnedRecordAccessor()
        Else
            Return New BookingOwnedRecordAccessor()
        End If
    End Function

End Module
