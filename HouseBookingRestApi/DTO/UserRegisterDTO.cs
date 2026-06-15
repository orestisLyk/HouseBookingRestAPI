using System.ComponentModel.DataAnnotations;

namespace HouseBookingRestApi.DTO
{
    public record UserRegisterDTO(
        [property: Required(ErrorMessage = "Username is required")]
        [property: StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        string Username,
        [property: Required(ErrorMessage = "Password is required")]
        [property: StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        string Password,
        [property: Required(ErrorMessage = "Email is required")]
        [property: EmailAddress(ErrorMessage = "Invalid email address")]
        string Email,
        [property: Required(ErrorMessage = "First name is required")]
        [property: StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        string FirstName,
        [property: Required(ErrorMessage = "Last name is required")]
        [property: StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        string LastName,
        [property: Required(ErrorMessage = "Role ID is required")]
        [property: Range(0, 2 , ErrorMessage = "Role ID must be 0, 1, or 2")]
        int RoleId
        )
    {
    }
}
