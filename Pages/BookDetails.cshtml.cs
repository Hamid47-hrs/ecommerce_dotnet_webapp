using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ecommerce_dotnet_webapp.Pages.Admin.Books.IndexModel;

namespace ecommerce_dotnet_webapp.Pages
{
    public class BookDetailsModel : PageModel
    {
        public BookInfo bookInfo = new();
        public string errorMessage = "";

        public void OnGet(int? id)
        {
            if (id == null)
            {
                Response.Redirect("/");
                return;
            }

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);
                connection.Open();

                string sql = "SELECT * FROM books_1 WHERE id=@id";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    bookInfo.Id = reader.GetInt32(0);
                    bookInfo.Title = reader.GetString(1);
                    bookInfo.Author = reader.GetString(2);
                    bookInfo.Isbn = reader.GetString(3);
                    bookInfo.NumPages = reader.GetInt32(4);
                    bookInfo.Price = reader.GetDecimal(5);
                    bookInfo.Category = reader.GetString(6);
                    bookInfo.Description = reader.GetString(7);
                    bookInfo.ImageFileName = reader.GetString(8);
                    bookInfo.CreatedAt = reader.GetDateTime(9).ToString("yyy/MM/dd");
                }
                else
                {
                    // Response.Redirect("/");
                    return;
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Response.Redirect("/");
                return;
            }
        }
    }
}
