using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using ecommerce_dotnet_webapp.Pages.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Client.Orders
{
    [RequiredAuthentication(RequiredRole = "client")]
    public class DetailsModel(IConfiguration configuration) : PageModel
    {
        public OrderInfo orderInfo = new();

        public string errorMessage = "";

        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnGet(int id) //* Read ID of the order from page URL with inject method.
        {
            int clientId = HttpContext.Session.GetInt32("id") ?? 0;

            if (id < 1)
            {
                Response.Redirect("/Client/Orders/Index");
                return;
            }

            try
            {
                using SqlConnection connection = new(connectionString);

                connection.Open();

                string sqlQuery = "SELECT * FROM orders WHERE id=@id AND client_id=@client_id";

                using SqlCommand command = new(sqlQuery, connection);

                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@client_id", clientId);

                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    orderInfo.id = reader.GetInt32(0);
                    orderInfo.clientId = reader.GetInt32(1);
                    orderInfo.orderDate = reader.GetDateTime(2).ToString("yyyy/MM/dd");
                    orderInfo.shippingFee = reader.GetDecimal(3);
                    orderInfo.deliveryAddress = reader.GetString(4);
                    orderInfo.paymentMethod = reader.GetString(5);
                    orderInfo.paymentStatus = reader.GetString(6);
                    orderInfo.orderStatus = reader.GetString(7);

                    orderInfo.orderItemInfoList = OrderInfo.GetOrderItems(orderInfo.id);
                }
                else
                {
                    Response.Redirect("/Client/Orders/Index");
                    return;
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
        }
    }
}
