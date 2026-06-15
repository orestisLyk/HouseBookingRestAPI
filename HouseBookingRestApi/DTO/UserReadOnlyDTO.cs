namespace HouseBookingRestApi.DTO
{
    public record UserReadOnlyDTO(
        int Id,
        string Username,
        string Email,
        string FirstName,
        string LastName,
        string RoleName
        )
    {
    }
}
