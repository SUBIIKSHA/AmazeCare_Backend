namespace AmazeCareAPI.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName)
            : base($"No {entityName} were found.")
        {
        }
    }
}
