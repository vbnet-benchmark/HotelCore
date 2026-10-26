
-- SP: Payment - Get Receipt By ID
CREATE   PROCEDURE sp_Payment_GetReceiptById
    @PaymentID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
        p.PaymentID,
        p.BookingID,
        p.PaymentReference,
        p.PaymentMethod,
        p.Amount,
        p.Currency,
        p.Status,
        p.ProcessorTransactionID,
        p.PaymentDate,
        b.BookingReference,
        b.UserID AS OwnerUserID
    FROM Payments p
    INNER JOIN Bookings b ON p.BookingID = b.BookingID
    WHERE p.PaymentID = @PaymentID;
END
