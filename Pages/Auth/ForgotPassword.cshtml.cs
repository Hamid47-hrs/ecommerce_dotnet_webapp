using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Auth
{
    [RequiredNoAuthentication]
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty, Required(ErrorMessage = "The E-Mail is REQUIERD!"), EmailAddress]
        public string Email { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet() { }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Data Validation Failed!";
                return;
            }

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);
                connection.Open();

                string sql = "SELECT * FROM users WHERE email=@email";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@email", Email);

                using SqlDataReader reader = command.ExecuteReader();

                // * Check the user validation with email.
                if (reader.Read())
                {
                    string token = Guid.NewGuid().ToString();

                    SaveToken(Email, token);

                    string ResetUrl = Url.PageLink("/Auth/ResetPassword") + "?token" + token;

                    // * Send the reset password E-Mail with token to the user.
                    // ! #### IT MUST BE IMPLEMENTED ###  !//
                }
                else
                {
                    errorMessage = "There is no user with this E-Mail.";
                    return;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }
        }

        private void SaveToken(string email, string token)
        {
            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "DELETE FROM reset_password WHERE email=@email";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);

                        command.ExecuteNonQuery();
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql =
                        "INSERT INTO reset_password (email, token) VALUES (@email, @token)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@token", token);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
