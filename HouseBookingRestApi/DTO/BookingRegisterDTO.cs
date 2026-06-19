using System.ComponentModel.DataAnnotations;

namespace HouseBookingRestApi.DTO
{
    public record BookingRegisterDTO(
        [Range(0, int.MaxValue)]
        int HouseId,

        [Range(0, int.MaxValue)]
        int RenterId,

        DateTime StartDate,
        DateTime EndDate
    );
}
