<%@ Page Title="Contact Organizer" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="PRG3204_Project_Group4.Enquiries.Contact" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="h3 mt-3 mb-1">Contact Organizer</h2>
    <p class="text-muted small">Choose an event (its organizer will receive your message) or pick an organizer for a general enquiry.</p>

    <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" />

    <div class="col-md-8">
        <div class="mb-2">
            <label for="<%= ddlEvent.ClientID %>" class="form-label">Event (optional)</label>
            <asp:DropDownList ID="ddlEvent" runat="server" CssClass="form-select" />
        </div>
        <div class="mb-2">
            <label for="<%= ddlOrganizer.ClientID %>" class="form-label">Organizer (needed if no event is chosen)</label>
            <asp:DropDownList ID="ddlOrganizer" runat="server" CssClass="form-select" />
        </div>
        <div class="mb-2">
            <label for="<%= txtSubject.ClientID %>" class="form-label">Subject *</label>
            <asp:TextBox ID="txtSubject" runat="server" CssClass="form-control" MaxLength="150" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtSubject" Display="Dynamic"
                CssClass="text-danger small" ErrorMessage="Subject is required." />
        </div>
        <div class="mb-2">
            <label for="<%= txtMessage.ClientID %>" class="form-label">Message *</label>
            <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" Rows="5" CssClass="form-control" MaxLength="1000" />
            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtMessage" Display="Dynamic"
                CssClass="text-danger small" ErrorMessage="Message is required." />
            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtMessage" Display="Dynamic"
                CssClass="text-danger small" ValidationExpression="^[\s\S]{0,1000}$"
                ErrorMessage="Message must be 1000 characters or fewer." />
        </div>
        <asp:Button ID="btnSend" runat="server" Text="Send enquiry" CssClass="btn btn-primary" OnClick="btnSend_Click" />
    </div>

    <h3 class="h5 mt-4">Your enquiries</h3>
    <asp:GridView ID="gvEnquiries" runat="server" AutoGenerateColumns="false"
        CssClass="table table-sm table-striped" GridLines="None" EmptyDataText="You have not sent any enquiries.">
        <Columns>
            <asp:BoundField DataField="SubmittedMyt" HeaderText="Sent" DataFormatString="{0:dd MMM yyyy, HH:mm}" />
            <asp:BoundField DataField="Subject" HeaderText="Subject" />
            <asp:BoundField DataField="Status" HeaderText="Status" />
            <asp:BoundField DataField="OrganizerReply" HeaderText="Reply" NullDisplayText="-" />
        </Columns>
    </asp:GridView>
</asp:Content>