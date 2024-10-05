using Microsoft.EntityFrameworkCore;
using ForexAITradingPC1Main.Models;

namespace ForexAITradingPC1Main.Database;

public class ForexDbContext : DbContext
{
    public ForexDbContext(DbContextOptions<ForexDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RiskManagement> RiskManagements => Set<RiskManagement>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<HogaData> HogaData => Set<HogaData>();
    public DbSet<DealData> DealData => Set<DealData>();
    public DbSet<OrderData> OrderData => Set<OrderData>();
    public DbSet<AccountStatusLog> AccountStatusLogs => Set<AccountStatusLog>();
    public DbSet<TradePerformanceLog> TradePerformanceLogs => Set<TradePerformanceLog>();
    public DbSet<AccountDailySummary> AccountDailySummaries => Set<AccountDailySummary>();
    public DbSet<SystemPerformance> SystemPerformances => Set<SystemPerformance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.HtsApiId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.HtsApiPassword).IsRequired().HasMaxLength(255);
            entity.Property(e => e.HtsCertPassword).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AccountType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.KYCStatus).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CustomerCategory).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AccountStatus).IsRequired().HasMaxLength(20);
            entity.HasOne(d => d.Customer)
                .WithMany(p => p.Accounts)
                .HasForeignKey(d => d.CustomerId);
        });

        modelBuilder.Entity<RiskManagement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.Account)
                .WithOne(p => p.RiskManagement)
                .HasForeignKey<RiskManagement>(d => d.AccountId);
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.YearCode).IsRequired().HasMaxLength(4);
            entity.Property(e => e.MonthCode).IsRequired().HasMaxLength(2);
        });

        modelBuilder.Entity<HogaData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeH).IsRequired().HasMaxLength(8);
            entity.Property(e => e.Time).IsRequired().HasMaxLength(6);
            entity.Property(e => e.PartitionKey).IsRequired().HasMaxLength(50);
            entity.HasOne(d => d.Series)
                .WithMany(p => p.HogaData)
                .HasForeignKey(d => d.SeriesId);
        });

        modelBuilder.Entity<DealData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeH).IsRequired().HasMaxLength(8);
            entity.Property(e => e.Time).IsRequired().HasMaxLength(6);
            entity.Property(e => e.Confirmation).IsRequired().HasMaxLength(1);
            entity.Property(e => e.Sign).IsRequired().HasMaxLength(1);
            entity.Property(e => e.PartitionKey).IsRequired().HasMaxLength(50);
            entity.HasOne(d => d.Series)
                .WithMany(p => p.DealData)
                .HasForeignKey(d => d.SeriesId);
        });

        modelBuilder.Entity<OrderData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeH).IsRequired().HasMaxLength(8);
            entity.Property(e => e.Time).IsRequired().HasMaxLength(6);
            entity.Property(e => e.TimeS).IsRequired().HasMaxLength(9);
            entity.Property(e => e.ContractMonth).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Test).HasMaxLength(10);
            entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BuySellType).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PriceType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ExecutionType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ConditionType).HasMaxLength(20);
            entity.Property(e => e.HTSType).HasMaxLength(10);
            entity.Property(e => e.UserID).HasMaxLength(50);
            entity.Property(e => e.FullExecutionType).HasMaxLength(20);
            entity.Property(e => e.PartitionKey).IsRequired().HasMaxLength(50);
            entity.HasOne(d => d.Series)
                .WithMany(p => p.OrderData)
                .HasForeignKey(d => d.SeriesId);
            entity.HasOne(d => d.Account)
                .WithMany(p => p.OrderData)
                .HasForeignKey(d => d.AccountId);
        });

        modelBuilder.Entity<AccountStatusLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PartitionKey).IsRequired().HasMaxLength(50);
            entity.HasOne(d => d.Account)
                .WithMany(p => p.AccountStatusLogs)
                .HasForeignKey(d => d.AccountId);
        });

        modelBuilder.Entity<TradePerformanceLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PartitionKey).IsRequired().HasMaxLength(50);
            entity.HasOne(d => d.Account)
                .WithMany(p => p.TradePerformanceLogs)
                .HasForeignKey(d => d.AccountId);
        });

        modelBuilder.Entity<AccountDailySummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PartitionKey).IsRequired().HasMaxLength(50);
            entity.HasOne(d => d.Account)
                .WithMany(p => p.AccountDailySummaries)
                .HasForeignKey(d => d.AccountId);
        });

        modelBuilder.Entity<SystemPerformance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PartitionKey).IsRequired().HasMaxLength(50);
        });
    }
}


/*
 * 
 * 이것은 누락이 없는 완전한 ForexDbContext.cs 파일입니다. 여기에는 다음이 포함됩니다.

시스템의 각 엔터티에 대한 모든 DbSet 속성입니다.
다음을 포함하여 각 엔터티에 대한 세부 구성을 포함하는 OnModelCreating 메서드:

기본 키 정의
속성 구성(필수 필드, 최대 길이)
관계 구성(일대다, 일대일)



이 파일은 .NET 8.0과 호환되는 DbSet 속성에 대한 파일 범위 네임스페이스 및 표현식 본문 멤버와 같은 최신 C# 기능을 사용합니다.
이 파일의 특정 부분에 대한 설명이 필요하거나 변경하고 싶은 사항이 있으면 알려주십시오. 기꺼이 도와드리겠습니다.



/*이 ForexDbContext 클래스는 다음과 같은 주요 특징을 가지고 있습니다:
 * 이 ForexDbContext 클래스는 다음과 같은 주요 특징을 가지고 있습니다:
 * 
 * 이 ForexDbContext 클래스는 다음과 같은 주요 특징을 가지고 있습니다:

.NET 8.0 스타일의 파일 범위 라벨스페이스를 사용합니다.
주요 모든 부분에 대한 DbSet 속성이 수학 본문 멤버(expression-bodied member)로 정의되어 있습니다.
OnModelCreating 메서드에서 각 부분의 구성을 구체적으로 정의하고 있습니다.
각 회의의 키, 필수 필드, 최대 길이 등을 설정하고 있습니다.
대립되는 관계(예: 일대다, 일대일)를 정의하고 있습니다.
모든 파티션에 대해 PartitionKey를 설정하여 데이터 보호를 지원합니다.

이 특성은 .NET 8.0과 Entity Framework Core의 최신 기능을 활용하고 있습니다. 만약 문제가 발생한 경우, 다음 사항에 대해 알려주세요:

프로젝트의 .csproj 파일에서 TargetFramework가 net8.0으로 설정되어 있는지 확인
Microsoft.EntityFrameworkCore 패키지가 8.0.x 버전으로 설치되어 있는지 확인합니다.
모든 모델 클래스(고객, 계정 등)가 ForexAITradingPC1Main.Models 접착 스페이스에 정의되어 있는지 확인

추가적인 문제나 질문이 있으시면 말씀해 주십시오.
 * 네, 물론입니다. Database/ForexDbContext.cs 파일의 전체 내용을 보여드리겠습니다.
Database/ForexDbContext.csClick to open code
이 ForexDbContext 클래스는 다음과 같은 주요 특징을 가지고 있습니다:

모든 주요 엔티티에 대한 DbSet 속성이 정의되어 있습니다.
OnModelCreating 메서드에서 각 엔티티의 구성을 세부적으로 정의하고 있습니다.
각 엔티티의 키, 필수 필드, 최대 길이 등을 설정하고 있습니다.
엔티티 간의 관계(예: 일대다, 일대일)를 정의하고 있습니다.
모든 엔티티에 대해 PartitionKey를 설정하여 데이터 분할을 지원합니다.

이 구현에서 Account와 OrderData 사이의 관계가 올바르게 설정되어 있음을 확인할 수 있습니다:
csharpCopymodelBuilder.Entity<OrderData>(entity =>
{
    // ... 다른 설정들 ...
    entity.HasOne(d => d.Account)
        .WithMany(p => p.OrderData)
        .HasForeignKey(d => d.AccountId);
});
이 설정은 Account 클래스의 OrderData 컬렉션 속성과 OrderData 클래스의 Account 속성을 올바르게 매핑합니다.
만약 여전히 문제가 발생한다면, 다음 사항들을 확인해 주세요:

OrderData 클래스에 AccountId 속성과 Account navigation 속성이 정의되어 있는지 확인
Account 클래스에 OrderData 컬렉션 속성이 정의되어 있는지 확인
모든 관련 클래스들이 ForexAITradingPC1Main.Models 네임스페이스 안에 있는지 확인

이 정보를 바탕으로 문제를 더 정확히 진단하고 해결할 수 있을 것입니다. 추가적인 질문이나 확인이 필요한 부분이 있다면 말씀해 주세요. CopyRetryClaude does not have the ability to run the code it generates yet.

모든 주요 부분에 대한 DbSet 속성이 정의되어 있습니다.
OnModelCreating 메서드에서 각 부분의 구성을 구체적으로 정의하고 있습니다.
각 회의의 키, 필수 필드, 최대 길이 등을 설정하고 있습니다.
대립되는 관계(예: 일대다, 일대일)를 정의하고 있습니다.
모든 파티션에 대해 PartitionKey를 설정하여 데이터 보호를 지원합니다.

이 상황을 사용하기 위해서는 다음 사항을 확인해야 합니다:

모든 관련 모델 클래스(Customer, Account, OrderData 등)가 모델 폴더에 정의되어 있어야 합니다.
각 모델 클래스에는 필요한 탐색 속성이 있어야 합니다.
Startup.cs 또는 Program.cs에서 이 DbContext를 서비스로 등록해야 합니다.

만약 현재 문제가 발생한다면, 다음 정보를 제공해 주시기 바랍니다:

계속해서 질문할 내용이 없습니다.
모델 폴더의 전체 내용 목록
프로젝트의 전체 폴더 구조

이 정보를 바탕으로 문제를 더 정확하게 진단하고 분석할 수 있을 것입니다.
 * 
 * 이 ForexDbContext 클래스는 다음과 같은 주요 특징을 가지고 있습니다:

모든 주요 부분에 대한 DbSet 속성이 정의되어 있습니다.
OnModelCreating 메서드에서 각 부분의 구성을 구체적으로 정의하고 있습니다.
각 회의의 키, 필수 필드, 최대 길이 등을 설정하고 있습니다.
대립되는 관계(예: 일대다, 일대일)를 정의하고 있습니다.

이 상황을 사용하기 위해서는 다음 사항을 확인해야 합니다:

모든 관련 모델 클래스(Customer, Account, OrderData 등)가 모델 폴더에 정의되어 있어야 합니다.
각 모델 클래스에는 필수 탐색 속성이 있어야 합니다(예: Account 클래스에 public virtual ICollection<OrderData> OrderData { get; set; }등).
Startup.cs 또는 Program.cs에서 이 DbContext를 서비스로 등록해야 합니다.

만약 현재 문제가 발생한다면, 다음 정보를 제공해 주시기 바랍니다:

OrderData 클래스의 전체 정의
Series와 Account 클래스의 정의( OrderData와 관련된 부분)
프로젝트의 전체 폴더 구조

이 정보를 바탕으로 문제를 더 정확하게 진단하고 분석할 수 있을 것입니다.

DbSet 속성들: 각 유형에 대한 DbSet을 정의합니다.
해당 구성원들에 대한 쿼리와 변경 작업을 수행할 수 있습니다.
OnModelCreating 메서드: 분리 구성을 정의합니다. 
여기서는 각 파티의 키, 필수 필드, 최대 길이, 관계 등을 설정합니다.
생성자: DbContextOptions를 다양하게 수용하여 클래스의 생성자에 전달합니다.
은폐된 데이터베이스 연결 문자열 등의 옵션을 접근할 수 있습니다.

이 수업을 사용하려면 다음 사항을 확인해야 합니다:

모든 클래스 클래스(고객, 계정 등)가 모델 폴더에 정의되어 있어야 합니다.
Startup.cs 또는 Program.cs에서 이 DbContext를 서비스로 등록해야 합니다.
필요한 Entity Framework Core 패키지를 프로젝트에 설치해야 합니다.

만약 이 클래스를 수정하거나 추가 구성이 필요하다면, 프로젝트를 특별히 요구하는 경우에는 재능이 있을 수 있습니다. 
특히, 관계와 관계를 맺는 것과 같은 것을 더 상세하게 정의할 수 있습니다.


*/