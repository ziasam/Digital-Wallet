namespace DigitalWalletDemo.Application.Exceptions
{
    public class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException(string message)
            : base(message)
        {
        }

        public InsufficientBalanceException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
