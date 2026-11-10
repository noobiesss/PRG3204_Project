<%@ Page Title="Profile" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="PRG3204_Project_Group4.Account.Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
    <div class="col-md-8 col-lg-6">
        <h1 class="mb-3">My Profile</h1>
        <asp:Label ID="lblMessage" runat="server" Visible="false" role="alert" />
        <dl class="row">
            <dt class="col-sm-4">Matric / Staff No.</dt><dd class="col-sm-8"><asp:Literal ID="litMatricNo" runat="server" Mode="Encode" /></dd>
            <dt class="col-sm-4">School Email</dt><dd class="col-sm-8"><asp:Literal ID="litSchoolEmail" runat="server" Mode="Encode" /></dd>
            <dt class="col-sm-4">Role</dt><dd class="col-sm-8"><asp:Literal ID="litRole" runat="server" Mode="Encode" /></dd>
        </dl>
        <div class="mb-3">
            <label class="form-label" for="<%= txtFullName.ClientID %>">Full Name</label>
            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" MaxLength="100" autocomplete="name" />
            <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName" ErrorMessage="Full name is required." Display="Dynamic" CssClass="text-danger small" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtPersonalEmail.ClientID %>">Personal Email (optional)</label>
            <asp:TextBox ID="txtPersonalEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="100" autocomplete="email" />
            <asp:RegularExpressionValidator ID="revPersonalEmail" runat="server" ControlToValidate="txtPersonalEmail" ErrorMessage="Enter a valid email address." Display="Dynamic" CssClass="text-danger small" ValidationExpression="^(?=.{1,100}$)[^@\s]+@[^@\s]+\.[^@\s]+$" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= ddlGender.ClientID %>">Gender (optional)</label>
            <asp:DropDownList ID="ddlGender" runat="server" CssClass="form-select">
                <asp:ListItem Value="" Text="Prefer not to say" />
                <asp:ListItem Value="Male" Text="Male" />
                <asp:ListItem Value="Female" Text="Female" />
            </asp:DropDownList>
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtDateOfBirth.ClientID %>">Date of Birth (optional)</label>
            <asp:TextBox ID="txtDateOfBirth" runat="server" CssClass="form-control" TextMode="Date" />
            <asp:CustomValidator ID="cvDateOfBirth" runat="server" ControlToValidate="txtDateOfBirth" ErrorMessage="Enter a valid date of birth in the past." Display="Dynamic" CssClass="text-danger small" OnServerValidate="cvDateOfBirth_ServerValidate" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtPhone.ClientID %>">Contact Number (optional)</label>
            <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control" TextMode="Phone" MaxLength="20" autocomplete="tel" />
            <asp:RegularExpressionValidator ID="revPhone" runat="server" ControlToValidate="txtPhone" ErrorMessage="Enter a valid contact number." Display="Dynamic" CssClass="text-danger small" ValidationExpression="^\+?[0-9][0-9 \-]{6,19}$" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtFaculty.ClientID %>">Faculty / Department (optional)</label>
            <asp:TextBox ID="txtFaculty" runat="server" CssClass="form-control" MaxLength="100" />
        </div>
        <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-primary" OnClick="btnSave_Click" />
    </div>
    </div>
</asp:Content>
