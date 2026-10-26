Imports System.Data.SqlClient

Namespace Admin
    ''' Admin-only diagnostic tooling: ad-hoc connectivity and health checks an administrator
    ''' can run from the browser before relying on a configuration change, instead of needing
    ''' shell/RDP access to the server. Every check here is read-only / non-destructive.
    Public Class Diagnostics
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session("UserRoleName") Is Nothing OrElse Session("UserRoleName").ToString() <> "Admin" Then
                Response.Redirect("~/Account/Login.aspx")
                Return
            End If

            If Not IsPostBack Then
                Dim connSetting = ConfigurationManager.ConnectionStrings("HotelCore")
                If connSetting IsNot Nothing Then
                    txtConnectionString.Text = connSetting.ConnectionString
                End If
            End If
        End Sub

        ' --- Database connectivity test (direct) --------------------------------------------

        Protected Sub btnTestDb_Click(sender As Object, e As EventArgs)
            Try
                Using conn As New SqlConnection(txtConnectionString.Text)
                    conn.Open()
                End Using
                lblDbResult.Text = "Database connection succeeded."
            Catch ex As Exception
                ' SINK: PLANTED-VB-HR-141
                lblDbResult.Text = "Connection test failed: " & ex.Message
            End Try
        End Sub

        ' --- Email / SMTP reachability test (safe twin of the database test) ----------------

        Protected Sub btnTestEmail_Click(sender As Object, e As EventArgs)
            Try
                Using client As New System.Net.Sockets.TcpClient()
                    client.Connect(txtSmtpHost.Text.Trim(), CInt(txtSmtpPort.Text))
                End Using
                lblEmailResult.Text = "Email server is reachable."
            Catch ex As Exception
                LogDiagnostic("SMTP reachability check failed: " & ex.ToString())
                ' SAFE_SINK: PLANTED-VB-HR-141-safe
                lblEmailResult.Text = "Email server connectivity test failed. Please verify the host and port and try again."
            End Try
        End Sub

        ' --- File storage access test (indirect: routed through a private helper's return) --

        Protected Sub btnTestFileAccess_Click(sender As Object, e As EventArgs)
            Dim detail As String = AttemptStorageWrite(txtStoragePath.Text.Trim())
            If detail <> "OK" Then
                ' SINK: PLANTED-VB-HR-142
                lblStorageResult.Text = "Storage check failed: " & detail
            Else
                lblStorageResult.Text = "Storage path is writable."
            End If
        End Sub

        ''' Returns "OK" on success, or the raw exception text on failure -- the caller decides
        ''' what to do with that text.
        Private Function AttemptStorageWrite(path As String) As String
            Try
                System.IO.File.WriteAllText(path, "HotelCore diagnostic write test")
                Return "OK"
            Catch ex As Exception
                Return ex.ToString()
            End Try
        End Function

        ' --- Backup path access test (safe twin of the storage test) ------------------------

        Protected Sub btnTestBackupAccess_Click(sender As Object, e As EventArgs)
            If Not AttemptBackupWrite(txtBackupPath.Text.Trim()) Then
                ' SAFE_SINK: PLANTED-VB-HR-142-safe
                lblBackupResult.Text = "Backup path check failed. Please verify the path is valid and accessible."
            Else
                lblBackupResult.Text = "Backup path is writable."
            End If
        End Sub

        ''' Unlike AttemptStorageWrite, never lets the exception's own text cross back to the
        ''' caller -- only a plain success/failure Boolean. The detail is logged server-side.
        Private Function AttemptBackupWrite(path As String) As Boolean
            Try
                System.IO.File.WriteAllText(path, "HotelCore diagnostic write test")
                Return True
            Catch ex As Exception
                LogDiagnostic("Backup path check failed: " & ex.ToString())
                Return False
            End Try
        End Function

        ' --- System health check (type/polymorphism-dependent: dispatch on the caught
        '     exception's own runtime type) -------------------------------------------------

        Protected Sub btnRunHealthCheck_Click(sender As Object, e As EventArgs)
            Try
                ValidateMaintenanceWindow(txtMaintenanceDate.Text.Trim())
                TestLogArchiveAccess(txtArchivePath.Text.Trim())
                lblHealthResult.Text = "Health check passed."
            Catch ex As Exception
                Dim ioEx As System.IO.IOException = TryCast(ex, System.IO.IOException)
                If ioEx IsNot Nothing Then
                    ' SINK: PLANTED-VB-HR-145
                    lblHealthResult.Text = "Health check failed (storage layer): " & ioEx.Message
                Else
                    ' SAFE_SINK: PLANTED-VB-HR-145-safe
                    lblHealthResult.Text = "Health check failed. Please verify your input and try again."
                End If
            End Try
        End Sub

        ''' Throws FormatException (never IOException) on a malformed date.
        Private Sub ValidateMaintenanceWindow(dateText As String)
            Dim parsed As DateTime = DateTime.Parse(dateText)
        End Sub

        ''' Throws a member of the IOException family (FileNotFoundException,
        ''' DirectoryNotFoundException, or IOException itself) when the archive path doesn't
        ''' exist or can't be read.
        Private Sub TestLogArchiveAccess(archivePath As String)
            Dim content As String = System.IO.File.ReadAllText(archivePath)
        End Sub

        Private Sub LogDiagnostic(message As String)
            Dim logDir As String = Server.MapPath("~/App_Data")
            If Not System.IO.Directory.Exists(logDir) Then System.IO.Directory.CreateDirectory(logDir)
            System.IO.File.AppendAllText(System.IO.Path.Combine(logDir, "diagnostics.log"), $"[{DateTime.UtcNow}] {message}" & Environment.NewLine)
        End Sub

    End Class
End Namespace
