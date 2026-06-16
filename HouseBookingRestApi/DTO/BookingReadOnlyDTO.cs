namespace HouseBookingRestApi.DTO
{
    public record BookingReadOnlyDTO(
        int Id,
        DateTime StartDate,
        DateTime EndDate,
        int HouseId,
        int RenterId
        )
    {
    }
}
