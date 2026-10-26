Imports HotelCore.BLL

Namespace Booking
    Public Class RoomDetails
        Inherits System.Web.UI.Page

        Private _roomID As Integer = 0

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Not Integer.TryParse(Request.QueryString("id"), _roomID) Then
                ShowError("Invalid room ID.")
                Return
            End If

            If Not IsPostBack Then
                LoadRoomDetails()
            End If
        End Sub

        Private Sub LoadRoomDetails()
            Try
                Dim roomService As New RoomService()
                Dim result = roomService.GetRoomById(_roomID)

                If result.Success AndAlso result.Data IsNot Nothing Then
                    Dim room = result.Data

                    litRoomType.Text = room.TypeName
                    Page.Title = room.TypeName & " - HotelCore"

                    litRoomTypeName.Text = room.TypeName
                    litRoomNumber.Text = room.RoomNumber
                    litFloor.Text = room.FloorNumber.ToString()
                    litSize.Text = room.SquareFeet.ToString()
                    litCapacity.Text = room.MaxOccupancy.ToString()
                    litBedConfig.Text = If(String.IsNullOrEmpty(room.BedConfiguration), "Standard", room.BedConfiguration)
                    litDailyRate.Text = room.BaseDailyRate.ToString("N2")
                    litEstDailyRate.Text = room.BaseDailyRate.ToString("N2")
                    litStatus.Text = room.Status
                    litDescription.Text = If(String.IsNullOrEmpty(room.Description), "Experience comfort and luxury in our well-appointed room.", room.Description)

                    ViewState("RoomID") = room.RoomID
                    ViewState("DailyRate") = room.BaseDailyRate
                    ViewState("MaxOccupancy") = room.MaxOccupancy

                    ' Determine, once, which negotiated rate plan this guest is eligible for
                    ' (loyalty-tier based) and cache it for the price calculator below -- avoids
                    ' re-querying the guest's loyalty tier on every "Calculate Price" postback.
                    ' Re-derived fresh by the "Verified Rate" calculator instead of trusting this
                    ' cached code -- see CalculatePriceVerified.
                    If Session("UserID") IsNot Nothing Then
                        Dim userService As New UserService()
                        Dim loyaltyTier As String = userService.GetLoyaltyTierForUser(CInt(Session("UserID")))
                        hfRatePlanCode.Value = GetEligibleRatePlanCode(loyaltyTier)
                    Else
                        hfRatePlanCode.Value = "STANDARD"
                    End If

                    For Each item As ListItem In ddlGuests.Items
                        Dim guestCount As Integer = CInt(item.Value)
                        item.Enabled = guestCount <= room.MaxOccupancy
                    Next
                Else
                    ShowError(If(String.IsNullOrEmpty(result.ErrorMessage), "Room not found.", result.ErrorMessage))
                End If

            Catch ex As Exception
                ShowError("An error occurred while loading room details.")
            End Try
        End Sub

        ''' Maps a loyalty tier to the negotiated rate plan it unlocks. There is no genuine
        ''' polymorphic rate-plan hierarchy in this codebase (unlike, say, BookingNotificationService's
        ''' INotificationSender dispatch, which is a plain routing choice, not a pricing decision) --
        ''' this Select Case substitutes for one.
        Private Function GetEligibleRatePlanCode(loyaltyTier As String) As String
            Select Case loyaltyTier
                Case "Gold"
                    Return "CORPORATE"
                Case "Silver"
                    Return "MEMBER"
                Case Else
                    Return "STANDARD"
            End Select
        End Function

        Protected Sub btnCalculate_Click(sender As Object, e As EventArgs)
            CalculatePrice()
        End Sub

        Protected Sub ddlGuests_SelectedIndexChanged(sender As Object, e As EventArgs)
            CalculatePrice()
        End Sub

        Private Sub CalculatePrice()
            Try
                Dim checkIn As DateTime
                Dim checkOut As DateTime

                If Not DateTime.TryParse(txtCheckIn.Text, checkIn) OrElse Not DateTime.TryParse(txtCheckOut.Text, checkOut) Then
                    litNights.Text = "0"
                    litTotalPrice.Text = "0.00"
                    Return
                End If

                If checkIn >= checkOut Then
                    litNights.Text = "0"
                    litTotalPrice.Text = "0.00"
                    Return
                End If

                Dim roomID As Integer = 0
                If ViewState("RoomID") IsNot Nothing Then
                    roomID = CInt(ViewState("RoomID"))
                ElseIf Integer.TryParse(Request.QueryString("id"), roomID) Then
                Else
                    Return
                End If

                Dim bookingService As New BookingService()
                Dim result = bookingService.EstimatePrice(roomID, checkIn, checkOut)

                Dim nights As Integer = CInt(Math.Ceiling((checkOut - checkIn).TotalDays))
                Dim baseTotal As Decimal
                Dim haveBaseTotal As Boolean = False

                If result.Success Then
                    baseTotal = result.Data
                    haveBaseTotal = True
                ElseIf ViewState("DailyRate") IsNot Nothing Then
                    Dim dailyRate As Decimal = CDec(ViewState("DailyRate"))
                    baseTotal = dailyRate * nights
                    haveBaseTotal = True
                End If

                If haveBaseTotal Then
                    litNights.Text = nights.ToString()

                    ' Substituted dispatch tier: picks the per-night rate multiplier for the
                    ' guest's negotiated rate plan. The rate plan CODE was decided once, in
                    ' LoadRoomDetails, based on the guest's loyalty tier at that time -- it is
                    ' trusted here, unchanged, even though the hidden field carrying it is
                    ' re-submitted (and therefore editable) on every "Calculate Price" postback.
                    Dim ratePlanMultiplier As Decimal = 1.0D
                    Select Case hfRatePlanCode.Value
                        ' SINK: PLANTED-VB-HR-334
                        Case "CORPORATE"
                            ratePlanMultiplier = 0.7D
                        Case "MEMBER"
                            ratePlanMultiplier = 0.85D
                        Case Else
                            ratePlanMultiplier = 1.0D
                    End Select

                    litTotalPrice.Text = (baseTotal * ratePlanMultiplier).ToString("N2")
                End If

            Catch ex As Exception
            End Try
        End Sub

        ''' Hardened counterpart of CalculatePrice: re-derives the guest's loyalty tier (and
        ''' therefore their eligible rate plan) fresh, by their authenticated UserID, instead of
        ''' trusting the rate-plan code cached in hfRatePlanCode. Writes to a separate "Verified
        ''' Rate" total so the two calculators can be compared side by side.
        Private Sub CalculatePriceVerified()
            Try
                Dim checkIn As DateTime
                Dim checkOut As DateTime

                If Not DateTime.TryParse(txtCheckIn.Text, checkIn) OrElse Not DateTime.TryParse(txtCheckOut.Text, checkOut) Then
                    litTotalPriceVerified.Text = "0.00"
                    Return
                End If

                If checkIn >= checkOut Then
                    litTotalPriceVerified.Text = "0.00"
                    Return
                End If

                Dim roomID As Integer = 0
                If ViewState("RoomID") IsNot Nothing Then
                    roomID = CInt(ViewState("RoomID"))
                ElseIf Integer.TryParse(Request.QueryString("id"), roomID) Then
                Else
                    Return
                End If

                Dim bookingService As New BookingService()
                Dim result = bookingService.EstimatePrice(roomID, checkIn, checkOut)

                Dim nights As Integer = CInt(Math.Ceiling((checkOut - checkIn).TotalDays))
                Dim baseTotal As Decimal
                Dim haveBaseTotal As Boolean = False

                If result.Success Then
                    baseTotal = result.Data
                    haveBaseTotal = True
                ElseIf ViewState("DailyRate") IsNot Nothing Then
                    Dim dailyRate As Decimal = CDec(ViewState("DailyRate"))
                    baseTotal = dailyRate * nights
                    haveBaseTotal = True
                End If

                If haveBaseTotal Then
                    Dim freshRatePlanCode As String = "STANDARD"
                    If Session("UserID") IsNot Nothing Then
                        Dim userService As New UserService()
                        Dim freshTier As String = userService.GetLoyaltyTierForUser(CInt(Session("UserID")))
                        freshRatePlanCode = GetEligibleRatePlanCode(freshTier)
                    End If

                    Dim ratePlanMultiplier As Decimal = 1.0D
                    Select Case freshRatePlanCode
                        ' SAFE_SINK: PLANTED-VB-HR-334-safe
                        Case "CORPORATE"
                            ratePlanMultiplier = 0.7D
                        Case "MEMBER"
                            ratePlanMultiplier = 0.85D
                        Case Else
                            ratePlanMultiplier = 1.0D
                    End Select

                    litTotalPriceVerified.Text = (baseTotal * ratePlanMultiplier).ToString("N2")
                End If

            Catch ex As Exception
            End Try
        End Sub

        Protected Sub btnCalculateVerified_Click(sender As Object, e As EventArgs)
            CalculatePriceVerified()
        End Sub

        Protected Sub btnBookNow_Click(sender As Object, e As EventArgs)
            Dim checkIn As DateTime
            Dim checkOut As DateTime

            If Not DateTime.TryParse(txtCheckIn.Text, checkIn) Then
                ShowError("Please select a valid check-in date.")
                Return
            End If

            If Not DateTime.TryParse(txtCheckOut.Text, checkOut) Then
                ShowError("Please select a valid check-out date.")
                Return
            End If

            If checkIn >= checkOut Then
                ShowError("Check-out date must be after check-in date.")
                Return
            End If

            If checkIn < DateTime.Today Then
                ShowError("Check-in date cannot be in the past.")
                Return
            End If

            If Session("UserID") Is Nothing Then
                Session("PendingBooking_RoomID") = Request.QueryString("id")
                Session("PendingBooking_CheckIn") = checkIn
                Session("PendingBooking_CheckOut") = checkOut
                Session("PendingBooking_Guests") = ddlGuests.SelectedValue
                
                Response.Redirect("~/Account/Login.aspx?returnUrl=" & Server.UrlEncode("~/Booking/Checkout.aspx"))
                Return
            End If

            Response.Redirect(String.Format("~/Booking/Checkout.aspx?roomId={0}&checkIn={1}&checkOut={2}&guests={3}",
                Request.QueryString("id"),
                Server.UrlEncode(checkIn.ToShortDateString()),
                Server.UrlEncode(checkOut.ToShortDateString()),
                ddlGuests.SelectedValue))
        End Sub

        Private Sub ShowError(message As String)
            pnlError.Visible = True
            pnlRoomDetails.Visible = False
            litError.Text = "<i class='fa fa-exclamation-circle mr-2'></i> " & message
        End Sub

    End Class
End Namespace
