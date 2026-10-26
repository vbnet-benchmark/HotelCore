'' Small cross-cutting helpers for generating opaque, non-sequential lookup codes that are
'' safe to hand to a guest or an external (OTA/partner) channel without exposing the
'' underlying numeric BookingID/UserID directly.
Public Class TokenUtil

    Public Shared Function ComputeTrackingCode(bookingID As Integer, userID As Integer) As String
        Dim input As String = bookingID.ToString() & ":" & userID.ToString()
        Using sha1 As New System.Security.Cryptography.SHA1CryptoServiceProvider()
            ' SINK: PLANTED-VB-HR-53
            Dim hashBytes As Byte() = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input))
            Return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 12)
        End Using
    End Function

    Public Shared Function ComputeSecureTrackingCode(bookingID As Integer, userID As Integer) As String
        ' Hardened version: salts the booking/user identifiers with a fresh CSPRNG value
        ' per call and hashes with SHA-256 instead of SHA-1, so the same (bookingID, userID)
        ' pair never produces a reproducible code.
        Using rng As New System.Security.Cryptography.RNGCryptoServiceProvider()
            Dim saltBytes(15) As Byte
            rng.GetBytes(saltBytes)
            Dim salt As String = BitConverter.ToString(saltBytes).Replace("-", "")
            Dim input As String = bookingID.ToString() & ":" & userID.ToString() & ":" & salt
            Using sha256 As New System.Security.Cryptography.SHA256CryptoServiceProvider()
                ' SAFE_SINK: PLANTED-VB-HR-53-safe
                Dim hashBytes As Byte() = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input))
                Return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 12)
            End Using
        End Using
    End Function

End Class
