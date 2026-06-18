namespace HouseBookingRestApi.Exceptions
{
    public class EntityForbiddenException : Exception
    {
        public EntityForbiddenException() { }

        public EntityForbiddenException(string message) : base(message) { }
    }
}
