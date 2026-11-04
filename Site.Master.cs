using System;

namespace PRG3204_Project_Group4
{
    public partial class Site : System.Web.UI.MasterPage
    {
        // Name shown in the account menu
        protected string UserName
        {
            get { return Session["FullName"] as string; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = Page.Title + " - Campus Events";

            string role = Session["Role"] as string;
            bool isLoggedIn = Session["UserId"] != null;

            // Show the menu items for the current role
            phGuest.Visible = !isLoggedIn;
            phAccount.Visible = isLoggedIn;
            phContact.Visible = isLoggedIn;
            phRegistrations.Visible = role == "Student" || role == "Staff";
            phManage.Visible = role == "Staff" || role == "Admin";
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Default.aspx");
        }

        protected string NavClass(string page)
        {
            return IsCurrentPage(page) ? "nav-link text-dark active fw-semibold" : "nav-link text-dark";
        }

        protected string DropdownClass(string page)
        {
            return IsCurrentPage(page) ? "dropdown-item active" : "dropdown-item";
        }

        private bool IsCurrentPage(string page)
        {
            return string.Equals(Request.AppRelativeCurrentExecutionFilePath, page, StringComparison.OrdinalIgnoreCase);
        }
    }
}
