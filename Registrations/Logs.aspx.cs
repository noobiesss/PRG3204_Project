using System;
using System.Web.UI;
using PRG3204_Project_Group4.Services;

namespace PRG3204_Project_Group4.Registrations
{
    public partial class Logs : Page
    {
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
            if (!IsPostBack)
            {
                gvLogs.DataSource = RegistrationChangeService.GetLogs(Convert.ToInt32(Session["UserId"]));
                gvLogs.DataBind();
            }
        }
    }
}