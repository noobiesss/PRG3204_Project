<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="PRG3204_Project_Group4.Account.ChangePassword" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
    <div class="col-md-6 col-lg-5">
        <h1 class="mb-3">Change Password</h1>
        <asp:Label ID="lblMessage" runat="server" Visible="false" role="alert" />
        <div class="mb-3">
            <label class="form-label" for="<%= txtCurrentPassword.ClientID %>">Current Password</label>
            <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="form-control" TextMode="Password" ValidateRequestMode="Disabled" MaxLength="128" autocomplete="current-password" />
            <asp:RequiredFieldValidator ID="rfvCurrentPassword" runat="server" ControlToValidate="txtCurrentPassword" ErrorMessage="Current password is required." Display="Dynamic" CssClass="text-danger small" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtNewPassword.ClientID %>">New Password</label>
            <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password" ValidateRequestMode="Disabled" MaxLength="128" autocomplete="new-password" />
            <asp:RequiredFieldValidator ID="rfvNewPassword" runat="server" ControlToValidate="txtNewPassword" ErrorMessage="New password is required." Display="Dynamic" CssClass="text-danger small" />
            <asp:RegularExpressionValidator ID="revNewPassword" runat="server" ControlToValidate="txtNewPassword" ErrorMessage="Use 8 to 128 characters with at least one letter and one number." Display="Dynamic" CssClass="text-danger small" ValidationExpression="^(?=.*[A-Za-z])(?=.*\d).{8,128}$" />
            <asp:CompareValidator ID="cvDifferentPassword" runat="server" ControlToValidate="txtNewPassword" ErrorMessage="New password must be different from the current password." Display="Dynamic" CssClass="text-danger small" ControlToCompare="txtCurrentPassword" Operator="NotEqual" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtConfirmPassword.ClientID %>">Confirm New Password</label>
            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" ValidateRequestMode="Disabled" MaxLength="128" autocomplete="new-password" />
            <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ErrorMessage="Please confirm your new password." Display="Dynamic" CssClass="text-danger small" />
            <asp:CompareValidator ID="cvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ErrorMessage="Passwords do not match." Display="Dynamic" CssClass="text-danger small" ControlToCompare="txtNewPassword" />
        </div>
        <asp:Button ID="btnChange" runat="server" Text="Change Password" CssClass="btn btn-primary" OnClick="btnChange_Click" />
    </div>
    </div>
</asp:Content>
