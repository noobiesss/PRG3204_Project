using System.Configuration;
using System.Data.SqlClient;

namespace PRG3204_Project_Group4.Helpers
{
    public static class DatabaseHelper
    {
        public static SqlConnection CreateConnection()
        {
            ConnectionStringSettings settings =
                ConfigurationManager
                    .ConnectionStrings["CampusEventDb"];

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    "CampusEventDb connection string is missing.");
            }

            return new SqlConnection(settings.ConnectionString);
        }
    }
}