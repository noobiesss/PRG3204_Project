<%@ Page Title="Search Events" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="PRG3204_Project_Group4.Events.Search" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="h3 mt-3 mb-1">Search Events</h2>
    <p class="text-muted small">Filter upcoming events by date, category or organizer. Times are in Malaysia Time (UTC+8).</p>

    <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" CssClass="alert alert-warning d-block" />

    <div class="row g-2 mb-3">
        <div class="col-md-3">
            <label for="<%= txtDate.ClientID %>" class="form-label">Date</label>
            <asp:TextBox ID="txtDate" runat="server" TextMode="Date" CssClass="form-control" />
        </div>
        <div class="col-md-3">
            <label for="<%= ddlCategory.ClientID %>" class="form-label">Category</label>
            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
        </div>
        <div class="col-md-3">
            <label for="<%= ddlOrganizer.ClientID %>" class="form-label">Organizer</label>
            <asp:DropDownList ID="ddlOrganizer" runat="server" CssClass="form-select" />
        </div>
        <div class="col-md-3 d-flex align-items-end">
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary me-2" OnClick="btnSearch_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-secondary" OnClick="btnClear_Click" />
        </div>
    </div>

    <asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="false"
        CssClass="table table-sm table-striped" GridLines="None"
        EmptyDataText="No events match your search.">
        <Columns>
            <asp:BoundField DataField="Title" HeaderText="Event" />
            <asp:BoundField DataField="CategoryName" HeaderText="Category" />
            <asp:BoundField DataField="OrganizerName" HeaderText="Organizer" />
            <asp:BoundField DataField="Location" HeaderText="Location" />
            <asp:BoundField DataField="StartMyt" HeaderText="Starts" DataFormatString="{0:dd MMM yyyy, HH:mm}" />
            <asp:BoundField DataField="SeatsLeft" HeaderText="Seats left" />
            <asp:TemplateField HeaderText="">
                <ItemTemplate>
                    <asp:HyperLink runat="server" CssClass="btn btn-sm btn-primary"
                        NavigateUrl='<%# "~/Registrations/Create.aspx?eventId=" + Eval("EventId") %>'
                        Visible='<%# IsStudent && Convert.ToInt32(Eval("SeatsLeft")) > 0 %>'>Register</asp:HyperLink>
                    <asp:Label runat="server" CssClass="badge bg-secondary" Text="Full"
                        Visible='<%# Convert.ToInt32(Eval("SeatsLeft")) <= 0 %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Content>