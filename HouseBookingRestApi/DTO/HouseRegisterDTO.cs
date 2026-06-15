namespace HouseBookingRestApi.DTO
{
    public record HouseRegisterDTO(
        string Name,
        string Description,
        string Address,
        decimal PricePerNight,
        int OwnerId
        )
    {
    }
}
