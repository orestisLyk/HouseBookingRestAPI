using System.ComponentModel.DataAnnotations;

namespace HouseBookingRestApi.DTO
{
    public record HouseRegisterDTO(
        [Required(ErrorMessage = "Name is required")]
        string Name,

        [Required(ErrorMessage = "Description is required")]
        string Description,

        [Required(ErrorMessage = "Address is required")]
        string Address,

        [Required(ErrorMessage = "Region is required")]
        string Region,

        [Required(ErrorMessage = "Price per night is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price per night must be greater than 0")]
        decimal PricePerNight,

        [Required(ErrorMessage = "Owner ID is required")]
        int OwnerId
    );
}
