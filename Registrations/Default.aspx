<%@ Page Title="My Registrations" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PRG3204_Project_Group4.Registrations.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
        <div>
            <h1 class="mb-2">My Registrations</h1>
            <p class="text-muted mb-0">View your event registrations and attendance records.</p>
        </div>
        <asp:HyperLink ID="lnkBrowse" runat="server" CssClass="btn btn-outline-primary" NavigateUrl="~/Events/Default.aspx" Text="Browse Events" />
    </div>

    <asp:Panel ID="pnlAccess" runat="server" Visible="false" CssClass="alert alert-info" role="alert">
        <asp:Literal ID="litAccess" runat="server" Mode="Encode" />
        <asp:HyperLink ID="lnkLogin" runat="server" Visible="false" NavigateUrl="~/Account/Login.aspx" CssClass="alert-link ms-2" Text="Login" />
    </asp:Panel>

    <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="card">
        <div class="card-body py-5 text-center">
            <h2 class="h5">No registrations yet</h2>
            <p class="text-muted">Browse upcoming events to find an activity you would like to join.</p>
            <asp:HyperLink ID="lnkEmptyBrowse" runat="server" NavigateUrl="~/Events/Default.aspx" CssClass="btn btn-primary" Text="Browse Events" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlHistory" runat="server" Visible="false">
        <div class="mb-3">
    <a href="<%= ResolveUrl("~/Registrations/Modify.aspx") %>"
       class="btn btn-outline-primary">
        Change / Cancel
    </a>
</div>
        <p class="text-muted">Total: <asp:Literal ID="litTotal" runat="server" Mode="Encode" />. All times shown in Malaysia time (UTC+08:00).</p>
        <div class="table-responsive">
            <table class="table table-striped table-hover align-middle">
                <caption>Your event registration history, newest registration first.</caption>
                <thead class="table-light">
                    <tr>
                        <th scope="col">Event</th><th scope="col">Schedule</th>
                        <th scope="col">Registered On</th><th scope="col">Registration Status</th>
                        <th scope="col">Attendance</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptHistory" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <div class="fw-semibold"><asp:Literal runat="server" Mode="Encode" Text='<%# Eval("Title") %>' /></div>
                                    <div class="small text-muted"><asp:Literal runat="server" Mode="Encode" Text='<%# Eval("Location") %>' /></div>
                                    <div class="small text-muted">Event status: <asp:Literal runat="server" Mode="Encode" Text='<%# Eval("EventStatus") %>' /></div>
                                </td>
                                <td>
                                    <div><asp:Literal runat="server" Mode="Encode" Text='<%# FormatDate(Eval("StartDateTime")) %>' /></div>
                                    <div class="small text-muted">to <asp:Literal runat="server" Mode="Encode" Text='<%# FormatDate(Eval("EndDateTime")) %>' /></div>
                                </td>
                                <td><asp:Literal runat="server" Mode="Encode" Text='<%# FormatDate(Eval("RegisteredAt")) %>' /></td>
                                <td>
                                    <span class="badge bg-secondary"><asp:Literal runat="server" Mode="Encode" Text='<%# Eval("RegistrationStatus") %>' /></span>
                                    <asp:Panel runat="server" CssClass="small text-muted mt-1" Visible='<%# IsCancelled(Eval("RegistrationStatus")) %>'>
                                        <asp:Literal runat="server" Mode="Encode" Text='<%# FormatDate(Eval("CancelledAt")) %>' />
                                    </asp:Panel>
                                </td>
                                <td><asp:Literal runat="server" Mode="Encode" Text='<%# FormatAttendance(Eval("IsAttended")) %>' /></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
    </asp:Panel>
</asp:Content>
