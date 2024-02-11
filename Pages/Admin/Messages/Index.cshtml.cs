using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{
    public class IndexModel : PageModel
    {
        public string ErrorMessage { get; set; } = "";
        public List<MessageInfo> listMessages = [];

        public void OnGet()
        {
            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using (SqlConnection connection = new(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM messages ORDER BY id DESC";

                    using (SqlCommand command = new(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
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
                    }
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
