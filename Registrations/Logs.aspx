<%@ Page Title="Registration Logs" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Logs.aspx.cs" Inherits="PRG3204_Project_Group4.Registrations.Logs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="h3 mt-3 mb-1">Registration Logs</h2>
    <p class="text-muted small">Every registration, cancellation and change you made. Times are in Malaysia Time (UTC+8).</p>

    <asp:GridView ID="gvLogs" runat="server" AutoGenerateColumns="false"
        CssClass="table table-sm table-striped" GridLines="None"
        EmptyDataText="No registration activity yet.">
        <Columns>
            <asp:BoundField DataField="ChangedMyt" HeaderText="When" DataFormatString="{0:dd MMM yyyy, HH:mm}" />
            <asp:BoundField DataField="Action" HeaderText="Action" />
            <asp:BoundField DataField="OldStatus" HeaderText="Old status" NullDisplayText="-" />
            <asp:BoundField DataField="NewStatus" HeaderText="New status" />
            <asp:BoundField DataField="OldEventTitle" HeaderText="From event" NullDisplayText="-" />
            <asp:BoundField DataField="NewEventTitle" HeaderText="To event" NullDisplayText="-" />
            <asp:BoundField DataField="ChangeReason" HeaderText="Reason" NullDisplayText="-" />
        </Columns>
    </asp:GridView>
    <a href="<%= ResolveUrl("~/Registrations/Modify.aspx") %>" class="btn btn-outline-secondary btn-sm">Back to change registration</a>
</asp:Content>