<%@ Page Title="Mark Attendance" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MarkAttendance.aspx.cs" Inherits="PRG3204_Project_Group4.Reports.MarkAttendance" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <style>
        .attendance-card { background:#fff; border:1px solid #dee2e6; border-radius:12px; padding:1.5rem; }
        .attendance-meta { color:#6c757d; font-size:.925rem; }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="h2 mt-3">Mark Attendance</h1>
    <p class="text-muted">Select an event that has already started, tick students who attended, and save your changes. Times shown are Malaysia Time (UTC+8).</p>
    <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />
    <div class="attendance-card mb-4">
        <label for="<%= ddlEvents.ClientID %>" class="form-label fw-semibold">Select event</label>
        <asp:DropDownList ID="ddlEvents" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="Events_SelectedIndexChanged" />
        <asp:Label ID="lblSummary" runat="server" CssClass="attendance-meta d-block mt-3" />
    </div>
    <asp:Panel ID="pnlRegistrations" runat="server" Visible="false" CssClass="attendance-card mb-4">
        <h2 class="h5">Registered students</h2>
        <p class="attendance-meta">Tick = Attended; untick = Not attended. Cancelled registrations are excluded.</p>
        <div class="table-responsive">
            <asp:GridView ID="gvStudents" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="table table-hover table-bordered align-middle" DataKeyNames="RegistrationId" EmptyDataText="No registered students for this event.">
                <Columns>
                    <asp:BoundField DataField="MatricOrStaffNo" HeaderText="Matric / Staff No." HtmlEncode="true" />
                    <asp:BoundField DataField="FullName" HeaderText="Student name" HtmlEncode="true" />
                    <asp:TemplateField HeaderText="Attended">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkAttended" runat="server" Checked='<%# Convert.ToBoolean(Eval("IsAttended")) %>' ToolTip="Mark this student as attended" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
        <asp:Button ID="btnSave" runat="server" Text="Save attendance" CssClass="btn btn-primary" CausesValidation="false" OnClick="SaveButton_Click" />
    </asp:Panel>
</asp:Content>
