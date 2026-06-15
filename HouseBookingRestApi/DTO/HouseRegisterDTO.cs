using System.ComponentModel.DataAnnotations;

namespace HouseBookingRestApi.DTO
{
    public record HouseRegisterDTO(
        [property: Required(ErrorMessage = "Name is required")]
        string Name,

        [property: Required(ErrorMessage = "Description is required")]
        string Description,

        [property: Required(ErrorMessage = "Address is required")]
        string Address,

        [property: Required(ErrorMessage = "Region is required")]
        string Region,

        [property: Required(ErrorMessage = "Price per night is required")]
        [property: Range(0.01f, float.MaxValue, ErrorMessage = "Price per night must be greater than 0")]
        decimal PricePerNight,

        [property: Required(ErrorMessage = "Owner ID is required")]
        int OwnerId
        )
    {
    }
}
