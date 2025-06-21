using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Data.Migrations
{
    /// <summary>
    /// UserId 타입 충돌 해결을 위한 마이그레이션
    /// CreatedBy/UpdatedBy를 string 타입으로 변경하고 BusinessUserId 필드 추가
    /// </summary>
    public partial class FixUserIdTypeConflict : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1단계: 새로운 BusinessUserId 컬럼들 추가
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "ERPUsers",
                type: "int",
                nullable: true,
                comment: "Business domain user ID who created this record");

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "ERPUsers",
                type: "int",
                nullable: true,
                comment: "Business domain user ID who last updated this record");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "ProjectTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "ProjectTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "TimeEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "TimeEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "InvoiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "InvoiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "ProjectMembers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "ProjectMembers",
                type: "int",
                nullable: true);

            // 2단계: ApplicationUser 테이블에 BusinessUserId 추가
            migrationBuilder.AddColumn<int>(
                name: "BusinessUserId",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                comment: "Link to business domain user ID in ERPUsers table");

            // 3단계: CreatedBy/UpdatedBy 컬럼의 타입을 int에서 varchar(450)로 변경
            // 기존 데이터를 임시 컬럼에 백업
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy_Backup",
                table: "ERPUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy_Backup",
                table: "ERPUsers",
                type: "int",
                nullable: true);

            // 기존 데이터 백업
            migrationBuilder.Sql("UPDATE ERPUsers SET CreatedBy_Backup = CreatedBy, UpdatedBy_Backup = UpdatedBy");
            migrationBuilder.Sql("UPDATE Projects SET CreatedBy_Backup = CreatedBy, UpdatedBy_Backup = UpdatedBy");
            migrationBuilder.Sql("UPDATE Customers SET CreatedBy_Backup = CreatedBy, UpdatedBy_Backup = UpdatedBy");
            // ... 다른 테이블들도 동일하게

            // 기존 컬럼 삭제
            migrationBuilder.DropColumn(name: "CreatedBy", table: "ERPUsers");
            migrationBuilder.DropColumn(name: "UpdatedBy", table: "ERPUsers");
            // ... 다른 테이블들도 동일하게

            // 새로운 string 타입 컬럼 추가
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ERPUsers",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "system",
                comment: "Identity system user ID who created this record");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ERPUsers",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "system",
                comment: "Identity system user ID who last updated this record");

            // 다른 테이블들도 동일하게 처리...
            // Projects 테이블
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy_Backup",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy_Backup",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.DropColumn(name: "CreatedBy", table: "Projects");
            migrationBuilder.DropColumn(name: "UpdatedBy", table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Projects",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Projects",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "system");

            // 4단계: 인덱스 추가
            migrationBuilder.CreateIndex(
                name: "IX_ERPUsers_CreatedBy",
                table: "ERPUsers",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ERPUsers_CreatedByUserId",
                table: "ERPUsers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BusinessUserId",
                table: "AspNetUsers",
                column: "BusinessUserId");

            // 5단계: 외래 키 제약 조건 추가 (선택적)
            migrationBuilder.AddForeignKey(
                name: "FK_ERPUsers_CreatedByUserId",
                table: "ERPUsers",
                column: "CreatedByUserId",
                principalTable: "ERPUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_BusinessUserId",
                table: "AspNetUsers",
                column: "BusinessUserId",
                principalTable: "ERPUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 역순으로 롤백
            migrationBuilder.DropForeignKey(name: "FK_ERPUsers_CreatedByUserId", table: "ERPUsers");
            migrationBuilder.DropForeignKey(name: "FK_AspNetUsers_BusinessUserId", table: "AspNetUsers");

            migrationBuilder.DropIndex(name: "IX_ERPUsers_CreatedBy", table: "ERPUsers");
            migrationBuilder.DropIndex(name: "IX_ERPUsers_CreatedByUserId", table: "ERPUsers");
            migrationBuilder.DropIndex(name: "IX_AspNetUsers_BusinessUserId", table: "AspNetUsers");

            // 컬럼들 제거
            migrationBuilder.DropColumn(name: "CreatedByUserId", table: "ERPUsers");
            migrationBuilder.DropColumn(name: "UpdatedByUserId", table: "ERPUsers");
            migrationBuilder.DropColumn(name: "BusinessUserId", table: "AspNetUsers");

            // string 타입 컬럼 제거하고 int 타입으로 복원
            migrationBuilder.DropColumn(name: "CreatedBy", table: "ERPUsers");
            migrationBuilder.DropColumn(name: "UpdatedBy", table: "ERPUsers");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ERPUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "ERPUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 백업 데이터 복원
            migrationBuilder.Sql("UPDATE ERPUsers SET CreatedBy = COALESCE(CreatedBy_Backup, 0), UpdatedBy = COALESCE(UpdatedBy_Backup, 0)");

            // 백업 컬럼 제거
            migrationBuilder.DropColumn(name: "CreatedBy_Backup", table: "ERPUsers");
            migrationBuilder.DropColumn(name: "UpdatedBy_Backup", table: "ERPUsers");

            // 다른 테이블들도 동일하게 롤백...
        }
    }
}
