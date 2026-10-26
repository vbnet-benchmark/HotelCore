Imports HotelCore.BLL

Namespace Booking
    Public Class Checkout
        Inherits System.Web.UI.Page

        ' Standard per-additional-adult-guest service fee (per booking, not per night) and the
        ' standard refundable security deposit held at booking time -- both can be reduced to
        ' zero for sufficiently-loyal guests; see ApplyLoyaltyDiscount/ComputeAdditionalGuestFees
        ' and PaymentService.ComputeSecurityDepositAmount for the actual waiver decisions.
        Private Const ADDITIONAL_ADULT_GUEST_FEE As Decimal = 15D
        Private Const STANDARD_SECURITY_DEPOSIT As Decimal = 50D

        Private _roomID As Integer = 0
        Private _checkIn As DateTime
        Private _checkOut As DateTime
        Private _guestCount As Integer = 1

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session("UserID") Is Nothing Then
                Response.Redirect("~/Account/Login.aspx?returnUrl=" & Server.UrlEncode(Request.RawUrl))
                Return
            End If

            If Not IsPostBack Then
                LoadBookingDetails()
                PopulateExpiryYears()
                PopulateUserInfo()
            End If
        End Sub

        Private Sub LoadBookingDetails()
            Try
                If Not String.IsNullOrEmpty(Request.QueryString("roomId")) Then
                    _roomID = CInt(Request.QueryString("roomId"))
                    DateTime.TryParse(Server.UrlDecode(Request.QueryString("checkIn")), _checkIn)
                    DateTime.TryParse(Server.UrlDecode(Request.QueryString("checkOut")), _checkOut)
                    Integer.TryParse(Request.QueryString("guests"), _guestCount)
                ElseIf Session("PendingBooking_RoomID") IsNot Nothing Then
                    _roomID = CInt(Session("PendingBooking_RoomID"))
                    _checkIn = CDate(Session("PendingBooking_CheckIn"))
                    _checkOut = CDate(Session("PendingBooking_CheckOut"))
                    _guestCount = CInt(Session("PendingBooking_Guests"))
                    
                    Session.Remove("PendingBooking_RoomID")
                    Session.Remove("PendingBooking_CheckIn")
                    Session.Remove("PendingBooking_CheckOut")
                    Session.Remove("PendingBooking_Guests")
                Else
                    ShowError("No booking information found. Please start from room selection.")
                    Return
                End If

                If _checkIn = DateTime.MinValue OrElse _checkOut = DateTime.MinValue Then
                    ShowError("Invalid booking dates.")
                    Return
                End If

                ViewState("RoomID") = _roomID
                ViewState("CheckIn") = _checkIn
                ViewState("CheckOut") = _checkOut
                ViewState("GuestCount") = _guestCount

                Dim roomService As New RoomService()
                Dim result = roomService.GetRoomById(_roomID)

                If result.Success AndAlso result.Data IsNot Nothing Then
                    Dim room = result.Data
                    
                    litRoomType.Text = room.TypeName
                    litRoomNumber.Text = room.RoomNumber
                    litCheckIn.Text = _checkIn.ToShortDateString()
                    litCheckOut.Text = _checkOut.ToShortDateString()
                    litGuests.Text = _guestCount.ToString()

                    Dim nights As Integer = CInt(Math.Ceiling((_checkOut - _checkIn).TotalDays))
                    litNights.Text = nights.ToString()
                    litDailyRate.Text = room.BaseDailyRate.ToString("N2")

                    Dim subtotal As Decimal = room.BaseDailyRate * nights
                    Dim tax As Decimal = subtotal * 0.1D
                    Dim total As Decimal = subtotal + tax

                    litSubtotal.Text = subtotal.ToString("N2")
                    litTax.Text = tax.ToString("N2")
                    litTotal.Text = total.ToString("N2")

                    ViewState("DailyRate") = room.BaseDailyRate
                    ViewState("Total") = total

                    ' Cache this guest's loyalty-tier pricing eligibility once, on this initial
                    ' (non-postback) load, so the discount/fee-waiver/deposit-waiver checks at
                    ' booking completion don't need to re-query the user's tier on every
                    ' postback of this page (e.g. re-rendering after an additional-guest row is
                    ' added). Re-derived fresh by the "Verified" completion path instead of
                    ' trusting these cached values -- see ApplyLoyaltyDiscountSecure,
                    ' ComputeAdditionalGuestFeesSecure and PaymentService.ComputeSecurityDepositAmountSecure.
                    Dim userService As New UserService()
                    Dim loyaltyTier As String = userService.GetLoyaltyTierForUser(CInt(Session("UserID")))
                    hfLoyaltyDiscountPercent.Value = GetLoyaltyDiscountPercentForTier(loyaltyTier).ToString()
                    Dim isGoldTier As Boolean = (loyaltyTier = "Gold")
                    hfGuestFeeWaived.Value = isGoldTier.ToString()
                    hfDepositWaived.Value = isGoldTier.ToString()

                    If _guestCount > 1 Then
                        pnlAdditionalGuests.Visible = True
                        Dim additionalGuestCount As Integer = _guestCount - 1
                        Dim guestList As New List(Of Integer)
                        For i As Integer = 1 To additionalGuestCount
                            guestList.Add(i)
                        Next
                        rptAdditionalGuests.DataSource = guestList
                        rptAdditionalGuests.DataBind()
                    End If
                Else
                    ShowError("Room not found.")
                End If

            Catch ex As Exception
                ShowError("An error occurred while loading booking details.")
            End Try
        End Sub

        Private Sub PopulateExpiryYears()
            ddlExpiryYear.Items.Clear()
            Dim currentYear As Integer = DateTime.Now.Year
            For i As Integer = currentYear To currentYear + 10
                ddlExpiryYear.Items.Add(New ListItem(i.ToString(), i.ToString()))
            Next
        End Sub

        Private Sub PopulateUserInfo()
            If Session("UserEmail") IsNot Nothing Then
                txtEmail.Text = Session("UserEmail").ToString()
            End If
            If Session("UserFirstName") IsNot Nothing Then
                txtFirstName.Text = Session("UserFirstName").ToString()
            End If
            If Session("UserLastName") IsNot Nothing Then
                txtLastName.Text = Session("UserLastName").ToString()
            End If
            If Session("UserPhone") IsNot Nothing Then
                txtPhone.Text = Session("UserPhone").ToString()
            End If
        End Sub

        ''' Shared booking-construction logic used by both the vulnerable and the "Verified"
        ''' completion paths below -- extracted when the promo-code/loyalty-pricing feature was
        ''' added so the two paths differ only in how the final charged amount is computed, not
        ''' in how the booking/guest list itself is built.
        Private Function BuildBookingFromForm(userID As Integer, guestCount As Integer) As BookingModel
            Dim roomID As Integer = CInt(ViewState("RoomID"))
            Dim checkIn As DateTime = CDate(ViewState("CheckIn"))
            Dim checkOut As DateTime = CDate(ViewState("CheckOut"))

            Dim booking As New BookingModel With {
                .UserID = userID,
                .RoomID = roomID,
                .StartDateTime = checkIn,
                .EndDateTime = checkOut,
                .GuestCount = guestCount,
                .AdultCount = guestCount,
                .ChildCount = 0,
                .InfantCount = 0,
                .SpecialRequests = txtSpecialRequests.Text.Trim()
            }

            If guestCount > 1 Then
                booking.Guests = New List(Of GuestModel)()
                For Each item As RepeaterItem In rptAdditionalGuests.Items
                    Dim txtName As TextBox = CType(item.FindControl("txtGuestName"), TextBox)
                    Dim ddlAge As DropDownList = CType(item.FindControl("ddlAgeCategory"), DropDownList)
                    Dim txtGuestEmail As TextBox = CType(item.FindControl("txtGuestEmail"), TextBox)

                    If txtName IsNot Nothing AndAlso Not String.IsNullOrEmpty(txtName.Text) Then
                        booking.Guests.Add(New GuestModel With {
                            .FullName = txtName.Text.Trim(),
                            .AgeCategory = ddlAge.SelectedValue,
                            .Email = If(txtGuestEmail IsNot Nothing, txtGuestEmail.Text.Trim(), ""),
                            .GuestType = "Additional"
                        })

                        Select Case ddlAge.SelectedValue
                            Case "Child"
                                booking.ChildCount += 1
                                booking.AdultCount -= 1
                            Case "Infant"
                                booking.InfantCount += 1
                                booking.AdultCount -= 1
                        End Select
                    End If
                Next
            End If

            Return booking
        End Function

        ''' Looks up the discount percentage a promo code is worth, from a small fixed table of
        ''' currently-running promotions. An unrecognized code is worth 0% -- there is no way
        ''' for an arbitrary code string to map to an arbitrary percentage through this lookup.
        Private Function GetPromoDiscountPercent(code As String) As Decimal
            Select Case If(code, "").Trim().ToUpperInvariant()
                Case "SAVE10"
                    Return 10D
                Case "SAVE20"
                    Return 20D
                Case "WELCOME5"
                    Return 5D
                Case Else
                    Return 0D
            End Select
        End Function

        ''' Translates a loyalty tier into its stay-discount percentage.
        Private Function GetLoyaltyDiscountPercentForTier(loyaltyTier As String) As Decimal
            Select Case loyaltyTier
                Case "Gold"
                    Return 15D
                Case "Silver"
                    Return 8D
                Case Else
                    Return 0D
            End Select
        End Function

        ''' Applies the entered promo code, looking up its discount percentage from the fixed
        ''' table above and caching both the code and the resulting percentage for display and
        ''' for use when the booking is completed.
        Protected Sub btnApplyPromo_Click(sender As Object, e As EventArgs)
            Dim code As String = txtPromoCode.Text.Trim().ToUpperInvariant()
            Dim discountPercent As Decimal = GetPromoDiscountPercent(code)

            hfPromoCode.Value = code
            hfPromoDiscountPercent.Value = discountPercent.ToString()

            If discountPercent > 0D Then
                litPromoStatus.Text = $"Promo code applied: {discountPercent}% off your stay."
            Else
                litPromoStatus.Text = "That promo code isn't recognized."
            End If
        End Sub

        ''' Indirect tier: applies the cached per-user loyalty discount to an already
        ''' promo-adjusted amount. Called only from btnCompleteBooking_Click below -- the
        ''' discount percentage itself was cached into hfLoyaltyDiscountPercent back in
        ''' LoadBookingDetails, the first time this page was requested for the authenticated
        ''' guest, and is trusted here unchanged no matter how many postbacks (adding a guest
        ''' row, applying a promo code) happen before the booking is actually completed.
        Private Function ApplyLoyaltyDiscount(amount As Decimal) As Decimal
            Dim loyaltyPercent As Decimal = 0D
            Decimal.TryParse(hfLoyaltyDiscountPercent.Value, loyaltyPercent)
            ' SINK: PLANTED-VB-HR-332
            Return amount - (amount * loyaltyPercent / 100D)
        End Function

        ''' Hardened counterpart of ApplyLoyaltyDiscount: re-derives the guest's loyalty tier
        ''' fresh, by their authenticated UserID, instead of trusting the cached hidden field.
        Private Function ApplyLoyaltyDiscountSecure(amount As Decimal, userID As Integer) As Decimal
            Dim userService As New UserService()
            Dim freshTier As String = userService.GetLoyaltyTierForUser(userID)
            Dim loyaltyPercent As Decimal = GetLoyaltyDiscountPercentForTier(freshTier)
            ' SAFE_SINK: PLANTED-VB-HR-332-safe
            Return amount - (amount * loyaltyPercent / 100D)
        End Function

        ''' Loop tier: walks every additional-guest row and, for each adult guest, adds the
        ''' standard per-guest service fee unless the cached loyalty fee-waiver flag says this
        ''' guest's tier covers it. The flag was computed once, in LoadBookingDetails, when the
        ''' additional-guest panel was first populated, and is trusted again here for every row
        ''' without ever being re-derived.
        Private Function ComputeAdditionalGuestFees() As Decimal
            Dim isFeeWaived As Boolean = False
            Boolean.TryParse(hfGuestFeeWaived.Value, isFeeWaived)

            Dim totalFees As Decimal = 0D
            For Each item As RepeaterItem In rptAdditionalGuests.Items
                Dim ddlAge As DropDownList = CType(item.FindControl("ddlAgeCategory"), DropDownList)
                If ddlAge IsNot Nothing AndAlso ddlAge.SelectedValue = "Adult" Then
                    ' SINK: PLANTED-VB-HR-335
                    If Not isFeeWaived Then totalFees += ADDITIONAL_ADULT_GUEST_FEE
                End If
            Next
            Return totalFees
        End Function

        ''' Hardened counterpart of ComputeAdditionalGuestFees: re-derives the guest's loyalty
        ''' tier fresh, by their authenticated UserID, once before the loop, instead of trusting
        ''' a flag cached on an earlier page load.
        Private Function ComputeAdditionalGuestFeesSecure(userID As Integer) As Decimal
            Dim userService As New UserService()
            Dim freshTier As String = userService.GetLoyaltyTierForUser(userID)
            Dim isFeeWaived As Boolean = (freshTier = "Gold")

            Dim totalFees As Decimal = 0D
            For Each item As RepeaterItem In rptAdditionalGuests.Items
                Dim ddlAge As DropDownList = CType(item.FindControl("ddlAgeCategory"), DropDownList)
                If ddlAge IsNot Nothing AndAlso ddlAge.SelectedValue = "Adult" Then
                    ' SAFE_SINK: PLANTED-VB-HR-335-safe
                    If Not isFeeWaived Then totalFees += ADDITIONAL_ADULT_GUEST_FEE
                End If
            Next
            Return totalFees
        End Function

        Protected Sub btnCompleteBooking_Click(sender As Object, e As EventArgs)
            Try
                If Not chkTerms.Checked Then
                    ShowError("Please accept the Terms and Conditions to continue.")
                    Return
                End If

                Dim guestCount As Integer = CInt(ViewState("GuestCount"))
                Dim userID As Integer = CInt(Session("UserID"))

                Dim booking As BookingModel = BuildBookingFromForm(userID, guestCount)

                Dim bookingService As New BookingService()
                Dim result = bookingService.CreateBooking(booking, userID)

                If result.Success AndAlso result.Data IsNot Nothing Then
                    Dim totalAmount As Decimal = CDec(ViewState("Total"))

                    ' Direct tier: the promo code's cached discount percentage is read and
                    ' consumed right here, with no intervening helper call.
                    Dim promoPercent As Decimal = 0D
                    Decimal.TryParse(hfPromoDiscountPercent.Value, promoPercent)
                    ' SINK: PLANTED-VB-HR-331
                    Dim afterPromo As Decimal = totalAmount - (totalAmount * promoPercent / 100D)

                    Dim afterLoyalty As Decimal = ApplyLoyaltyDiscount(afterPromo)
                    Dim guestFees As Decimal = ComputeAdditionalGuestFees()
                    Dim chargeAmount As Decimal = afterLoyalty + guestFees

                    If ddlPaymentMethod.SelectedValue <> "PayAtHotel" Then
                        Dim paymentService As New PaymentService()

                        ' Interprocedural tier: the deposit-waiver flag crosses into
                        ' PaymentService.vb (a different file/class) unchanged -- the actual
                        ' trust decision is made there, in ComputeSecurityDepositAmount.
                        Dim waiveDeposit As Boolean = False
                        Boolean.TryParse(hfDepositWaived.Value, waiveDeposit)
                        Dim depositAmount As Decimal = paymentService.ComputeSecurityDepositAmount(STANDARD_SECURITY_DEPOSIT, waiveDeposit)

                        Dim payment As New PaymentModel With {
                            .BookingID = result.Data.BookingID,
                            .PaymentMethod = ddlPaymentMethod.SelectedValue,
                            .Amount = chargeAmount + depositAmount,
                            .ProcessorTransactionID = Guid.NewGuid().ToString()
                        }

                        Dim paymentResult = paymentService.ProcessPayment(payment, userID)
                    End If

                    Response.Redirect("~/Booking/Confirmation.aspx?id=" & result.Data.BookingID)
                Else
                    ShowError(If(String.IsNullOrEmpty(result.ErrorMessage), "An error occurred while creating the booking.", result.ErrorMessage))
                End If

            Catch ex As Exception
                ShowError("An error occurred while processing your booking. Please try again.")
            End Try
        End Sub

        ''' Hardened counterpart of btnCompleteBooking_Click: every pricing decision below is
        ''' re-derived at the moment of charging, from the promo code text / the authenticated
        ''' guest's own UserID, rather than trusting values cached into hidden fields on an
        ''' earlier page load or postback.
        Protected Sub btnCompleteBookingVerified_Click(sender As Object, e As EventArgs)
            Try
                If Not chkTerms.Checked Then
                    ShowError("Please accept the Terms and Conditions to continue.")
                    Return
                End If

                Dim guestCount As Integer = CInt(ViewState("GuestCount"))
                Dim userID As Integer = CInt(Session("UserID"))

                Dim booking As BookingModel = BuildBookingFromForm(userID, guestCount)

                Dim bookingService As New BookingService()
                Dim result = bookingService.CreateBooking(booking, userID)

                If result.Success AndAlso result.Data IsNot Nothing Then
                    Dim totalAmount As Decimal = CDec(ViewState("Total"))

                    Dim verifiedPromoPercent As Decimal = GetPromoDiscountPercent(hfPromoCode.Value)
                    ' SAFE_SINK: PLANTED-VB-HR-331-safe
                    Dim afterPromo As Decimal = totalAmount - (totalAmount * verifiedPromoPercent / 100D)

                    Dim afterLoyalty As Decimal = ApplyLoyaltyDiscountSecure(afterPromo, userID)
                    Dim guestFees As Decimal = ComputeAdditionalGuestFeesSecure(userID)
                    Dim chargeAmount As Decimal = afterLoyalty + guestFees

                    If ddlPaymentMethod.SelectedValue <> "PayAtHotel" Then
                        Dim paymentService As New PaymentService()
                        Dim depositAmount As Decimal = paymentService.ComputeSecurityDepositAmountSecure(STANDARD_SECURITY_DEPOSIT, userID)

                        Dim payment As New PaymentModel With {
                            .BookingID = result.Data.BookingID,
                            .PaymentMethod = ddlPaymentMethod.SelectedValue,
                            .Amount = chargeAmount + depositAmount,
                            .ProcessorTransactionID = Guid.NewGuid().ToString()
                        }

                        Dim paymentResult = paymentService.ProcessPayment(payment, userID)
                    End If

                    Response.Redirect("~/Booking/Confirmation.aspx?id=" & result.Data.BookingID)
                Else
                    ShowError(If(String.IsNullOrEmpty(result.ErrorMessage), "An error occurred while creating the booking.", result.ErrorMessage))
                End If

            Catch ex As Exception
                ShowError("An error occurred while processing your booking. Please try again.")
            End Try
        End Sub

        Private Sub ShowError(message As String)
            pnlError.Visible = True
            litError.Text = "<i class='fa fa-exclamation-circle mr-2'></i> " & message
        End Sub

    End Class
End Namespace
