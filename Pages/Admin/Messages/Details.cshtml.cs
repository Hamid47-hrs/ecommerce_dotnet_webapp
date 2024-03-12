using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Admin.Messages
{
    [RequiredAuthentication(RequiredRole = "admin")]
    public class DetailsModel(IConfiguration configuration) : PageModel
    {
        public string ErrorMessage { get; set; } = "";
        public MessageInfo messageInfo = new();

        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnGet()
        {
            string? requestId = Request.Query["id"];

            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();
                string sql = "SELECT * FROM messages WHERE id=@id";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", requestId);

                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    messageInfo.Id = reader.GetInt32(0);
                    messageInfo.FirstName = reader.GetString(1);
                    messageInfo.LastName = reader.GetString(2);
                    messageInfo.Email = reader.GetString(3);
                    messageInfo.Phone = reader.GetString(4);
                    messageInfo.Subject = reader.GetString(5);
                    messageInfo.Message = reader.GetString(6);
                    messageInfo.CreatedAt = reader.GetDateTime(7).ToString("MM/dd/yyy");
                }
                else
                {
                    Response.Redirect("/Admin/Messages/Index");
                }
            }
            catch (Exception ex)
            {
                Response.Redirect("/Admin/Messages/Index");
                ErrorMessage = ex.Message;
            }
        }
    }
}
