namespace nauth_asp.Exceptions
{
    public class NauthException : Exception
    {
        public WrResponseStatus Status { get; }

        public NauthException(WrResponseStatus status) : base()
        {
            Status = status;
        }

        public NauthException(WrResponseStatus status, string message) : base(message)
        {
            Status = status;
        }

        public NauthException(WrResponseStatus status, string message, Exception innerException) : base(message, innerException)
        {
            Status = status;
        }
    }
}
