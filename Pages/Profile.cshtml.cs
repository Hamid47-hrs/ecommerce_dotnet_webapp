using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages
{
    [RequiredAuthentication]
    [BindProperties]
    public class ProfileModel(IConfiguration configuration) : PageModel
    {
        [Required(ErrorMessage = "The First Name is REQUIRED!")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "The Last Name is REQUIRED!")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "The E-Mail is REQUIRED!"), EmailAddress]
        public string Email { get; set; } = "";
        public string? Phone { get; set; } = "";
        public string? Password { get; set; } = "";
        public string? ConfirmPassword { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnGet()
        {
            // * (?? "") If return NULL value replaces with empty string. (!) Ensures than return a value.
            FirstName = HttpContext.Session.GetString("firstname") ?? "";
            LastName = HttpContext.Session.GetString("lastname") ?? "";
            Email = HttpContext.Session.GetString("email") ?? "";
            Phone = HttpContext.Session.GetString("phone");
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Data Validation Failed!";
                return;
            }

            Phone ??= "";

            string submitButton = Request.Form["action"]!;

            int? userId = HttpContext.Session.GetInt32("id");

            if (submitButton.Equals("profile"))
            {
                try
                {
                    using SqlConnection connection = new(connectionString);
                    connection.Open();

                    string sql =
                        "UPDATE users SET firstname=@firstname, lastname=@lastname, email=@email, phone=@phone WHERE id=@id";

                    using SqlCommand command = new(sql, connection);

                    command.Parameters.AddWithValue("@firstname", FirstName);
                    command.Parameters.AddWithValue("@lastname", LastName);
                    command.Parameters.AddWithValue("@email", Email);
                    command.Parameters.AddWithValue("@phone", Phone);
                    command.Parameters.AddWithValue("@id", userId);

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return;
                }

                HttpContext.Session.SetString("firstname", FirstName);
                HttpContext.Session.SetString("lastname", LastName);
                HttpContext.Session.SetString("email", Email);
                HttpContext.Session.SetString("phone", Phone);

                successMessage = "Profile Updated Successfully.";
            }
            else if (submitButton.Equals("password"))
            {
                if (Password == null || Password.Length < 8)
                {
                    errorMessage = "Password Length should be more than '8' characters.";
                    return;
                }

                if (ConfirmPassword == null || !ConfirmPassword.Equals(Password))
                {
                    errorMessage = "Password & Confirm Password does not match.";
                    return;
                }

                try
                {
                    using SqlConnection connection = new(connectionString);
                    connection.Open();

                    string sql = "UPDATE users SET password=@password WHERE id=@id";

                    var passwordHasher = new PasswordHasher<IdentityUser>();
                    string hashedPassowrd = passwordHasher.HashPassword(
                        new IdentityUser(),
                        Password
                    );

                    using SqlCommand command = new(sql, connection);
                    command.Parameters.AddWithValue("@password", hashedPassowrd);
                    command.Parameters.AddWithValue("@id", userId);

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return;
                }

                successMessage = "Password Updated Successfully.";
            }
        }
    }
}
