using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Admin.Users
{
    [RequiredAuthentication(RequiredRole = "admin")]
    [BindProperties]
    public class EditRoleModel() : PageModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }

        [Required(ErrorMessage = "The Role is REQUIRED!")]
        public string Role { get; set; } = "";
        public string CreatedAt { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            string? requestId = Request.Query["id"];

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);
                connection.Open();

                string sql = "SELECT * FROM users WHERE id=@id";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", requestId);

                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Id = reader.GetInt32(0);
                    FirstName = reader.GetString(1);
                    LastName = reader.GetString(2);
                    Email = reader.GetString(3);
                    Phone = reader.GetString(4);
                    Role = reader.GetString(6);
                    CreatedAt = reader.GetDateTime(7).ToString("yyy/MM/dd");
                }
                else
                {
                    Response.Redirect("/Admin/Users/Index");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/Admin/Users/Index");
            }
        }

        public void OnPost()
        {
            /* if (!ModelState.IsValid)
            {
                errorMessage = "Data Validation Faild!";
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                return;
            } */

            int currentUserId = (int)HttpContext.Session.GetInt32("id")!;

            if (Id == currentUserId)
            {
                errorMessage = "You Can not change your own role!";
                return;
            }

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using SqlConnection connection = new(connectionString);
                connection.Open();

                string sql = "UPDATE users SET role=@role WHERE id=@id";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@id", Id);
                command.Parameters.AddWithValue("@role", Role);

                command.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            successMessage = "User updated successfully.";
            Response.Redirect("/Admin/Users/Index");
        }
    }
}
