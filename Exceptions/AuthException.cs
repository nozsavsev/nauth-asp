namespace nauth_asp.Exceptions
{
    public class NauthException : Exception
    {
        public WrResponseStatus Status { get; }
        public AuthFailureReasons? Reason { get; set; }

        public NauthException(WrResponseStatus status) : base()
        {
            Status = status;
        }

        public NauthException(WrResponseStatus status, AuthFailureReasons reason) : base()
        {
            Status = status;
            Reason = reason;
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
