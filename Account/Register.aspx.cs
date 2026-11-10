using System;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Account
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] != null)
            {
                Response.Redirect(ResolveUrl("~/Default.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void cvDateOfBirth_ServerValidate(object source, ServerValidateEventArgs args)
        {
            DateTime? dateOfBirth;
            args.IsValid = AccountService.TryParseDateOfBirth(args.Value, out dateOfBirth);
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            DateTime? dateOfBirth;
            AccountService.TryParseDateOfBirth(txtDateOfBirth.Text, out dateOfBirth);

            UserAccount account = new UserAccount
            {
                MatricOrStaffNo = txtMatricNo.Text,
                FullName = txtFullName.Text,
                SchoolEmail = txtEmail.Text,
                PersonalEmail = txtPersonalEmail.Text,
                Gender = ddlGender.SelectedValue,
                DateOfBirth = dateOfBirth,
                PhoneNumber = txtPhone.Text,
                Faculty = txtFaculty.Text
            };

            AccountResult result;
            try
            {
                result = AccountService.RegisterStudent(account, txtPassword.Text);
            }
            catch (SqlException exception)
            {
                System.Diagnostics.Trace.TraceError("Registration database error: {0}", exception);
                ShowMessage("Your account could not be created. Please try again later.", "alert-danger");
                return;
            }

            if (!result.Success)
            {
                ShowMessage(result.Message, "alert-danger");
                return;
            }

            Response.Redirect(ResolveUrl("~/Account/Login.aspx?registered=1"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowMessage(string message, string cssClass)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            lblMessage.CssClass = "alert " + cssClass + " d-block";
            lblMessage.Visible = true;
        }
    }
}
