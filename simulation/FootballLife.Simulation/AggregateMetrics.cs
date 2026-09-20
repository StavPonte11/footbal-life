using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Statistical distribution summary for a numerical metric across simulated careers.
    /// </summary>
    public sealed record MetricDistribution(
        double Mean,
        double Median,
        double P10,
        double P90,
        double Min,
        double Max,
        double StdDev)
    {
        public static MetricDistribution Create(IReadOnlyList<double> values)
        {
            if (values == null || values.Count == 0)
            {
                return new MetricDistribution(0, 0, 0, 0, 0, 0, 0);
            }

            var sorted = values.OrderBy(v => v).ToArray();
            int n = sorted.Length;
            double mean = sorted.Average();
            double min = sorted[0];
            double max = sorted[n - 1];

            double median = Percentile(sorted, 0.50);
            double p10 = Percentile(sorted, 0.10);
            double p90 = Percentile(sorted, 0.90);

            double sumSquaredDiff = sorted.Sum(v => (v - mean) * (v - mean));
            double stdDev = Math.Sqrt(sumSquaredDiff / n);

            return new MetricDistribution(mean, median, p10, p90, min, max, stdDev);
        }

        private static double Percentile(double[] sorted, double p)
        {
            if (sorted.Length == 1) return sorted[0];

            double index = p * (sorted.Length - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);
            if (lower == upper) return sorted[lower];

            double weight = index - lower;
            return sorted[lower] * (1.0 - weight) + sorted[upper] * weight;
        }
    }

    /// <summary>
    /// Aggregate statistical report consolidating distributions across a batch of simulated careers.
    /// </summary>
    public sealed class AggregateReport
    {
        public int TotalCareers { get; }
        public MetricDistribution PeakOverall { get; }
        public MetricDistribution RetirementAge { get; }
        public MetricDistribution SeasonsPlayed { get; }
        public MetricDistribution TotalGoals { get; }
        public MetricDistribution TotalAssists { get; }
        public MetricDistribution TotalAppearances { get; }
        public MetricDistribution AverageRating { get; }
        public MetricDistribution TotalEarnings { get; }
        public MetricDistribution FinalBalance { get; }
        public MetricDistribution Transfers { get; }
        public double BankruptcyRate { get; }

        public AggregateReport(IReadOnlyList<CareerStatistics> careers)
        {
            if (careers == null) throw new ArgumentNullException(nameof(careers));
            TotalCareers = careers.Count;

            if (TotalCareers == 0)
            {
                PeakOverall = MetricDistribution.Create(Array.Empty<double>());
                RetirementAge = MetricDistribution.Create(Array.Empty<double>());
                SeasonsPlayed = MetricDistribution.Create(Array.Empty<double>());
                TotalGoals = MetricDistribution.Create(Array.Empty<double>());
                TotalAssists = MetricDistribution.Create(Array.Empty<double>());
                TotalAppearances = MetricDistribution.Create(Array.Empty<double>());
                AverageRating = MetricDistribution.Create(Array.Empty<double>());
                TotalEarnings = MetricDistribution.Create(Array.Empty<double>());
                FinalBalance = MetricDistribution.Create(Array.Empty<double>());
                Transfers = MetricDistribution.Create(Array.Empty<double>());
                BankruptcyRate = 0;
                return;
            }

            PeakOverall = MetricDistribution.Create(careers.Select(c => (double)c.PeakOverall).ToList());
            RetirementAge = MetricDistribution.Create(careers.Select(c => (double)c.RetirementAge).ToList());
            SeasonsPlayed = MetricDistribution.Create(careers.Select(c => (double)c.SeasonsPlayed).ToList());
            TotalGoals = MetricDistribution.Create(careers.Select(c => (double)c.TotalGoals).ToList());
            TotalAssists = MetricDistribution.Create(careers.Select(c => (double)c.TotalAssists).ToList());
            TotalAppearances = MetricDistribution.Create(careers.Select(c => (double)c.TotalAppearances).ToList());
            AverageRating = MetricDistribution.Create(careers.Select(c => (double)c.AverageRating).ToList());
            TotalEarnings = MetricDistribution.Create(careers.Select(c => (double)c.TotalEarnings).ToList());
            FinalBalance = MetricDistribution.Create(careers.Select(c => (double)c.FinalBalance).ToList());
            Transfers = MetricDistribution.Create(careers.Select(c => (double)c.TransferCount).ToList());
            BankruptcyRate = careers.Count(c => c.BankruptcyOccurred) / (double)TotalCareers * 100.0;
        }

        public string GenerateAsciiHistogram(IReadOnlyList<double> values, double min, double max, int bucketCount, string title, int barWidth = 30)
        {
            if (values.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"--- {title} Distribution ({values.Count} careers) ---");

            double bucketSize = (max - min) / bucketCount;
            int[] counts = new int[bucketCount];

            foreach (var v in values)
            {
                int bucket = (int)((v - min) / bucketSize);
                if (bucket < 0) bucket = 0;
                if (bucket >= bucketCount) bucket = bucketCount - 1;
                counts[bucket]++;
            }

            int maxCount = counts.Max();
            if (maxCount == 0) maxCount = 1;

            for (int i = 0; i < bucketCount; i++)
            {
                double bStart = min + i * bucketSize;
                double bEnd = bStart + bucketSize;
                int barLen = (int)Math.Round((double)counts[i] / maxCount * barWidth);
                string bar = new string('#', barLen);
                double pct = (double)counts[i] / values.Count * 100.0;
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "[{0,4:F0} - {1,4:F0}]: {2,-30} {3,5} ({4,5:F1}%)", bStart, bEnd, bar, counts[i], pct));
            }

            return sb.ToString();
        }

        public string FormatSummaryTable()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=======================================================================================================");
            sb.AppendLine($"                         CAREER SIMULATOR BATCH REPORT ({TotalCareers:N0} CAREERS)                     ");
            sb.AppendLine("=======================================================================================================");
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0,-20} | {1,8} | {2,8} | {3,8} | {4,8} | {5,8} | {6,8} | {7,8}", "Metric", "Mean", "Median", "P10", "P90", "Min", "Max", "StdDev"));
            sb.AppendLine("-------------------------------------------------------------------------------------------------------");
            AppendRow(sb, "Peak Overall", PeakOverall, "F1");
            AppendRow(sb, "Retirement Age", RetirementAge, "F1");
            AppendRow(sb, "Seasons Played", SeasonsPlayed, "F1");
            AppendRow(sb, "Total Appearances", TotalAppearances, "F0");
            AppendRow(sb, "Career Goals", TotalGoals, "F0");
            AppendRow(sb, "Career Assists", TotalAssists, "F0");
            AppendRow(sb, "Average Rating", AverageRating, "F2");
            AppendRow(sb, "Career Transfers", Transfers, "F1");
            AppendRow(sb, "Total Earnings (£)", TotalEarnings, "N0");
            AppendRow(sb, "Final Balance (£)", FinalBalance, "N0");
            sb.AppendLine("-------------------------------------------------------------------------------------------------------");
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Bankruptcy Rate: {0:F1}%", BankruptcyRate));
            sb.AppendLine("=======================================================================================================");
            return sb.ToString();
        }

        private static void AppendRow(StringBuilder sb, string name, MetricDistribution d, string fmt)
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0,-20} | {1,8} | {2,8} | {3,8} | {4,8} | {5,8} | {6,8} | {7,8}",
                name,
                d.Mean.ToString(fmt, CultureInfo.InvariantCulture),
                d.Median.ToString(fmt, CultureInfo.InvariantCulture),
                d.P10.ToString(fmt, CultureInfo.InvariantCulture),
                d.P90.ToString(fmt, CultureInfo.InvariantCulture),
                d.Min.ToString(fmt, CultureInfo.InvariantCulture),
                d.Max.ToString(fmt, CultureInfo.InvariantCulture),
                d.StdDev.ToString("F1", CultureInfo.InvariantCulture)));
        }
    }
}
