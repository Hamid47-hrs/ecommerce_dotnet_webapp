using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ecommerce_dotnet_webapp.Pages.Admin.Users.IndexModel;

namespace ecommerce_dotnet_webapp.Pages.Admin
{
    [RequiredAuthentication(RequiredRole = "admin")]
    public class DetailsModel : PageModel
    {
        public OrderInfo orderInfo = new();
        public UserInfo userInfo = new();
        public string errorMessage = "";

        public void OnGet(int id) //* Read ID of the order from page URL with inject method.
        {
            if (id < 1)
            {
                Response.Redirect("/Admin/Orders/Index");
                return;
            }

            string paymentStatus = Request.Query["payment_status"];
            string orderStatus = Request.Query["order_status"];

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);

                connection.Open();

                if (paymentStatus != null)
                {
                    string sqlUpdate =
                        "UPDATE orders SET payment_status=@payment_status WHERE id=@id";

                    using SqlCommand updatePaymentCommand = new(sqlUpdate, connection);
                    updatePaymentCommand.Parameters.AddWithValue("@payment_status", paymentStatus);
                    updatePaymentCommand.Parameters.AddWithValue("@id", id);

                    updatePaymentCommand.ExecuteNonQuery();
                }

                if (orderStatus != null)
                {
                    string sqlUpdate = "UPDATE orders SET order_status=@order_status WHERE id=@id";

                    using SqlCommand updateOrderCommand = new(sqlUpdate, connection);
                    updateOrderCommand.Parameters.AddWithValue("@order_status", orderStatus);
                    updateOrderCommand.Parameters.AddWithValue("@id", id);

                    updateOrderCommand.ExecuteNonQuery();
                }

                string sqlQuery = "SELECT * FROM orders WHERE id=@id";

                using SqlCommand command = new(sqlQuery, connection);

                command.Parameters.AddWithValue("@id", id);

                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    orderInfo.id = reader.GetInt32(0);
                    orderInfo.clientId = reader.GetInt32(1);
                    orderInfo.orderDate = reader.GetDateTime(2).ToString("yyy/MM/dd");
                    orderInfo.shippingFee = reader.GetDecimal(3);
                    orderInfo.deliveryAddress = reader.GetString(4);
                    orderInfo.paymentMethod = reader.GetString(5);
                    orderInfo.paymentStatus = reader.GetString(6);
                    orderInfo.orderStatus = reader.GetString(7);

                    orderInfo.orderItemInfoList = OrderInfo.getOrderItems(orderInfo.id);
                }
                else
                {
                    Response.Redirect("/Admin/Orders/Index");
                    return;
                }

                reader.Close();

                sqlQuery = "SELECT * FROM users WHERE id=@id";

                using SqlCommand clientCommand = new(sqlQuery, connection);

                clientCommand.Parameters.AddWithValue("@id", orderInfo.clientId);

                using SqlDataReader clientDataReader = clientCommand.ExecuteReader();

                if (clientDataReader.Read())
                {
                    userInfo.id = clientDataReader.GetInt32(0);
                    userInfo.firstName = clientDataReader.GetString(1);
                    userInfo.lastName = clientDataReader.GetString(2);
                    userInfo.email = clientDataReader.GetString(3);
                    userInfo.phone = clientDataReader.GetString(4);
                    userInfo.role = clientDataReader.GetString(6);
                    userInfo.createdAt = clientDataReader.GetDateTime(7).ToString("yyyy/MM/dd");
                }
                else
                {
                    errorMessage =
                        "The Client with ID:\"" + orderInfo.clientId + "\" hase not found!";
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
