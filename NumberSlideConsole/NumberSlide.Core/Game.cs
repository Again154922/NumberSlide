namespace NumberSlide.Core;

public class Game(byte boardWidth)
{
    private static readonly Dictionary<Direction, (int dx, int dy)> dir = new() {
        { Direction.Up, (1, 0) },
        { Direction.Left, (0, 1) },
        { Direction.Down, (-1, 0) },
        { Direction.Right, (0, -1) }
    };
    public readonly Board board = new Board(boardWidth);
    private readonly Stack<byte[,]> MoveStack = new();

    public void Move(Direction direction)
    {
        int dx = dir[direction].dx;
        int dy = dir[direction].dy;
        
        if (!(board.Empty.x + dx < board.Width && board.Empty.x + dx >= 0 &&
              board.Empty.y + dy < board.Width && board.Empty.y + dy >= 0))
            return;
        
        CopyMap(out byte[,] mapCopy);
        MoveStack.Push(mapCopy);
        
        board.Swap(board.Empty, ((byte)(board.Empty.x + dx), (byte)(board.Empty.y + dy)));
    }

    public void Undo()
    {
        if (!MoveStack.TryPop(out byte[,]? boardMap))
            return;

        board.BoardMap = boardMap;
        board.SetEmpty();
    }

    public void Restart()
    {
        MoveStack.Clear();
        board.InitMap();
    }

    public bool IsComplete()
    {
        for (byte row = 0; row < board.Width; row++)
        {
            for (byte column = 0; column < board.Width; column++)
            {
                byte expected = row == board.Width - 1 && column == board.Width - 1
                    ? (byte)0
                    : (byte)(row * board.Width + column + 1);

                if (board.BoardMap[row, column] != expected)
                    return false;
            }
        }

        return true;
    }

    private void CopyMap(out byte[,] mapCopy)
    {
        mapCopy = new byte[board.Width, board.Width];

        for (int i = 0; i < board.Width; i++)
        {
            for (int j = 0; j < board.Width; j++)
            {
                mapCopy[i, j] = board.BoardMap[i, j];
            }
        }
    }
}
