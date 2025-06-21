namespace ERP.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Identity system user ID (string) - NULL 안전 처리
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Identity system user ID (string) - NULL 안전 처리
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Business domain user ID (int) - ERP 시스템 내 사용자 ID (선택적)
        /// </summary>
        public int? CreatedByUserId { get; set; }

        /// <summary>
        /// Business domain user ID (int) - ERP 시스템 내 사용자 ID (선택적)
        /// </summary>
        public int? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }
    }

    public abstract class BaseAuditableEntity : BaseEntity
    {
        private readonly List<BaseEvent> _domainEvents = new();

        public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(BaseEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(BaseEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
