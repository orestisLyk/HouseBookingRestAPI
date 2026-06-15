namespace HouseBookingRestApi.Models
{
    public class House : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string Region { get; set; }

        public float PricePerNight { get; set; }

        public int OwnerId { get; set; }

        public Owner Owner { get; set; } = null!;

        public ICollection<HouseImage> HouseImages { get; set; } = new HashSet<HouseImage>();

        public ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
    }
}
