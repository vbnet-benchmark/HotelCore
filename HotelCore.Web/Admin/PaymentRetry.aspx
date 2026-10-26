<%@ Page Title="Retry Payment - HotelCore Admin" Language="vb" AutoEventWireup="false" MasterPageFile="~/Shared/Site.Master" CodeBehind="PaymentRetry.aspx.vb" Inherits="HotelCore.Web.Admin.PaymentRetry" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Breadcrumb Section -->
    <section class="hotelcore-breadcrumb">
        <div class="container">
            <h1>Retry Payment</h1>
            <div class="hotelcore-breadcrumb-nav">
                <a href="<%= ResolveUrl("~/Default.aspx") %>">Home</a>
                <span>/</span>
                <a href="<%= ResolveUrl("~/Admin/Dashboard.aspx") %>">Admin</a>
                <span>/</span>
                <span>Retry Payment</span>
            </div>
        </div>
    </section>

    <!-- Retry Section -->
    <section class="hotelcore-section">
        <div class="container">
            <div class="hotelcore-card">
                <h3>Resubmit a Declined Payment</h3>
                <p class="text-muted">Use this when a guest's card was declined due to a transient gateway issue rather than an actual decline.</p>
                <div class="form-group">
                    <label>Payment ID</label>
                    <asp:TextBox ID="txtPaymentID" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <asp:Button ID="btnRetryGatewayA" runat="server" CssClass="btn btn-primary" Text="Retry via Gateway A (legacy)" OnClick="btnRetryGatewayA_Click" />
                <asp:Button ID="btnRetryGatewayB" runat="server" CssClass="btn btn-secondary" Text="Retry via Gateway B (current)" OnClick="btnRetryGatewayB_Click" />
            </div>
        </div>
    </section>

</asp:Content>
