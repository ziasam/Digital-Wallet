namespace DigitalWalletDemo.Application.Exceptions
{
    public class TransactionCooldownException : Exception
    {
        public TransactionCooldownException(string message)
            : base(message)
        {
        }

        public TransactionCooldownException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
