using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ecommerce_dotnet_webapp.Pages.Admin.Books.IndexModel;

namespace ecommerce_dotnet_webapp.Pages;

public class IndexModel(IConfiguration configuration) : PageModel
{
    public List<BookInfo> newestBooksList = [];
    public List<BookInfo> topSalesList = [];

    public string errorMessage = "";

    private readonly string connectionString = configuration.GetConnectionString(
        "DefaultConnection"
    )!;

    public void OnGet()
    {
        try
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();

            string sql = "SELECT TOP 4 * FROM books_1 ORDER BY created_at DESC";

            using SqlCommand newesBooksCommand = new(sql, connection);

            using SqlDataReader newesBooksReader = newesBooksCommand.ExecuteReader();

            while (newesBooksReader.Read())
            {
                BookInfo bookInfo =
                    new()
                    {
                        Id = newesBooksReader.GetInt32(0),
                        Title = newesBooksReader.GetString(1),
                        Author = newesBooksReader.GetString(2),
                        Isbn = newesBooksReader.GetString(3),
                        NumPages = newesBooksReader.GetInt32(4),
                        Price = newesBooksReader.GetDecimal(5),
                        Category = newesBooksReader.GetString(6),
                        Description = newesBooksReader.GetString(7),
                        ImageFileName = newesBooksReader.GetString(8),
                        CreatedAt = newesBooksReader.GetDateTime(9).ToString("yyy/MM/dd"),
                    };

                newestBooksList.Add(bookInfo);
            }

            newesBooksReader.Close();

            sql =
                "SELECT TOP 4 books_1.*, (SELECT SUM(order_items.quantity) FROM order_items WHERE books_1.id = order_items.id) AS total_sales FROM books_1 ORDER BY total_sales DESC";

            using SqlCommand topSalesCommand = new(sql, connection);

            using SqlDataReader topSalesReader = topSalesCommand.ExecuteReader();

            while (topSalesReader.Read())
            {
                BookInfo bookinfo =
                    new()
                    {
                        Id = topSalesReader.GetInt32(0),
                        Title = topSalesReader.GetString(1),
                        Author = topSalesReader.GetString(2),
                        Isbn = topSalesReader.GetString(3),
                        NumPages = topSalesReader.GetInt32(4),
                        Price = topSalesReader.GetDecimal(5),
                        Category = topSalesReader.GetString(6),
                        Description = topSalesReader.GetString(7),
                        ImageFileName = topSalesReader.GetString(8),
                        CreatedAt = topSalesReader.GetDateTime(9).ToString("yyy/MM/dd"),
                    };

                topSalesList.Add(bookinfo);
            }

            topSalesReader.Close();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return;
        }
    }
}
