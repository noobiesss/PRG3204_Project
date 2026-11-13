<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="PRG3204_Project_Group4.Account.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
    <div class="col-md-8 col-lg-6 clearable-form">
        <h1 class="mb-3">Student Registration</h1>
        <asp:Label ID="lblMessage" runat="server" Visible="false" role="alert" />
        <div class="mb-3">
            <label class="form-label" for="<%= txtMatricNo.ClientID %>">Matric No.</label>
            <asp:TextBox ID="txtMatricNo" runat="server" CssClass="form-control" MaxLength="20" />
            <asp:RequiredFieldValidator ID="rfvMatricNo" runat="server" ControlToValidate="txtMatricNo" ErrorMessage="Matric number is required." Display="Dynamic" CssClass="text-danger small" />
            <asp:RegularExpressionValidator ID="revMatricNo" runat="server" ControlToValidate="txtMatricNo" ErrorMessage="Use up to 20 letters, numbers, / or -." Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[A-Za-z0-9/\-]{1,20}$" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtFullName.ClientID %>">Full Name</label>
            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" MaxLength="100" autocomplete="name" />
            <asp:RequiredFieldValidator ID="rfvFullName" runat="server" ControlToValidate="txtFullName" ErrorMessage="Full name is required." Display="Dynamic" CssClass="text-danger small" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtEmail.ClientID %>">School Email</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="100" autocomplete="username" pattern="[^@\s]+@[^@\s]+\.edu\.my" title="Use your school email, e.g. name@school.edu.my" />
            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="School email is required." Display="Dynamic" CssClass="text-danger small" />
            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Enter your school email, e.g. name@school.edu.my." Display="Dynamic" CssClass="text-danger small" ValidationExpression="^(?=.{1,100}$)[^@\s]+@[^@\s]+\.edu\.my$" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtPassword.ClientID %>">Password</label>
            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" ValidateRequestMode="Disabled" MaxLength="128" autocomplete="new-password" />
            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Password is required." Display="Dynamic" CssClass="text-danger small" />
            <asp:RegularExpressionValidator ID="revPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Use 8 to 128 characters with at least one letter and one number." Display="Dynamic" CssClass="text-danger small" ValidationExpression="^(?=.*[A-Za-z])(?=.*\d).{8,128}$" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtConfirmPassword.ClientID %>">Confirm Password</label>
            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" ValidateRequestMode="Disabled" MaxLength="128" autocomplete="new-password" />
            <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ErrorMessage="Please confirm your password." Display="Dynamic" CssClass="text-danger small" />
            <asp:CompareValidator ID="cvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ErrorMessage="Passwords do not match." Display="Dynamic" CssClass="text-danger small" ControlToCompare="txtPassword" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtPersonalEmail.ClientID %>">Personal Email (optional)</label>
            <asp:TextBox ID="txtPersonalEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="100" autocomplete="email" />
            <asp:RegularExpressionValidator ID="revPersonalEmail" runat="server" ControlToValidate="txtPersonalEmail" ErrorMessage="Enter a valid email address." Display="Dynamic" CssClass="text-danger small" ValidationExpression="^(?=.{1,100}$)[^@\s]+@[^@\s]+\.[^@\s]+$" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= ddlGender.ClientID %>">Gender</label>
            <asp:DropDownList ID="ddlGender" runat="server" CssClass="form-select">
                <asp:ListItem Value="" Text="Select gender" />
                <asp:ListItem Value="Male" Text="Male" />
                <asp:ListItem Value="Female" Text="Female" />
            </asp:DropDownList>
            <asp:RequiredFieldValidator ID="rfvGender" runat="server" ControlToValidate="ddlGender" ErrorMessage="Gender is required." Display="Dynamic" CssClass="text-danger small" />
        </div>
        <div class="mb-3">
            <label class="form-label" for="<%= txtDateOfBirth.ClientID %>">Date of Birth</label>
            <asp:TextBox ID="txtDateOfBirth" runat="server" CssClass="form-control" TextMode="Date" />
            <asp:RequiredFieldValidator ID="rfvDateOfBirth" runat="server" ControlToValidate="txtDateOfBirth" ErrorMessage="Date of birth is required." Display="Dynamic" CssClass="text-danger small" />
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
        <asp:Button ID="btnRegister" runat="server" Text="Create Account" CssClass="btn btn-primary" OnClick="btnRegister_Click" />
        <p class="mt-3 mb-0">Already registered? <a href="<%: ResolveUrl("~/Account/Login.aspx") %>">Login</a></p>
    </div>
    </div>
</asp:Content>
