<%@ Page Title="Export Reports - HotelCore Admin" Language="vb" AutoEventWireup="false" MasterPageFile="~/Shared/Site.Master" CodeBehind="ReportExport.aspx.vb" Inherits="HotelCore.Web.Admin.ReportExport" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Breadcrumb Section -->
    <section class="hotelcore-breadcrumb">
        <div class="container">
            <h1>Export Reports</h1>
            <div class="hotelcore-breadcrumb-nav">
                <a href="<%= ResolveUrl("~/Default.aspx") %>">Home</a>
                <span>/</span>
                <a href="<%= ResolveUrl("~/Admin/Dashboard.aspx") %>">Admin</a>
                <span>/</span>
                <span>Export Reports</span>
            </div>
        </div>
    </section>

    <!-- Export Section -->
    <section class="hotelcore-section">
        <div class="container">
            <asp:Panel ID="pnlSuccess" runat="server" CssClass="hotelcore-alert hotelcore-alert-success" Visible="false">
                <asp:Literal ID="litSuccess" runat="server"></asp:Literal>
            </asp:Panel>

            <asp:Panel ID="pnlError" runat="server" CssClass="hotelcore-alert hotelcore-alert-error" Visible="false">
                <asp:Literal ID="litError" runat="server"></asp:Literal>
            </asp:Panel>

            <div class="hotelcore-card">
                <h3>Export My Booking History</h3>
                <div class="form-group">
                    <label>Export File Name</label>
                    <asp:TextBox ID="txtExportFileName" runat="server" CssClass="form-control" Text="booking-history"></asp:TextBox>
                </div>
                <div class="form-group form-check">
                    <asp:CheckBox ID="chkShowTechnicalDetails" runat="server" Text=" Show technical details if the export fails (for support troubleshooting)" />
                </div>
                <asp:Button ID="btnExport" runat="server" CssClass="btn btn-primary" Text="Export Report" OnClick="btnExport_Click" />
            </div>
        </div>
    </section>

</asp:Content>
