<%@ Page Title="Submit Event Feedback" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" ValidateRequest="false" CodeBehind="Feedback.aspx.cs" Inherits="PRG3204_Project_Group4.Registrations.Feedback" %>
<asp:Content ID="FeedbackHead" ContentPlaceHolderID="head" runat="server">
    <style>.feedback-card{max-width:820px;margin:2rem auto;padding:1.5rem;border:1px solid #dee2e6;border-radius:12px;background:#fff}</style>
</asp:Content>
<asp:Content ID="FeedbackBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="feedback-card">
        <h1 class="h2">Submit Event Feedback</h1>
        <p class="text-muted">Share your experience after an event has ended. One evaluation per registration. Event dates are displayed in Malaysia Time (UTC+8).</p>
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />
        <asp:Label ID="lblNoEvents" runat="server" CssClass="alert alert-info d-block" Visible="false" Text="You have no completed events available for evaluation." />
        <asp:Panel ID="pnlForm" runat="server">
            <div class="mb-3">
                <label class="form-label" for="<%= ddlRegistration.ClientID %>">Completed event *</label>
                <asp:DropDownList ID="ddlRegistration" runat="server" CssClass="form-select" />
            </div>
            <div class="mb-3">
                <label class="form-label" for="<%= ddlRating.ClientID %>">Rating *</label>
                <asp:DropDownList ID="ddlRating" runat="server" CssClass="form-select">
                    <asp:ListItem Value="">-- Select a rating --</asp:ListItem>
                    <asp:ListItem Value="1">1 - Very poor</asp:ListItem>
                    <asp:ListItem Value="2">2 - Poor</asp:ListItem>
                    <asp:ListItem Value="3">3 - Average</asp:ListItem>
                    <asp:ListItem Value="4">4 - Good</asp:ListItem>
                    <asp:ListItem Value="5">5 - Excellent</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="mb-3">
                <label class="form-label" for="<%= txtComments.ClientID %>">Comments (optional)</label>
                <asp:TextBox ID="txtComments" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" MaxLength="1000" />
                <div class="form-text">Plain text only (no HTML tags). Maximum 1,000 characters.</div>
            </div>
            <div class="mb-3">
                <label class="form-label" for="<%= ddlRecommend.ClientID %>">Would you recommend similar events?</label>
                <asp:DropDownList ID="ddlRecommend" runat="server" CssClass="form-select">
                    <asp:ListItem Value="">Prefer not to say</asp:ListItem>
                    <asp:ListItem Value="1">Yes</asp:ListItem>
                    <asp:ListItem Value="0">No</asp:ListItem>
                </asp:DropDownList>
            </div>
            <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" Text="Submit evaluation" CausesValidation="false" OnClick="SubmitButton_Click" />
        </asp:Panel>
    </div>
</asp:Content>
