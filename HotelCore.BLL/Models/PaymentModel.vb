'' DTO for Payment data
Public Class PaymentModel
    Public Property PaymentID As Integer
    Public Property BookingID As Integer
    Public Property PaymentReference As String
    Public Property PaymentMethod As String
    Public Property Amount As Decimal
    Public Property Currency As String
    Public Property Status As String
    Public Property ProcessorTransactionID As String
    Public Property PaymentDate As DateTime
    Public Property RefundedAmount As Decimal
    Public Property RefundedDate As DateTime?
    '' Tamper-evidence signature covering the payment's own key fields, surfaced on the
    '' receipt so a guest/finance reviewer can spot an altered amount or method. Transient --
    '' not persisted here.
    Public Property ReceiptSignature As String
End Class
