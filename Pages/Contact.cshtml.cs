using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{
    public class ContactModel : PageModel
    {
        public void OnGet() { }

        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Email { get; set; } = "";

        public string? Phone { get; set; } = "";

        public string Subject { get; set; } = "";

        public string Message { get; set; } = "";

        public string SuccessMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";

        public void OnPost()
        {
            FirstName = Request.Form["firstname"];
            LastName = Request.Form["lastname"];
            Email = Request.Form["email"];
            Phone = Request.Form["phone"];
            Subject = Request.Form["subject"];
            Message = Request.Form["message"];

            if (
                FirstName.Length == 0
                || LastName.Length == 0
                || Email.Length == 0
                || Subject.Length == 0
                || Message.Length == 0
            )
            {
                ErrorMessage = "Please fill all required fields.";
                return;
            }

            SuccessMessage = "Your Form is submitted successfully.";
        }
    }
}
