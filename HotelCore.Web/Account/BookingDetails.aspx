<%@ Page Title="Booking Details - HotelCore" Language="vb" AutoEventWireup="false" MasterPageFile="~/Shared/Site.Master" CodeBehind="BookingDetails.aspx.vb" Inherits="HotelCore.Web.Account.BookingDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <!-- Breadcrumb Section -->
    <section class="hotelcore-breadcrumb">
        <div class="container">
            <h1>Booking Details</h1>
            <div class="hotelcore-breadcrumb-nav">
                <a href="<%= ResolveUrl("~/Default.aspx") %>">Home</a>
                <span>/</span>
                <a href="<%= ResolveUrl("~/Account/Profile.aspx?tab=bookings") %>">My Bookings</a>
                <span>/</span>
                <span>Details</span>
            </div>
        </div>
    </section>
    
    <!-- Booking Details Section -->
    <section class="hotelcore-section">
        <div class="container">
            <asp:Panel ID="pnlError" runat="server" CssClass="hotelcore-alert hotelcore-alert-error" Visible="false">
                <asp:Literal ID="litError" runat="server"></asp:Literal>
            </asp:Panel>
            
            <asp:Panel ID="pnlSuccess" runat="server" CssClass="hotelcore-alert hotelcore-alert-success" Visible="false">
                <asp:Literal ID="litSuccess" runat="server"></asp:Literal>
            </asp:Panel>
            
            <asp:Panel ID="pnlBookingDetails" runat="server">
                <div class="row">
                    <div class="col-lg-8">
                        <!-- Booking Information -->
                        <div class="hotelcore-card">
                            <div class="hotelcore-card-header d-flex justify-content-between align-items-center">
                                <h4>Booking Information</h4>
                                <span class="badge badge-<%# GetStatusBadgeClass() %> badge-lg p-2"><asp:Literal ID="litStatus" runat="server"></asp:Literal></span>
                            </div>
                            
                            <div class="row">
                                <div class="col-md-6">
                                    <table class="w-100">
                                        <tr>
                                            <td class="py-2 text-muted">Booking Reference:</td>
                                            <td class="py-2 text-right"><strong><asp:Literal ID="litBookingRef" runat="server"></asp:Literal></strong></td>
                                        </tr>
                                        <tr>
                                            <td class="py-2 text-muted">Room Type:</td>
                                            <td class="py-2 text-right"><asp:Literal ID="litRoomType" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <td class="py-2 text-muted">Room Number:</td>
                                            <td class="py-2 text-right"><asp:Literal ID="litRoomNumber" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <td class="py-2 text-muted">Booking Date:</td>
                                            <td class="py-2 text-right"><asp:Literal ID="litBookingDate" runat="server"></asp:Literal></td>
                                        </tr>
                                    </table>
                                </div>
                                <div class="col-md-6">
                                    <table class="w-100">
                                        <tr>
                                            <td class="py-2 text-muted">Check-In:</td>
                                            <td class="py-2 text-right"><asp:Literal ID="litCheckIn" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <td class="py-2 text-muted">Check-Out:</td>
                                            <td class="py-2 text-right"><asp:Literal ID="litCheckOut" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <td class="py-2 text-muted">Number of Guests:</td>
                                            <td class="py-2 text-right"><asp:Literal ID="litGuests" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <td class="py-2 text-muted">Total Amount:</td>
                                            <td class="py-2 text-right text-sage"><strong>$<asp:Literal ID="litTotal" runat="server"></asp:Literal></strong></td>
                                        </tr>
                                    </table>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Guest List -->
                        <asp:Panel ID="pnlGuests" runat="server" Visible="false">
                            <div class="hotelcore-card mt-4">
                                <div class="hotelcore-card-header">
                                    <h4>Guest List</h4>
                                </div>
                                <asp:Repeater ID="rptGuests" runat="server">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-between py-2 border-bottom">
                                            <div>
                                                <strong><%# Eval("FullName") %></strong>
                                                <span class="text-muted ml-2">(<%# Eval("GuestType") %>)</span>
                                            </div>
                                            <div>
                                                <span class="badge badge-secondary"><%# Eval("AgeCategory") %></span>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </asp:Panel>
                        
                        <!-- Payment History -->
                        <asp:Panel ID="pnlPayments" runat="server" Visible="false">
                            <div class="hotelcore-card mt-4">
                                <div class="hotelcore-card-header">
                                    <h4>Payment History</h4>
                                </div>
                                <asp:Repeater ID="rptPayments" runat="server">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-between py-2 border-bottom">
                                            <div>
                                                <strong>$<%# Eval("Amount", "{0:N2}") %></strong>
                                                <span class="text-muted ml-2">via <%# Eval("PaymentMethod") %></span>
                                            </div>
                                            <div>
                                                <span class="badge badge-success"><%# Eval("Status") %></span>
                                                <small class="text-muted ml-2"><%# CDate(Eval("PaymentDate")).ToShortDateString() %></small>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </asp:Panel>
                    </div>
                    
                    <!-- Sidebar Actions -->
                    <div class="col-lg-4">
                        <div class="hotelcore-card">
                            <h5>Actions</h5>
                            <asp:Panel ID="pnlCanCancel" runat="server">
                                <p class="text-muted small">You can cancel this booking up to 24 hours before check-in.</p>
                                <asp:Button ID="btnCancelBooking" runat="server" Text="Cancel Booking" CssClass="btn btn-outline-danger w-100 mb-2" OnClick="btnCancelBooking_Click" OnClientClick="return confirm('Are you sure you want to cancel this booking?');" />
                            </asp:Panel>
                            <a href="<%= ResolveUrl("~/Account/Profile.aspx?tab=bookings") %>" class="btn btn-outline-secondary w-100">Back to My Bookings</a>
                        </div>
                        
                        <!-- Cancellation Info -->
                        <asp:Panel ID="pnlCancellationInfo" runat="server" Visible="false">
                            <div class="hotelcore-card mt-3 bg-light">
                                <h5 class="text-danger">Booking Cancelled</h5>
                                <p class="text-muted small mb-1"><strong>Cancelled On:</strong> <asp:Literal ID="litCancelledDate" runat="server"></asp:Literal></p>
                                <p class="text-muted small mb-0"><strong>Reason:</strong> <asp:Literal ID="litCancellationReason" runat="server"></asp:Literal></p>
                            </div>
                        </asp:Panel>
                        
                        <!-- Contact Support -->
                        <div class="hotelcore-card mt-3">
                            <h5>Need Help?</h5>
                            <p class="text-muted small">Contact our support team for any questions about your booking.</p>
                            <p class="mb-1"><i class="fa fa-phone text-sage mr-2"></i> +1 234 567 8900</p>
                            <p class="mb-0"><i class="fa fa-envelope text-sage mr-2"></i> support@hotelcore.com</p>
                        </div>

                        <!-- Resend Confirmation -->
                        <div class="hotelcore-card mt-3">
                            <h5>Resend Confirmation</h5>
                            <p class="text-muted small">Email a copy of this booking's confirmation to another address.</p>
                            <asp:TextBox ID="txtResendEmail" runat="server" CssClass="form-control mb-2" placeholder="name@example.com"></asp:TextBox>
                            <asp:Button ID="btnResendConfirmation" runat="server" Text="Resend" CssClass="btn btn-outline-secondary w-100 mb-2" OnClick="btnResendConfirmation_Click" CausesValidation="false" />
                            <asp:Button ID="btnResendConfirmationSecure" runat="server" Text="Resend (Validated)" CssClass="btn btn-outline-secondary w-100" OnClick="btnResendConfirmationSecure_Click" CausesValidation="false" />
                        </div>

                        <!-- Notify About This Booking -->
                        <div class="hotelcore-card mt-3">
                            <h5>Notify About This Booking</h5>
                            <p class="text-muted small">Send a booking-update notification to the guest or to internal staff.</p>
                            <asp:DropDownList ID="ddlNotifyTarget" runat="server" CssClass="form-control mb-2">
                                <asp:ListItem Value="Guest">Guest</asp:ListItem>
                                <asp:ListItem Value="Staff">Staff</asp:ListItem>
                            </asp:DropDownList>
                            <asp:TextBox ID="txtNotifyEmail" runat="server" CssClass="form-control mb-2" placeholder="name@example.com"></asp:TextBox>
                            <asp:Button ID="btnSendNotification" runat="server" Text="Send Notification" CssClass="btn btn-outline-secondary w-100" OnClick="btnSendNotification_Click" CausesValidation="false" />
                        </div>

                        <!-- Group Booking Roster Lookup -->
                        <div class="hotelcore-card mt-3">
                            <h5>Group Booking Roster</h5>
                            <p class="text-muted small">Coordinating a group trip? Look up another booking's guest roster by its booking ID.</p>
                            <asp:TextBox ID="txtGroupBookingID" runat="server" CssClass="form-control mb-2" placeholder="Booking ID"></asp:TextBox>
                            <asp:Button ID="btnLookupGroupRoster" runat="server" Text="Look Up Roster" CssClass="btn btn-outline-secondary w-100 mb-2" OnClick="btnLookupGroupRoster_Click" CausesValidation="false" />
                            <asp:Button ID="btnLookupGroupRosterSecure" runat="server" Text="Look Up Roster (Verified)" CssClass="btn btn-outline-secondary w-100" OnClick="btnLookupGroupRosterSecure_Click" CausesValidation="false" />

                            <asp:Panel ID="pnlGroupRoster" runat="server" Visible="false" CssClass="mt-3">
                                <h6 class="text-muted">Roster</h6>
                                <asp:Repeater ID="rptGroupRoster" runat="server">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-between py-1">
                                            <span><%# Eval("FullName") %></span>
                                            <span class="text-muted"><%# Eval("GuestType") %></span>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </asp:Panel>

                            <asp:Panel ID="pnlGroupRosterSecure" runat="server" Visible="false" CssClass="mt-3">
                                <h6 class="text-muted">Roster (Verified)</h6>
                                <asp:Repeater ID="rptGroupRosterSecure" runat="server">
                                    <ItemTemplate>
                                        <div class="d-flex justify-content-between py-1">
                                            <span><%# Eval("FullName") %></span>
                                            <span class="text-muted"><%# Eval("GuestType") %></span>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </asp:Panel>
                        </div>

                        <!-- Payment Receipt Lookup -->
                        <div class="hotelcore-card mt-3">
                            <h5>View Payment Receipt</h5>
                            <p class="text-muted small">Enter a payment ID to view its receipt detail.</p>
                            <asp:TextBox ID="txtReceiptPaymentID" runat="server" CssClass="form-control mb-2" placeholder="Payment ID"></asp:TextBox>
                            <asp:Button ID="btnViewPaymentReceipt" runat="server" Text="View Receipt" CssClass="btn btn-outline-secondary w-100 mb-2" OnClick="btnViewPaymentReceipt_Click" CausesValidation="false" />
                            <asp:Button ID="btnViewPaymentReceiptSecure" runat="server" Text="View Receipt (Verified)" CssClass="btn btn-outline-secondary w-100" OnClick="btnViewPaymentReceiptSecure_Click" CausesValidation="false" />

                            <asp:Panel ID="pnlReceipt" runat="server" Visible="false" CssClass="mt-3">
                                <table class="w-100">
                                    <tr><td class="text-muted">Reference:</td><td class="text-right"><asp:Literal ID="litReceiptReference" runat="server"></asp:Literal></td></tr>
                                    <tr><td class="text-muted">Method:</td><td class="text-right"><asp:Literal ID="litReceiptMethod" runat="server"></asp:Literal></td></tr>
                                    <tr><td class="text-muted">Amount:</td><td class="text-right">$<asp:Literal ID="litReceiptAmount" runat="server"></asp:Literal></td></tr>
                                    <tr><td class="text-muted">Status:</td><td class="text-right"><asp:Literal ID="litReceiptStatus" runat="server"></asp:Literal></td></tr>
                                    <tr><td class="text-muted">Booking Ref:</td><td class="text-right"><asp:Literal ID="litReceiptBookingRef" runat="server"></asp:Literal></td></tr>
                                </table>
                            </asp:Panel>

                            <asp:Panel ID="pnlReceiptSecure" runat="server" Visible="false" CssClass="mt-3">
                                <h6 class="text-muted">Receipt (Verified)</h6>
                                <table class="w-100">
                                    <tr><td class="text-muted">Reference:</td><td class="text-right"><asp:Literal ID="litReceiptReferenceSecure" runat="server"></asp:Literal></td></tr>
                                    <tr><td class="text-muted">Method:</td><td class="text-right"><asp:Literal ID="litReceiptMethodSecure" runat="server"></asp:Literal></td></tr>
                                    <tr><td class="text-muted">Amount:</td><td class="text-right">$<asp:Literal ID="litReceiptAmountSecure" runat="server"></asp:Literal></td></tr>
                                    <tr><td class="text-muted">Status:</td><td class="text-right"><asp:Literal ID="litReceiptStatusSecure" runat="server"></asp:Literal></td></tr>
                                    <tr><td class="text-muted">Booking Ref:</td><td class="text-right"><asp:Literal ID="litReceiptBookingRefSecure" runat="server"></asp:Literal></td></tr>
                                </table>
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </asp:Panel>
            
            <!-- Cancel Booking Modal Content -->
            <asp:Panel ID="pnlCancelForm" runat="server" Visible="false">
                <div class="hotelcore-card">
                    <div class="hotelcore-card-header">
                        <h4>Cancel Booking</h4>
                    </div>
                    <p>Please provide a reason for cancellation:</p>
                    <div class="form-group">
                        <asp:TextBox ID="txtCancellationReason" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="Enter cancellation reason (minimum 10 characters)"></asp:TextBox>
                    </div>
                    <asp:Button ID="btnConfirmCancel" runat="server" Text="Confirm Cancellation" CssClass="btn-hotelcore btn-hotelcore-primary" OnClick="btnConfirmCancel_Click" />
                    <asp:Button ID="btnCancelCancelation" runat="server" Text="Go Back" CssClass="btn btn-outline-secondary ml-2" OnClick="btnCancelCancelation_Click" CausesValidation="false" />
                </div>
            </asp:Panel>
        </div>
    </section>
    
</asp:Content>
