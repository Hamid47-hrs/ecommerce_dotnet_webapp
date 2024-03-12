using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using ecommerce_dotnet_webapp.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ecommerce_dotnet_webapp.Pages.Admin.Books
{
    [RequiredAuthentication(RequiredRole = "admin")]
    public class EditModel(IWebHostEnvironment env, IConfiguration configuration) : PageModel
    {
        [BindProperty]
        public int Id { get; set; }

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
        public IFormFile? ImageFile { get; set; }

        [BindProperty]
        public string ImageFileName { get; set; } = "";

        [BindProperty]
        [MaxLength(
            1000,
            ErrorMessage = "The Description of the Book should not be more than '10000' characters."
        )]
        public string? Description { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        public IWebHostEnvironment webHostEnvironment = env;
        private readonly string connectionString = configuration.GetConnectionString(
            "DefaultConnection"
        )!;

        public void OnGet()
        {
            string? requestId = Request.Query["id"];

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM books_1 WHERE id=@id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", requestId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Id = reader.GetInt32(0);
                                Title = reader.GetString(1);
                                Authors = reader.GetString(2);
                                ISBN = reader.GetString(3);
                                NumPages = reader.GetInt32(4);
                                Price = reader.GetDecimal(5);
                                Category = reader.GetString(6);
                                Description = reader.GetString(7);
                                ImageFileName = reader.GetString(8);
                            }
                            else
                            {
                                Response.Redirect("/Admin/Books/Index");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/Admin/Books/Index");
            }
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Data Validation Faild!";
                return;
            }

            if (Description == null)
                Description = "";

            string newImageFileName = ImageFileName;

            if (ImageFile != null)
            {
                newImageFileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                newImageFileName += Path.GetExtension(ImageFile.FileName);

                string imageFolder = webHostEnvironment.WebRootPath + "/images/books/";
                string newImageFullPath = Path.Combine(imageFolder, newImageFileName);

                using (var stream = System.IO.File.Create(newImageFullPath))
                {
                    ImageFile.CopyTo(stream);
                }

                string oldImageFullPath = Path.Combine(imageFolder, ImageFileName);
                System.IO.File.Delete(oldImageFullPath);

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string sql =
                            "UPDATE books_1 SET title=@title, authors=@authors, isbn=@isbn, num_pages=@num_pages, price=@price, category=@category, description=@description, image_filename=@image_filename WHERE id=@id";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@id", Id);
                            command.Parameters.AddWithValue("@title", Title);
                            command.Parameters.AddWithValue("@authors", Authors);
                            command.Parameters.AddWithValue("@isbn", ISBN);
                            command.Parameters.AddWithValue("@num_pages", NumPages);
                            command.Parameters.AddWithValue("@price", Price);
                            command.Parameters.AddWithValue("@category", Category);
                            command.Parameters.AddWithValue("@description", Description);
                            command.Parameters.AddWithValue("@image_filename", newImageFileName);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    errorMessage = ex.Message;
                    return;
                }

                successMessage = "Book updated successfully.";
                Response.Redirect("/Admin/Books/Index");
            }
        }
    }
}
