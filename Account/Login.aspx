<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="PRG3204_Project_Group4.Account.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
    <div class="col-md-6 col-lg-5">
        <h1 class="mb-3">Login</h1>
        <asp:Label ID="lblMessage" runat="server" Visible="false" role="alert" />
        <div class="mb-3">
            <label class="form-label" for="<%= txtEmail.ClientID %>">School Email</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="100" autocomplete="username" />
            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="School email is required." Display="Dynamic" CssClass="text-danger small" />
            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Enter your school email ending in .edu.my." Display="Dynamic" CssClass="text-danger small" ValidationExpression="^(?=.{1,100}$)[^@\s]+@[^@\s]+\.edu\.my$" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtPassword.ClientID %>">Password</label>
            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" ValidateRequestMode="Disabled" MaxLength="128" autocomplete="current-password" />
            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Password is required." Display="Dynamic" CssClass="text-danger small" />
        </div>
        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
        <p class="mt-3 mb-0">New student? <a href="<%: ResolveUrl("~/Account/Register.aspx") %>">Create an account</a></p>
    </div>
    </div>
</asp:Content>
