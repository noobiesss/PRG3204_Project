
<%@ Page Title="Upcoming Events Calendar" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Calendar.aspx.cs" Inherits="PRG3204_Project_Group4.Events.Calendar" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="head"
    runat="server">
</asp:Content>

<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <div class="container py-3">

        <h2>Upcoming Events Calendar</h2>

        <p class="text-muted">
            Select a day to see its upcoming events.
            Dates and times are Malaysia Time (UTC+8).
        </p>

        <!-- Event Calendar -->
        <asp:Calendar
            ID="calEvents"
            runat="server"
            CssClass="table table-bordered text-center"
            Width="100%"
            NextPrevFormat="ShortMonth"
            OnDayRender="calEvents_DayRender"
            OnVisibleMonthChanged="calEvents_VisibleMonthChanged"
            OnSelectionChanged="calEvents_SelectionChanged">
        </asp:Calendar>

        <!-- Selected Date -->
        <h4 class="mt-3">
            <asp:Literal
                ID="litSelectedDay"
                runat="server">
            </asp:Literal>
        </h4>

        <!-- Events on Selected Date -->
        <asp:Repeater ID="rptDayEvents" runat="server">

            <ItemTemplate>

                <div class="card mb-2">

                    <div class="card-body">

                        <!-- Event Title -->
                        <strong>
                            <%#: Eval("Title") %>
                        </strong>

                        <!-- Event Time -->
                        <span class="text-muted">

                            (
                            <%# Eval("StartMY", "{0:HH:mm}") %>
                            –
                            <%# Eval("EndMY", "{0:HH:mm}") %>
                            MYT
                            )

                        </span>

                        <!-- Event Location -->
                        <div class="mt-2">
                            <%#: Eval("Location") %>
                        </div>

                        <!-- Registration Link -->
                        <div class="mt-2">

                            <a href='<%# ResolveUrl("~/Registrations/Create.aspx?eventId=" + Eval("EventId")) %>'
                               class="btn btn-primary btn-sm">

                                Register

                            </a>

                        </div>

                    </div>

                </div>

            </ItemTemplate>

        </asp:Repeater>

        <!-- Empty Event Message -->
        <asp:Label
            ID="lblEmptyDay"
            runat="server"
            Text="No upcoming events on this date."
            CssClass="text-muted">
        </asp:Label>

    </div>

</asp:Content>
