using System;
using System.Web;

namespace PRG3204_Project_Group4.Helpers
{
    // Checks every page request. Add new protected pages to the lists below.
    public static class AccessControl
    {
        // Every page in Reports is for staff; the student feedback form is in Registrations
        private static readonly string[] StaffPages = { "~/Events/Manage.aspx", "~/Reports/" };
        private static readonly string[] LoginPages = { "~/Account/Profile.aspx", "~/Account/ChangePassword.aspx", "~/Reports/Feedback.aspx", "~/Enquiries/", "~/Registrations/" };

        public static void CheckRequest(HttpContext context)
        {
            // Static files such as CSS have no session and need no check
            if (context.Session == null) return;

            string path = context.Request.AppRelativeCurrentExecutionFilePath;
            bool staffOnly = Matches(path, StaffPages);
            if (!staffOnly && !Matches(path, LoginPages)) return;

            if (context.Session["UserId"] == null)
            {
                string login = VirtualPathUtility.ToAbsolute("~/Account/Login.aspx");
                Redirect(context, login + "?ReturnUrl=" + HttpUtility.UrlEncode(context.Request.RawUrl));
                return;
            }

            string role = context.Session["Role"] as string;
            if (staffOnly && role != "Staff" && role != "Admin")
            {
                Redirect(context, VirtualPathUtility.ToAbsolute("~/Account/AccessDenied.aspx"));
            }
        }

        private static bool Matches(string path, string[] rules)
        {
            foreach (string rule in rules)
            {
                if (path.StartsWith(rule, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static void Redirect(HttpContext context, string url)
        {
            context.Response.Redirect(url, false);
            context.ApplicationInstance.CompleteRequest();
        }
    }
}
