using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Auth
{
    [RequiredNoAuthentication]
    public class ResetPasswordModel(IConfiguration configuration) : PageModel
    {
        [BindProperty, Required(ErrorMessage = "The Password is REQUIERD!")]
        [MinLength(8, ErrorMessage = "The password should be at least '8' characters!")]
        public string Password { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "The Comfirm Password is REQUIRED!")]
        [Compare("Password", ErrorMessage = "Password & Confirm Password do not match!")]
        public string ConfirmPassword { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnGet()
        {
            string? token = Request.Query["token"];

            if (string.IsNullOrEmpty(token))
            {
                Response.Redirect("/");
                return;
            }
        }

        public void OnPost()
        {
            string? token = Request.Query["token"];

            if (string.IsNullOrEmpty(token))
            {
                Response.Redirect("/");
                return;
            }

            if (!ModelState.IsValid)
            {
                errorMessage = "Data Validation Failed!";
                return;
            }

            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                string email = "";
                string sql = "SELECT * FROM reset_password WHERE token=@token";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@token", token);

                using SqlDataReader reader = command.ExecuteReader();

                // * Check if the token is valid or not
                if (reader.Read())
                {
                    email = reader.GetString(0);
                }
                else
                {
                    errorMessage = "The token is no Valid.";
                    return;
                }

                var passwordHasher = new PasswordHasher<IdentityUser>();
                string hashedPassword = passwordHasher.HashPassword(new IdentityUser(), Password);

                sql = "UPDATE users SET password=@password WHERE email=@email";
                using SqlCommand resetPassCommand = new(sql, connection);
                resetPassCommand.Parameters.AddWithValue("@password", hashedPassword);
                resetPassCommand.Parameters.AddWithValue("email", email);

                command.ExecuteNonQuery();

                sql = "DELETE FROM reset_password WHERE email=@email";
                using SqlCommand deleteTokenCommand = new(sql, connection);
                deleteTokenCommand.Parameters.AddWithValue("@email", email);

                deleteTokenCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            successMessage = "Password Successfully Reset";
        }
    }
}
