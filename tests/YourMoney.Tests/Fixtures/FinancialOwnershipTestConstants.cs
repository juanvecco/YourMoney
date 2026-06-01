namespace YourMoney.Tests.Fixtures
{
    public static class FinancialOwnershipTestConstants
    {
        public const string TargetUserId = "3c3e04ec-651e-4de3-8e7a-5dc6f47c2a10";
        public const string LegacyTransitionUserId = "legacy-transition-user";

        public static readonly string[] ScopedTables =
        {
            "tbCategoria",
            "tbContaFinanceira",
            "tbDespesa",
            "tbInvestimento",
            "tbMeta",
            "tbReceita"
        };
    }
}
