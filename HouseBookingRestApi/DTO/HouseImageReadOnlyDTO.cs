namespace HouseBookingRestApi.DTO
{
    public record HouseImageReadOnlyDTO(
        int Id,
        int HouseId,
        string Url
        )
    {
    }
}
