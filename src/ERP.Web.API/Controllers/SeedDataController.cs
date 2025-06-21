using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // 임시로 인증 없이 접근 가능
    public class SeedDataController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;

        public SeedDataController(IApplicationDbContext context, IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        [HttpPost("create-safe-data")]
        public async Task<IActionResult> CreateSafeData(CancellationToken cancellationToken = default)
        {
            try
            {
                var timestamp = DateTime.Now.Ticks;

                // ⭐ 먼저 기존 데이터가 있는지 전체 확인
                var existingCompanyCount = await _context.Companies.CountAsync(cancellationToken);
                if (existingCompanyCount > 0)
                {
                    return Ok(new
                    {
                        message = "기초 데이터가 이미 존재합니다. clean-data API를 먼저 실행해주세요.",
                        existingCompanies = existingCompanyCount,
                        suggestion = "POST /api/seeddata/clean-data 를 먼저 호출하세요."
                    });
                }

                var result = new
                {
                    companies = 0,
                    users = 0,
                    customers = 0,
                    projects = 0
                };

                // 1. Company 생성 - 완전히 고유한 값으로
                var companyDomain = $"demo-{timestamp}";

                // ⭐ 디버깅: Domain 값 확인
                Console.WriteLine($"[DEBUG] Creating company with domain: '{companyDomain}'");
                Console.WriteLine($"[DEBUG] Timestamp: {timestamp}");

                var company = new Company
                {
                    Name = $"데모 컴퍼니 {timestamp}",
                    Domain = companyDomain, // 명시적으로 설정
                    ContactEmail = $"admin-{timestamp}@demo.com",
                    ContactPhone = "02-1234-5678",
                    Plan = SubscriptionPlan.Professional,
                    CreatedAt = _dateTime.UtcNow,
                    UpdatedAt = _dateTime.UtcNow,
                    IsDeleted = false
                };

                // ⭐ 디버깅: 엔티티 값 확인
                Console.WriteLine($"[DEBUG] Company object - Name: '{company.Name}', Domain: '{company.Domain}'");

                _context.Companies.Add(company);

                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    Console.WriteLine("[DEBUG] Company saved successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] SaveChanges failed: {ex.Message}");
                    Console.WriteLine($"[DEBUG] Inner exception: {ex.InnerException?.Message}");
                    throw; // 다시 던져서 원래 오류 처리 로직 실행
                }

                // 생성된 Company의 ID 가져오기
                var savedCompany = await _context.Companies
                    .Where(c => c.Domain == companyDomain)
                    .FirstAsync(cancellationToken);
                var companyId = savedCompany.Id;

                result = result with { companies = 1 };

                // 2. Users 생성 - PasswordHash 필수 필드 추가
                var users = new List<User>
                    {
                        new User
                        {
                            CompanyId = companyId,
                            FirstName = "김",
                            LastName = "관리자",
                            Email = $"admin-{timestamp}@demo.com",
                            PasswordHash = "$2a$11$dummy.hash.for.demo.user.only", // ⭐ 필수 필드 추가
                            Phone = "010-1111-1111",
                            EmployeeId = $"EMP001-{timestamp}",
                            Department = Department.PM,
                            Position = "시스템 관리자",
                            Status = UserStatus.Active,
                            HireDate = DateTime.Today.AddYears(-2),
                            HourlyRate = 60000,
                            MonthlySalary = 5000000,
                            CreatedAt = _dateTime.UtcNow,
                            UpdatedAt = _dateTime.UtcNow,
                            // ⭐ Users 테이블은 CreatedBy/UpdatedBy가 있으므로 유지
                            CreatedBy = "system",
                            UpdatedBy = "system",
                            IsDeleted = false
                        },
                        new User
                        {
                            CompanyId = companyId,
                            FirstName = "이",
                            LastName = "프매니저",
                            Email = $"pm-{timestamp}@demo.com",
                            PasswordHash = "$2a$11$dummy.hash.for.demo.user.only", // ⭐ 필수 필드 추가
                            Phone = "010-2222-2222",
                            EmployeeId = $"EMP002-{timestamp}",
                            Department = Department.PM,
                            Position = "프로젝트 매니저",
                            Status = UserStatus.Active,
                            HireDate = DateTime.Today.AddYears(-1),
                            HourlyRate = 50000,
                            MonthlySalary = 4000000,
                            CreatedAt = _dateTime.UtcNow,
                            UpdatedAt = _dateTime.UtcNow,
                            CreatedBy = "system",
                            UpdatedBy = "system",
                            IsDeleted = false
                        },
                        new User
                        {
                            CompanyId = companyId,
                            FirstName = "박",
                            LastName = "개발자",
                            Email = $"dev-{timestamp}@demo.com",
                            PasswordHash = "$2a$11$dummy.hash.for.demo.user.only", // ⭐ 필수 필드 추가
                            Phone = "010-3333-3333",
                            EmployeeId = $"EMP003-{timestamp}",
                            Department = Department.Development,
                            Position = "시니어 개발자",
                            Status = UserStatus.Active,
                            HireDate = DateTime.Today.AddMonths(-6),
                            HourlyRate = 45000,
                            MonthlySalary = 3500000,
                            CreatedAt = _dateTime.UtcNow,
                            UpdatedAt = _dateTime.UtcNow,
                            CreatedBy = "system",
                            UpdatedBy = "system",
                            IsDeleted = false
                        }
                    };

                _context.Users.AddRange(users);
                await _context.SaveChangesAsync(cancellationToken);
                result = result with { users = users.Count };

                // 3. Customers 생성
                var customers = new List<Customer>
                    {
                        new Customer
                        {
                            CompanyId = companyId,
                            Name = $"ABC 기업 {timestamp}",
                            ContactName = "김고객",
                            ContactEmail = $"contact-{timestamp}@abc.com",
                            ContactPhone = "02-1111-2222",
                            Address = """{"street": "강남대로 123", "city": "서울", "zipCode": "06241"}""",
                            BusinessNumber = $"123-45-{timestamp % 100000:D5}",
                            Industry = "제조업",
                            Type = CustomerType.Enterprise,
                            Status = CustomerStatus.Active,
                            CreatedAt = _dateTime.UtcNow,
                            UpdatedAt = _dateTime.UtcNow,
                            // ⭐ Customers 테이블에 CreatedBy/UpdatedBy가 있는지 확인 필요
                            // 일단 안전하게 제거 (실제 스키마에 맞춰 조정)
                            IsDeleted = false
                        },
                        new Customer
                        {
                            CompanyId = companyId,
                            Name = $"XYZ 솔루션 {timestamp}",
                            ContactName = "이담당자",
                            ContactEmail = $"contact2-{timestamp}@xyz.com",
                            ContactPhone = "02-2222-3333",
                            Address = """{"street": "테헤란로 456", "city": "서울", "zipCode": "06164"}""",
                            BusinessNumber = $"987-65-{timestamp % 100000:D5}",
                            Industry = "IT서비스",
                            Type = CustomerType.SME,
                            Status = CustomerStatus.Active,
                            CreatedAt = _dateTime.UtcNow,
                            UpdatedAt = _dateTime.UtcNow,
                            IsDeleted = false
                        }
                    };

                _context.Customers.AddRange(customers);
                await _context.SaveChangesAsync(cancellationToken);
                result = result with { customers = customers.Count };

                // 4. 생성된 사용자와 고객 ID 가져오기
                var createdUsers = await _context.Users
                    .Where(u => u.CompanyId == companyId)
                    .ToListAsync(cancellationToken);
                var createdCustomers = await _context.Customers
                    .Where(c => c.CompanyId == companyId)
                    .ToListAsync(cancellationToken);

                // 5. Projects 생성
                if (createdUsers.Count >= 2 && createdCustomers.Count >= 1)
                {
                    var pmUser = createdUsers.FirstOrDefault(u => u.Department == Department.PM);
                    var techLeadUser = createdUsers.FirstOrDefault(u => u.Department == Department.Development);

                    var projects = new List<Project>
                        {
                            new Project
                            {
                                CompanyId = companyId,
                                Name = $"웹사이트 리뉴얼 프로젝트 {timestamp}",
                                Description = "고객사 웹사이트 전면 리뉴얼 프로젝트입니다.",
                                Code = $"WEB-{timestamp % 10000:D4}",
                                StartDate = DateTime.Today.AddDays(-30),
                                EndDate = DateTime.Today.AddDays(60),
                                ActualStartDate = DateTime.Today.AddDays(-30),
                                Status = ProjectStatus.InProgress,
                                Type = ProjectType.WebDevelopment,
                                Priority = Priority.High,
                                Budget = 50000000,
                                ActualCost = 15000000,
                                Progress = 30,
                                CustomerId = createdCustomers[0].Id,
                                ProjectManagerId = pmUser?.Id ?? createdUsers[0].Id,
                                TechnicalLeadId = techLeadUser?.Id,
                                CreatedAt = _dateTime.UtcNow,
                                UpdatedAt = _dateTime.UtcNow,
                                // ⭐ Projects 테이블에 CreatedBy/UpdatedBy가 있는지 확인 필요
                                // 일단 안전하게 제거 (실제 스키마에 맞춰 조정)
                                IsDeleted = false
                            },
                            new Project
                            {
                                CompanyId = companyId,
                                Name = $"모바일 앱 개발 프로젝트 {timestamp}",
                                Description = "신규 모바일 애플리케이션 개발 프로젝트입니다.",
                                Code = $"APP-{timestamp % 10000:D4}",
                                StartDate = DateTime.Today.AddDays(10),
                                EndDate = DateTime.Today.AddDays(120),
                                Status = ProjectStatus.Planning,
                                Type = ProjectType.MobileApp,
                                Priority = Priority.Medium,
                                Budget = 80000000,
                                ActualCost = 0,
                                Progress = 5,
                                CustomerId = createdCustomers.Count > 1 ? createdCustomers[1].Id : createdCustomers[0].Id,
                                ProjectManagerId = pmUser?.Id ?? createdUsers[0].Id,
                                TechnicalLeadId = techLeadUser?.Id,
                                CreatedAt = _dateTime.UtcNow,
                                UpdatedAt = _dateTime.UtcNow,
                                IsDeleted = false
                            }
                        };

                    _context.Projects.AddRange(projects);
                    await _context.SaveChangesAsync(cancellationToken);
                    result = result with { projects = projects.Count };
                }

                return Ok(new
                {
                    message = "안전한 기초 데이터가 성공적으로 생성되었습니다!",
                    created = result,
                    companyId = companyId,
                    companyDomain = companyDomain,
                    timestamp = _dateTime.UtcNow,
                    ready = true,
                    note = "이제 프로젝트 API 테스트가 가능합니다!",
                    testUsers = new
                    {
                        admin = $"admin-{timestamp}@demo.com",
                        pm = $"pm-{timestamp}@demo.com",
                        developer = $"dev-{timestamp}@demo.com"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "기초 데이터 생성 중 오류가 발생했습니다.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace,
                    timestamp = _dateTime.UtcNow
                });
            }
        }

        [HttpPost("clean-data")]
        public async Task<IActionResult> CleanData(CancellationToken cancellationToken = default)
        {
            try
            {
                // 개발 환경에서만 사용할 수 있도록 제한
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment != "Development")
                {
                    return BadRequest(new { message = "이 기능은 개발 환경에서만 사용할 수 있습니다." });
                }

                // 외래 키 제약조건 때문에 순서대로 삭제
                var projects = await _context.Projects.ToListAsync(cancellationToken);
                _context.Projects.RemoveRange(projects);

                var customers = await _context.Customers.ToListAsync(cancellationToken);
                _context.Customers.RemoveRange(customers);

                var users = await _context.Users.ToListAsync(cancellationToken);
                _context.Users.RemoveRange(users);

                var companies = await _context.Companies.ToListAsync(cancellationToken);
                _context.Companies.RemoveRange(companies);

                await _context.SaveChangesAsync(cancellationToken);

                return Ok(new
                {
                    message = "모든 데이터가 정리되었습니다.",
                    deletedCounts = new
                    {
                        projects = projects.Count,
                        customers = customers.Count,
                        users = users.Count,
                        companies = companies.Count
                    },
                    timestamp = _dateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "데이터 정리 중 오류가 발생했습니다.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetDataStatus(CancellationToken cancellationToken = default)
        {
            try
            {
                var companies = await _context.Companies.CountAsync(cancellationToken);
                var users = await _context.Users.CountAsync(cancellationToken);
                var customers = await _context.Customers.CountAsync(cancellationToken);
                var projects = await _context.Projects.CountAsync(cancellationToken);

                // 최근 생성된 회사 정보
                var latestCompany = await _context.Companies
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                return Ok(new
                {
                    message = "현재 데이터베이스 상태",
                    counts = new { companies, users, customers, projects },
                    ready = companies > 0 && users > 0 && customers > 0,
                    latestCompany = latestCompany != null ? new
                    {
                        id = latestCompany.Id,
                        name = latestCompany.Name,
                        domain = latestCompany.Domain,
                        createdAt = latestCompany.CreatedAt
                    } : null,
                    timestamp = _dateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "데이터 상태 확인 중 오류가 발생했습니다.",
                    error = ex.Message,
                    timestamp = _dateTime.UtcNow
                });
            }
        }
    }
}
