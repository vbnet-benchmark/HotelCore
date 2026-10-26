'' Business Logic Service for User Operations

Imports HotelCore.DAL

Public Class UserService

    Private ReadOnly _userRepo As IUserRepository

    Public Sub New()
        _userRepo = New UserRepository()
    End Sub

    Public Sub New(userRepo As IUserRepository)
        _userRepo = userRepo
    End Sub

    '' Authenticate user login
    Public Function AuthenticateUser(email As String, password As String, ipAddress As String, userAgent As String) As OperationResult(Of UserModel)
        Dim result As New OperationResult(Of UserModel)

        Try
            ' BUSINESS RULE VALIDATION
            If String.IsNullOrWhiteSpace(email) Then
                result.Success = False
                result.ErrorMessage = "Email is required"
                Return result
            End If

            If String.IsNullOrWhiteSpace(password) Then
                result.Success = False
                result.ErrorMessage = "Password is required"
                Return result
            End If

            ' Hash password
            Dim passwordHash As String = HashPassword(password)

            ' Call DAL
            Dim userID As Integer = 0
            Dim roleID As Integer = 0
            Dim roleName As String = ""
            Dim fullName As String = ""
            Dim errorCode As Integer = 0
            Dim errorMessage As String = ""

            userID = _userRepo.Authenticate(
                email,
                passwordHash,
                ipAddress,
                userAgent,
                roleID,
                roleName,
                fullName,
                errorCode,
                errorMessage
            )

            If userID > 0 AndAlso errorCode = 0 Then
                result.Success = True
                result.Data = New UserModel With {
                    .UserID = userID,
                    .Email = email,
                    .FullName = fullName,
                    .RoleID = roleID,
                    .RoleName = roleName
                }
                result.Message = "Login successful"
            Else
                result.Success = False
                result.ErrorCode = errorCode
                result.ErrorMessage = errorMessage
            End If

        Catch ex As Exception
            result.Success = False
            result.ErrorMessage = $"Error during authentication: {ex.Message}"
        End Try

        Return result
    End Function

    ''Register new user
    Public Function RegisterUser(model As UserModel, password As String, currentUserID As Integer?) As OperationResult(Of UserModel)
        Dim result As New OperationResult(Of UserModel)

        Try
            ' BUSINESS RULE VALIDATION
            Dim validationErrors As List(Of String) = ValidateUserRegistration(model, password)
            If validationErrors.Count > 0 Then
                result.Success = False
                result.ErrorMessage = String.Join("; ", validationErrors)
                Return result
            End If

            ' Hash password
            Dim passwordHash As String = HashPassword(password)
            Dim passwordSalt As String = GenerateSalt()

            ' Call DAL
            Dim errorCode As Integer = 0
            Dim errorMessage As String = ""

            Dim userID As Integer = _userRepo.CreateUser(
                model.Email,
                passwordHash,
                passwordSalt,
                model.FirstName,
                model.LastName,
                model.PhoneNumber,
                model.RoleID,
                currentUserID,
                errorCode,
                errorMessage
            )

            If userID > 0 AndAlso errorCode = 0 Then
                model.UserID = userID

                If currentUserID.HasValue Then
                    ' Front-desk/admin-created account: the verification code is a SHA-256
                    ' digest of the email salted with a fresh CSPRNG value per registration
                    ' (the same RNGCryptoServiceProvider already used above for password
                    ' salts), so the result can't be reproduced from the email alone.
                    Using rng As New System.Security.Cryptography.RNGCryptoServiceProvider()
                        Dim saltBytes(15) As Byte
                        rng.GetBytes(saltBytes)
                        Dim salt As String = BitConverter.ToString(saltBytes).Replace("-", "")
                        Using sha256 As New System.Security.Cryptography.SHA256CryptoServiceProvider()
                            Dim codeBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(model.Email & "|" & salt)
                            ' SAFE_SINK: PLANTED-VB-HR-51-safe
                            Dim codeHash As Byte() = sha256.ComputeHash(codeBytes)
                            model.EmailVerificationCode = salt & BitConverter.ToString(codeHash).Replace("-", "")
                        End Using
                    End Using
                Else
                    ' Self-service web registration: generate an email-verification code so a
                    ' "confirm your email" link can be sent before full account privileges are
                    ' enabled.
                    Using md5 As New System.Security.Cryptography.MD5CryptoServiceProvider()
                        Dim codeBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(model.Email)
                        ' SINK: PLANTED-VB-HR-51
                        Dim codeHash As Byte() = md5.ComputeHash(codeBytes)
                        model.EmailVerificationCode = BitConverter.ToString(codeHash).Replace("-", "")
                    End Using
                End If

                result.Success = True
                result.Data = model
                result.Message = "User registered successfully"
            Else
                result.Success = False
                result.ErrorCode = errorCode
                result.ErrorMessage = errorMessage
            End If

        Catch ex As Exception
            result.Success = False
            result.ErrorMessage = $"Error during registration: {ex.Message}"
        End Try

        Return result
    End Function

    ''' Generate a password-reset token for the given email so a "reset your password" link
    ''' can be emailed. Valid only within the current UTC hour window so a captured/leaked
    ''' token can't be replayed indefinitely -- see ComputeResetTokenHash for how it's built.
    Public Function GeneratePasswordResetToken(email As String, Optional useSecureToken As Boolean = False) As String
        If useSecureToken Then
            Return ComputeResetTokenSecure(email)
        Else
            Return ComputeResetTokenHash(email)
        End If
    End Function

    Private Function ComputeResetTokenHash(email As String) As String
        Dim tokenInput As String = email.ToLowerInvariant() & "|" & DateTime.UtcNow.ToString("yyyyMMddHH")
        Using sha1 As New System.Security.Cryptography.SHA1CryptoServiceProvider()
            ' SINK: PLANTED-VB-HR-52
            Dim tokenBytes As Byte() = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(tokenInput))
            Return BitConverter.ToString(tokenBytes).Replace("-", "")
        End Using
    End Function

    Private Function ComputeResetTokenSecure(email As String) As String
        ' Hardened version: salts the email with a fresh CSPRNG value per request (rather
        ' than an hour-granularity timestamp an attacker could also compute), then hashes
        ' with SHA-256 instead of SHA-1.
        Using rng As New System.Security.Cryptography.RNGCryptoServiceProvider()
            Dim saltBytes(31) As Byte
            rng.GetBytes(saltBytes)
            Dim salt As String = BitConverter.ToString(saltBytes).Replace("-", "")
            Dim tokenInput As String = email.ToLowerInvariant() & "|" & salt
            Using sha256 As New System.Security.Cryptography.SHA256CryptoServiceProvider()
                ' SAFE_SINK: PLANTED-VB-HR-52-safe
                Dim tokenBytes As Byte() = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(tokenInput))
                Return salt & BitConverter.ToString(tokenBytes).Replace("-", "")
            End Using
        End Using
    End Function

    '' Update user profile
    Public Function UpdateUserProfile(model As UserModel, currentUserID As Integer) As OperationResult
        Dim result As New OperationResult
        ' Captured up front so the ops/security log entries further below reflect exactly what
        ' was submitted, even though it isn't read again until after the DAL call several
        ' statements down.
        Dim submittedPhone As String = model.PhoneNumber

        Try
            ' BUSINESS RULE VALIDATION
            If String.IsNullOrWhiteSpace(model.FirstName) Then
                result.Success = False
                result.ErrorMessage = "First name is required"
                Return result
            End If

            If String.IsNullOrWhiteSpace(model.LastName) Then
                result.Success = False
                result.ErrorMessage = "Last name is required"
                Return result
            End If

            ' Call DAL
            Dim errorCode As Integer = 0
            Dim errorMessage As String = ""

            Dim success As Boolean = _userRepo.UpdateProfile(
                model.UserID,
                model.FirstName,
                model.LastName,
                model.PhoneNumber,
                currentUserID,
                errorCode,
                errorMessage
            )

            If success AndAlso errorCode = 0 Then
                Dim profileLogDir As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data")
                If Not System.IO.Directory.Exists(profileLogDir) Then System.IO.Directory.CreateDirectory(profileLogDir)

                ' Ops log: records the submitted phone number exactly as entered -- support
                ' staff cross-reference this file against the CRM export verbatim.
                ' SINK: PLANTED-VB-HR-94
                System.IO.File.AppendAllText(System.IO.Path.Combine(profileLogDir, "profile-activity.log"), $"[{DateTime.UtcNow}] PROFILE_UPDATED - UserID: {model.UserID} - Phone: {submittedPhone}" & Environment.NewLine)

                ' Security/SIEM feed: same event, but the phone number is sanitized first,
                ' since the external log-aggregation pipeline that ingests this file treats an
                ' embedded newline as a record separator.
                Dim safePhone As String = If(submittedPhone, "").Replace(vbCr, " ").Replace(vbLf, " ")
                ' SAFE_SINK: PLANTED-VB-HR-94-safe
                System.IO.File.AppendAllText(System.IO.Path.Combine(profileLogDir, "profile-security.log"), $"[{DateTime.UtcNow}] PROFILE_UPDATED - UserID: {model.UserID} - Phone: {safePhone}" & Environment.NewLine)

                result.Success = True
                result.Message = "Profile updated successfully"
            Else
                result.Success = False
                result.ErrorCode = errorCode
                result.ErrorMessage = errorMessage
            End If

        Catch ex As Exception
            result.Success = False
            result.ErrorMessage = $"Error updating profile: {ex.Message}"
        End Try

        Return result
    End Function

    ' Private validation methods
    Private Function ValidateUserRegistration(model As UserModel, password As String) As List(Of String)
        Dim errors As New List(Of String)

        ' Email validation
        If String.IsNullOrWhiteSpace(model.Email) Then
            errors.Add("Email is required")
        ElseIf Not IsValidEmail(model.Email) Then
            errors.Add("Invalid email format")
        End If

        ' Password validation
        If String.IsNullOrWhiteSpace(password) Then
            errors.Add("Password is required")
        ElseIf password.Length < 8 Then
            errors.Add("Password must be at least 8 characters")
        End If

        ' Name validation
        If String.IsNullOrWhiteSpace(model.FirstName) Then
            errors.Add("First name is required")
        End If

        If String.IsNullOrWhiteSpace(model.LastName) Then
            errors.Add("Last name is required")
        End If

        Return errors
    End Function

    Private Function IsValidEmail(email As String) As Boolean
        Try
            Dim addr = New System.Net.Mail.MailAddress(email)
            Return addr.Address = email
        Catch
            Return False
        End Try
    End Function

    Private Function HashPassword(password As String) As String

        Using sha256 As New System.Security.Cryptography.SHA256Managed()
            Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(password)
            Dim hash As Byte() = sha256.ComputeHash(bytes)
            Return Convert.ToBase64String(hash)
        End Using
    End Function

    Private Function GenerateSalt() As String
        Using rng As New System.Security.Cryptography.RNGCryptoServiceProvider()
            Dim saltBytes(31) As Byte
            rng.GetBytes(saltBytes)
            Return Convert.ToBase64String(saltBytes)
        End Using
    End Function

    ''' Looks up the authenticated guest's current loyalty tier by their UserID -- the
    ''' authoritative source re-derived from the database, never from anything client-supplied.
    ''' Used by the "safe" twin of every Checkout/RoomDetails loyalty-gated pricing decision to
    ''' confirm the tier at the moment it actually matters, instead of trusting a value cached
    ''' from an earlier page load. Defaults to "Standard" (no discount/waiver eligibility) if
    ''' the lookup fails for any reason, so a lookup error can never widen eligibility.
    Public Function GetLoyaltyTierForUser(userID As Integer) As String
        Try
            Return _userRepo.GetLoyaltyTier(userID)
        Catch ex As Exception
            Return "Standard"
        End Try
    End Function

End Class