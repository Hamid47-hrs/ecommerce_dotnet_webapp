using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Admin.Books
{
    [RequiredAuthentication(RequiredRole = "admin")]
    public class CreateModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "The Title of the Book is REQUIRED!")]
        [MaxLength(
            100,
            ErrorMessage = "The length of Title should not be more than '100' characters."
        )]
        public string Title { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The Author of the Book is REQUIRED!")]
        [MaxLength(
            255,
            ErrorMessage = "The length of Author should not be more than '255' characters."
        )]
        public string Authors { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The ISBN of the Book is REQUIRED!")]
        [MaxLength(
            20,
            ErrorMessage = "The length of ISBN should not be more than '20' characters."
        )]
        public string ISBN { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The Number of the Pages of the Book is REQUIRED!")]
        [Range(
            1,
            10000,
            ErrorMessage = "The range of the Number of the Pages should be between '1' to '1000'."
        )]
        public int NumPages { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "The Price of the Book is REQUIRED!")]
        public decimal Price { get; set; }

        [BindProperty, Required]
        public string Category { get; set; } = "";

        [BindProperty]
        [Required(ErrorMessage = "The Image of the Book is REQUIRED!")]
        public IFormFile ImageFile { get; set; }

        [BindProperty]
        [MaxLength(
            1000,
            ErrorMessage = "The Description of the Book should not be more than '10000' characters."
        )]
        public string Description { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        private IWebHostEnvironment webHostEnvironment;

        public CreateModel(IWebHostEnvironment env)
        {
            webHostEnvironment = env;
        }

        public void OnGet() { }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Failed to Create the Book.";
                return;
            }

            if (Description == null) // OR Description ??= "";
                Description = "";

            string newFileName = DateTime.Now.ToString("yyyMMddHHmmss");
            newFileName += Path.GetExtension(ImageFile.FileName);
            string imageFolder = webHostEnvironment.WebRootPath + "/images/books/";

            string imageFullPath = Path.Combine(imageFolder, newFileName);

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                ImageFile.CopyTo(stream);
            }

            try
            {
                string connectionString =
                    "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;User ID=myDomain\\sa;Password=tlou2;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql =
                        "INSERT INTO books_1"
                        + "(title, authors, isbn, num_pages, price, category, description, image_filename) VALUES"
                        + "(@title, @authors, @isbn, @num_pages, @price, @category, @description, @image_filename)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@title", Title);
                        command.Parameters.AddWithValue("@authors", Authors);
                        command.Parameters.AddWithValue("@isbn", ISBN);
                        command.Parameters.AddWithValue("@num_pages", NumPages);
                        command.Parameters.AddWithValue("@price", Price);
                        command.Parameters.AddWithValue("@category", Category);
                        command.Parameters.AddWithValue("@description", Description);
                        command.Parameters.AddWithValue("@image_filename", newFileName);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            Response.Redirect("/Admin/Books/Index");
        }
    }
}
