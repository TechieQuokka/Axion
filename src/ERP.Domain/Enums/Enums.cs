using System.ComponentModel;

namespace ERP.Domain.Enums
{
    public enum SubscriptionPlan
    {
        [Description("Free")]
        Free = 0,
        [Description("Basic")]
        Basic = 1,
        [Description("Professional")]
        Professional = 2,
        [Description("Enterprise")]
        Enterprise = 3
    }

    public enum Department
    {
        [Description("Development")]
        Development = 0,
        [Description("Design")]
        Design = 1,
        [Description("QA")]
        QA = 2,
        [Description("PM")]
        PM = 3,
        [Description("Sales")]
        Sales = 4,
        [Description("HR")]
        HR = 5,
        [Description("Finance")]
        Finance = 6,
        [Description("Marketing")]
        Marketing = 7,
        [Description("Support")]
        Support = 8
    }

    public enum UserStatus
    {
        [Description("Active")]
        Active = 0,
        [Description("Inactive")]
        Inactive = 1,
        [Description("Terminated")]
        Terminated = 2
    }

    public enum CustomerType
    {
        [Description("Individual")]
        Individual = 0,
        [Description("SME")]
        SME = 1,
        [Description("Enterprise")]
        Enterprise = 2,
        [Description("Government")]
        Government = 3
    }

    public enum CustomerStatus
    {
        [Description("Active")]
        Active = 0,
        [Description("Inactive")]
        Inactive = 1,
        [Description("Potential")]
        Potential = 2
    }

    public enum ProjectStatus
    {
        [Description("Planning")]
        Planning = 0,
        [Description("InProgress")]
        InProgress = 1,
        [Description("OnHold")]
        OnHold = 2,
        [Description("Completed")]
        Completed = 3,
        [Description("Cancelled")]
        Cancelled = 4
    }

    public enum ProjectType
    {
        [Description("WebDevelopment")]
        WebDevelopment = 0,
        [Description("MobileApp")]
        MobileApp = 1,
        [Description("Maintenance")]
        Maintenance = 2,
        [Description("Consulting")]
        Consulting = 3,
        [Description("Integration")]
        Integration = 4,
        [Description("Other")]
        Other = 5
    }

    public enum Priority
    {
        [Description("Low")]
        Low = 0,
        [Description("Medium")]
        Medium = 1,
        [Description("High")]
        High = 2,
        [Description("Critical")]
        Critical = 3
    }

    public enum TaskStatus
    {
        [Description("ToDo")]
        ToDo = 0,
        [Description("InProgress")]
        InProgress = 1,
        [Description("Review")]
        Review = 2,
        [Description("Testing")]
        Testing = 3,
        [Description("Done")]
        Done = 4,
        [Description("Blocked")]
        Blocked = 5
    }

    public enum TimeEntryStatus
    {
        [Description("Draft")]
        Draft = 0,
        [Description("Submitted")]
        Submitted = 1,
        [Description("Approved")]
        Approved = 2,
        [Description("Rejected")]
        Rejected = 3
    }

    public enum InvoiceStatus
    {
        [Description("Draft")]
        Draft = 0,
        [Description("Sent")]
        Sent = 1,
        [Description("Paid")]
        Paid = 2,
        [Description("Overdue")]
        Overdue = 3,
        [Description("Cancelled")]
        Cancelled = 4
    }

    public enum ProjectRole
    {
        [Description("ProjectManager")]
        ProjectManager = 0,
        [Description("TechLead")]
        TechLead = 1,
        [Description("SeniorDeveloper")]
        SeniorDeveloper = 2,
        [Description("Developer")]
        Developer = 3,
        [Description("JuniorDeveloper")]
        JuniorDeveloper = 4,
        [Description("Designer")]
        Designer = 5,
        [Description("QA")]
        QA = 6,
        [Description("Analyst")]
        Analyst = 7,
        [Description("DevOps")]
        DevOps = 8
    }

    // Enum 확장 메서드 (유틸리티)
    public static class EnumExtensions
    {
        public static string GetDescription(this System.Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute?)field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
            return attribute?.Description ?? value.ToString();
        }

        public static T GetEnumFromDescription<T>(string description) where T : System.Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                var attribute = (DescriptionAttribute?)field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
                if (attribute?.Description == description)
                {
                    return (T)field.GetValue(null)!;
                }
                if (field.Name == description)
                {
                    return (T)field.GetValue(null)!;
                }
            }
            throw new ArgumentException($"Enum value not found for description: {description}");
        }
    }
}