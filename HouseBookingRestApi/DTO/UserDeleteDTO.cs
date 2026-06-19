using System.ComponentModel.DataAnnotations;

namespace HouseBookingRestApi.DTO
{
    public record UserDeleteDTO(
        [Required]
        int userId)
    {
    }
}
