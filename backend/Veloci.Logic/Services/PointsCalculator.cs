namespace Veloci.Logic.Services;

public class PointsCalculator
{
    private readonly Dictionary<int, int> _pointsTable;

    public PointsCalculator()
    {
        _pointsTable = BuildPointsTable();
    }

    public int PointsByPosition(int position)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 1);

        return position >= _pointsTable.Count
            ? _pointsTable.Last().Value
            : _pointsTable[position];
    }

    private static Dictionary<int, int> BuildPointsTable()
    {
        var result = new Dictionary<int, int>
        {
            {1, 100},
            {2, 85},
            {3, 75},
            {4, 67},
            {5, 60},
            {6, 54},
            {7, 49},
            {8, 45},
            {9, 41},
            {10, 38},
            {11, 35},
            {12, 33},
            {13, 31},
            {14, 29},
            {15, 27},
            {16, 25},
            {17, 23},
            {18, 22},
            {19, 21},
            {20, 20},
            {21, 19},
            {22, 18},
            {23, 17},
            {24, 16},
            {25, 15},
            {26, 14},
            {27, 13},
            {28, 12},
            {29, 11},
            {30, 10},
            {31, 9},
            {32, 8},
            {33, 7},
            {34, 6},
            {35, 5},
            {36, 4},
            {37, 3},
            {38, 2},
            {39, 1},
        };

        return result;
    }
}
