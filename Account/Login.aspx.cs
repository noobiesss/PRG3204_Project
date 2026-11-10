using System;
using System.Data.SqlClient;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] != null)
            {
                RedirectTo(ResolveUrl("~/Default.aspx"));
                return;
            }

            if (!IsPostBack && Request.QueryString["registered"] == "1")
            {
                ShowMessage("Registration successful. Please log in.", "alert-success");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            AccountResult result;
            try
            {
                result = AccountService.Login(txtEmail.Text, txtPassword.Text);
            }
            catch (SqlException exception)
            {
                System.Diagnostics.Trace.TraceError("Login database error: {0}", exception);
                ShowMessage("Login is unavailable right now. Please try again later.", "alert-danger");
                return;
            }

            if (!result.Success)
            {
                ShowMessage(result.Message, "alert-danger");
                return;
            }

            // Start a clean session for the signed-in user
            Session.Clear();
            Session["UserId"] = result.User.UserId;
            Session["Role"] = result.User.Role;
            Session["FullName"] = result.User.FullName;

            RedirectTo(GetReturnUrl());
        }

        // Only return to pages on this site, never to an outside address
        private string GetReturnUrl()
        {
            string url = Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrEmpty(url) && url.StartsWith("/") && !url.StartsWith("//")
                && Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                return url;
            }
            return ResolveUrl("~/Default.aspx");
        }

        private void RedirectTo(string url)
        {
            Response.Redirect(url, false);
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
