Imports System.Data
Imports System.Data.SqlClient

Public Class UserRepository
    Implements IUserRepository

    Public Function CreateUser(email As String, passwordHash As String, passwordSalt As String,
                              firstName As String, lastName As String, phoneNumber As String,
                              roleID As Integer, currentUserID As Integer?,
                              ByRef errorCode As Integer, ByRef errorMessage As String) As Integer Implements IUserRepository.CreateUser

        Dim userID As Integer = 0

        Dim parameters As New List(Of SqlParameter) From {
            SqlHelper.CreateParameter("@Email", SqlDbType.NVarChar, email),
            SqlHelper.CreateParameter("@PasswordHash", SqlDbType.NVarChar, passwordHash),
            SqlHelper.CreateParameter("@PasswordSalt", SqlDbType.NVarChar, passwordSalt),
            SqlHelper.CreateParameter("@FirstName", SqlDbType.NVarChar, firstName),
            SqlHelper.CreateParameter("@LastName", SqlDbType.NVarChar, lastName),
            SqlHelper.CreateParameter("@PhoneNumber", SqlDbType.NVarChar, phoneNumber),
            SqlHelper.CreateParameter("@RoleID", SqlDbType.Int, roleID),
            SqlHelper.CreateParameter("@CurrentUserID", SqlDbType.Int, currentUserID),
            SqlHelper.CreateOutputParameter("@UserID", SqlDbType.Int),
            SqlHelper.CreateOutputParameter("@ErrorCode", SqlDbType.Int),
            SqlHelper.CreateOutputParameter("@ErrorMessage", SqlDbType.NVarChar, 500)
        }

        Dim outputParams As New Dictionary(Of String, Object) From {
            {"@UserID", Nothing},
            {"@ErrorCode", Nothing},
            {"@ErrorMessage", Nothing}
        }

        SqlHelper.ExecuteWithOutputs("sp_User_Create", outputParams, parameters.ToArray())

        userID = SqlHelper.SafeGetValue(outputParams("@UserID"), 0)
        errorCode = SqlHelper.SafeGetValue(outputParams("@ErrorCode"), -999)
        errorMessage = SqlHelper.SafeGetValue(outputParams("@ErrorMessage"), "Unknown error")

        Return userID
    End Function

    Public Function Authenticate(email As String, passwordHash As String, ipAddress As String,
                                userAgent As String, ByRef roleID As Integer, ByRef roleName As String,
                                ByRef fullName As String, ByRef errorCode As Integer,
                                ByRef errorMessage As String) As Integer Implements IUserRepository.Authenticate

        Dim userID As Integer = 0

        Dim parameters As New List(Of SqlParameter) From {
            SqlHelper.CreateParameter("@Email", SqlDbType.NVarChar, email),
            SqlHelper.CreateParameter("@PasswordHash", SqlDbType.NVarChar, passwordHash),
            SqlHelper.CreateParameter("@IPAddress", SqlDbType.NVarChar, ipAddress),
            SqlHelper.CreateParameter("@UserAgent", SqlDbType.NVarChar, userAgent),
            SqlHelper.CreateOutputParameter("@UserID", SqlDbType.Int),
            SqlHelper.CreateOutputParameter("@RoleID", SqlDbType.Int),
            SqlHelper.CreateOutputParameter("@RoleName", SqlDbType.NVarChar, 50),
            SqlHelper.CreateOutputParameter("@FullName", SqlDbType.NVarChar, 200),
            SqlHelper.CreateOutputParameter("@ErrorCode", SqlDbType.Int),
            SqlHelper.CreateOutputParameter("@ErrorMessage", SqlDbType.NVarChar, 500)
        }

        Dim outputParams As New Dictionary(Of String, Object) From {
            {"@UserID", Nothing},
            {"@RoleID", Nothing},
            {"@RoleName", Nothing},
            {"@FullName", Nothing},
            {"@ErrorCode", Nothing},
            {"@ErrorMessage", Nothing}
        }

        SqlHelper.ExecuteWithOutputs("sp_User_Authenticate", outputParams, parameters.ToArray())

        userID = SqlHelper.SafeGetValue(outputParams("@UserID"), 0)
        roleID = SqlHelper.SafeGetValue(outputParams("@RoleID"), 0)
        roleName = SqlHelper.SafeGetValue(outputParams("@RoleName"), "")
        fullName = SqlHelper.SafeGetValue(outputParams("@FullName"), "")
        errorCode = SqlHelper.SafeGetValue(outputParams("@ErrorCode"), -999)
        errorMessage = SqlHelper.SafeGetValue(outputParams("@ErrorMessage"), "Unknown error")

        Return userID
    End Function

    Public Function UpdateProfile(userID As Integer, firstName As String, lastName As String,
                                 phoneNumber As String, currentUserID As Integer,
                                 ByRef errorCode As Integer, ByRef errorMessage As String) As Boolean Implements IUserRepository.UpdateProfile

        Dim parameters As New List(Of SqlParameter) From {
            SqlHelper.CreateParameter("@UserID", SqlDbType.Int, userID),
            SqlHelper.CreateParameter("@FirstName", SqlDbType.NVarChar, firstName),
            SqlHelper.CreateParameter("@LastName", SqlDbType.NVarChar, lastName),
            SqlHelper.CreateParameter("@PhoneNumber", SqlDbType.NVarChar, phoneNumber),
            SqlHelper.CreateParameter("@CurrentUserID", SqlDbType.Int, currentUserID),
            SqlHelper.CreateOutputParameter("@ErrorCode", SqlDbType.Int),
            SqlHelper.CreateOutputParameter("@ErrorMessage", SqlDbType.NVarChar, 500)
        }

        Dim outputParams As New Dictionary(Of String, Object) From {
            {"@ErrorCode", Nothing},
            {"@ErrorMessage", Nothing}
        }

        SqlHelper.ExecuteWithOutputs("sp_User_UpdateProfile", outputParams, parameters.ToArray())

        errorCode = SqlHelper.SafeGetValue(outputParams("@ErrorCode"), -999)
        errorMessage = SqlHelper.SafeGetValue(outputParams("@ErrorMessage"), "Unknown error")

        Return errorCode = 0
    End Function

    ''' Looks up the guest's current loyalty tier by UserID. Returns "Standard" if the user
    ''' has no tier on record (new/never-tiered accounts), rather than Nothing, so every
    ''' caller can compare the result directly without a null check.
    Public Function GetLoyaltyTier(userID As Integer) As String Implements IUserRepository.GetLoyaltyTier
        Dim parameters As New List(Of SqlParameter) From {
            SqlHelper.CreateParameter("@UserID", SqlDbType.Int, userID)
        }

        Dim dt As DataTable = SqlHelper.ExecuteDataTable("sp_User_GetLoyaltyTier", parameters.ToArray())

        If dt.Rows.Count > 0 AndAlso Not IsDBNull(dt.Rows(0)("LoyaltyTier")) Then
            Return dt.Rows(0)("LoyaltyTier").ToString()
        End If

        Return "Standard"
    End Function

End Class
