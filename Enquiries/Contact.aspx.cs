using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Enquiries
{
    public partial class Contact : Page
    {
        private int CurrentUserId
        {
            get { object v = Session["UserId"]; return v == null ? 0 : Convert.ToInt32(v); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Account/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }
            if (!IsPostBack)
            {
                BindDropDowns();
                BindEnquiries();
            }
        }

        private void BindDropDowns()
        {
            ddlEvent.DataSource = EnquiryService.GetUpcomingEvents();
            ddlEvent.DataTextField = "Title";
            ddlEvent.DataValueField = "EventId";
            ddlEvent.DataBind();
            ddlEvent.Items.Insert(0, new ListItem("-- General enquiry (no event) --", ""));

            ddlOrganizer.DataSource = EventSearchService.GetOrganizers();
            ddlOrganizer.DataTextField = "FullName";
            ddlOrganizer.DataValueField = "UserId";
            ddlOrganizer.DataBind();
            ddlOrganizer.Items.Insert(0, new ListItem("-- Select organizer --", ""));
        }

        private void BindEnquiries()
        {
            gvEnquiries.DataSource = EnquiryService.GetMyEnquiries(CurrentUserId);
            gvEnquiries.DataBind();
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int ev, org;
            int? eventId = int.TryParse(ddlEvent.SelectedValue, out ev) ? ev : (int?)null;
            int? organizerId = int.TryParse(ddlOrganizer.SelectedValue, out org) ? org : (int?)null;

            string error = EnquiryService.Submit(CurrentUserId, eventId, organizerId, txtSubject.Text, txtMessage.Text);
            if (error == null)
            {
                lblMessage.Text = "Your enquiry has been sent to the organizer.";
                lblMessage.CssClass = "alert alert-success d-block";
                txtSubject.Text = "";
                txtMessage.Text = "";
                ddlEvent.SelectedIndex = 0;
                ddlOrganizer.SelectedIndex = 0;
                BindEnquiries();
            }
            else
            {
                lblMessage.Text = Server.HtmlEncode(error);
                lblMessage.CssClass = "alert alert-danger d-block";
            }
            lblMessage.Visible = true;
        }
    }
}