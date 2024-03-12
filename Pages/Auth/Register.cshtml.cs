using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Auth
{
    [RequiredNoAuthentication]
    [BindProperties]
    public class RegisterModel(IConfiguration configuration) : PageModel
    {
        [Required(ErrorMessage = "The 'First Name' is REQUIRED!")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "The 'Last Name' is REQUIRED!")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "The 'E-Mail' is REQUIRED!"), EmailAddress]
        public string Email { get; set; } = "";

        public string? Phone { get; set; } = "";

        [Required(ErrorMessage = "The 'Password' is REQUIRED!")]
        [MinLength(8, ErrorMessage = "The length of Password must be more that '8' characters.")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "The 'Confirm Password' is REQUIRED!")]
        [Compare("Password", ErrorMessage = "Password & Confirm Password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnGet() { }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Data Validation Failed!";
                return;
            }

            if (Phone == null)
                Phone = "";

            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                string sql =
                    "INSERT INTO users "
                    + "(firstname, lastname, email, phone, password, role) VALUES "
                    + "(@firstname, @lastname, @email, @phone, @password, 'client');";

                var passwordHasher = new PasswordHasher<IdentityUser>();
                string hashedPassword = passwordHasher.HashPassword(new IdentityUser(), Password);

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@firstname", FirstName);
                command.Parameters.AddWithValue("@lastname", LastName);
                command.Parameters.AddWithValue("@email", Email);
                command.Parameters.AddWithValue("@phone", Phone);
                command.Parameters.AddWithValue("@password", hashedPassword);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(Email))
                {
                    errorMessage = "This E-Mail is already used.";
                    return;
                }
                else
                {
                    errorMessage = ex.Message;
                }

                return;
            }

            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                string sql = "SELECT * FROM users WHERE email=@email";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@email", Email);

                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string firstName = reader.GetString(1);
                    string lastName = reader.GetString(2);
                    string email = reader.GetString(3);
                    string phone = reader.GetString(4);
                    string role = reader.GetString(6);
                    string createdAt = reader.GetDateTime(7).ToString("yyy/MM/dd");

                    HttpContext.Session.SetInt32("id", id);
                    HttpContext.Session.SetString("firstname", firstName);
                    HttpContext.Session.SetString("lastname", lastName);
                    HttpContext.Session.SetString("email", email);
                    HttpContext.Session.SetString("phone", phone);
                    HttpContext.Session.SetString("role", role);
                    HttpContext.Session.SetString("created_at", createdAt);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            successMessage = "Account Created Successfully.";
            Response.Redirect("/");
        }
    }
}
