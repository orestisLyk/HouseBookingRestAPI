namespace HouseBookingRestApi.DTO
{
    public record HouseReadOnlyDTO(
        int Id,
        string Name,
        string Description,
        string Address,
        string Region,
        decimal PricePerNight,
        string OwnerFullName
        )
    {
    }
}
