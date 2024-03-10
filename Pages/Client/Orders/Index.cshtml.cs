using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using ecommerce_dotnet_webapp.Pages.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Client.Orders
{
    [RequiredAuthentication(RequiredRole = "client")]
    public class IndexModel : PageModel
    {
        public List<OrderInfo> orderInfoList = [];

        public int currentPage = 1;
        public int totalPages = 0;
        private readonly int pageSize = 5;

        public string errorMessage = "";

        public void OnGet()
        {
            int clientId = HttpContext.Session.GetInt32("id") ?? 0;

            try
            {
                string? requestPage = Request.Query["page"];
                currentPage = int.Parse(requestPage);
            }
            catch (Exception)
            {
                currentPage = 1;
            }

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);

                connection.Open();

                string sqlCount = "SELECT COUNT(*) FROM orders WHERE client_id= @client_id";

                using SqlCommand countCommand = new(sqlCount, connection);

                countCommand.Parameters.AddWithValue("@client_id", clientId);

                decimal count = (int)countCommand.ExecuteScalar();
                totalPages = (int)Math.Ceiling(count / pageSize);

                string sqlQuery =
                    "SELECT * FROM orders WHERE client_id=@client_id ORDER BY id DESC"
                    + " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";

                using SqlCommand command = new(sqlQuery, connection);

                command.Parameters.AddWithValue("@client_id", clientId);
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

                    orderInfo.orderItemInfoList = OrderInfo.getOrderItems(orderInfo.id);

                    orderInfoList.Add(orderInfo);
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
