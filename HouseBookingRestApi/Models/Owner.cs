namespace HouseBookingRestApi.Models
{
    public class Owner: BaseEntity
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public ICollection<House> Houses { get; set; } = new HashSet<House>();
    }
}
