using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Events
{
    public partial class Search : Page
    {
        protected bool IsStudent
        {
            get { return (Session["Role"] as string) == "Student"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindFilters();
                RunSearch();
            }
        }

        private void BindFilters()
        {
            ddlCategory.DataSource = EventSearchService.GetCategories();
            ddlCategory.DataTextField = "CategoryName";
            ddlCategory.DataValueField = "CategoryId";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new ListItem("All categories", ""));

            ddlOrganizer.DataSource = EventSearchService.GetOrganizers();
            ddlOrganizer.DataTextField = "FullName";
            ddlOrganizer.DataValueField = "UserId";
            ddlOrganizer.DataBind();
            ddlOrganizer.Items.Insert(0, new ListItem("All organizers", ""));
        }

        private void RunSearch()
        {
            DateTime? date = null;
            if (!string.IsNullOrWhiteSpace(txtDate.Text))
            {
                DateTime parsed;
                if (!DateTime.TryParseExact(txtDate.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                {
                    lblMessage.Text = "Please enter a valid date.";
                    lblMessage.Visible = true;
                    return;
                }
                date = parsed;
            }

            int category, organizer;
            int? categoryId = int.TryParse(ddlCategory.SelectedValue, out category) ? category : (int?)null;
            int? organizerId = int.TryParse(ddlOrganizer.SelectedValue, out organizer) ? organizer : (int?)null;

            gvResults.DataSource = EventSearchService.Search(date, categoryId, organizerId);
            gvResults.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            RunSearch();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtDate.Text = "";
            ddlCategory.SelectedIndex = 0;
            ddlOrganizer.SelectedIndex = 0;
            RunSearch();
        }
    }
}