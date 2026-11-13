using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Registrations
{
    public partial class Modify : Page
    {
        // Always taken from the logged-in Session, never from the browser
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
            if ((Session["Role"] as string) != "Student")
            {
                Response.Redirect("~/Account/AccessDenied.aspx");
                return;
            }
            if (!IsPostBack) BindRegistrations();
        }

        private void BindRegistrations()
        {
            gvRegistrations.DataSource = RegistrationChangeService.GetChangeableRegistrations(CurrentUserId);
            gvRegistrations.DataBind();
        }

        protected void gvRegistrations_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int registrationId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out registrationId)) return;

            if (e.CommandName == "CancelReg")
            {
                RegistrationResult result = RegistrationChangeService.Cancel(CurrentUserId, registrationId, null);
                ShowMessage(result.Message, result.Success);
                pnlChange.Visible = false;
                BindRegistrations();
            }
            else if (e.CommandName == "ChangeReg")
            {
                ViewState["ChangeRegistrationId"] = registrationId;
                BindNewEvents();
                pnlChange.Visible = true;
            }
        }

        private void BindNewEvents()
        {
            ddlNewEvent.Items.Clear();
            DataTable events = RegistrationChangeService.GetOpenEvents(CurrentUserId);
            foreach (DataRow row in events.Rows)
            {
                string text = Convert.ToString(row["Title"]) + " - "
                    + Convert.ToDateTime(row["StartMyt"]).ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture)
                    + " (" + row["SeatsLeft"] + " seats left)";
                ddlNewEvent.Items.Add(new ListItem(text, Convert.ToString(row["EventId"])));
            }
            if (ddlNewEvent.Items.Count == 0)
                ddlNewEvent.Items.Add(new ListItem("No other open events available", ""));
        }

        protected void btnConfirmChange_Click(object sender, EventArgs e)
        {
            object stored = ViewState["ChangeRegistrationId"];
            int newEventId;
            if (stored == null || !int.TryParse(ddlNewEvent.SelectedValue, out newEventId))
            {
                ShowMessage("Please choose an event.", false);
                return;
            }

            RegistrationResult result = RegistrationChangeService.Update(
                CurrentUserId, (int)stored, newEventId, txtReason.Text);
            ShowMessage(result.Message, result.Success);
            if (result.Success)
            {
                pnlChange.Visible = false;
                txtReason.Text = "";
            }
            BindRegistrations();
        }

        protected void btnCloseChange_Click(object sender, EventArgs e)
        {
            pnlChange.Visible = false;
        }

        private void ShowMessage(string text, bool success)
        {
            lblMessage.Text = Server.HtmlEncode(text);
            lblMessage.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
            lblMessage.Visible = true;
        }
    }
}