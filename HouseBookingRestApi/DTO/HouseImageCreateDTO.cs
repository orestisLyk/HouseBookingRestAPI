using System.ComponentModel.DataAnnotations;

namespace HouseBookingRestApi.DTO
{
    public record HouseImageCreateDTO(
        [property: Required(ErrorMessage = "HouseId is required")]
        [property: Range(0, int.MaxValue, ErrorMessage = "HouseId must be a positive integer")]
        int HouseId,
        [property: Required(ErrorMessage = "File is required")]
        IFormFile File
        )
    {

    }
}
