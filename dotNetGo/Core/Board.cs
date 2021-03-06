using System;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.XPath;

namespace dotNetGo
{
    class Board 
    {
        public enum GameState
        {
            GameIsNotOver,
            BlackSurrendered,
            WhiteSurrendered,
            DoublePass
        }
        //board size
        public const int Size = GameParameters.BoardSize;
        private const double _komi = GameParameters.Komi;
        //cardinal directions
        private static readonly int[,] CardinalDirections = {{-1, 0}, {0, 1}, {1, 0}, {0, -1}};
        private static readonly int[,] DiagonalDirections = {{-1, 1}, {1, 1}, {1, -1}, {-1, -1}};
        public const int DirectionCount = 4;
        private int _passes = 0;
        //turn number - needed for simulations
        public int TurnNumber { get; private set; }

        public int Passes
        {
            get
            {
                return _passes;
            }
            private set
            {
                _passes = value;
                if (_passes >= 2)
                    State = GameState.DoublePass;
            }
        }
        public byte ActivePlayer { get; private set; }

        public GameState State { get; private set; }

        public byte OppositePlayer
        {
            get { return (byte)(3 - ActivePlayer); }
        }

        private readonly bool[,] _visited = new bool[Size,Size];
        private readonly byte[,] _buffer = new byte[Size,Size];
        //needed for ko checks
        private readonly byte[,] _lastPosition = new byte[Size,Size];

        readonly byte[,] _board = new byte[Size, Size];

        public byte this[int i, int j]
        {
            get { return _board[i, j]; }
            set { _board[i, j] = value; }
        }

        public byte this[Move m]
        {
            get { return _board[m.row, m.column]; }
            set { _board[m.row, m.column] = value; }
        }

        public byte[,] GetBoard()
        {
            return _board;
        }
        public Board()
        {
            ActivePlayer = 1;
            TurnNumber = 1;
        }
        public void CopyStateFrom(Board b)
        {
            TurnNumber = b.TurnNumber;
            Passes = b.Passes;
            ActivePlayer = b.ActivePlayer;
            State = b.State;
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                {
                    _visited[i, j] = b._visited[i, j];
                    _board[i, j] = b[i, j];
                    _lastPosition[i, j] = b._lastPosition[i, j];
                }
        }

        public bool PlaceStone(Move move)
        {
            if (State != GameState.GameIsNotOver)
                return false;
            if (move.row == -1 && move.column == -1)
            {
                Pass();
                return true;
            }
            //check if the move is on the board
            if (IsOnBoard(move) == false)
            {
                return false;
            }
            //check if the intersection is already occupied
            if (_board[move.row, move.column] != 0)
            {
                return false;
            }
            //check if the move is forbidden because of the Ko rule
            if (IsKo(move.row, move.column))
            {
                return false;
            }
            Array.Copy(_board, _buffer, _board.Length);
            this[move] = ActivePlayer;

            //if there is an enemy group nearby, it won't contain newly-placed stone - have to check each one individually
            Array.Clear(_visited, 0, _visited.Length);
            bool isSuicide = true; //для более быстрого определения возможности хода. Если рядом с новым камнем есть свободное пересечение, то это точно не суицид
            for (int i = 0; i < DirectionCount; i++) //first check opponent's groups
            {
                int testRow = move.row + CardinalDirections[i, 0];
                int testCol = move.column + CardinalDirections[i, 1];
                //если клетка находится за доской или на ней стоит союзный камень - пропускаем. проверка союзных камней будет позже
                //if an intersection is outside the board or has allied stone - skip it for now. allied checks will come later
                if (IsOnBoard(testRow, testCol) == false || _board[testRow, testCol] == ActivePlayer)
                    continue;
                if (_board[testRow, testCol] == 0)
                    //if a neighbouring intersection is empty, then the new stone will definitely have at least 1 liberty
                {
                    isSuicide = false;
                    continue;
                }
                if (_board[testRow, testCol] == OppositePlayer)
                {
                    if (_visited[testRow, testCol] == false)
                    {
                        Array.Clear(_visited, 0, _visited.Length);
                        if (IsGroupDead(testRow, testCol))
                        {
                            RemoveGroup(testRow, testCol);
                        }
                    }
                }
            }
            Array.Clear(_visited, 0, _visited.Length);
            //если рядом с новым камнем есть дракон, то он включает в себя и этот камень
            //if there is a nearby friendly dragon, it will contain newly-placed stone
            if (isSuicide == true && IsGroupDead(move.row, move.column) == true)
            {
                this[move] = 0;
                return false;
            }

            ActivePlayer = OppositePlayer;
            Passes = 0;
            TurnNumber++;
            Array.Copy(_buffer, _lastPosition, _lastPosition.Length);
            return true;
        }
        

        int RemoveGroup(int row, int col)
        {
            if (IsOnBoard(row, col) == false || IsFree(row, col) == true)
                return 0;
            if (_board[row, col] == ActivePlayer)
                //if we encounter an active's player stone - skip it, because suicide is forbidden
                return 0;

            //AT THIS POINT, we know that an intersection is on board and it contains an opponent's stone
            int result = 1;
            _board[row, col] = 0;
            for (int i = 0; i < DirectionCount; i++)
            {
                int testRow = row + CardinalDirections[i, 0];
                int testCol = col + CardinalDirections[i, 1];
                result += RemoveGroup(testRow, testCol);
            }
            return result;
        }

        int RemoveGroup(Move m)
        {
            return RemoveGroup(m.row, m.column);
        }
        
        public void Pass()
        {
            ActivePlayer = OppositePlayer;
            TurnNumber++;
            Passes++;
        }

        //counts liberties of a stone group that include the stone at coordinates of m
        //return values:
        //-1 if m is empty space, or if this space has already been visited (to remove redundant dragon checks)
        //number of liberties of the dragon otherwise
        private bool IsGroupDead(int row, int col)
        {
            if (_board[row, col] == 0 || _visited[row, col] == true)
                return false;
            _visited[row, col] = true;
            for (int i = 0; i < DirectionCount; i++)
            {
                int testRow = row + CardinalDirections[i, 0];
                int testCol = col + CardinalDirections[i, 1];
                if (IsOnBoard(testRow, testCol) == false || _visited[testRow, testCol] == true)
                    continue;
                if (IsFree(testRow, testCol))
                {
                    return false;
                }
                else if (_board[testRow, testCol] == _board[row, col])
                    if (IsGroupDead(testRow, testCol) == false)
                        return false;
            }
            return true;
        }
        public bool IsFree(int row, int col)
        {
            return IsOnBoard(row, col) && _board[row, col] == 0;
        }
        public bool IsFree(Move m)
        {
            return IsOnBoard(m) && _board[m.row, m.column] == 0;
        }
        public bool IsOnBoard(int row, int col)
        {
            return row >= 0 & row < Size && col >= 0 && col < Size;
        }
        public bool IsOnBoard(Move m)
        {
            return m.row >= 0 & m.row < Size && m.column >= 0 && m.column < Size;
        }
        public int DetermineWinner(out double blackScore, out double whiteScore)
        {
            whiteScore = 0;
            blackScore = 0;

            if (State == GameState.BlackSurrendered)
                return 2;
            if (State == GameState.WhiteSurrendered)
                return 1;
            if (IsGameOver() == false)
                return 0;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    switch (_board[i, j])
                    {
                        case 1:
                            blackScore++;
                            break;
                        case 2:
                            whiteScore++;
                            break;
                        case 0:
                            switch (CheckEye(i, j))
                            {
                                case 1:
                                    blackScore++;
                                    break;
                                case 2:
                                    whiteScore++;
                                    break;
                                default:
                                    throw new Exception("Scoring error");
                            }
                            break;
                    }
                }
            }
            whiteScore += _komi;
            if (blackScore > whiteScore)
                return 1;
            else return 2;
        }

        public int DetermineWinner()
        {
            int whiteScore = 0;
            int blackScore = 0;

            if (State == GameState.BlackSurrendered)
                return 2;
            if (State == GameState.WhiteSurrendered)
                return 1;
            if (IsGameOver() == false)
                return 0;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    switch (_board[i, j])
                    {
                        case 1:
                            blackScore++;
                            break;
                        case 2:
                            whiteScore++;
                            break;
                        case 0:
                            switch (CheckEye(i, j))
                            {
                                case 1:
                                    blackScore++;
                                    break;
                                case 2:
                                    whiteScore++;
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                }
            }
            whiteScore += (int)_komi;
            if (blackScore <= whiteScore)
                return 2;
            return 1;
        }

        public void Surrender()
        {
            switch (ActivePlayer)
            {
                case 1:
                    State = GameState.BlackSurrendered;
                    break;
                case 2:
                    State = GameState.WhiteSurrendered;
                    break;
            }
        }

        //checks whether the game is over
        //returns true if there all empty spaces are 1-space eyes
        //returns false when there are potential moves left
        public bool IsGameOver()
        {
            if (State != GameState.GameIsNotOver)
                return true;
            //now check if all existing empty intersection are eyes. If they are - the game is definitely over
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (CheckEye(i, j) == 0)
                        return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("  ");
            for (int i = 0; i < Size; i++)
                sb.Append((i%10).ToString());
            sb.AppendLine();
            for (int i = 0; i < Size; i++)
            {
                sb.Append(String.Format("{0} ", i%10));
                for (int j = 0; j < Size; j++)
                {
                    switch (_board[i, j])
                    {
                        case 1:
                            sb.Append("b");
                            break;
                        case 2:
                            sb.Append("w");
                            break;
                        default:
                            sb.Append(".");
                            break;
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public Board Clone()
        {
            Board result = new Board();
            result.CopyStateFrom(this);
            return result;
        }

        //return values:
        //0 - not an eye
        //1 - black eye
        //2 - white eye
        //an intersection is an eye when all immediately surrounding stones belong to the same dragon - also fixes false eyes
        public int CheckEye(int row, int col)
        {
            if (IsOnBoard(row, col) == false || IsFree(row, col) == false)
            {
                return 0;
            }
            int black = 0;
            int white = 0;
            for (int i = 0; i < 4; i++)
            {
                int testRow = row + CardinalDirections[i, 0];
                int testCol = col + CardinalDirections[i, 1];

                if (IsOnBoard(testRow, testCol) == false) //for board borders
                {
                    black++;
                    white++;
                    continue;
                }

                if (IsFree(testRow, testCol)) //if a neighbouring intersection is empty, then it is not an eye (eye-space does not count)
                    return 0;
            }
            for (int i = row - 1; i <= row + 1; i++)
            {
                for (int j = col - 1; j <= col + 1; j++)
                {
                    if (i < 0 || i >= Size || j < 0 || j >= Size || i == j)
                    {
                        black++;
                        white++;
                        continue;
                    }
                    switch (_board[i,j])
                    {
                        case 1:
                            black++;
                            break;
                        case 2:
                            white++;
                            break;
                    }
                }
            }
            if (row == 0 || row == Size-1)
            {
                return black == 8 ? 1 : white == 8 ? 2 : 0;
            }
            else if (col == 0 || col == Size - 1)
            {
                return black == 8 ? 1 : white == 8 ? 2 : 0;
            }
            else
                return black >= 7 ? 1 : white >= 7 ? 2 : 0;

        }

        public int CheckEye(Move move) //false eyes fixed
        {
            return CheckEye(move.row, move.column);
        }

        public bool IsKo(int row, int col)
        {
            int differences = 0;
            if (_lastPosition[row, col] != ActivePlayer)
                return false;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (_board[i, j] != _lastPosition[i, j])
                        differences++;
                }
                if (differences > 2)
                    return false;
            }
            //AT THIS POINT: we know that one of the differences is the current point, and there are 2 of them in total
            //the other one MUST be adjacent (cannot be otherwise), so it is Ko and the move is forbidden
            return true;
        }
    }
}