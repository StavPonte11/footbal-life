using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Pure C# closed beta enrollment service managing invitation passes, cohort distribution,
    /// expiration checks, and redemption limits (#P7-901).
    /// </summary>
    public sealed class BetaEnrollmentService
    {
        private static readonly Lazy<BetaEnrollmentService> _lazyInstance =
            new Lazy<BetaEnrollmentService>(() => new BetaEnrollmentService());

        public static BetaEnrollmentService Instance => _lazyInstance.Value;

        private readonly object _lock = new object();
        private readonly Dictionary<string, BetaAccessPass> _passes =
            new Dictionary<string, BetaAccessPass>(StringComparer.OrdinalIgnoreCase);

        public BetaEnrollmentService()
        {
            RegisterDefaultBetaPasses();
        }

        private void RegisterDefaultBetaPasses()
        {
            // Seed default passes for each target closed beta cohort
            RegisterPass(new BetaAccessPass(
                Code: "PIONEER-2026",
                Cohort: BetaCohortType.ExternalPioneers,
                ExpirationUtc: DateTime.UtcNow.AddMonths(3),
                MaxRedemptions: 250
            ));

            RegisterPass(new BetaAccessPass(
                Code: "MOBILE-KICKOFF",
                Cohort: BetaCohortType.MobileGamers,
                ExpirationUtc: DateTime.UtcNow.AddMonths(3),
                MaxRedemptions: 150
            ));

            RegisterPass(new BetaAccessPass(
                Code: "SIM-DERBY",
                Cohort: BetaCohortType.FootballSimFans,
                ExpirationUtc: DateTime.UtcNow.AddMonths(3),
                MaxRedemptions: 100
            ));

            RegisterPass(new BetaAccessPass(
                Code: "VIP-BALLON",
                Cohort: BetaCohortType.CommunityVIP,
                ExpirationUtc: DateTime.UtcNow.AddMonths(6),
                MaxRedemptions: 50
            ));
        }

        public void RegisterPass(BetaAccessPass pass)
        {
            if (pass == null) throw new ArgumentNullException(nameof(pass));
            lock (_lock)
            {
                _passes[pass.Code.Trim()] = pass;
            }
        }

        public BetaAccessPass? GetPass(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            lock (_lock)
            {
                return _passes.TryGetValue(code.Trim(), out var pass) ? pass : null;
            }
        }

        /// <summary>
        /// Validates and redeems an invitation pass code, checking active status, expiration, and capacity.
        /// Returns a deterministic authentication token on success.
        /// </summary>
        public BetaEnrollmentResult RedeemCode(string code, string? deviceId = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new BetaEnrollmentResult(BetaEnrollmentStatus.CodeNotFound, "Invitation code cannot be empty.", null, null);
            }

            lock (_lock)
            {
                string cleanCode = code.Trim();
                if (!_passes.TryGetValue(cleanCode, out var pass))
                {
                    return new BetaEnrollmentResult(BetaEnrollmentStatus.CodeNotFound, $"Invalid invitation code '{cleanCode}'.", null, null);
                }

                if (!pass.IsActive)
                {
                    return new BetaEnrollmentResult(BetaEnrollmentStatus.Revoked, "This beta invitation code has been revoked.", pass, null);
                }

                if (DateTime.UtcNow > pass.ExpirationUtc)
                {
                    return new BetaEnrollmentResult(BetaEnrollmentStatus.Expired, "This beta invitation code has expired.", pass, null);
                }

                if (pass.CurrentRedemptions >= pass.MaxRedemptions)
                {
                    return new BetaEnrollmentResult(BetaEnrollmentStatus.CohortFull, "This beta cohort is currently at maximum capacity.", pass, null);
                }

                // Increment redemption count
                var updatedPass = pass with { CurrentRedemptions = pass.CurrentRedemptions + 1 };
                _passes[cleanCode] = updatedPass;

                // Deterministic beta access token
                string salt = deviceId ?? "general_beta_device";
                string rawToken = $"{cleanCode}:{updatedPass.Cohort}:{updatedPass.CurrentRedemptions}:{salt}";
                using var sha = SHA256.Create();
                string token = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(rawToken))).Replace("-", "").ToLowerInvariant();

                return new BetaEnrollmentResult(
                    Status: BetaEnrollmentStatus.Valid,
                    Message: $"Successfully enrolled in {updatedPass.Cohort} beta cohort.",
                    Pass: updatedPass,
                    BetaToken: token
                );
            }
        }
    }
}
