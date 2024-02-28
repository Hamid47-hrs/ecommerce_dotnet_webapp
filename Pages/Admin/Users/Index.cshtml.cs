using System.Data.Common;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Admin.Users
{
    [RequiredAuthentication(RequiredRole = "admin")]
    public class IndexModel : PageModel
    {
        public string errorMessage = "";
        public List<UserInfo> usersList = [];

        public void OnGet()
        {
            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);
                connection.Open();

                string sql = "SELECT * FROM users";

                using SqlCommand command = new(sql, connection);

                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    UserInfo usersInfo =
                        new()
                        {
                            id = reader.GetInt32(0),
                            firstName = reader.GetString(1),
                            lastName = reader.GetString(2),
                            email = reader.GetString(3),
                            phone = reader.GetString(4),
                            role = reader.GetString(6),
                            createdAt = reader.GetDateTime(7).ToString("yyy/MM/dd")
                        };

                    usersList.Add(usersInfo);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
        }

        public class UserInfo
        {
            public int id;
            public string firstName = "";
            public string lastName = "";
            public string email = "";
            public string? phone = "";
            public string role = "";
            public string createdAt = "";
        }
    }
}
