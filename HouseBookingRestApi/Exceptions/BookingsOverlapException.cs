namespace HouseBookingRestApi.Exceptions
{
    public class BookingsOverlapException : Exception
    {
        public BookingsOverlapException(string message) : base(message)
        {
        }
    }
}
