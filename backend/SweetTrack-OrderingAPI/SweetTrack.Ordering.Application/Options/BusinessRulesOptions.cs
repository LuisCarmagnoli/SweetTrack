namespace SweetTrack.Ordering.Application.Options;

public class BusinessRulesOptions
{
    public const string SectionName = "BusinessRules";

    public decimal MinimumOrderAmount { get; set; } = 50000.00m;
    public int AbandonedCartExpirationHours { get; set; } = 24;
}

