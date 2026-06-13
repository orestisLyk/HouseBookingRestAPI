namespace HouseBookingRestApi.Models
{
    public class Renter: BaseEntity
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
    }
}
