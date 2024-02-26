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
    public class LoginModel : PageModel
    {
        [Required(ErrorMessage = "The E-Mail is REQUIRED!"), EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "The Password is REQUIRED!")]
        public string Password { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        /*
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);

            if (HttpContext.Session.GetString("role") != null)
            {
                context.Result = new RedirectResult("/");
            }
        }
        */

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
                if (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string firstName = reader.GetString(1);
                    string lastName = reader.GetString(2);
                    string email = reader.GetString(3);
                    string phone = reader.GetString(4);
                    string hashedPassword = reader.GetString(5);
                    string role = reader.GetString(6);
                    string createdAt = reader.GetDateTime(7).ToString("yyy/MM/dd");

                    var passwordHasher = new PasswordHasher<IdentityUser>();
                    var result = passwordHasher.VerifyHashedPassword(
                        new IdentityUser(),
                        hashedPassword,
                        Password
                    );

                    if (
                        result == PasswordVerificationResult.Success
                        || result == PasswordVerificationResult.SuccessRehashNeeded
                    )
                    {
                        HttpContext.Session.SetInt32("id", id);
                        HttpContext.Session.SetString("firstname", firstName);
                        HttpContext.Session.SetString("lastname", lastName);
                        HttpContext.Session.SetString("email", email);
                        HttpContext.Session.SetString("phone", phone);
                        HttpContext.Session.SetString("role", role);
                        HttpContext.Session.SetString("created_at", createdAt);

                        Response.Redirect("/");
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            errorMessage = "Wrong E-Mail or Password!";
        }
    }
}
