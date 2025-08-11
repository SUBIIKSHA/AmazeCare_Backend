namespace AmazeCareAPI.Exceptions
{
    public class NoSuchEntityException : Exception
    {
        public NoSuchEntityException()
            : base("The requested entity was not found.") { }

        public NoSuchEntityException(string message)
            : base(message) { }
    }
}
