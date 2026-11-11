<%@ Page Title="Manage Events" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Manage.aspx.cs" Inherits="PRG3204_Project_Group4.Events.Manage" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="head"
    runat="server">
</asp:Content>

<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <div class="container py-3">

        <!-- Page Heading -->
        <h2>Manage Events</h2>

        <p class="text-muted">
            Create, edit or cancel campus events.
            Times are Malaysia Time (UTC+8).
        </p>

        <!-- Success / Error Message -->
        <asp:Label ID="lblMessage"
            runat="server"
            CssClass="d-block mb-3">
        </asp:Label>

        <!-- Create / Update Event Form -->
        <div class="card mb-4">

            <div class="card-header">
                <strong>
                    <asp:Literal ID="litFormHeading"
                        runat="server"
                        Text="Create Event">
                    </asp:Literal>
                </strong>
            </div>

            <div class="card-body">

                <asp:HiddenField ID="hidEventId"
                    runat="server" />

                <div class="row g-3">

                    <!-- Event Title -->
                    <div class="col-md-8">

                        <label class="form-label">
                            Title *
                        </label>

                        <asp:TextBox ID="txtTitle"
                            runat="server"
                            ValidateRequestMode="Disabled"
                            MaxLength="200"
                            CssClass="form-control">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvtxtTitle"
                            runat="server" ControlToValidate="txtTitle"
                            ValidationGroup="EventForm"
                            ErrorMessage="Title is required."
                            CssClass="text-danger small" Display="Dynamic" EnableClientScript="true" />

                    </div>

                    <!-- Category -->
                    <div class="col-md-4">

                        <label class="form-label">
                            Category *
                        </label>

                        <asp:DropDownList ID="ddlCategory"
                            runat="server"
                            CssClass="form-select">
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvddlCategory"
                            runat="server" ControlToValidate="ddlCategory"
                            ValidationGroup="EventForm" InitialValue=""
                            ErrorMessage="Category is required."
                            CssClass="text-danger small" Display="Dynamic" EnableClientScript="true" />

                    </div>

                    <!-- Description -->
                    <div class="col-12">

                        <label class="form-label">
                            Description
                        </label>

                        <asp:TextBox ID="txtDescription"
                            runat="server"
                            ValidateRequestMode="Disabled"
                            TextMode="MultiLine"
                            Rows="3"
                            CssClass="form-control">
                        </asp:TextBox>

                    </div>

                    <!-- Location -->
                    <div class="col-md-6">

                        <label class="form-label">
                            Location *
                        </label>

                        <asp:TextBox ID="txtLocation"
                            runat="server"
                            ValidateRequestMode="Disabled"
                            MaxLength="200"
                            CssClass="form-control">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvtxtLocation"
                            runat="server" ControlToValidate="txtLocation"
                            ValidationGroup="EventForm"
                            ErrorMessage="Location is required."
                            CssClass="text-danger small" Display="Dynamic" EnableClientScript="true" />

                    </div>

                    <!-- Capacity -->
                    <div class="col-md-6">

                        <label class="form-label">
                            Capacity *
                        </label>

                        <asp:TextBox ID="txtCapacity"
                            runat="server"
                            TextMode="Number"
                            CssClass="form-control">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvtxtCapacity"
                            runat="server" ControlToValidate="txtCapacity"
                            ValidationGroup="EventForm"
                            ErrorMessage="Capacity is required."
                            CssClass="text-danger small" Display="Dynamic" EnableClientScript="true" />
                        <asp:RangeValidator ID="rvCapacity" runat="server"
                            ControlToValidate="txtCapacity" ValidationGroup="EventForm"
                            Type="Integer" MinimumValue="1" MaximumValue="10000"
                            ErrorMessage="Capacity must be between 1 and 10000."
                            CssClass="text-danger small" Display="Dynamic" EnableClientScript="true" />

                    </div>

                    <!-- Start Date & Time -->
                    <div class="col-md-6">

                        <label class="form-label">
                            Start (Malaysia time) *
                        </label>

                        <asp:TextBox ID="txtStartDate"
                            runat="server"
                            TextMode="DateTimeLocal"
                            CssClass="form-control">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvtxtStartDate"
                            runat="server" ControlToValidate="txtStartDate"
                            ValidationGroup="EventForm"
                            ErrorMessage="Start date/time is required."
                            CssClass="text-danger small" Display="Dynamic" EnableClientScript="true" />

                    </div>

                    <!-- End Date & Time -->
                    <div class="col-md-6">

                        <label class="form-label">
                            End (Malaysia time) *
                        </label>

                        <asp:TextBox ID="txtEndDate"
                            runat="server"
                            TextMode="DateTimeLocal"
                            CssClass="form-control">
                        </asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvtxtEndDate"
                            runat="server" ControlToValidate="txtEndDate"
                            ValidationGroup="EventForm"
                            ErrorMessage="End date/time is required."
                            CssClass="text-danger small" Display="Dynamic" EnableClientScript="true" />

                    </div>

                </div>

                <!-- Form Buttons -->
                <div class="mt-3">

                    <asp:Button ID="btnSave"
                        runat="server"
                        CssClass="btn btn-primary"
                        Text="Create Event"
                        ValidationGroup="EventForm"
                        OnClick="btnSave_Click" />

                    <asp:Button ID="btnClear"
                        runat="server"
                        CssClass="btn btn-outline-secondary ms-2"
                        Text="Clear / New"
                        CausesValidation="false"
                        OnClick="btnClear_Click" />

                </div>

            </div>

        </div>

        <!-- Upload Event Resources -->
        <asp:Panel ID="pnlUpload"
            runat="server"
            Visible="false"
            CssClass="card mb-4">

            <div class="card-header">

                <strong>
                    Upload Resource for Selected Event
                </strong>

            </div>

            <div class="card-body">

                <p class="text-muted small">
                    PDF, JPG or PNG, maximum 3 MB.
                    Save an event first, then select Edit
                    to upload resources.
                </p>

                <div class="d-flex flex-wrap gap-2 align-items-center">

                    <asp:FileUpload ID="fuResource"
                        runat="server"
                        CssClass="form-control"
                        style="max-width:450px" />

                    <asp:Button ID="btnUpload"
                        runat="server"
                        Text="Upload File"
                        CausesValidation="false"
                        CssClass="btn btn-outline-primary"
                        OnClick="btnUpload_Click" />

                </div>

                <!-- Uploaded Resource List -->
                <asp:Repeater ID="rptResources"
                    runat="server">

                    <HeaderTemplate>
                        <ul class="list-group mt-3">
                    </HeaderTemplate>

                    <ItemTemplate>

                        <li class="list-group-item d-flex justify-content-between">

                            <a href='<%# ResolveUrl(Convert.ToString(Eval("FilePath"))) %>'
                               target="_blank"
                               rel="noopener noreferrer">

                                <%#: Eval("FileName") %>

                            </a>

                            <span class="text-muted small">
                                <%#: Eval("FileType") %>
                            </span>

                        </li>

                    </ItemTemplate>

                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>

                </asp:Repeater>

            </div>

        </asp:Panel>

        <!-- Event Listing -->
        <h4>Your Events</h4>

        <div class="table-responsive">

            <asp:GridView ID="gvEvents"
                runat="server"
                AutoGenerateColumns="false"
                CssClass="table table-striped table-bordered align-middle"
                GridLines="None"
                EmptyDataText="No events created yet."
                DataKeyNames="EventId"
                OnRowCommand="gvEvents_RowCommand">

               <Columns>

    <asp:BoundField
        DataField="Title"
        HeaderText="Title" />

    <asp:BoundField
        DataField="CategoryName"
        HeaderText="Category" />

    <asp:BoundField
        DataField="StartMY"
        HeaderText="Start (MYT)"
        DataFormatString="{0:dd MMM yyyy HH:mm}"
        HtmlEncode="false" />

    <asp:BoundField
        DataField="Capacity"
        HeaderText="Capacity" />

    <asp:BoundField
        DataField="RegisteredCount"
        HeaderText="Registered" />

    <asp:BoundField
        DataField="Status"
        HeaderText="Status" />

    <asp:TemplateField HeaderText="Actions">

        <ItemTemplate>

            <asp:LinkButton
                runat="server"
                CssClass="btn btn-sm btn-outline-primary me-1"
                Text="Edit / Upload"
                CommandName="EditEvent"
                CommandArgument='<%# Eval("EventId") %>'
                CausesValidation="false" />

            <asp:LinkButton
                runat="server"
                CssClass="btn btn-sm btn-outline-danger"
                Text="Cancel"
                CommandName="CancelEvent"
                CommandArgument='<%# Eval("EventId") %>'
                Visible='<%# Convert.ToString(Eval("Status")) != "Cancelled" %>'
                OnClientClick="return confirm('Cancel this event? Existing registrations will remain in history.');"
                CausesValidation="false" />

        </ItemTemplate>

    </asp:TemplateField>

</Columns>

            </asp:GridView>

        </div>

    </div>


    <!-- Validate only after the user leaves a field, or when Save is clicked. -->
    <script type="text/javascript">
        document.addEventListener("DOMContentLoaded", function () {
            var fieldIds = [
                "<%= txtTitle.ClientID %>",
                "<%= ddlCategory.ClientID %>",
                "<%= txtLocation.ClientID %>",
                "<%= txtCapacity.ClientID %>",
                "<%= txtStartDate.ClientID %>",
                "<%= txtEndDate.ClientID %>"
            ];

            function validateField(element) {
                if (typeof Page_Validators === "undefined" ||
                    typeof ValidatorValidate !== "function") {
                    return;
                }

                for (var i = 0; i < Page_Validators.length; i++) {
                    var validator = Page_Validators[i];
                    if (validator.controltovalidate === element.id &&
                        validator.validationGroup === "EventForm") {
                        ValidatorValidate(validator);
                    }
                }

                if (typeof ValidatorUpdateIsValid === "function") {
                    ValidatorUpdateIsValid();
                }
            }

            fieldIds.forEach(function (fieldId) {
                var field = document.getElementById(fieldId);
                if (!field) return;

                var touched = false;
                field.addEventListener("blur", function () {
                    touched = true;
                    validateField(field);
                });
                field.addEventListener("input", function () {
                    if (touched) validateField(field);
                });
                field.addEventListener("change", function () {
                    if (touched) validateField(field);
                });
            });
        });
    </script>

</asp:Content>
