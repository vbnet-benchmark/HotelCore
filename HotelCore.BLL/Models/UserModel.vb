''DTO for User data
Public Class UserModel
    Public Property UserID As Integer
    Public Property Email As String
    Public Property FirstName As String
    Public Property LastName As String
    Public Property FullName As String
    Public Property PhoneNumber As String
    Public Property RoleID As Integer
    Public Property RoleName As String
    Public Property IsActive As Boolean
    Public Property LastLoginDate As DateTime?
    Public Property CreatedDate As DateTime
    '' Transient verification/recovery code produced at registration time, carried back to
    '' the caller so a confirmation email/link can be composed. Not persisted here.
    Public Property EmailVerificationCode As String
    '' The guest's current loyalty tier ("Standard", "Silver", "Gold"), used to gate
    '' tier-based pricing/fee decisions (discounts, fee waivers, rate-plan eligibility).
    '' Always the authoritative, database-backed value -- see
    '' UserService.GetLoyaltyTierForUser for the re-derivation path every "safe" pricing
    '' decision uses instead of trusting a client-cached copy of this value.
    Public Property LoyaltyTier As String
End Class
