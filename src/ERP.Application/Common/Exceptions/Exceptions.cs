using FluentValidation.Results;

namespace ERP.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
            : base()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        public ValidationException(string propertyName, string errorMessage)
            : this()
        {
            Errors = new Dictionary<string, string[]>
            {
                { propertyName, new[] { errorMessage } }
            };
        }

        public ValidationException(Dictionary<string, string[]> errors)
            : this()
        {
            Errors = errors;
        }

        public IDictionary<string, string[]> Errors { get; }
    }

    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException() : base() { }

        public ForbiddenAccessException(string message)
            : base(message) { }

        public ForbiddenAccessException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
