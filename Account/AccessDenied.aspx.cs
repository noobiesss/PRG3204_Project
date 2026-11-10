using System;

namespace PRG3204_Project_Group4.Account
{
    public partial class AccessDenied : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}
