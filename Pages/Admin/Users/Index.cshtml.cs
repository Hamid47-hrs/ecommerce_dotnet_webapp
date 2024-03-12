using System.Data.Common;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Admin.Users
{
    [RequiredAuthentication(RequiredRole = "admin")]
    public class IndexModel(IConfiguration configuration) : PageModel
    {
        public List<UserInfo> usersList = [];
        public string errorMessage = "";

        public string? search = "";

        public int currentPage = 1;
        public int totalPages = 0;
        private readonly int pageSize = 4;

        public string? column = "id";
        public string order = "DESC";

        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnGet()
        {
            search = Request.Query["search"];
            if (search == null)
                search = "";

            currentPage = 1;
            string? requestPage = Request.Query["page"];
            if (requestPage != null)
            {
                currentPage = int.Parse(requestPage);
            }
            else
            {
                currentPage = 1;
            }

            string[] validColumns = ["id", "lastname", "email", "role", "created_at"];

            column = Request.Query["column"];
            if (column == null || !validColumns.Contains(column))
            {
                column = "id";
            }

            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                string sqlCount = "SELECT COUNT(*) FROM users";

                if (column.Length > 0)
                {
                    sqlCount += " WHERE lastname LIKE @search OR email LIKE @search";
                }

                using SqlCommand command = new(sqlCount, connection);

                command.Parameters.AddWithValue("@search", "%" + search + "%");
                decimal count = (int)command.ExecuteScalar();
                totalPages = (int)Math.Ceiling(count / pageSize);

                string sql = "SELECT * FROM users";

                if (search.Length > 0)
                {
                    sql += " WHERE lastname LIKE @search OR email LIKE @search";
                }

                sql += " ORDER BY " + column + " " + order;
                sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                using SqlCommand command_2 = new(sql, connection);

                command_2.Parameters.AddWithValue("@search", "%" + search + "%");
                command_2.Parameters.AddWithValue("@skip", (currentPage - 1) * pageSize);
                command_2.Parameters.AddWithValue("@pageSize", pageSize);

                using SqlDataReader reader = command_2.ExecuteReader();

                while (reader.Read())
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
            public string? phone;
            public string role = "";
            public string createdAt = "";
        }
    }
}
