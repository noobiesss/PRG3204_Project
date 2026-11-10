<%@ Page Title="Access Denied" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="PRG3204_Project_Group4.Account.AccessDenied" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Access Denied</h1>
    <p class="text-muted">Your account does not have permission to open this page.</p>
    <a class="btn btn-primary" href="<%: ResolveUrl("~/Default.aspx") %>">Back to Home</a>
</asp:Content>
