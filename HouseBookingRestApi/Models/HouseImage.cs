namespace HouseBookingRestApi.Models
{
    public class HouseImage : BaseEntity
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public int HouseId { get; set; }

        public House House { get; set; } = null!;
    }
}
