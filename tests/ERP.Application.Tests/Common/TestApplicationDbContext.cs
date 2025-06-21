using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Entities;
using ERP.Domain.Common;
using ERP.Infrastructure.Identity;
using System.Reflection;

namespace ERP.Application.Tests.Common
{
    public class TestApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public TestApplicationDbContext(
            DbContextOptions<TestApplicationDbContext> options,
            ICurrentUserService currentUserService,
            IDateTime dateTime) : base(options)
        {
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        // DbSets
        public DbSet<Company> Companies => Set<Company>();
        public new DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
        public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BaseEvent를 Entity Framework 모델에서 제외
            modelBuilder.Ignore<BaseEvent>();

            // Apply all configurations from Infrastructure assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ERP.Infrastructure.Data.ApplicationDbContext).Assembly);

            // Identity 테이블 이름 설정
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("ERPUsers");
            });

            // 테스트용: Global Query Filter 적용하지 않음
            // 모든 데이터에 접근 가능
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // CreatedBy는 string 타입이므로 IdentityUserId 사용
                        entry.Entity.CreatedBy = _currentUserService?.IdentityUserId ?? "system";
                        entry.Entity.CreatedAt = _dateTime.UtcNow;

                        // CreatedByUserId는 int? 타입이므로 BusinessUserId 사용 (테스트에서는 선택적)
                        if (_currentUserService?.IsAuthenticated == true)
                        {
                            try
                            {
                                entry.Entity.CreatedByUserId = _currentUserService.BusinessUserId;
                            }
                            catch
                            {
                                // 테스트 환경에서 BusinessUserId 조회 실패 시 null로 유지
                                entry.Entity.CreatedByUserId = null;
                            }
                        }
                        break;

                    case EntityState.Modified:
                        // UpdatedBy는 string 타입이므로 IdentityUserId 사용
                        entry.Entity.UpdatedBy = _currentUserService?.IdentityUserId ?? "system";
                        entry.Entity.UpdatedAt = _dateTime.UtcNow;

                        // UpdatedByUserId는 int? 타입이므로 BusinessUserId 사용 (테스트에서는 선택적)
                        if (_currentUserService?.IsAuthenticated == true)
                        {
                            try
                            {
                                entry.Entity.UpdatedByUserId = _currentUserService.BusinessUserId;
                            }
                            catch
                            {
                                // 테스트 환경에서 BusinessUserId 조회 실패 시 null로 유지
                                entry.Entity.UpdatedByUserId = null;
                            }
                        }
                        break;
                }
            }

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                // BaseAuditableEntity에서 이미 처리된 것은 제외
                if (entry.Entity is BaseAuditableEntity)
                    continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        // CreatedBy는 string 타입이므로 IdentityUserId 사용
                        entry.Entity.CreatedBy = _currentUserService?.IdentityUserId ?? "system";
                        entry.Entity.CreatedAt = _dateTime.UtcNow;

                        // CreatedByUserId는 int? 타입이므로 BusinessUserId 사용 (테스트에서는 선택적)
                        if (_currentUserService?.IsAuthenticated == true)
                        {
                            try
                            {
                                entry.Entity.CreatedByUserId = _currentUserService.BusinessUserId;
                            }
                            catch
                            {
                                // 테스트 환경에서 BusinessUserId 조회 실패 시 null로 유지
                                entry.Entity.CreatedByUserId = null;
                            }
                        }
                        break;

                    case EntityState.Modified:
                        // UpdatedBy는 string 타입이므로 IdentityUserId 사용
                        entry.Entity.UpdatedBy = _currentUserService?.IdentityUserId ?? "system";
                        entry.Entity.UpdatedAt = _dateTime.UtcNow;

                        // UpdatedByUserId는 int? 타입이므로 BusinessUserId 사용 (테스트에서는 선택적)
                        if (_currentUserService?.IsAuthenticated == true)
                        {
                            try
                            {
                                entry.Entity.UpdatedByUserId = _currentUserService.BusinessUserId;
                            }
                            catch
                            {
                                // 테스트 환경에서 BusinessUserId 조회 실패 시 null로 유지
                                entry.Entity.UpdatedByUserId = null;
                            }
                        }
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}