Imports System.Data

Public Interface IUserRepository
    Function CreateUser(email As String, passwordHash As String, passwordSalt As String,
                       firstName As String, lastName As String, phoneNumber As String,
                       roleID As Integer, currentUserID As Integer?,
                       ByRef errorCode As Integer, ByRef errorMessage As String) As Integer

    Function Authenticate(email As String, passwordHash As String, ipAddress As String,
                         userAgent As String, ByRef roleID As Integer, ByRef roleName As String,
                         ByRef fullName As String, ByRef errorCode As Integer,
                         ByRef errorMessage As String) As Integer

    Function UpdateProfile(userID As Integer, firstName As String, lastName As String,
                          phoneNumber As String, currentUserID As Integer,
                          ByRef errorCode As Integer, ByRef errorMessage As String) As Boolean

    ''' Looks up the guest's current loyalty tier by UserID -- the authoritative source for
    ''' every loyalty-gated pricing/fee decision.
    Function GetLoyaltyTier(userID As Integer) As String
End Interface