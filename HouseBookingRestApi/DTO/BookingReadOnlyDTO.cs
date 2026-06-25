namespace HouseBookingRestApi.DTO
{
    public record BookingReadOnlyDTO(
        int Id,
        DateOnly StartDate,
        DateOnly EndDate,
        int HouseId,
        int RenterId,
        string HouseName  
        )
    {
    }
}
