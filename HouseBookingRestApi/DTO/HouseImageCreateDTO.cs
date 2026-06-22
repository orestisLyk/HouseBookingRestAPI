using System.ComponentModel.DataAnnotations;

namespace HouseBookingRestApi.DTO
{
    public record HouseImageCreateDTO(
        [Required(ErrorMessage = "HouseId is required")]
        [Range(0, int.MaxValue, ErrorMessage = "HouseId must be a positive integer")]
        int HouseId,
        [Required(ErrorMessage = "File is required")]
        IFormFile File
        )
    {

    }
}
