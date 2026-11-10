<%@ Page Title="Event Feedback" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Feedback.aspx.cs" Inherits="PRG3204_Project_Group4.Reports.Feedback" %>
<asp:Content ID="FeedbackHead" ContentPlaceHolderID="head" runat="server">
    <link href="<%= ResolveUrl("~/Styles/M4Reports.css") %>" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.6.1/dist/chart.umd.min.js"></script>
</asp:Content>
<asp:Content ID="FeedbackBody" ContentPlaceHolderID="MainContent" runat="server">
<asp:Panel ID="pnlManager" runat="server">
<div class="m4-report">
    <div class="m4-eyebrow">M4 / Evaluation</div>
    <h1 class="m4-title h2 mt-1">Event feedback and evaluation</h1>
    <p class="m4-subtitle">Review participant ratings, recommendation rates and feedback comments to improve future campus events. Submission times are shown in Malaysia Time (UTC+8).</p>
    <div class="m4-panel">
        <div class="row g-3 align-items-end">
            <div class="col-md-8"><label class="form-label">Choose an event</label><asp:DropDownList ID="ddlEvent" runat="server" CssClass="form-select" /></div>
            <div class="col-md-4"><asp:Button ID="btnFilter" runat="server" Text="View feedback" OnClick="ResultsButton_Click" CssClass="btn btn-primary w-100" /></div>
        </div>
    </div>
    <div class="row g-3 mb-3">
        <div class="col-md-4"><div class="m4-kpi"><div class="m4-kpi-label">Feedback responses</div><div class="m4-kpi-value"><asp:Label ID="lblResponses" runat="server" Text="0" /></div></div></div>
        <div class="col-md-4"><div class="m4-kpi"><div class="m4-kpi-label">Average rating / 5</div><div class="m4-kpi-value"><asp:Label ID="lblAverage" runat="server" Text="N/A" /></div></div></div>
        <div class="col-md-4"><div class="m4-kpi"><div class="m4-kpi-label">Would recommend</div><div class="m4-kpi-value"><asp:Label ID="lblRecommend" runat="server" Text="N/A" /></div><div class="m4-small">Of responses that answered</div></div></div>
    </div>
    <div class="row g-3 mb-3">
        <div class="col-lg-5"><div class="m4-panel h-100"><h2 class="h5">Rating distribution</h2><p class="m4-small">Number of reviews for each score</p><div class="chart-wrap"><canvas id="feedbackRatingsChart" aria-label="Rating distribution bar chart" role="img"></canvas></div></div></div>
        <div class="col-lg-7"><div class="m4-panel h-100"><h2 class="h5 mb-3">Ratings by event</h2><div class="table-responsive"><asp:GridView ID="gvEventRatings" runat="server" CssClass="table table-bordered table-hover" GridLines="None" AutoGenerateColumns="false" EmptyDataText="No reviews have been submitted.">
            <Columns><asp:BoundField DataField="Title" HeaderText="Event" HtmlEncode="true" /><asp:BoundField DataField="Responses" HeaderText="Responses" /><asp:BoundField DataField="AverageRating" HeaderText="Avg. rating" DataFormatString="{0:0.0}" /></Columns>
        </asp:GridView></div></div></div>
    </div>
    <div class="m4-panel"><h2 class="h5 mb-3">Participant comments</h2>
        <div class="table-responsive"><asp:GridView ID="gvComments" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="table table-bordered table-hover" EmptyDataText="No written feedback for this selection." AllowPaging="true" PageSize="10" OnPageIndexChanging="CommentsGrid_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="Title" HeaderText="Event" HtmlEncode="true" />
                <asp:BoundField DataField="Rating" HeaderText="Rating / 5" />
                <asp:BoundField DataField="Comments" HeaderText="Comments" HtmlEncode="true" />
                <asp:BoundField DataField="Recommendation" HeaderText="Recommend" HtmlEncode="true" />
                <asp:BoundField DataField="SubmittedMalaysia" HeaderText="Submitted (MYT)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
            </Columns>
        </asp:GridView></div>
    </div>
    <asp:HiddenField ID="hidRatings" runat="server" />
    <div id="m4FeedbackChartUnavailable" class="alert alert-warning d-none">Charts could not load. Please check the Chart.js connection.</div>
</div>
</asp:Panel>
<asp:Panel ID="pnlStudent" runat="server">
<div class="m4-report" style="max-width:800px;margin:auto">
    <div class="m4-eyebrow">M4 / Attendee evaluation</div>
    <h1 class="m4-title h2 mt-1">Submit event feedback</h1>
    <p class="m4-subtitle">You can evaluate a completed, non-cancelled event after registering for it. Only one response is permitted per registration. Event times are displayed in Malaysia Time (UTC+8).</p>
    <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />
    <asp:Label ID="lblNoEvents" runat="server" CssClass="alert alert-info d-block" Visible="false" Text="No feedback forms are currently available. You must be registered for a completed event that you have not already evaluated." />
    <asp:Panel ID="pnlForm" runat="server" CssClass="m4-panel">
        <div class="mb-3"><label class="form-label">Completed event *</label>
            <asp:DropDownList ID="ddlRegistration" runat="server" CssClass="form-select" />
        </div>
        <div class="mb-3"><label class="form-label">Rating *</label>
            <asp:DropDownList ID="ddlRating" runat="server" CssClass="form-select">
                <asp:ListItem Value="">-- Select rating --</asp:ListItem>
                <asp:ListItem Value="1">1 - Very poor</asp:ListItem>
                <asp:ListItem Value="2">2 - Poor</asp:ListItem>
                <asp:ListItem Value="3">3 - Average</asp:ListItem>
                <asp:ListItem Value="4">4 - Good</asp:ListItem>
                <asp:ListItem Value="5">5 - Excellent</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="mb-3"><label class="form-label">Comments (optional)</label>
            <asp:TextBox ID="txtComments" runat="server" CssClass="form-control" TextMode="MultiLine" MaxLength="1000" Rows="5" placeholder="What went well? What could improve?" />
            <span class="form-text">Maximum 1,000 characters.</span>
        </div>
        <div class="mb-4"><label class="form-label">Would you recommend similar events?</label>
            <asp:DropDownList ID="ddlRecommend" runat="server" CssClass="form-select">
                <asp:ListItem Value="">Prefer not to say</asp:ListItem>
                <asp:ListItem Value="1">Yes</asp:ListItem>
                <asp:ListItem Value="0">No</asp:ListItem>
            </asp:DropDownList>
        </div>
        <asp:Button ID="btnSubmit" runat="server" Text="Submit evaluation" CssClass="btn btn-primary" OnClick="SubmitButton_Click" />
    </asp:Panel>
</div>
</asp:Panel>
<script>
(function () {
    if (!document.getElementById('feedbackRatingsChart')) return;
    if (typeof Chart === 'undefined') { document.getElementById('m4FeedbackChartUnavailable').classList.remove('d-none'); return; }
    var data = JSON.parse(document.getElementById('<%= hidRatings.ClientID %>').value || '[0,0,0,0,0]');
    new Chart(document.getElementById('feedbackRatingsChart'), {
        type: 'bar', data: { labels: ['1 star', '2 stars', '3 stars', '4 stars', '5 stars'],
            datasets: [{ label: 'Responses', data: data, backgroundColor: ['#fb7185','#f59e0b','#facc15','#60a5fa','#14b8a6'], borderRadius: 5 }] },
        options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { display: false } } }
    });
})();
</script>
</asp:Content>
