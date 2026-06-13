namespace HouseBookingRestApi.Models
{
    public class User: BaseEntity
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;

        public Renter? Renter { get; set; }
    }
}
