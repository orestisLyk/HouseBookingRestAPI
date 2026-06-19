using System.ComponentModel.DataAnnotations;

namespace HouseBookingRestApi.DTO
{
    public record UserRegisterDTO(
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        string Username,

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        string Password,

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        string Email,

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        string FirstName,

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        string LastName,

        [Required(ErrorMessage = "Role ID is required")]
        [Range(0, 2, ErrorMessage = "Role ID must be 0, 1, or 2")]
        int RoleId
    );
}
