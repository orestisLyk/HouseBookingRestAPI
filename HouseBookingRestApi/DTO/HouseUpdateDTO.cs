namespace HouseBookingRestApi.DTO
{
    public record HouseUpdateDTO(
        int Id,
        string? Name,
        string? Description,
        string? Address,
        string? Region,
        float? PricePerNight

        )
    {

    }
}
