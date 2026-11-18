using FluentAssertions;
using FpvBattle.Domain;

namespace FpvBattle.Tests;

public class PatreonSupporterTests
{
    private static readonly DateTime Now = new(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void first_payment_is_recorded_once()
    {
        var supporter = Supporter(PatreonSupporter.ActivePatronStatus, amount: 5);

        supporter.TryRecordFirstPayment(Now).Should().BeTrue();
        supporter.FirstPaidAt.Should().Be(Now);

        supporter.TryRecordFirstPayment(Now.AddDays(1)).Should().BeFalse();
        supporter.FirstPaidAt.Should().Be(Now);
    }

    [Fact]
    public void free_member_has_no_first_payment()
    {
        var supporter = Supporter(PatreonSupporter.ActivePatronStatus, amount: 0);

        supporter.TryRecordFirstPayment(Now).Should().BeFalse();
        supporter.FirstPaidAt.Should().BeNull();
    }

    [Fact]
    public void recovered_declined_payment_is_not_a_first_payment()
    {
        var supporter = Supporter(PatreonSupporter.ActivePatronStatus, amount: 5);
        supporter.TryRecordFirstPayment(Now);

        supporter.Status = "declined_patron";
        supporter.TryRecordFirstPayment(Now.AddDays(1)).Should().BeFalse();

        supporter.Status = PatreonSupporter.ActivePatronStatus;
        supporter.TryRecordFirstPayment(Now.AddDays(2)).Should().BeFalse();
    }

    private static PatreonSupporter Supporter(string status, decimal amount) => new()
    {
        PatreonId = "1",
        Name = "kimi",
        Status = status,
        Amount = amount
    };
}
