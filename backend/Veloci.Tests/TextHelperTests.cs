using FluentAssertions;
using Veloci.Logic.Helpers;

namespace Veloci.Tests;

public class TextHelperTests
{
    private const string LeaderboardText =
        "🗓 Проміжні результати місяця\n\n" +
        "1 - CrashAirlines - 2057 балів\n" +
        "2 - Shkvarok - 1648 балів\n" +
        "3 - JoderStack - 1595 балів\n" +
        "4 - YANIS FPV - 1507 балів\n" +
        "5 - vpro - 1482 балів\n" +
        "6 - KAKA0 - 1293 балів\n" +
        "7 - mur - 1257 балів\n" +
        "8 - iamslavko - 1056 балів\n" +
        "9 - MKWay13 - 893 балів\n" +
        "10 - TribeXFire - 811 балів\n" +
        "11 - timmal - 747 балів\n" +
        "12 - StDuck - 711 балів\n" +
        "13 - Jaxon - 697 балів\n" +
        "14 - VaderFPV - 684 балів\n" +
        "15 - SKRVXMX - 661 балів\n" +
        "16 - Edrin - 639 балів\n" +
        "17 - _Yui_ - 631 балів\n" +
        "18 - Georgich_iCop.. - 576 балів\n" +
        "19 - fpv.rodriguez - 484 балів\n" +
        "20 - Zergatul_UA - 461 балів\n" +
        "21 - INKcat - 455 балів\n" +
        "22 - KEMFPV - 454 балів\n" +
        "23 - Plasmid - 453 балів\n" +
        "24 - serpw1ng - 417 балів\n" +
        "25 - Didko - 415 балів\n" +
        "26 - brfpv - 413 балів\n" +
        "27 - aLen nato - 403 балів\n" +
        "28 - paszkevi4 - 403 балів\n" +
        "29 - kimi_lhy - 402 балів\n" +
        "30 - VesPerFPV - 323 балів\n" +
        "31 - GravityD0101 - 321 балів\n" +
        "32 - chater - 301 балів\n" +
        "33 - CrushUA - 277 балів\n" +
        "34 - f1ct1onal - 266 балів\n" +
        "35 - LEOP4RD - 249 балів\n" +
        "36 - FPViktor97 - 216 балів\n" +
        "37 - Ch1Rp_ - 200 балів\n" +
        "38 - LolikLA - 167 балів\n" +
        "39 - Bam_ - 167 балів\n" +
        "40 - r00d1k - 151 балів\n" +
        "41 - Hennadii - 145 балів\n" +
        "42 - Glazovsky  - 140 балів\n" +
        "43 - SENKIV - 137 балів\n" +
        "44 - GIFPV - 121 балів\n" +
        "45 - mst - 117 балів\n" +
        "46 - SerhFPV - 103 балів\n" +
        "47 - Enay - 69 балів\n" +
        "48 - _Galik_ - 67 балів\n" +
        "49 - unnamed_1991 - 66 балів\n" +
        "50 - vinprok - 62 балів\n" +
        "51 - TomyLEE - 60 балів\n" +
        "52 - olegeau - 59 балів\n" +
        "53 - eguire - 52 балів\n" +
        "54 - K1R - 41 балів\n" +
        "55 - -BumblebeeFPV- - 27 балів\n" +
        "56 - kingdu - 20 балів\n" +
        "57 - garza - 19 балів\n" +
        "58 - g0rsky - 17 балів\n" +
        "59 - Funtik - 15 балів\n" +
        "60 - stonedsailor - 15 балів\n" +
        "61 - dmitriprank - 15 балів\n" +
        "62 - BATYA_64ukaTEAM - 15 балів\n" +
        "63 - BadDeadGinger - 11 балів\n" +
        "64 - RedKiborg - 10 балів\n" +
        "65 - Royur - 9 балів\n" +
        "66 - vitaliybvo - ";

    [Fact]
    public void short_message_returns_single_chunk()
    {
        var result = TextHelper.SplitIntoChunks("Hello", 100);

        result.Should().ContainSingle().Which.Should().Be("Hello");
    }

    [Fact]
    public void message_at_exact_limit_returns_single_chunk()
    {
        var message = new string('a', 100);

        var result = TextHelper.SplitIntoChunks(message, 100);

        result.Should().ContainSingle().Which.Should().Be(message);
    }

    [Fact]
    public void splits_at_last_newline_within_limit()
    {
        // chunkSize=15: window "line one\nline t" — last \n at index 8
        var result = TextHelper.SplitIntoChunks("line one\nline two\nline three", chunkSize: 15);

        result.Should().Equal("line one", "line two", "line three");
    }

    [Fact]
    public void falls_back_to_hard_cut_when_no_newline_in_range()
    {
        var result = TextHelper.SplitIntoChunks("ABCDEFGHIJKLMNOPQRST", chunkSize: 10);

        result.Should().Equal("ABCDEFGHIJ", "KLMNOPQRST");
    }

    [Fact]
    public void leaderboard_message_splits_into_multiple_chunks_each_within_limit()
    {
        const int chunkSize = 500;

        var result = TextHelper.SplitIntoChunks(LeaderboardText, chunkSize);

        result.Should().HaveCountGreaterThan(1);
        result.Should().AllSatisfy(chunk => chunk.Length.Should().BeLessThanOrEqualTo(chunkSize));
    }

    [Fact]
    public void leaderboard_message_reassembles_to_original_after_split()
    {
        const int chunkSize = 500;

        var result = TextHelper.SplitIntoChunks(LeaderboardText, chunkSize);

        string.Join("\n", result).Should().Be(LeaderboardText);
    }
}
