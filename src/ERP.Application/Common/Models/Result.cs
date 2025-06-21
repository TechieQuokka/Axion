namespace ERP.Application.Common.Models
{
    public class Result
    {
        internal Result(bool succeeded, IEnumerable<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }
            Succeeded = succeeded;
            Errors = errors.ToArray();
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }

        public static Result Success()
        {
            return new Result(true, Array.Empty<string>());
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors);
        }
    }

    public class Result<T> : Result
    {
#pragma warning disable CS8601 // 가능한 null 참조 할당입니다.
        internal Result(bool succeeded, IEnumerable<string> errors, T data = default)
#pragma warning restore CS8601 // 가능한 null 참조 할당입니다.
            : base(succeeded, errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            Data = data;
        }

        public T Data { get; init; }

        public static Result<T> Success(T data)
        {
            return new Result<T>(true, Array.Empty<string>(), data);
        }

        public new static Result<T> Failure(IEnumerable<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }
            return new Result<T>(false, errors);
        }
    }
}
