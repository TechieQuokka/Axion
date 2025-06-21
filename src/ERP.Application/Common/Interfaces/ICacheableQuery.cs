namespace ERP.Application.Common.Interfaces
{
    public interface ICacheableQuery
    {
        bool UseCache { get; }
        TimeSpan SlidingExpiration { get; }
        TimeSpan AbsoluteExpiration { get; }
    }
}
