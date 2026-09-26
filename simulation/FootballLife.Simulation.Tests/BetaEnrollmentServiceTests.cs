using System;
using FootballLife.Domain;
using Xunit;

namespace FootballLife.Simulation.Tests
{
    public sealed class BetaEnrollmentServiceTests
    {
        [Fact]
        public void RedeemCode_ValidDefaultCode_EnrollsSuccessfully()
        {
            var service = new BetaEnrollmentService();

            var result = service.RedeemCode("PIONEER-2026", "device_alpha_1");

            Assert.Equal(BetaEnrollmentStatus.Valid, result.Status);
            Assert.NotNull(result.Pass);
            Assert.Equal(BetaCohortType.ExternalPioneers, result.Pass.Cohort);
            Assert.False(string.IsNullOrWhiteSpace(result.BetaToken));
            Assert.True(result.Pass.CurrentRedemptions > 0);
        }

        [Fact]
        public void RedeemCode_UnknownCode_ReturnsCodeNotFound()
        {
            var service = new BetaEnrollmentService();

            var result = service.RedeemCode("NON-EXISTENT-CODE", "device_xyz");

            Assert.Equal(BetaEnrollmentStatus.CodeNotFound, result.Status);
            Assert.Null(result.Pass);
            Assert.Null(result.BetaToken);
        }

        [Fact]
        public void RedeemCode_ExpiredPass_ReturnsExpired()
        {
            var service = new BetaEnrollmentService();
            var expiredPass = new BetaAccessPass(
                Code: "EXPIRED-BETA",
                Cohort: BetaCohortType.MobileGamers,
                ExpirationUtc: DateTime.UtcNow.AddDays(-1),
                MaxRedemptions: 100
            );
            service.RegisterPass(expiredPass);

            var result = service.RedeemCode("EXPIRED-BETA", "device_1");

            Assert.Equal(BetaEnrollmentStatus.Expired, result.Status);
            Assert.Null(result.BetaToken);
        }

        [Fact]
        public void RedeemCode_RevokedPass_ReturnsRevoked()
        {
            var service = new BetaEnrollmentService();
            var revokedPass = new BetaAccessPass(
                Code: "REVOKED-CODE",
                Cohort: BetaCohortType.FootballSimFans,
                ExpirationUtc: DateTime.UtcNow.AddMonths(1),
                MaxRedemptions: 100,
                IsActive: false
            );
            service.RegisterPass(revokedPass);

            var result = service.RedeemCode("REVOKED-CODE");

            Assert.Equal(BetaEnrollmentStatus.Revoked, result.Status);
            Assert.Null(result.BetaToken);
        }

        [Fact]
        public void RedeemCode_CohortFull_ReturnsCohortFull()
        {
            var service = new BetaEnrollmentService();
            var fullPass = new BetaAccessPass(
                Code: "CAPACITY-TEST",
                Cohort: BetaCohortType.CommunityVIP,
                ExpirationUtc: DateTime.UtcNow.AddMonths(1),
                MaxRedemptions: 2,
                CurrentRedemptions: 2
            );
            service.RegisterPass(fullPass);

            var result = service.RedeemCode("CAPACITY-TEST");

            Assert.Equal(BetaEnrollmentStatus.CohortFull, result.Status);
            Assert.Null(result.BetaToken);
        }
    }
}
