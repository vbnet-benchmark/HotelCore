<%@ Page Title="Booking Activity - HotelCore" Language="vb" AutoEventWireup="false" MasterPageFile="~/Shared/Site.Master" CodeBehind="BookingTimeline.aspx.vb" Inherits="HotelCore.Web.Account.BookingTimeline" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Breadcrumb Section -->
    <section class="hotelcore-breadcrumb">
        <div class="container">
            <h1>Booking Activity</h1>
            <div class="hotelcore-breadcrumb-nav">
                <a href="<%= ResolveUrl("~/Default.aspx") %>">Home</a>
                <span>/</span>
                <a href="<%= ResolveUrl("~/Account/Profile.aspx?tab=bookings") %>">My Bookings</a>
                <span>/</span>
                <span>Activity</span>
            </div>
        </div>
    </section>

    <!-- Booking Activity Section -->
    <section class="hotelcore-section">
        <div class="container">
            <asp:Panel ID="pnlError" runat="server" CssClass="hotelcore-alert hotelcore-alert-error" Visible="false">
                <asp:Literal ID="litError" runat="server"></asp:Literal>
            </asp:Panel>

            <!-- Quick view: open directly from the "My Bookings" list's timeline link,
                 which passes the booking's raw ID in the query string. -->
            <asp:Panel ID="pnlTimeline" runat="server" Visible="false">
                <div class="hotelcore-card">
                    <div class="hotelcore-card-header">
                        <h4>Activity Timeline</h4>
                    </div>
                    <asp:Repeater ID="rptTimeline" runat="server">
                        <ItemTemplate>
                            <div class="d-flex justify-content-between py-2 border-bottom">
                                <div>
                                    <strong><%# Eval("Title") %></strong>
                                    <span class="text-muted ml-2"><%# Eval("Description") %></span>
                                </div>
                                <div>
                                    <span class="badge badge-secondary"><%# Eval("Status") %></span>
                                    <small class="text-muted ml-2"><%# CDate(Eval("EventDateTime")).ToShortDateString() %></small>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>

            <!-- Verified lookup: re-checks booking ownership before the activity timeline for
                 an arbitrary booking ID is ever shown. -->
            <div class="hotelcore-card mt-4">
                <div class="hotelcore-card-header">
                    <h4>Look Up Activity by Booking ID (Verified)</h4>
                </div>
                <p class="text-muted small">Enter a booking ID to view its activity timeline. Only bookings you own can be viewed this way.</p>
                <asp:TextBox ID="txtSecureBookingID" runat="server" CssClass="form-control mb-2" placeholder="Booking ID"></asp:TextBox>
                <asp:Button ID="btnViewSecure" runat="server" Text="View Activity (Verified)" CssClass="btn-hotelcore btn-hotelcore-primary" OnClick="btnViewSecure_Click" CausesValidation="false" />

                <asp:Panel ID="pnlTimelineSecure" runat="server" Visible="false" CssClass="mt-3">
                    <asp:Repeater ID="rptTimelineSecure" runat="server">
                        <ItemTemplate>
                            <div class="d-flex justify-content-between py-2 border-bottom">
                                <div>
                                    <strong><%# Eval("Title") %></strong>
                                    <span class="text-muted ml-2"><%# Eval("Description") %></span>
                                </div>
                                <div>
                                    <span class="badge badge-secondary"><%# Eval("Status") %></span>
                                    <small class="text-muted ml-2"><%# CDate(Eval("EventDateTime")).ToShortDateString() %></small>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>
            </div>
        </div>
    </section>

</asp:Content>
