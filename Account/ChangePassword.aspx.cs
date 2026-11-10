using System;
using System.Data.SqlClient;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Account
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        protected void btnChange_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            AccountResult result;
            try
            {
                result = AccountService.ChangePassword(Convert.ToInt32(Session["UserId"]), txtCurrentPassword.Text, txtNewPassword.Text);
            }
            catch (SqlException exception)
            {
                System.Diagnostics.Trace.TraceError("Change password database error: {0}", exception);
                ShowMessage("Your password could not be changed. Please try again later.", "alert-danger");
                return;
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
