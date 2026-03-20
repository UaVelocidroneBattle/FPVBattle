namespace Veloci.Data.Domain;

public class QuadModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Class { get; set; }
}

public static class QuadClasses
{
    public const int Race = 1;
    public const int Mega = 2;
    public const int Micro = 3;
    public const int ToothPick = 4;
    public const int Combat = 5;
    public const int Freestyle = 6;
    public const int StreetLeague = 7;
    public const int FreedomSpec = 8;
    public const int TbsSpec = 9;
    public const int ProSpec = 10;
}
