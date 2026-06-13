namespace HouseBookingRestApi.Models
{
    public class Booking: BaseEntity
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int HouseId { get; set; }

        public House House { get; set; } = null!;

        public int RenterId { get; set; }

        public Renter Renter { get; set; } = null!;
    }
}
