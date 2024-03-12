using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Admin.Messages
{
    [RequiredAuthentication(RequiredRole = "admin")]
    public class IndexModel(IConfiguration configuration) : PageModel
    {
        public string ErrorMessage { get; set; } = "";
        public List<MessageInfo> listMessages = [];

        public int page = 1;
        public int totalPages = 0;
        private readonly int pageSize = 4;

        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnGet()
        {
            page = 1;
            string? requestPage = Request.Query["page"];

            if (requestPage != null)
            {
                try
                {
                    page = int.Parse(requestPage);
                }
                catch (Exception)
                {
                    page = 1;
                }
            }
            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                string sqlCount = "SELECT COUNT(*) FROM messages";
                using SqlCommand command1 = new(sqlCount, connection);
                decimal count = (int)command1.ExecuteScalar();
                totalPages = (int)Math.Ceiling(count / pageSize);

                string sql = "SELECT * FROM messages ORDER BY id DESC";
                sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                command.Parameters.AddWithValue("@pageSize", pageSize);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    MessageInfo messageInfo =
                        new()
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Email = reader.GetString(3),
                            Phone = reader.GetString(4),
                            Subject = reader.GetString(5),
                            Message = reader.GetString(6),
                            CreatedAt = reader.GetDateTime(7).ToString("MM/dd/yyy")
                        };
                    listMessages.Add(messageInfo);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return;
            }
        }
    }

    public class MessageInfo
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }
}
