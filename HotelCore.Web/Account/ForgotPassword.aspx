<%@ Page Title="Forgot Password - HotelCore" Language="vb" AutoEventWireup="false" MasterPageFile="~/Shared/Site.Master" CodeBehind="ForgotPassword.aspx.vb" Inherits="HotelCore.Web.Account.ForgotPassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Forgot Password Section -->
    <section class="hotelcore-section pt-navbar">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-lg-5 col-md-7">
                    <div class="hotelcore-card">
                        <div class="text-center mb-4">
                            <h2>Forgot Password</h2>
                            <p class="text-muted">Enter your email and we'll send you a reset link</p>
                        </div>

                        <asp:Panel ID="pnlError" runat="server" CssClass="hotelcore-alert hotelcore-alert-error" Visible="false">
                            <asp:Literal ID="litError" runat="server"></asp:Literal>
                        </asp:Panel>

                        <asp:Panel ID="pnlSuccess" runat="server" CssClass="hotelcore-alert hotelcore-alert-success" Visible="false">
                            <asp:Literal ID="litSuccess" runat="server"></asp:Literal>
                        </asp:Panel>

                        <div class="hotelcore-form">
                            <div class="form-group">
                                <label>Email Address <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter your email"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" CssClass="text-danger small" ErrorMessage="Email is required" Display="Dynamic"></asp:RequiredFieldValidator>
                            </div>

                            <div class="form-group">
                                <asp:Button ID="btnRequestReset" runat="server" Text="Send Reset Link" CssClass="btn hotelcore-btn-primary btn-block" OnClick="btnRequestReset_Click" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

</asp:Content>
