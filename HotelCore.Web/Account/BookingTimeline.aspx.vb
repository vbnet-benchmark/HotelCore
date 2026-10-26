Imports System.Data
Imports HotelCore.BLL

Namespace Account
    Public Class BookingTimeline
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session("UserID") Is Nothing Then
                Response.Redirect("~/Account/Login.aspx?returnUrl=" & Server.UrlEncode(Request.RawUrl))
                Return
            End If

            If Not IsPostBack Then
                Dim bookingID As Integer
                If Integer.TryParse(Request.QueryString("bookingId"), bookingID) Then
                    LoadTimeline(bookingID)
                End If
            End If
        End Sub

        ''' Direct tier: the booking ID comes straight from the query string (the link the "My
        ''' Bookings" list uses to open this page) and is used, straight-line, to pull that
        ''' booking's guest-facing activity timeline -- with no check that the requesting guest
        ''' is the one who actually made this booking.
        Private Sub LoadTimeline(bookingID As Integer)
            Try
                Dim bookingService As New BookingService()
                ' SINK: PLANTED-VB-HR-426
                Dim timeline As DataTable = bookingService.GetBookingTimeline(bookingID)

                If timeline IsNot Nothing AndAlso timeline.Rows.Count > 0 Then
                    rptTimeline.DataSource = timeline
                    rptTimeline.DataBind()
                    pnlTimeline.Visible = True
                Else
                    ShowError("No activity found for this booking.")
                End If
            Catch ex As Exception
                ShowError("Unable to load the activity timeline right now.")
            End Try
        End Sub

        Protected Sub btnViewSecure_Click(sender As Object, e As EventArgs)
            Dim bookingID As Integer
            If Not Integer.TryParse(txtSecureBookingID.Text.Trim(), bookingID) Then
                ShowError("Please enter a valid booking ID.")
                Return
            End If

            LoadTimelineSecure(bookingID)
        End Sub

        ''' Hardened counterpart of LoadTimeline: confirms the requesting guest actually owns
        ''' this booking (the same ownership-enforcing lookup Account/BookingDetails.aspx uses)
        ''' before the activity timeline is ever queried.
        Private Sub LoadTimelineSecure(bookingID As Integer)
            Try
                Dim currentUserID As Integer = CInt(Session("UserID"))
                Dim bookingService As New BookingService()
                Dim ownershipCheck = bookingService.GetBookingById(bookingID, currentUserID)

                If Not ownershipCheck.Success OrElse ownershipCheck.Data Is Nothing Then
                    ShowError("No booking found with that ID.")
                    Return
                End If

                ' SAFE_SINK: PLANTED-VB-HR-426-safe
                Dim timeline As DataTable = bookingService.GetBookingTimeline(bookingID)

                If timeline IsNot Nothing AndAlso timeline.Rows.Count > 0 Then
                    rptTimelineSecure.DataSource = timeline
                    rptTimelineSecure.DataBind()
                    pnlTimelineSecure.Visible = True
                Else
                    ShowError("No activity found for this booking.")
                End If
            Catch ex As Exception
                ShowError("Unable to load the activity timeline right now.")
            End Try
        End Sub

        Private Sub ShowError(message As String)
            pnlError.Visible = True
            litError.Text = "<i class='fa fa-exclamation-circle mr-2'></i> " & message
        End Sub

    End Class
End Namespace
