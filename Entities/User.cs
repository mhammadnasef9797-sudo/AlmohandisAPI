namespace AlmohandisAPI.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        // لن نخزن كلمة المرور كنص عادي أبداً
        // هذان الحقلان لتخزين كلمة المرور بعد تشفيرها
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Role { get; set; } = "Member";
    }
}