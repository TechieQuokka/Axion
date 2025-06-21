using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Entities;
using ERP.Domain.Common;
using ERP.Infrastructure.Identity;
using System.Reflection;

namespace ERP.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService,
            IDateTime dateTime) : base(options)
        {
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        // DbSets - 명확한 이름으로 분리
        public DbSet<Company> Companies => Set<Company>();

        /// <summary>
        /// Business domain users (ERP Users)
        /// </summary>
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
            // Identity 테이블들을 먼저 설정
            base.OnModelCreating(modelBuilder);

            // BaseEvent를 Entity Framework 모델에서 제외
            modelBuilder.Ignore<BaseEvent>();

            // Apply all configurations from current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // ⭐ Enum을 문자열로 변환하는 설정 추가
            ConfigureEnumConversions(modelBuilder);

            // Identity 테이블과 비즈니스 테이블 분리
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers");
            });

            // ⭐ 핵심 수정: User 엔티티를 Users 테이블로 매핑 (ERPUsers가 아닌!)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users"); // ERPUsers 대신 Users 사용
            });

            // ⭐ 관계 설정 - 명확하고 단순하게
            modelBuilder.Entity<Project>(entity =>
            {
                // ProjectManager 관계만 설정 (가장 중요한 관계)
                entity.HasOne(p => p.ProjectManager)
                    .WithMany(u => u.ManagedProjects)
                    .HasForeignKey(p => p.ProjectManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // TechnicalLead 관계 설정 (선택적)
                entity.HasOne(p => p.TechnicalLead)
                    .WithMany(u => u.TechnicalLeadProjects)
                    .HasForeignKey(p => p.TechnicalLeadId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                // ⭐ Customer, Company 관계는 제거 - Shadow Property 충돌 방지
                // 실제 데이터베이스에 FK 제약조건이 있지만 EF에서는 Navigation Property만 사용
            });

            // ⭐ 다른 엔티티의 관계도 단순하게
            modelBuilder.Entity<User>(entity =>
            {
                // Company 관계는 BaseAuditableEntity에서 상속되므로 설정하지 않음
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                // Company 관계는 BaseAuditableEntity에서 상속되므로 설정하지 않음
            });

            // Global Query Filters - 테스트 환경에서는 적용하지 않음
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isTestEnvironment = environment == "Testing" ||
                                   Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

            if (!isTestEnvironment && _currentUserService?.IsAuthenticated == true && _currentUserService.CompanyId > 0)
            {
                var companyId = _currentUserService.CompanyId;

                modelBuilder.Entity<User>().HasQueryFilter(e => e.CompanyId == companyId);
                modelBuilder.Entity<Customer>().HasQueryFilter(e => e.CompanyId == companyId);
                modelBuilder.Entity<Project>().HasQueryFilter(e => e.CompanyId == companyId);
                modelBuilder.Entity<Invoice>().HasQueryFilter(e => e.CompanyId == companyId);
                modelBuilder.Entity<TimeEntry>().HasQueryFilter(e => e.CompanyId == companyId);
                modelBuilder.Entity<ProjectTask>().HasQueryFilter(e => e.CompanyId == companyId);
                modelBuilder.Entity<ProjectMember>().HasQueryFilter(e => e.CompanyId == companyId);
                modelBuilder.Entity<InvoiceItem>().HasQueryFilter(e => e.CompanyId == companyId);
            }
        }

        /// <summary>
        /// Enum을 문자열로 변환하는 설정을 구성합니다.
        /// 이렇게 하면 데이터베이스의 문자열 값과 C# Enum이 올바르게 매핑됩니다.
        /// </summary>
        private void ConfigureEnumConversions(ModelBuilder modelBuilder)
        {
            // ⭐ NULL 값 처리를 위한 설정 (UNIQUE 제약조건이 있는 필드는 제외)

            // User 엔티티의 nullable string 필드들
            modelBuilder.Entity<User>()
                .Property(u => u.Phone)
                .IsRequired(false); // NULL 허용

            modelBuilder.Entity<User>()
                .Property(u => u.EmployeeId)
                .IsRequired(false); // NULL 허용

            modelBuilder.Entity<User>()
                .Property(u => u.Position)
                .IsRequired(false); // NULL 허용

            // Company 엔티티의 nullable string 필드들 (Domain은 제외 - UNIQUE 제약조건 때문)
            modelBuilder.Entity<Company>()
                .Property(c => c.ContactEmail)
                .IsRequired(false); // NULL 허용

            modelBuilder.Entity<Company>()
                .Property(c => c.ContactPhone)
                .IsRequired(false); // NULL 허용

            // ⭐ 스마트 처리: 실제 DB에 있는 컬럼만 Shadow Property로 설정

            // Users 테이블은 CreatedBy, UpdatedBy 컬럼이 실제로 존재함
            modelBuilder.Entity<User>()
                .Property<string>("CreatedBy")
                .HasMaxLength(450)
                .IsRequired(false); // NULL 허용

            modelBuilder.Entity<User>()
                .Property<string>("UpdatedBy")
                .HasMaxLength(450)
                .IsRequired(false); // NULL 허용

            // Companies 테이블은 CreatedBy, UpdatedBy 컬럼이 없으므로 설정하지 않음
            // (CreatedAt, UpdatedAt만 있음)

            // 다른 테이블들도 실제 스키마에 따라 확인 필요
            // 일단 안전하게 설정하지 않음

            // ⭐ Enum 변환 설정

            // User 엔티티의 Enum들
            modelBuilder.Entity<User>()
                .Property(u => u.Department)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Company 엔티티의 Enum들
            modelBuilder.Entity<Company>()
                .Property(c => c.Plan)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Customer 엔티티의 Enum들  
            modelBuilder.Entity<Customer>()
                .Property(c => c.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Customer>()
                .Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Project 엔티티의 Enum들
            modelBuilder.Entity<Project>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Project>()
                .Property(p => p.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Project>()
                .Property(p => p.Priority)
                .HasConversion<string>()
                .HasMaxLength(50);

            // ProjectMember 엔티티의 Enum들
            modelBuilder.Entity<ProjectMember>()
                .Property(pm => pm.Role)
                .HasConversion<string>()
                .HasMaxLength(50);

            // ProjectTask 엔티티의 Enum들
            modelBuilder.Entity<ProjectTask>()
                .Property(pt => pt.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<ProjectTask>()
                .Property(pt => pt.Priority)
                .HasConversion<string>()
                .HasMaxLength(50);

            // TimeEntry 엔티티의 Enum들
            modelBuilder.Entity<TimeEntry>()
                .Property(te => te.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Invoice 엔티티의 Enum들
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = _dateTime.UtcNow;

                        // ⭐ 스마트 처리: User 엔티티만 CreatedBy 설정 (실제 컬럼이 있음)
                        if (entry.Entity is User)
                        {
                            entry.Entity.CreatedBy = _currentUserService?.IdentityUserId ?? "system";
                        }

                        // Business UserId (int) - 인증된 사용자인 경우에만
                        if (_currentUserService?.IsAuthenticated == true)
                        {
                            try
                            {
                                var businessUserId = _currentUserService.BusinessUserId;
                                if (businessUserId > 0)
                                {
                                    entry.Entity.CreatedByUserId = businessUserId;
                                }
                            }
                            catch (Exception)
                            {
                                entry.Entity.CreatedByUserId = null;
                                // BusinessUserId 조회 실패 시 무시하고 계속 진행
                            }
                        }

                        // CompanyId 자동 설정
                        if (entry.Entity is ICompanyEntity companyEntity &&
                            _currentUserService?.IsAuthenticated == true &&
                            companyEntity.CompanyId == 0)
                        {
                            try
                            {
                                companyEntity.CompanyId = _currentUserService.CompanyId;
                            }
                            catch (Exception)
                            {
                                // CompanyId 조회 실패 시 무시
                            }
                        }
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = _dateTime.UtcNow;

                        // ⭐ 스마트 처리: User 엔티티만 UpdatedBy 설정 (실제 컬럼이 있음)
                        if (entry.Entity is User)
                        {
                            entry.Entity.UpdatedBy = _currentUserService?.IdentityUserId ?? "system";
                        }

                        // Business UserId (int) - 인증된 사용자인 경우에만
                        if (_currentUserService?.IsAuthenticated == true)
                        {
                            try
                            {
                                var businessUserId = _currentUserService.BusinessUserId;
                                if (businessUserId > 0)
                                {
                                    entry.Entity.UpdatedByUserId = businessUserId;
                                }
                            }
                            catch (Exception)
                            {
                                // BusinessUserId 조회 실패 시 무시하고 계속 진행
                            }
                        }
                        break;
                }
            }

            // BaseEntity도 동일하게 처리
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                // BaseAuditableEntity에서 이미 처리된 것은 제외
                if (entry.Entity is BaseAuditableEntity)
                    continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = _dateTime.UtcNow;

                        // ⭐ 스마트 처리: User 엔티티만 CreatedBy 설정
                        if (entry.Entity is User)
                        {
                            entry.Entity.CreatedBy = _currentUserService?.IdentityUserId ?? "system";
                        }

                        if (_currentUserService?.IsAuthenticated == true)
                        {
                            try
                            {
                                var businessUserId = _currentUserService.BusinessUserId;
                                if (businessUserId > 0)
                                {
                                    entry.Entity.CreatedByUserId = businessUserId;
                                }
                            }
                            catch (Exception)
                            {
                                // 실패 시 무시
                            }
                        }

                        if (entry.Entity is ICompanyEntity companyEntity &&
                            _currentUserService?.IsAuthenticated == true &&
                            companyEntity.CompanyId == 0)
                        {
                            try
                            {
                                companyEntity.CompanyId = _currentUserService.CompanyId;
                            }
                            catch (Exception)
                            {
                                // 실패 시 무시
                            }
                        }
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = _dateTime.UtcNow;

                        // ⭐ 스마트 처리: User 엔티티만 UpdatedBy 설정
                        if (entry.Entity is User)
                        {
                            entry.Entity.UpdatedBy = _currentUserService?.IdentityUserId ?? "system";
                        }

                        if (_currentUserService?.IsAuthenticated == true)
                        {
                            try
                            {
                                var businessUserId = _currentUserService.BusinessUserId;
                                if (businessUserId > 0)
                                {
                                    entry.Entity.UpdatedByUserId = businessUserId;
                                }
                            }
                            catch (Exception)
                            {
                                // 실패 시 무시
                            }
                        }
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}