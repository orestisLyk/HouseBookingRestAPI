namespace HouseBookingRestApi.Security
{
    public class EncryptionUtil : IEncryptionUtil
    {
        public string Encrypt(string clearText)
        {
            return BCrypt.Net.BCrypt.HashPassword(clearText);
        }
        public bool Verify(string clearText, string hashedText)
        {
            return BCrypt.Net.BCrypt.Verify(clearText, hashedText);
        }
    }
}
