using System.ComponentModel.DataAnnotations;

namespace HouseBookingRestApi.DTO
{
    public record BookingRegisterDTO(
        [property: Range(0, int.MaxValue)]
        int HouseId,
        [property: Range(0, int.MaxValue)]
        int RenterId,
        DateTime StartDate,
        DateTime EndDate
        )
    {
    }
}
