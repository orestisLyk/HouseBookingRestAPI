using HouseBookingRestApi.Models;

namespace HouseBookingRestApi.DTO
{
    public record HouseReadOnlyDTO
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public string Address { get; init; }
        public string Region { get; init; }
        public decimal PricePerNight { get; init; }
        public List<string> ImageUrls { get; init; } = new();
        public int OwnerId { get; init; }
    }
}
