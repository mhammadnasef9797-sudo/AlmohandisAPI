using System.ComponentModel.DataAnnotations;

namespace AlmohandisAPI.DTOs
{
    public class UserRegisterDto
    {
        [Required] // يعني أن هذا الحقل إجباري
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress] // للتأكد من أن النص المدخل هو بريد إلكتروني صالح
        public string Email { get; set; }

        [Required] // <-- ▼▼▼ أضف هذا القسم ▼▼▼
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(6)] // الحد الأدنى لطول كلمة المرور
        public string Password { get; set; }
    }
}