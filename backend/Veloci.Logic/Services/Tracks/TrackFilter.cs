using System.Text.RegularExpressions;
using Veloci.Logic.Services.Tracks.Models;

namespace Veloci.Logic.Services.Tracks;

public class TrackFilter
{
    private static readonly Regex[] BlackListedTracks =
    {
        new ("Pylons", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("Freestyle", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("Betafpv", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("Beta 2S", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("Micro", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("NewBeeDrone", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("Toothpick", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("Trainer", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("Whoop", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("LiPo-Suction", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("WeeBleed", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("^level (01,02, 03)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

        //From old bot
        new ("collision", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("redbull dr.one", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("vrl season 3 track 3", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("vrl team championships", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("growers rock garden", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("vrl season 7 championships", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("gokartrelay", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("gods_of_quadhalla", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("vrl-freestyle-country", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("boners journey", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("world of war", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("corona", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("neon cage", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("tbs spec 4", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("tdl races - gamex 2019", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("^opg", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("boners bando towers", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("vrl-freestyle-coast", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("boners bonsai fpv 4 freestyle", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("freestyle_tower_of_magical_power", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("dragons_and_wizards", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("trainer", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("tropical heat", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("rona masters", RegexOptions.Compiled | RegexOptions.IgnoreCase),
    };


    public bool IsTrackGoodFor5inchRacing(ParsedTrackModel track)
    {
        return !BlackListedTracks.Any(b => b.IsMatch(track.Name));
    }
}
