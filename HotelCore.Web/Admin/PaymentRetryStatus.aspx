<%@ Page Title="Payment Retry Status - HotelCore Admin" Language="vb" AutoEventWireup="false" MasterPageFile="~/Shared/Site.Master" CodeBehind="PaymentRetryStatus.aspx.vb" Inherits="HotelCore.Web.Admin.PaymentRetryStatus" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Breadcrumb Section -->
    <section class="hotelcore-breadcrumb">
        <div class="container">
            <h1>Payment Retry Status</h1>
            <div class="hotelcore-breadcrumb-nav">
                <a href="<%= ResolveUrl("~/Default.aspx") %>">Home</a>
                <span>/</span>
                <a href="<%= ResolveUrl("~/Admin/Dashboard.aspx") %>">Admin</a>
                <span>/</span>
                <a href="<%= ResolveUrl("~/Admin/PaymentRetry.aspx") %>">Retry Payment</a>
                <span>/</span>
                <span>Status</span>
            </div>
        </div>
    </section>

    <!-- Status Section -->
    <section class="hotelcore-section">
        <div class="container">
            <div class="hotelcore-card">
                <p><asp:Literal ID="litStatusMessage" runat="server"></asp:Literal></p>
                <a href="<%= ResolveUrl("~/Admin/PaymentRetry.aspx") %>" class="btn btn-secondary">Back</a>
            </div>
        </div>
    </section>

</asp:Content>
