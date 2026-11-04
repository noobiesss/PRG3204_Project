<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PRG3204_Project_Group4.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="text-center">
        <h1 class="display-4">Campus Event Management System</h1>
        <p>Find, register for and manage seminars, workshops, sports events and club activities.</p>
        <a class="btn btn-primary" href="<%: ResolveUrl("~/Events/Default.aspx") %>">Browse Events</a>
    </div>
</asp:Content>
