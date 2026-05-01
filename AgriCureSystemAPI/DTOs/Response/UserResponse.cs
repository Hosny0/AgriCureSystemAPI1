namespace AgriCureSystemAPI.DTOs.Response
{
    public class UserResponse
    {
        public string Id { get; set; }
        public string FirstName { get; set; } // حرف F كبير
        public string LastName { get; set; }  // حرف L كبير
        public string Email { get; set; }     // ضيف الإيميل لو محتاجه
        public string UserName { get; set; }  // ضيف الـ UserName لو محتاجه
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }

    }
}
