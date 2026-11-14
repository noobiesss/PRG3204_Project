<%@ Page Title="Change Registration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Modify.aspx.cs" Inherits="PRG3204_Project_Group4.Registrations.Modify" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="h3 mt-3 mb-1">Cancel or Change Registration</h2>
    <p class="text-muted small">All times are displayed in Malaysia Time (UTC+8). Only events that have not started can be changed.</p>

    <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" />

    <asp:GridView ID="gvRegistrations" runat="server" AutoGenerateColumns="false"
        DataKeyNames="RegistrationId" CssClass="table table-sm table-striped" GridLines="None"
        EmptyDataText="You have no active registrations that can be changed."
        OnRowCommand="gvRegistrations_RowCommand">
        <Columns>
            <asp:BoundField DataField="Title" HeaderText="Event" />
            <asp:BoundField DataField="Location" HeaderText="Location" />
            <asp:BoundField DataField="StartMyt" HeaderText="Starts" DataFormatString="{0:dd MMM yyyy, HH:mm}" />
            <asp:BoundField DataField="EndMyt" HeaderText="Ends" DataFormatString="{0:dd MMM yyyy, HH:mm}" />
            <asp:TemplateField HeaderText="Actions">
                <ItemTemplate>
                    <asp:LinkButton runat="server" CommandName="ChangeReg" CausesValidation="false"
                        CommandArgument='<%# Eval("RegistrationId") %>' CssClass="btn btn-sm btn-outline-primary">Change event</asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="CancelReg" CausesValidation="false"
                        CommandArgument='<%# Eval("RegistrationId") %>' CssClass="btn btn-sm btn-outline-danger"
                        OnClientClick="return confirm('Cancel this registration?');">Cancel</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <asp:Panel ID="pnlChange" runat="server" Visible="false" CssClass="card card-body mt-3">
        <h3 class="h5">Move registration to another event</h3>
        <div class="mb-2">
            <label for="<%= ddlNewEvent.ClientID %>" class="form-label">New event</label>
            <asp:DropDownList ID="ddlNewEvent" runat="server" CssClass="form-select" />
        </div>
        <div class="mb-2">
            <label for="<%= txtReason.ClientID %>" class="form-label">Reason (optional)</label>
            <asp:TextBox ID="txtReason" runat="server" ValidateRequestMode="Disabled" CssClass="form-control" MaxLength="500" />
        </div>
        <div>
            <asp:Button ID="btnConfirmChange" runat="server" Text="Confirm change" CssClass="btn btn-primary"
                CausesValidation="false" OnClick="btnConfirmChange_Click" />
            <asp:Button ID="btnCloseChange" runat="server" Text="Close" CssClass="btn btn-outline-secondary"
                CausesValidation="false" OnClick="btnCloseChange_Click" />
        </div>
    </asp:Panel>

    <div class="mt-3">
        <a href="<%= ResolveUrl("~/Registrations/Logs.aspx") %>" class="btn btn-outline-secondary btn-sm">View registration logs</a>
    </div>
</asp:Content>