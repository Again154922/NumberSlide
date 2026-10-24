namespace NumberSlide.Core;

public class Board
{
    private const byte MinWidth = 3;
    private const byte MaxWidth = 16;

    public byte Width { get; }
    public byte[,] BoardMap { get; set; }
    public (byte x, byte y) Empty { get; private set; }

    public Board(byte width)
    {
        if (width is < MinWidth or > MaxWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                $"棋盘宽度必须在 {MinWidth} 到 {MaxWidth} 之间。");
        }

        Width = width;
        BoardMap = new byte[width, width];
        InitMap();
    }

    public void Swap((byte x, byte y) tile1, (byte x, byte y) tile2)
    {
        if (Width <= tile1.x || Width <= tile1.y || Width <= tile2.x || Width <= tile2.y) return;
        
        byte value1 = BoardMap[tile1.x, tile1.y];
        byte value2 = BoardMap[tile2.x, tile2.y];
        BoardMap[tile1.x, tile1.y] = value2;
        BoardMap[tile2.x, tile2.y] = value1;
        
        SetEmpty();
    }

    public void InitMap()
    {
        int cellCount = Width * Width;
        byte[] values = new byte[cellCount];
        for (int i = 0; i < values.Length; i++)
            values[i] = (byte)i;

        Random random = new();
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        int nextValueIndex = 0;
        for (byte row = 0; row < Width; row++)
        {
            for (byte column = 0; column < Width; column++)
            {
                BoardMap[row, column] = values[nextValueIndex++];
            }
        }

        if (!CanSolve())
        {
            // 随机排列有一半概率不可解，交换两个非空格即可修正奇偶性。
            (byte x, byte y) firstTile = default;
            bool hasFirstTile = false;

            for (byte row = 0; row < Width && !hasFirstTile; row++)
            {
                for (byte column = 0; column < Width; column++)
                {
                    if (BoardMap[row, column] == 0)
                        continue;

                    firstTile = (row, column);
                    hasFirstTile = true;
                    break;
                }
            }

            for (byte row = 0; row < Width; row++)
            {
                for (byte column = 0; column < Width; column++)
                {
                    if ((row, column) == firstTile || BoardMap[row, column] == 0)
                        continue;

                    Swap(firstTile, (row, column));
                    return;
                }
            }
        }

        SetEmpty();
    }

    public bool CanSolve()
    {
        // 16 × 16 正好使用 byte 能表达的 0..255。
        if (Width == 0 || Width > 16)
            return false;

        byte maxTileValue = (byte)(Width * Width - 1);
        bool[] seen = new bool[Width * Width];
        byte blankCount = 0;
        byte blankRow = 0;

        for (byte row = 0; row < Width; row++)
        {
            for (byte column = 0; column < Width; column++)
            {
                byte value = BoardMap[row, column];
                if (value > maxTileValue || seen[value])
                    return false;

                seen[value] = true;
                if (value != 0)
                    continue;

                blankCount++;
                blankRow = row;
            }
        }

        if (blankCount != 1)
            return false;

        if (Array.IndexOf(seen, false) >= 0)
            return false;

        byte[] values = new byte[Width * Width - 1];
        byte nextValueIndex = 0;

        for (byte row = 0; row < Width; row++)
        {
            for (byte column = 0; column < Width; column++)
            {
                byte value = BoardMap[row, column];
                if (value != 0)
                    values[nextValueIndex++] = value;
            }
        }

        byte inversionsParity = 0;
        for (byte i = 0; i < values.Length; i++)
        {
            for (byte j = (byte)(i + 1); j < values.Length; j++)
            {
                if (values[i] > values[j])
                    inversionsParity ^= 1;
            }
        }

        if ((Width & 1) == 1)
            return inversionsParity == 0;

        byte blankRowFromBottom = (byte)(Width - blankRow);
        return ((inversionsParity + blankRowFromBottom) & 1) == 1;
    }

    public void SetEmpty()
    {
        for (byte i = 0; i < Width; i++)
        {
            for (byte j = 0; j < Width; j++)
            {
                if (BoardMap[i, j] == 0) Empty = (i, j);
            }
        }
    }
}
