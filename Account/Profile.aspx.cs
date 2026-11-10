using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Account
{
    public partial class Profile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadProfile();
            }
        }

        private void LoadProfile()
        {
            UserAccount account;
            try
            {
                account = AccountService.GetUser(Convert.ToInt32(Session["UserId"]));
            }
            catch (SqlException exception)
            {
                System.Diagnostics.Trace.TraceError("Profile database error: {0}", exception);
                ShowMessage("Your profile could not be loaded. Please try again later.", "alert-danger");
                btnSave.Enabled = false;
                return;
            }

            if (account == null)
            {
                Session.Clear();
                Response.Redirect(ResolveUrl("~/Account/Login.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            litMatricNo.Text = account.MatricOrStaffNo;
            litSchoolEmail.Text = account.SchoolEmail;
            litRole.Text = account.Role;
            txtFullName.Text = account.FullName;
            txtPersonalEmail.Text = account.PersonalEmail;
            ddlGender.SelectedValue = account.Gender ?? "";
            txtDateOfBirth.Text = account.DateOfBirth.HasValue ? account.DateOfBirth.Value.ToString("yyyy-MM-dd") : "";
            txtPhone.Text = account.PhoneNumber;
            txtFaculty.Text = account.Faculty;
        }

        protected void cvDateOfBirth_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime? dateOfBirth;
            args.IsValid = AccountService.TryParseDateOfBirth(args.Value, out dateOfBirth);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            DateTime? dateOfBirth;
            AccountService.TryParseDateOfBirth(txtDateOfBirth.Text, out dateOfBirth);

            UserAccount account = new UserAccount
            {
                UserId = Convert.ToInt32(Session["UserId"]),
                FullName = txtFullName.Text,
                PersonalEmail = txtPersonalEmail.Text,
                Gender = ddlGender.SelectedValue,
                DateOfBirth = dateOfBirth,
                PhoneNumber = txtPhone.Text,
                Faculty = txtFaculty.Text
            };

            AccountResult result;
            try
            {
                result = AccountService.UpdateProfile(account);
            }
            catch (SqlException exception)
            {
                System.Diagnostics.Trace.TraceError("Profile database error: {0}", exception);
                ShowMessage("Your profile could not be saved. Please try again later.", "alert-danger");
                return;
            }

            if (result.Success)
            {
                // Keep the name in the navigation bar up to date
                Session["FullName"] = txtFullName.Text.Trim();
            }
            ShowMessage(result.Message, result.Success ? "alert-success" : "alert-danger");
        }

        private void ShowMessage(string message, string cssClass)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.CssClass = "alert " + cssClass + " d-block";
            lblMessage.Visible = true;
        }
    }
}
