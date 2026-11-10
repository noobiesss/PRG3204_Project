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
                            MaxLength="200"
                            CssClass="form-control">
                        </asp:TextBox>

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

                    </div>

                    <!-- Description -->
                    <div class="col-12">

                        <label class="form-label">
                            Description
                        </label>

                        <asp:TextBox ID="txtDescription"
                            runat="server"
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
                            MaxLength="200"
                            CssClass="form-control">
                        </asp:TextBox>

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

                    </div>

                </div>

                <!-- Form Buttons -->
                <div class="mt-3">

                    <asp:Button ID="btnSave"
                        runat="server"
                        CssClass="btn btn-primary"
                        Text="Create Event"
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

</asp:Content>