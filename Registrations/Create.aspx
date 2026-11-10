<%@ Page Title="Confirm Registration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Create.aspx.cs" Inherits="PRG3204_Project_Group4.Registrations.Create" EnableViewStateMac="true" EnableEventValidation="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="mb-3">Confirm Registration</h1>
    <% if (!string.IsNullOrEmpty(Notice)) { %>
        <div class="alert <%: NoticeClass %>" role="alert">
            <%: Notice %>
            <% if (NeedsLogin) { %>
                <a class="alert-link ms-2" href="<%: ResolveUrl("~/Account/Login.aspx") %>">Login</a>
            <% } %>
        </div>
    <% } %>
    <% if (CanShowEvent) { %>
        <div class="card mb-4">
            <div class="card-body">
                <h2 class="h4"><%: EventTitle %></h2>
                <dl class="row mb-0">
                    <dt class="col-sm-3">Location</dt><dd class="col-sm-9"><%: EventLocation %></dd>
                    <dt class="col-sm-3">Starts</dt><dd class="col-sm-9"><%: FormatDate(EventStart) %></dd>
                    <dt class="col-sm-3">Ends</dt><dd class="col-sm-9"><%: FormatDate(EventEnd) %></dd>
                    <dt class="col-sm-3">Event status</dt><dd class="col-sm-9"><%: EventStatus %></dd>
                    <dt class="col-sm-3">Capacity</dt><dd class="col-sm-9"><%: Capacity %></dd>
                    <dt class="col-sm-3">Seats remaining</dt><dd class="col-sm-9"><%: SeatsRemaining %></dd>
                </dl>
                <p class="small text-muted mt-3 mb-0">Times are shown in Malaysia time (UTC+08:00). Availability is checked again when you confirm.</p>
            </div>
        </div>
    <% } %>
    <asp:HiddenField ID="csrfToken" runat="server" />
    <div class="d-flex flex-wrap gap-2">
        <asp:Button ID="btnRegister" runat="server" Text="Confirm Registration" CssClass="btn btn-primary" OnClick="btnRegister_Click" Visible="false" />
        <a class="btn btn-outline-primary" href="<%: ResolveUrl("~/Registrations/Default.aspx") %>">My Registrations</a>
        <a class="btn btn-outline-secondary" href="<%: ResolveUrl("~/Events/Default.aspx") %>">Browse Events</a>
    </div>
</asp:Content>
