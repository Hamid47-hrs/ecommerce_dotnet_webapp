using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Admin.Books
{
    [RequiredAuthentication(RequiredRole = "admin")]
    public class IndexModel : PageModel
    {
        public string ErrorMessage = "";
        public List<BookInfo> BooksList = new List<BookInfo>();
        public string? search = "";

        public int currentPage = 1;
        public int totalPages = 0;
        private readonly int pageSize = 5;
        public string column = "id";
        public string order = "desc";

        public void OnGet()
        {
            search = Request.Query["search"];
            if (search == null)
                search = "";

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

            string[] validColumns = new string[7]
            {
                "id",
                "title",
                "authors",
                "num_pages",
                "price",
                "category",
                "created_at"
            };
            column = Request.Query["column"];
            if (column == null || !validColumns.Contains(column))
            {
                column = "id";
            }

            order = Request.Query["order"];
            if (order == null || !order.Equals("asc"))
            {
                order = "desc";
            }

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string paginationSql = "SELECT COUNT(*) FROM books_1";

                    if (search.Length > 0)
                    {
                        paginationSql += " WHERE title LIKE @search OR authors LIKE @search";
                    }

                    using (SqlCommand command = new SqlCommand(paginationSql, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");

                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    string sql = "SELECT * FROM books_1";
                    if (search.Length > 0)
                    {
                        sql += " WHERE title LIKE @search OR authors LIKE @search";
                    }
                    sql += " ORDER BY " + column + " " + order;
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@search", "%" + search + "%");
                        command.Parameters.AddWithValue("@skip", (currentPage - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BookInfo bookInfo = new BookInfo();
                                bookInfo.Id = reader.GetInt32(0);
                                bookInfo.Title = reader.GetString(1);
                                bookInfo.Author = reader.GetString(2);
                                bookInfo.Isbn = reader.GetString(3);
                                bookInfo.NumPages = reader.GetInt32(4);
                                bookInfo.Price = reader.GetDecimal(5);
                                bookInfo.Category = reader.GetString(6);
                                bookInfo.Description = reader.GetString(7);
                                bookInfo.ImageFileName = reader.GetString(8);
                                bookInfo.CreatedAt = reader.GetDateTime(9).ToString("MM/dd/yyy");

                                BooksList.Add(bookInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public class BookInfo
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string Author { get; set; } = "";
            public string Isbn { get; set; } = "";
            public int NumPages { get; set; }
            public decimal Price { get; set; }
            public string Category { get; set; } = "";
            public string Description { get; set; } = "";
            public string ImageFileName { get; set; } = "";
            public string CreatedAt { get; set; } = "";
        }
    }
}
