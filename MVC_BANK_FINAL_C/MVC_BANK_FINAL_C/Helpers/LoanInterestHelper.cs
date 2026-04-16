namespace MVC_BANK_FINAL_C.Helpers
{
    public static class LoanInterestHelper
    {
        private static readonly Dictionary<string, decimal> DefaultRates = new()
        {
            { "Personal",  5m },
            { "Home",      7m },
            { "Car",       8m },
            { "Education", 6m },
            { "Business",  9m }
        };

        /// <summary>Returns the default interest rate for a loan type.</summary>
        public static decimal GetInterestRate(string loanType)
        {
            if (string.IsNullOrWhiteSpace(loanType)) return 5m;
            return DefaultRates.TryGetValue(loanType, out decimal rate) ? rate : 5m;
        }

        /// <summary>
        /// Calculates monthly EMI using Simple Interest.
        /// SI = P × R × T / 100
        /// Total = P + SI
        /// EMI  = Total / (T × 12)
        /// </summary>
        public static decimal CalculateEMI(decimal principal, decimal rate, int tenureYears)
        {
            if (tenureYears <= 0) tenureYears = 1;
            decimal si = principal * rate * tenureYears / 100m;
            decimal total = principal + si;
            decimal emi = total / (tenureYears * 12);
            return Math.Round(emi, 2);
        }

        public static IEnumerable<string> LoanTypes =>
            DefaultRates.Keys;

        public static IEnumerable<int> TenureOptions =>
            new[] { 1, 2, 3, 4, 5 };
    }
}
