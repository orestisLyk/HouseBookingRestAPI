namespace HouseBookingRestApi.Exceptions
{
    public class EntityNotAuthorizedException : Exception
    {
        public EntityNotAuthorizedException(string message) : base(message) { }
    }
}
