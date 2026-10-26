<%@ Page Title="Diagnostics - HotelCore Admin" Language="vb" AutoEventWireup="false" MasterPageFile="~/Shared/Site.Master" CodeBehind="Diagnostics.aspx.vb" Inherits="HotelCore.Web.Admin.Diagnostics" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Breadcrumb Section -->
    <section class="hotelcore-breadcrumb">
        <div class="container">
            <h1>System Diagnostics</h1>
            <div class="hotelcore-breadcrumb-nav">
                <a href="<%= ResolveUrl("~/Default.aspx") %>">Home</a>
                <span>/</span>
                <a href="<%= ResolveUrl("~/Admin/Dashboard.aspx") %>">Admin</a>
                <span>/</span>
                <span>Diagnostics</span>
            </div>
        </div>
    </section>

    <!-- Diagnostics Section -->
    <section class="hotelcore-section">
        <div class="container">

            <!-- Database Connectivity Test -->
            <div class="hotelcore-card mb-4">
                <h3>Database Connectivity</h3>
                <div class="form-group">
                    <label>Connection String</label>
                    <asp:TextBox ID="txtConnectionString" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <asp:Button ID="btnTestDb" runat="server" CssClass="btn btn-primary" Text="Test Database Connection" OnClick="btnTestDb_Click" />
                <p class="mt-2"><asp:Label ID="lblDbResult" runat="server"></asp:Label></p>
            </div>

            <!-- Email / SMTP Reachability Test -->
            <div class="hotelcore-card mb-4">
                <h3>Email Server Reachability</h3>
                <div class="row">
                    <div class="col-md-8">
                        <div class="form-group">
                            <label>SMTP Host</label>
                            <asp:TextBox ID="txtSmtpHost" runat="server" CssClass="form-control" Text="smtp.hotelcore-internal.local"></asp:TextBox>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="form-group">
                            <label>Port</label>
                            <asp:TextBox ID="txtSmtpPort" runat="server" CssClass="form-control" Text="587"></asp:TextBox>
                        </div>
                    </div>
                </div>
                <asp:Button ID="btnTestEmail" runat="server" CssClass="btn btn-primary" Text="Test Email Configuration" OnClick="btnTestEmail_Click" />
                <p class="mt-2"><asp:Label ID="lblEmailResult" runat="server"></asp:Label></p>
            </div>

            <!-- File Storage Access Test -->
            <div class="hotelcore-card mb-4">
                <h3>File Storage Access</h3>
                <div class="form-group">
                    <label>Storage Path</label>
                    <asp:TextBox ID="txtStoragePath" runat="server" CssClass="form-control" Text="D:\HotelCoreStorage\test.tmp"></asp:TextBox>
                </div>
                <asp:Button ID="btnTestFileAccess" runat="server" CssClass="btn btn-primary" Text="Test Storage Access" OnClick="btnTestFileAccess_Click" />
                <p class="mt-2"><asp:Label ID="lblStorageResult" runat="server"></asp:Label></p>
            </div>

            <!-- Backup Path Access Test -->
            <div class="hotelcore-card mb-4">
                <h3>Backup Path Access</h3>
                <div class="form-group">
                    <label>Backup Path</label>
                    <asp:TextBox ID="txtBackupPath" runat="server" CssClass="form-control" Text="D:\HotelCoreBackups\test.tmp"></asp:TextBox>
                </div>
                <asp:Button ID="btnTestBackupAccess" runat="server" CssClass="btn btn-primary" Text="Test Backup Path" OnClick="btnTestBackupAccess_Click" />
                <p class="mt-2"><asp:Label ID="lblBackupResult" runat="server"></asp:Label></p>
            </div>

            <!-- System Health Check -->
            <div class="hotelcore-card mb-4">
                <h3>System Health Check</h3>
                <div class="row">
                    <div class="col-md-6">
                        <div class="form-group">
                            <label>Next Maintenance Window</label>
                            <asp:TextBox ID="txtMaintenanceDate" runat="server" CssClass="form-control" Text="2026-11-01"></asp:TextBox>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="form-group">
                            <label>Log Archive Path</label>
                            <asp:TextBox ID="txtArchivePath" runat="server" CssClass="form-control" Text="D:\HotelCoreLogs\archive.log"></asp:TextBox>
                        </div>
                    </div>
                </div>
                <asp:Button ID="btnRunHealthCheck" runat="server" CssClass="btn btn-primary" Text="Run Health Check" OnClick="btnRunHealthCheck_Click" />
                <p class="mt-2"><asp:Label ID="lblHealthResult" runat="server"></asp:Label></p>
            </div>

        </div>
    </section>

</asp:Content>
