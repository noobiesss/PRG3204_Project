using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

namespace PRG3204_Project_Group4.Helpers
{
    /// <summary>
    /// Central ASP.NET Web Forms page access rules.
    /// Global.asax.cs calls CheckRequest in Application_PostAcquireRequestState.
    /// Server-side SQL authorization is still required for individual data operations.
    /// </summary>
    public static class AccessControl
    {
        private static readonly HashSet<string> PublicPages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "~/Default.aspx",
            "~/Events/Default.aspx",
            "~/Events/Calendar.aspx",
            "~/Events/Search.aspx",
            "~/Account/Login.aspx",
            "~/Account/Register.aspx",
            "~/Account/AccessDenied.aspx"
        };

        // Only Student may use registration pages, including the moved feedback form.
        private static readonly HashSet<string> StudentPages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "~/Registrations/Default.aspx",
            "~/Registrations/Create.aspx",
            "~/Registrations/Feedback.aspx"
        };

        // Both Staff and Admin may use these pages. SQL must also check staff ownership.
        private static readonly HashSet<string> StaffPages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "~/Events/Manage.aspx",
            "~/Reports/Attendance.aspx",
            "~/Reports/Popular.aspx",
            "~/Reports/Feedback.aspx",
            "~/Reports/MarkAttendance.aspx"
        };

        private static readonly HashSet<string> SignedInPages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "~/Account/Profile.aspx",
            "~/Account/ChangePassword.aspx",
            "~/Enquiries/Contact.aspx"
        };

        public static void CheckRequest(HttpContext context)
        {
            if (context == null || context.Request == null) return;

            string path = context.Request.AppRelativeCurrentExecutionFilePath;
            if (String.IsNullOrEmpty(path) || !path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)) return;
            if (PublicPages.Contains(path)) return;

            HttpSessionState session = context.Session;
            int userId;
            bool isLoggedIn = session != null &&
                Int32.TryParse(Convert.ToString(session["UserId"]), out userId) && userId > 0;

            if (!isLoggedIn)
            {
                Redirect(context, "~/Account/Login.aspx");
                return;
            }

            string role = Convert.ToString(session["Role"]);
            bool isStudent = String.Equals(role, "Student", StringComparison.OrdinalIgnoreCase);
            bool isStaff = String.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase);
            bool isAdmin = String.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            // Explicit allowlists, plus folder protection so unlisted Reports URLs
            // cannot accidentally become available to students.
            if (StaffPages.Contains(path) || path.StartsWith("~/Reports/", StringComparison.OrdinalIgnoreCase))
            {
                if (!isStaff && !isAdmin) Redirect(context, "~/Account/AccessDenied.aspx");
                return;
            }

            if (StudentPages.Contains(path) || path.StartsWith("~/Registrations/", StringComparison.OrdinalIgnoreCase))
            {
                if (!isStudent) Redirect(context, "~/Account/AccessDenied.aspx");
                return;
            }

            if (SignedInPages.Contains(path)) return;

            // Non-public Events pages are event-management pages in the uploaded
            // project. Public browse/calendar/search pages are listed above.
            if (path.StartsWith("~/Events/", StringComparison.OrdinalIgnoreCase))
            {
                if (!isStaff && !isAdmin) Redirect(context, "~/Account/AccessDenied.aspx");
                return;
            }

            // Other Web Forms pages require a valid login by default.
        }

        private static void Redirect(HttpContext context, string appRelativePath)
        {
            string url = VirtualPathUtility.ToAbsolute(appRelativePath);
            context.Response.Redirect(url, false);
            context.ApplicationInstance.CompleteRequest();
        }
    }
}
