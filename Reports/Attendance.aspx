<%@ Page Title="Attendance Summary" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Attendance.aspx.cs" Inherits="PRG3204_Project_Group4.Reports.Attendance" %>
<asp:Content ID="AttendanceHead" ContentPlaceHolderID="head" runat="server">
    <link href="<%= ResolveUrl("~/Styles/M4Reports.css") %>" rel="stylesheet" />
</asp:Content>
<asp:Content ID="AttendanceBody" ContentPlaceHolderID="MainContent" runat="server">
<div class="m4-report">
    <div class="mb-4">
        <div class="m4-eyebrow">M4 / Reports</div>
        <h1 class="m4-title h2 mt-1">Event attendance summary</h1>
        <p class="m4-subtitle">View active registrations, recorded attendance and remaining event seats. Staff see their own events; admins see all events. All event dates use Malaysia Time (UTC+8).</p>
    </div>
    <div class="row g-3 mb-4">
        <div class="col-6 col-lg-3"><div class="m4-kpi"><div class="m4-kpi-label">Events</div><div class="m4-kpi-value"><asp:Label ID="lblEventCount" runat="server" Text="0" /></div></div></div>
        <div class="col-6 col-lg-3"><div class="m4-kpi"><div class="m4-kpi-label">Active registrations</div><div class="m4-kpi-value"><asp:Label ID="lblRegistered" runat="server" Text="0" /></div></div></div>
        <div class="col-6 col-lg-3"><div class="m4-kpi"><div class="m4-kpi-label">Checked in</div><div class="m4-kpi-value"><asp:Label ID="lblAttended" runat="server" Text="0" /></div></div></div>
        <div class="col-6 col-lg-3"><div class="m4-kpi"><div class="m4-kpi-label">Attendance rate</div><div class="m4-kpi-value"><asp:Label ID="lblAttendanceRate" runat="server" Text="0%" /></div></div></div>
    </div>
    <div class="m4-panel">
        <div class="row g-3 align-items-end">
            <div class="col-md-4"><label for="<%= txtSearch.ClientID %>" class="form-label">Search event title</label><asp:TextBox ID="txtSearch" runat="server" MaxLength="100" CssClass="form-control" placeholder="e.g. Career Workshop" /></div>
            <div class="col-md-3"><label for="<%= ddlCategory.ClientID %>" class="form-label">Category</label><asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" /></div>
            <div class="col-md-3"><label for="<%= ddlStatus.ClientID %>" class="form-label">Event status</label><asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select"><asp:ListItem Value="">All statuses</asp:ListItem><asp:ListItem>Upcoming</asp:ListItem><asp:ListItem>Ongoing</asp:ListItem><asp:ListItem>Completed</asp:ListItem><asp:ListItem>Cancelled</asp:ListItem></asp:DropDownList></div>
            <div class="col-md-2"><asp:Button ID="btnFilter" runat="server" Text="Apply filters" CssClass="btn btn-primary w-100" OnClick="FilterButton_Click" /></div>
        </div>
    </div>
    <div class="m4-panel">
        <h2 class="h5 mb-3">Event attendance records</h2>
        <div class="table-responsive">
            <asp:GridView ID="gvAttendance" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="table table-hover table-bordered align-middle" EmptyDataText="No events match these filters." AllowPaging="true" PageSize="12" OnPageIndexChanging="AttendanceGrid_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="Title" HeaderText="Event" HtmlEncode="true" />
                    <asp:BoundField DataField="CategoryName" HeaderText="Category" HtmlEncode="true" />
                    <asp:BoundField DataField="StartMalaysia" HeaderText="Starts (MYT)" DataFormatString="{0:dd MMM yyyy HH:mm}" HtmlEncode="true" />
                    <asp:BoundField DataField="Capacity" HeaderText="Seats" />
                    <asp:BoundField DataField="RegisteredCount" HeaderText="Registered" />
                    <asp:BoundField DataField="AttendedCount" HeaderText="Attended" />
                    <asp:BoundField DataField="CancelledCount" HeaderText="Cancelled" />
                    <asp:BoundField DataField="RemainingSeats" HeaderText="Available" />
                    <asp:BoundField DataField="AttendanceRate" HeaderText="Rate (%)" DataFormatString="{0:0.0}" />
                </Columns>
                <PagerStyle CssClass="pagination-ys" />
            </asp:GridView>
        </div>
        <p class="m4-small mb-0">Attendance counts are based on <code>Registrations.IsAttended</code>. Cancelled registrations are not counted as active attendees.</p>
    </div>
</div>
</asp:Content>
