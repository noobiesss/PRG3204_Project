using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using PRG3204_Project_Group4.Helpers;

namespace PRG3204_Project_Group4
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
        }

        // Session is available from this event onwards
        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            AccessControl.CheckRequest(Context);
        }
    }
}