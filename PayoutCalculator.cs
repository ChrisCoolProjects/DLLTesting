namespace DLLTesting
{
    internal static class PayoutCalculator
    {
        public static List<double?> CalcPayout(int numAttendees)
        {
            double? prizePool = numAttendees >= 11 ? 50 : 0;
            prizePool += (numAttendees * 5);
            List<double?> payouts = numAttendees switch
            {
                < 5 => new List<double?> { 0 },
                >= 5 and < 20 => new List<double?> { .50, .30, .20 },                     // Top 3 Payout
                >= 20 and < 30 => new List<double?> { .50, .30, .15, .10 },               // Top 4 Payout
                >= 30 and < 40 => new List<double?> { .45, .20, .15, .10, .05, .05 },     // Top 6 Payout
                >= 40 => new List<double?> { .42, .19, .14, .09, .05, .05, .03, .03 },    // Top 8 Payout
            };
            return payouts.Select(x => x * prizePool).ToList();
        }

        public static string IntToPodiumName(int numToConvert)
        {
            return numToConvert switch{
               1 => "1st",
               2 => "2nd",
               3 => "3rd",
               _ => $"{numToConvert}th"
            };
        }
    }
}
