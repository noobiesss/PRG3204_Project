<%@ Page Title="Events" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PRG3204_Project_Group4.Events.Default" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="head"
    runat="server">
</asp:Content>

<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <div class="container py-3">

        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h2>Upcoming Campus Events</h2>
                <p class="text-muted">
                    All times displayed in Malaysia Time (UTC+8).
                </p>
            </div>
        </div>

        <asp:Repeater ID="rptEvents"
            runat="server"
            OnItemDataBound="rptEvents_ItemDataBound">

            <ItemTemplate>

                <div class="card shadow-sm mb-3">

                    <div class="card-body">

                        <div class="d-flex justify-content-between flex-wrap">

                            <h5 class="card-title">
                                <%#: Eval("Title") %>
                            </h5>

                            <span class="badge bg-primary">
                                <%#: Eval("CategoryName") %>
                            </span>

                        </div>

                        <p class="text-muted mb-2">

                            <%#: Eval("Location") %>

                            <span class="mx-2">•</span>

                            <%#: Eval("StartMY", "{0:dd MMM yyyy HH:mm}") %>

                            -

                            <%#: Eval("EndMY", "{0:dd MMM yyyy HH:mm}") %>

                            (MYT)

                        </p>

                        <p>
                            <%#: Eval("Description") %>
                        </p>

                        <p>
                            Available seats:

                            <%#: Eval("SeatsRemaining") %>

                            /

                            <%#: Eval("Capacity") %>
                        </p>

                        <asp:HyperLink
                            runat="server"
                            CssClass="btn btn-primary"
                            Text="Register"
                            NavigateUrl='<%# ResolveUrl("~/Registrations/Create.aspx?eventId=" + Eval("EventId")) %>'
                            Visible='<%# Convert.ToInt32(Eval("SeatsRemaining")) > 0 %>'>
                        </asp:HyperLink>

                        <asp:Literal
                            runat="server"
                            Text='<span class="badge bg-secondary">Full</span>'
                            Visible='<%# Convert.ToInt32(Eval("SeatsRemaining")) <= 0 %>'>
                        </asp:Literal>

                        <asp:Repeater ID="rptFiles"
                            runat="server">

                            <HeaderTemplate>
                                <div class="mt-2">
                                    <strong>Resources:</strong>
                                </div>
                            </HeaderTemplate>

                            <ItemTemplate>

                                <a class="me-2"
                                   target="_blank"
                                   rel="noopener noreferrer"
                                   href='<%# ResolveUrl(Convert.ToString(Eval("FilePath"))) %>'>

                                    <%#: Eval("FileName") %>

                                </a>

                            </ItemTemplate>

                        </asp:Repeater>

                    </div>

                </div>

            </ItemTemplate>

        </asp:Repeater>

        <asp:Label ID="lblEmpty"
            runat="server"
            Text="No upcoming events available."
            Visible="false"
            CssClass="alert alert-info d-block">
        </asp:Label>

    </div>

</asp:Content>