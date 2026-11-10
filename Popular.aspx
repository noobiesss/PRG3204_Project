<%@ Page Title="Popular Events" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Popular.aspx.cs" Inherits="PRG3204_Project_Group4.Reports.Popular" %>
<asp:Content ID="PopularHead" ContentPlaceHolderID="head" runat="server">
    <link href="~/Styles/M4Reports.css" rel="stylesheet" runat="server" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.6.1/dist/chart.umd.min.js"></script>
</asp:Content>
<asp:Content ID="PopularBody" ContentPlaceHolderID="MainContent" runat="server">
<div class="m4-report">
    <div class="m4-eyebrow">M4 / Analytics</div>
    <h1 class="m4-title h2 mt-1">Popular events analytics</h1>
    <p class="m4-subtitle">Explore event registrations and attendance. Event times are shown in Malaysia Time (UTC+8).</p>
    <div class="m4-panel">
        <div class="row g-3 align-items-end">
            <div class="col-md-5"><label for="<%= ddlCategory.ClientID %>" class="form-label">Category</label><asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" /></div>
            <div class="col-md-5"><label for="<%= ddlDays.ClientID %>" class="form-label">Event starting date</label>
                <asp:DropDownList ID="ddlDays" runat="server" CssClass="form-select">
                    <asp:ListItem Value="0">All time</asp:ListItem>
                    <asp:ListItem Value="30">Within the past 30 days or upcoming</asp:ListItem>
                    <asp:ListItem Value="90">Within the past 90 days or upcoming</asp:ListItem>
                    <asp:ListItem Value="365">Within the past year or upcoming</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-md-2"><asp:Button ID="btnApply" runat="server" Text="Update charts" CssClass="btn btn-primary w-100" OnClick="ApplyButton_Click" /></div>
        </div>
    </div>
    <div class="row g-3 mb-3">
        <div class="col-6 col-lg-3"><div class="m4-kpi"><div class="m4-kpi-label">Events analysed</div><div class="m4-kpi-value"><asp:Label ID="lblEvents" runat="server" Text="0" /></div></div></div>
        <div class="col-6 col-lg-3"><div class="m4-kpi"><div class="m4-kpi-label">Registrations</div><div class="m4-kpi-value"><asp:Label ID="lblRegistrations" runat="server" Text="0" /></div></div></div>
        <div class="col-6 col-lg-3"><div class="m4-kpi"><div class="m4-kpi-label">Attended</div><div class="m4-kpi-value"><asp:Label ID="lblAttended" runat="server" Text="0" /></div></div></div>
        <div class="col-6 col-lg-3"><div class="m4-kpi"><div class="m4-kpi-label">Seat fill rate</div><div class="m4-kpi-value"><asp:Label ID="lblFillRate" runat="server" Text="0%" /></div></div></div>
    </div>
    <div class="row g-3 mb-3">
        <div class="col-lg-8"><div class="m4-panel h-100"><h2 class="h5">Top 10 events by registrations</h2><div class="m4-small mb-3">Registered vs actually attended</div><div class="chart-wrap chart-wrap-tall"><canvas id="popularChart" role="img" aria-label="Bar chart of the ten most registered campus events"></canvas></div></div></div>
        <div class="col-lg-4"><div class="m4-panel h-100"><h2 class="h5">Registrations by category</h2><div class="m4-small mb-3">Share of active registrations</div><div class="chart-wrap"><canvas id="categoryChart" role="img" aria-label="Doughnut chart of event registrations by category"></canvas></div></div></div>
    </div>
    <div class="m4-panel">
        <h2 class="h5 mb-3">Event popularity ranking</h2>
        <div class="table-responsive">
        <asp:GridView ID="gvPopular" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="table table-hover table-bordered" EmptyDataText="No events found for the chosen filters.">
            <Columns>
                <asp:BoundField DataField="Title" HeaderText="Event" HtmlEncode="true" />
                <asp:BoundField DataField="CategoryName" HeaderText="Category" HtmlEncode="true" />
                <asp:BoundField DataField="StartMalaysia" HeaderText="Starts (MYT)" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                <asp:BoundField DataField="Capacity" HeaderText="Seats" />
                <asp:BoundField DataField="RegisteredCount" HeaderText="Registered" />
                <asp:BoundField DataField="AttendedCount" HeaderText="Attended" />
                <asp:BoundField DataField="FillRate" HeaderText="Fill (%)" DataFormatString="{0:0.0}" />
            </Columns>
        </asp:GridView>
        </div>
    </div>
    <asp:HiddenField ID="hidLabels" runat="server" /><asp:HiddenField ID="hidRegistered" runat="server" />
    <asp:HiddenField ID="hidAttended" runat="server" /><asp:HiddenField ID="hidCategoryLabels" runat="server" /><asp:HiddenField ID="hidCategoryTotals" runat="server" />
    <div id="m4ChartUnavailable" class="alert alert-warning d-none">Charts could not load. Check your Internet connection for Chart.js, or install a local copy.</div>
</div>
<script>
(function () {
    function readJson(id) { return JSON.parse(document.getElementById(id).value || '[]'); }
    if (typeof Chart === 'undefined') {
        document.getElementById('m4ChartUnavailable').classList.remove('d-none');
        return;
    }
    var labels = readJson('<%= hidLabels.ClientID %>');
    var registered = readJson('<%= hidRegistered.ClientID %>');
    var attended = readJson('<%= hidAttended.ClientID %>');
    var categoryNames = readJson('<%= hidCategoryLabels.ClientID %>');
    var categoryTotals = readJson('<%= hidCategoryTotals.ClientID %>');
    new Chart(document.getElementById('popularChart'), {
        type: 'bar',
        data: { labels: labels, datasets: [
            { label: 'Registered', data: registered, backgroundColor: '#2563eb', borderRadius: 4 },
            { label: 'Attended', data: attended, backgroundColor: '#14b8a6', borderRadius: 4 }
        ]},
        options: { indexAxis: 'y', maintainAspectRatio: false, responsive: true,
            scales: { x: { beginAtZero: true, ticks: { precision: 0 } } },
            plugins: { legend: { position: 'bottom' } }
        }
    });
    new Chart(document.getElementById('categoryChart'), {
        type: 'doughnut',
        data: { labels: categoryNames, datasets: [{ data: categoryTotals,
            backgroundColor: ['#2563eb','#14b8a6','#f59e0b','#8b5cf6','#e879f9','#64748b','#ef4444','#0ea5e9'] }] },
        options: { responsive: true, maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom', labels: { boxWidth: 12 } } } }
    });
})();
</script>
</asp:Content>
