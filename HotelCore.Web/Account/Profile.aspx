<%@ Page Title="My Profile - HotelCore" Language="vb" AutoEventWireup="false" MasterPageFile="~/Shared/Site.Master" CodeBehind="Profile.aspx.vb" Inherits="HotelCore.Web.Account.Profile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <!-- Breadcrumb Section -->
    <section class="hotelcore-breadcrumb">
        <div class="container">
            <h1>My Profile</h1>
            <div class="hotelcore-breadcrumb-nav">
                <a href="<%= ResolveUrl("~/Default.aspx") %>">Home</a>
                <span>/</span>
                <span>My Profile</span>
            </div>
        </div>
    </section>
    
    <!-- Profile Section -->
    <section class="hotelcore-section">
        <div class="container">
            <div class="row">
                <!-- Profile Sidebar -->
                <div class="col-lg-3">
                    <div class="hotelcore-card">
                        <div class="text-center mb-4">
                            <i class="fa fa-user-circle fa-5x text-sage"></i>
                            <h5 class="mt-3"><asp:Literal ID="litUserName" runat="server"></asp:Literal></h5>
                            <p class="text-muted small"><asp:Literal ID="litUserEmail" runat="server"></asp:Literal></p>
                        </div>
                        <ul class="list-unstyled">
                            <li class="mb-2">
                                <asp:LinkButton ID="lnkProfile" runat="server" CssClass="d-block p-2 bg-light rounded" OnClick="lnkProfile_Click">
                                    <i class="fa fa-user mr-2"></i> Profile Information
                                </asp:LinkButton>
                            </li>
                            <li class="mb-2">
                                <asp:LinkButton ID="lnkBookings" runat="server" CssClass="d-block p-2" OnClick="lnkBookings_Click">
                                    <i class="fa fa-calendar mr-2"></i> My Bookings
                                </asp:LinkButton>
                            </li>
                            <li class="mb-2">
                                <asp:LinkButton ID="lnkChangePassword" runat="server" CssClass="d-block p-2" OnClick="lnkChangePassword_Click">
                                    <i class="fa fa-lock mr-2"></i> Change Password
                                </asp:LinkButton>
                            </li>
                        </ul>
                    </div>
                </div>
                
                <!-- Profile Content -->
                <div class="col-lg-9">
                    <asp:Panel ID="pnlSuccess" runat="server" CssClass="hotelcore-alert hotelcore-alert-success" Visible="false">
                        <asp:Literal ID="litSuccess" runat="server"></asp:Literal>
                    </asp:Panel>
                    
                    <asp:Panel ID="pnlError" runat="server" CssClass="hotelcore-alert hotelcore-alert-error" Visible="false">
                        <asp:Literal ID="litError" runat="server"></asp:Literal>
                    </asp:Panel>
                    
                    <!-- Profile Information Panel -->
                    <asp:Panel ID="pnlProfileInfo" runat="server">
                        <div class="hotelcore-card">
                            <div class="hotelcore-card-header">
                                <h4>Profile Information</h4>
                            </div>
                            <div class="hotelcore-form">
                                <div class="row">
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>First Name</label>
                                            <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Last Name</label>
                                            <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Email</label>
                                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Phone</label>
                                            <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                                <asp:Button ID="btnUpdateProfile" runat="server" Text="Update Profile" CssClass="btn-hotelcore btn-hotelcore-primary" OnClick="btnUpdateProfile_Click" />
                            </div>
                        </div>
                    </asp:Panel>
                    
                    <!-- My Bookings Panel -->
                    <asp:Panel ID="pnlBookings" runat="server" Visible="false">
                        <div class="hotelcore-card">
                            <div class="hotelcore-card-header">
                                <h4>My Bookings</h4>
                            </div>
                            
                            <!-- Filters -->
                            <div class="row mb-3">
                                <div class="col-md-4">
                                    <asp:DropDownList ID="ddlBookingStatus" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlBookingStatus_SelectedIndexChanged">
                                        <asp:ListItem Value="">All Statuses</asp:ListItem>
                                        <asp:ListItem Value="Pending">Pending</asp:ListItem>
                                        <asp:ListItem Value="Confirmed">Confirmed</asp:ListItem>
                                        <asp:ListItem Value="CheckedIn">Checked In</asp:ListItem>
                                        <asp:ListItem Value="CheckedOut">Checked Out</asp:ListItem>
                                        <asp:ListItem Value="Cancelled">Cancelled</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            
                            <asp:Panel ID="pnlNoBookings" runat="server" Visible="false" CssClass="text-center py-4">
                                <i class="fa fa-calendar-o fa-3x text-muted mb-3"></i>
                                <p>You don't have any bookings yet.</p>
                                <a href="<%= ResolveUrl("~/Booking/Search.aspx") %>" class="btn-hotelcore btn-hotelcore-primary">Book Now</a>
                            </asp:Panel>
                            
                            <asp:Repeater ID="rptBookings" runat="server">
                                <ItemTemplate>
                                    <div class="border rounded p-3 mb-3">
                                        <div class="row align-items-center">
                                            <div class="col-md-8">
                                                <h5 class="mb-1"><%# Eval("RoomType") %> - Room <%# Eval("RoomNumber") %></h5>
                                                <p class="text-muted mb-1">
                                                    <i class="fa fa-calendar mr-1"></i> 
                                                    <%# CDate(Eval("StartDateTime")).ToShortDateString() %> - <%# CDate(Eval("EndDateTime")).ToShortDateString() %>
                                                </p>
                                                <p class="mb-0">
                                                    <span class="badge badge-<%# GetStatusBadgeClass(Eval("Status").ToString()) %>"><%# Eval("Status") %></span>
                                                    <span class="text-sage ml-2"><strong>$<%# Eval("TotalAmount", "{0:N2}") %></strong></span>
                                                </p>
                                            </div>
                                            <div class="col-md-4 text-right">
                                                <small class="text-muted d-block mb-2">Ref: <%# Eval("BookingReference") %></small>
                                                <a href='<%# ResolveUrl("~/Account/BookingDetails.aspx?id=" & Eval("BookingID")) %>' class="btn btn-sm btn-outline-secondary">View Details</a>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <!-- Shared Booking Lookup -->
                            <div class="hotelcore-card mt-4">
                                <h5>View a Shared Booking</h5>
                                <p class="text-muted small">Have a booking ID from a travel coordinator or group organizer? Look it up here.</p>
                                <asp:TextBox ID="txtSharedBookingID" runat="server" CssClass="form-control mb-2" placeholder="Booking ID"></asp:TextBox>
                                <asp:Button ID="btnViewSharedBooking" runat="server" Text="View Booking" CssClass="btn btn-outline-secondary w-100 mb-2" OnClick="btnViewSharedBooking_Click" CausesValidation="false" />
                                <asp:Button ID="btnViewSharedBookingSecure" runat="server" Text="View Booking (Verified)" CssClass="btn btn-outline-secondary w-100" OnClick="btnViewSharedBookingSecure_Click" CausesValidation="false" />

                                <asp:Panel ID="pnlSharedBooking" runat="server" Visible="false" CssClass="mt-3">
                                    <p class="mb-1"><strong>Reference:</strong> <asp:Literal ID="litSharedBookingRef" runat="server"></asp:Literal></p>
                                    <p class="mb-1"><strong>Status:</strong> <asp:Literal ID="litSharedBookingStatus" runat="server"></asp:Literal></p>
                                    <p class="mb-1"><strong>Total:</strong> $<asp:Literal ID="litSharedBookingTotal" runat="server"></asp:Literal></p>
                                    <asp:Repeater ID="rptSharedBookingGuests" runat="server">
                                        <ItemTemplate>
                                            <div class="d-flex justify-content-between py-1">
                                                <span><%# Eval("FullName") %></span>
                                                <span class="text-muted"><%# Eval("GuestType") %></span>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </asp:Panel>

                                <asp:Panel ID="pnlSharedBookingSecure" runat="server" Visible="false" CssClass="mt-3">
                                    <h6 class="text-muted">Verified Result</h6>
                                    <p class="mb-1"><strong>Reference:</strong> <asp:Literal ID="litSharedBookingRefSecure" runat="server"></asp:Literal></p>
                                    <p class="mb-1"><strong>Status:</strong> <asp:Literal ID="litSharedBookingStatusSecure" runat="server"></asp:Literal></p>
                                    <p class="mb-1"><strong>Total:</strong> $<asp:Literal ID="litSharedBookingTotalSecure" runat="server"></asp:Literal></p>
                                    <asp:Repeater ID="rptSharedBookingGuestsSecure" runat="server">
                                        <ItemTemplate>
                                            <div class="d-flex justify-content-between py-1">
                                                <span><%# Eval("FullName") %></span>
                                                <span class="text-muted"><%# Eval("GuestType") %></span>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </asp:Panel>
                            </div>

                            <!-- Export Selected Bookings -->
                            <div class="hotelcore-card mt-4">
                                <h5>Export Selected Bookings</h5>
                                <p class="text-muted small">Enter a comma-separated list of booking IDs to export for your records (e.g. for an expense report).</p>
                                <asp:TextBox ID="txtExportBookingIDs" runat="server" CssClass="form-control mb-2" placeholder="e.g. 101, 102, 103"></asp:TextBox>
                                <asp:Button ID="btnExportSelectedBookings" runat="server" Text="Export" CssClass="btn btn-outline-secondary w-100 mb-2" OnClick="btnExportSelectedBookings_Click" CausesValidation="false" />
                                <asp:Button ID="btnExportSelectedBookingsSecure" runat="server" Text="Export (Verified)" CssClass="btn btn-outline-secondary w-100" OnClick="btnExportSelectedBookingsSecure_Click" CausesValidation="false" />

                                <asp:Panel ID="pnlExportResult" runat="server" Visible="false" CssClass="mt-3">
                                    <pre class="small"><asp:Literal ID="litExportContent" runat="server"></asp:Literal></pre>
                                </asp:Panel>

                                <asp:Panel ID="pnlExportResultSecure" runat="server" Visible="false" CssClass="mt-3">
                                    <h6 class="text-muted">Verified Export</h6>
                                    <pre class="small"><asp:Literal ID="litExportContentSecure" runat="server"></asp:Literal></pre>
                                </asp:Panel>
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- Change Password Panel -->
                    <asp:Panel ID="pnlChangePassword" runat="server" Visible="false">
                        <div class="hotelcore-card">
                            <div class="hotelcore-card-header">
                                <h4>Change Password</h4>
                            </div>
                            <div class="hotelcore-form">
                                <div class="form-group">
                                    <label>Current Password</label>
                                    <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label>New Password</label>
                                    <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                    <small class="text-muted">Minimum 8 characters</small>
                                </div>
                                <div class="form-group">
                                    <label>Confirm New Password</label>
                                    <asp:TextBox ID="txtConfirmNewPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                </div>
                                <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" CssClass="btn-hotelcore btn-hotelcore-primary" OnClick="btnChangePassword_Click" />
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </div>
    </section>
    
</asp:Content>
