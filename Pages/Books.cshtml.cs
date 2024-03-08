using System.Collections;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ecommerce_dotnet_webapp.Pages.Admin.Books.IndexModel;

namespace ecommerce_dotnet_webapp.Pages
{
    [BindProperties(SupportsGet = true)]
    public class BooksModel : PageModel
    {
        public string? Search { get; set; }
        public string PriceRange { get; set; } = "any";
        public string PageRange { get; set; } = "any";
        public string Category { get; set; } = "any";

        public List<BookInfo> booksList = [];

        public int currentPage = 1;
        public int totalPages = 0;
        private readonly int pageSize = 4;

        public string errorMessage = "";

        public void OnGet()
        {
            currentPage = 1;
            string? requestPage = Request.Query["page"];
            if (requestPage != null)
            {
                try
                {
                    currentPage = int.Parse(requestPage);
                }
                catch (Exception)
                {
                    currentPage = 1;
                }
            }
            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);

                connection.Open();

                string sqlCount = "SELECT COUNT(*) FROM books_1";
                sqlCount += " WHERE (title LIKE @search OR authors LIKE @search)";

                switch (PriceRange)
                {
                    case "0_50":
                        sqlCount += " AND price <= 50";
                        break;

                    case "50_100":
                        sqlCount += " AND price >= 50 AND price <= 100";
                        break;

                    case "above100":
                        sqlCount += " AND price >= 100";
                        break;
                }

                switch (PageRange)
                {
                    case "0_100":
                        sqlCount += " AND num_pages <=100";
                        break;

                    case "100_299":
                        sqlCount += " AND num_pages >= 100 AND num_pages <= 299";
                        break;

                    case "above300":
                        sqlCount += " AND num_pages >= 300";
                        break;
                }

                if (!Category.Equals("any"))
                {
                    sqlCount += " AND category=@category";
                }

                using SqlCommand commandCount = new(sqlCount, connection);
                commandCount.Parameters.AddWithValue("@search", "%" + Search + "%");
                commandCount.Parameters.AddWithValue("@category", Category);

                decimal count = (int)commandCount.ExecuteScalar();
                totalPages = (int)Math.Ceiling(count / pageSize);

                string sqlQuery = "SELECT * FROM books_1";
                sqlQuery += " WHERE (title LIKE @search OR authors LIKE @search)";

                switch (PriceRange)
                {
                    case "0_50":
                        sqlQuery += " AND price <= 50";
                        break;

                    case "50_100":
                        sqlQuery += " AND price >= 50 AND price <= 100";
                        break;

                    case "above100":
                        sqlQuery += " AND price >= 100";
                        break;
                }

                switch (PageRange)
                {
                    case "0_100":
                        sqlQuery += " AND num_pages <=100";
                        break;

                    case "100_299":
                        sqlQuery += " AND num_pages >= 100 AND num_pages <= 299";
                        break;

                    case "above300":
                        sqlQuery += " AND num_pages >= 300";
                        break;
                }

                if (!Category.Equals("any"))
                {
                    sqlQuery += " AND category=@category";
                }

                sqlQuery += " ORDER BY id DESC OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                using SqlCommand command = new(sqlQuery, connection);
                command.Parameters.AddWithValue("@search", "%" + Search + "%");
                command.Parameters.AddWithValue("@category", Category);
                command.Parameters.AddWithValue("@skip", (currentPage - 1) * pageSize);
                command.Parameters.AddWithValue("@pageSize", pageSize);

                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    BookInfo bookinfo =
                        new()
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Author = reader.GetString(2),
                            Isbn = reader.GetString(3),
                            NumPages = reader.GetInt32(4),
                            Price = reader.GetDecimal(5),
                            Category = reader.GetString(6),
                            Description = reader.GetString(7),
                            ImageFileName = reader.GetString(8),
                            CreatedAt = reader.GetDateTime(9).ToString("yyy/MM/dd"),
                        };

                    booksList.Add(bookinfo);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
        }
    }
}
