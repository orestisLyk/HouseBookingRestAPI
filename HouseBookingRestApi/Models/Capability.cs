namespace HouseBookingRestApi.Models
{
    public class Capability
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Role> Roles { get; set; } = new HashSet<Role>();
    }
}
