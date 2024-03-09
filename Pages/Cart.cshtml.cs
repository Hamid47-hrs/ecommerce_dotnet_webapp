using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ecommerce_dotnet_webapp.Pages.Admin.Books.IndexModel;

namespace ecommerce_dotnet_webapp.Pages
{
    [RequiredAuthentication]
    [BindProperties]
    public class CartModel : PageModel
    {
        [Required(ErrorMessage = "The Address is REQUIRED!")]
        public string Address { get; set; } = "";

        [Required]
        public string PaymentMethod { get; set; } = "";

        public List<OrderItem> orderItemsList = [];

        public decimal subTotal = 0;
        public decimal shippingFee = 5;
        public decimal totalFee = 0;

        private Dictionary<String, int> getBookDictionary()
        {
            var bookDictionary = new Dictionary<string, int>();

            string cookieValue = Request.Cookies["shopping_cart"] ?? "";

            if (cookieValue.Length > 0)
            {
                string[] bookIdArray = cookieValue.Split("-");

                foreach (var item in bookIdArray)
                {
                    if (!bookDictionary.TryAdd(item, 1))
                    {
                        bookDictionary[item] += 1;
                    }
                }
            }

            return bookDictionary;
        }

        private decimal getBookPrice(string bookId)
        {
            decimal price = 0;

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);

                connection.Open();

                string sqlQuery = "SELECT price FROM books_1 WHERE id=@id";

                using SqlCommand command = new(sqlQuery, connection);

                command.Parameters.AddWithValue("@id", bookId);

                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    price = reader.GetDecimal(0);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            return price;
        }

        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            var bookDictionary = getBookDictionary();

            string? action = Request.Query["action"];
            string? userId = Request.Query["id"];

            if (action != null && userId != null && bookDictionary.ContainsKey(userId))
            {
                switch (action)
                {
                    case "add":
                        bookDictionary[userId] += 1;
                        break;

                    case "sub":
                        if (bookDictionary[userId] > 1)
                            bookDictionary[userId] -= 1;
                        break;

                    case "del":
                        bookDictionary.Remove(userId);
                        break;
                }

                string newCookieValue = "";

                foreach (var keyValuePair in bookDictionary)
                {
                    for (int i = 0; i < keyValuePair.Value; i++)
                        newCookieValue += "-" + keyValuePair.Key;
                }

                if (newCookieValue.Length > 0)
                    newCookieValue = newCookieValue[1..];

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(365),
                    Path = "/"
                };

                Response.Cookies.Append("shopping_cart", newCookieValue, cookieOptions);

                Response.Redirect(Request.Path.ToString());
                return;
            }

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);

                connection.Open();

                string sqlQuery = "SELECT * FROM books_1 WHERE id=@id";

                foreach (var KeyValuePair in bookDictionary)
                {
                    string bookId = KeyValuePair.Key;

                    using SqlCommand command = new(sqlQuery, connection);
                    command.Parameters.AddWithValue("@id", bookId);

                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        OrderItem item = new();

                        item.bookInfo.Id = reader.GetInt32(0);
                        item.bookInfo.Title = reader.GetString(1);
                        item.bookInfo.Author = reader.GetString(2);
                        item.bookInfo.Isbn = reader.GetString(3);
                        item.bookInfo.NumPages = reader.GetInt32(4);
                        item.bookInfo.Price = reader.GetDecimal(5);
                        item.bookInfo.Category = reader.GetString(6);
                        item.bookInfo.Description = reader.GetString(7);
                        item.bookInfo.ImageFileName = reader.GetString(8);
                        item.bookInfo.CreatedAt = reader.GetDateTime(9).ToString("yyy/MM/dd");

                        item.numCopies = KeyValuePair.Value;
                        item.totalPrice = (int)(item.numCopies * item.bookInfo.Price);

                        orderItemsList.Add(item);

                        subTotal += item.totalPrice;
                        totalFee = subTotal + shippingFee;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Address = HttpContext.Session.GetString("address") ?? "";
        }

        public void OnPost()
        {
            int clientId = HttpContext.Session.GetInt32("id") ?? 0;

            if (clientId < 1)
            {
                Response.Redirect("/Auth/Login");
                return;
            }

            if (!ModelState.IsValid)
            {
                errorMessage = "Data Validation Faild!";
                return;
            }

            var bookDictionary = getBookDictionary();

            if (bookDictionary.Count < 1)
            {
                errorMessage = "Your Cart is Empty.";
                return;
            }

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);

                connection.Open();

                int orderId = 0;

                string sqlQuery = "INSERT INTO orders";
                sqlQuery +=
                    " (client_id, order_date, shipping_fee, delivery_address, payment_method, payment_status, order_status)"
                    + " OUTPUT INSERTED.id"
                    + " VALUES (@client_id, CURRENT_TIMESTAMP, @shipping_fee, @delivery_address, @payment_method, 'pending', 'created')";

                using SqlCommand command = new(sqlQuery, connection);

                command.Parameters.AddWithValue("@client_id", clientId);
                command.Parameters.AddWithValue("@shipping_fee", shippingFee);
                command.Parameters.AddWithValue("@delivery_address", Address);
                command.Parameters.AddWithValue("@payment_method", PaymentMethod);

                orderId = (int)command.ExecuteScalar();

                // * Add the ordered books to the order_items table.
                string sqlOrderItem =
                    "INSERT INTO order_items"
                    + " (order_id, book_id, quantity, unit_price)"
                    + " VALUES (@order_id, @book_id, @quantity, @unit_price)";

                foreach (var keyValuePair in bookDictionary)
                {
                    string bookId = keyValuePair.Key;
                    int quantity = keyValuePair.Value;
                    decimal unitPrice = getBookPrice(bookId);

                    using SqlCommand orderItemsCommand = new(sqlOrderItem, connection);

                    orderItemsCommand.Parameters.AddWithValue("@order_id", orderId);
                    orderItemsCommand.Parameters.AddWithValue("@book_id", bookId);
                    orderItemsCommand.Parameters.AddWithValue("@quantity", quantity);
                    orderItemsCommand.Parameters.AddWithValue("@unit_price", unitPrice);

                    orderItemsCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Response.Cookies.Delete("shopping_cart");

            successMessage = "Order Created Successfully.";
        }
    }

    public class OrderItem
    {
        public BookInfo bookInfo = new();
        public int numCopies = 0;
        public int totalPrice = 0;
    }
}
