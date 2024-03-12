using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ecommerce_dotnet_webapp.Pages
{
    public class ContactModel(IConfiguration configuration) : PageModel
    {
        public void OnGet() { }

        [BindProperty]
        [Required(ErrorMessage = "The First Name is required.")]
        [Display(Name = "First Name *")]
        public string FirstName { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "The Last Name is required.")]
        [Display(Name = "Last Name *")]
        public string LastName { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "The Email is required.")]
        [Display(Name = "E-mail *")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [BindProperty]
        public string? Phone { get; set; } = "";

        [BindProperty, Required]
        [Display(Name = "Subject *")]
        public string Subject { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "The Message is required.")]
        [MinLength(5, ErrorMessage = "The Message should be at least 5 characters.")]
        [MaxLength(1024, ErrorMessage = "The Message should be at Most 1024 characters.")]
        [Display(Name = "Message *")]
        public string Message { get; set; } = "";

        public List<SelectListItem> SubjectList { get; } =
            new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "Order Status",
                    Text = "Order Status",
                    Selected = true
                },
                new SelectListItem { Value = "Refund Request", Text = "Refund Request" },
                new SelectListItem { Value = "Job Application", Text = "Job Application" },
                new SelectListItem { Value = "Other", Text = "Other" },
            };
        public string SuccessMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fill all required fields.";
                return;
            }

            if (Phone == null)
                Phone = "";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql =
                        "INSERT INTO messages"
                        + "(first_name, last_name, email, phone, subject, message) VALUES "
                        + "(@firstname, @lastname, @email, @phone, @subject, @message);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", FirstName);
                        command.Parameters.AddWithValue("@lastname", LastName);
                        command.Parameters.AddWithValue("@email", Email);
                        command.Parameters.AddWithValue("@phone", Phone);
                        command.Parameters.AddWithValue("@subject", Subject);
                        command.Parameters.AddWithValue("@message", Message);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return;
            }

            SuccessMessage = "Your Form is submitted successfully.";

            FirstName = "";
            LastName = "";
            Email = "";
            Phone = "";
            Subject = "";
            Message = "";

            ModelState.Clear();
        }
    }
}
