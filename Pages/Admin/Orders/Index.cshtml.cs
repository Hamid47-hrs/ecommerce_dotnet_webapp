using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ecommerce_dotnet_webapp.Pages.Admin.Books.IndexModel;

namespace ecommerce_dotnet_webapp.Pages.Admin
{
    [RequiredAuthentication(RequiredRole = "admin")]
    public class IndexModel(IConfiguration configuration) : PageModel
    {
        public List<OrderInfo> orderInfoList = [];

        public int currentPage = 1;
        public int totalPages = 0;
        private readonly int pageSize = 5;
        public string errorMessage = "";

        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnGet()
        {
            try
            {
                string? requestPage = Request.Query["page"];
                currentPage = int.Parse(requestPage);
            }
            catch (Exception)
            {
                currentPage = 1;
            }

            using SqlConnection connection = new(connectionString);

            connection.Open();

            string sqlCount = "SELECT COUNT(*) FROM orders";

            using SqlCommand commandCount = new(sqlCount, connection);

            decimal count = (int)commandCount.ExecuteScalar();
            totalPages = (int)Math.Ceiling(count / pageSize);

            string sqlQuery =
                "SELECT * FROM orders ORDER BY id DESC"
                + " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

            using SqlCommand command = new(sqlQuery, connection);
            command.Parameters.AddWithValue("@skip", (currentPage - 1) * pageSize);
            command.Parameters.AddWithValue("@pageSize", pageSize);

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                OrderInfo orderInfo =
                    new()
                    {
                        id = reader.GetInt32(0),
                        clientId = reader.GetInt32(1),
                        orderDate = reader.GetDateTime(2).ToString("yyy/MM/dd"),
                        shippingFee = reader.GetDecimal(3),
                        deliveryAddress = reader.GetString(4),
                        paymentMethod = reader.GetString(5),
                        paymentStatus = reader.GetString(6),
                        orderStatus = reader.GetString(7),
                    };
                orderInfo.orderItemInfoList = OrderInfo.GetOrderItems(orderInfo.id);

                orderInfoList.Add(orderInfo);
            }
        }
    }

    public class OrderItemInfo
    {
        public int id;
        public int orderId;
        public int bookId;
        public int quantity;
        public decimal unitPrice;

        public BookInfo bookInfo = new();
    }

    public class OrderInfo
    {
        public int id;
        public int clientId;
        public string orderDate;
        public decimal shippingFee;
        public string deliveryAddress;
        public string paymentMethod;
        public string paymentStatus;
        public string orderStatus;

        public List<OrderItemInfo> orderItemInfoList = [];

        public static List<OrderItemInfo> GetOrderItems(int orderId)
        {
            List<OrderItemInfo> items = [];

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);

                connection.Open();

                string sqlQuery =
                    "SELECT order_items.*, books_1.* FROM order_items, books_1"
                    + " WHERE order_items.order_id=@order_id AND order_items.book_id=books_1.id";

                using SqlCommand command = new(sqlQuery, connection);

                command.Parameters.AddWithValue("@order_id", orderId);

                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    OrderItemInfo item =
                        new()
                        {
                            id = reader.GetInt32(0),
                            orderId = reader.GetInt32(1),
                            bookId = reader.GetInt32(2),
                            quantity = reader.GetInt32(3),
                            unitPrice = reader.GetDecimal(4),
                        };

                    item.bookInfo.Id = reader.GetInt32(5);
                    item.bookInfo.Title = reader.GetString(6);
                    item.bookInfo.Author = reader.GetString(7);
                    item.bookInfo.Isbn = reader.GetString(8);
                    item.bookInfo.NumPages = reader.GetInt32(9);
                    item.bookInfo.Price = reader.GetDecimal(10);
                    item.bookInfo.Category = reader.GetString(11);
                    item.bookInfo.Description = reader.GetString(12);
                    item.bookInfo.ImageFileName = reader.GetString(13);
                    item.bookInfo.CreatedAt = reader.GetDateTime(14).ToString("yyy/MM/dd");

                    items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return items;
        }
    }
}
