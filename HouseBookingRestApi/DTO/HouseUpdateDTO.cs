namespace HouseBookingRestApi.DTO
{
    public record HouseUpdateDTO(
        string? Name,
        string? Description,
        string? Address,
        string? Region,
        decimal? PricePerNight
        )
    {

    }
}
