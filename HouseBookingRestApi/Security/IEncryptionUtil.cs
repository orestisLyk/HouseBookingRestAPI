namespace HouseBookingRestApi.Security
{
    public interface IEncryptionUtil
    {
        string Encrypt(string clearText);

        bool Verify(string clearText, string hashedText);

    }
}
