using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace dotNetGo
{
    //using the naive approach - no complex heuristics
    internal class MonteCarlo : IPlayer
    {
        private const int Size = GameParameters.BoardSize;
        private Board _actualBoard = new Board();
        [ThreadStatic] private static Move[] _availableMoves;
        [ThreadStatic] private static Board _testingBoard;
        [ThreadStatic] private static Board _startingTestingBoard;

        int GetAvailableMoves(Board b)
        {
            if (_availableMoves == null)
                _availableMoves = new Move[Size*Size+1];
            int moveCount = 0;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    //is on empty space on the board and not a friendly eye
                    if (b[i, j] == 0 && b.IsEye(i,j) != b.ActivePlayer)
                    {
                            _availableMoves[moveCount++] = new Move(i, j);
                    }
                }
            }
            return moveCount;
        }

        public int PlaySimulation()
        {
            if (_testingBoard == null)
                _testingBoard = new Board();
            _testingBoard.CopyStateFrom(_startingTestingBoard);
            int turnsSimulated = 0;
            while (turnsSimulated < GameParameters.GameDepth && _testingBoard.IsGameOver() == false)
            {
                turnsSimulated++;
                int moveCount = GetAvailableMoves(_testingBoard);
                Move pass = new Move(-1, -1); //добавить в список возможных ходов пас
                _availableMoves[moveCount++] = pass;
                _availableMoves.Shuffle(moveCount);
                for (int i = 0; i < moveCount; i++)
                {
                    if (_testingBoard.PlaceStone(_availableMoves[i]) == true)
                    {
                        break;
                    }
                }
            }
            int winner = _testingBoard.DetermineWinner();
            return winner;
        }

        double GetWinrate(Move move)
        {
            if (_startingTestingBoard == null)
                _startingTestingBoard = new Board();
            _startingTestingBoard.CopyStateFrom(_actualBoard);
            if (_startingTestingBoard.PlaceStone(move) == false)
                return -1;
            UInt64 sim = 0;
            int wins = 0;
            while (sim < GameParameters.Simulations)
            {
                int winner = PlaySimulation();
                if (winner != 0)
                {
                    sim++;
                    if (winner == _actualBoard.ActivePlayer)
                        wins++;
                }
            }
            return sim > 0 ? (double)wins / sim : -1;
        }

        public bool ReceiveTurn(Move m)
        {
            return _actualBoard.PlaceStone(m);
        }

        public string Name
        {
            get { return "MonteCarlo"; }
        }

        public Move GetMove()
        {
            DateTime start = DateTime.Now;
            int turnCount = GetAvailableMoves(_actualBoard);
            //most simple logic for the first couple of turns
            //reduces required computations and forbids AI from making stupid turns (should not do them anyway)
            turnCount = ApplyHeuristics(_actualBoard, turnCount);
            Node[] nodes = new Node[turnCount];
            for (int i = 0; i < turnCount; i++)
                nodes[i] = new Node(_availableMoves[i]);
            Parallel.For(0, turnCount, (i) =>
            {
                if (_availableMoves == null)
                    _availableMoves = new Move[Size*Size + 1];
                nodes[i].Winrate = GetWinrate(nodes[i].Pos);
            });
            double maxWin = -1;
            int maxWinIndex = -1;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Winrate > maxWin && nodes[i].Winrate >= 0)
                {
                    maxWin = nodes[i].Winrate;
                    maxWinIndex = i;
                }
            }
            DateTime end = DateTime.Now;
            TimeSpan ts = end - start;
            Move bestMove;
            if (maxWin < 0)
            {
                bestMove = new Move(-1, -1);
            }
            else
            {
                bestMove = nodes[maxWinIndex].Pos;
            }
            Console.WriteLine("Turbo-{1} has found move {2}({3},{4}) in {0} after {5} total sims", ts, _actualBoard.ActivePlayer == 1 ? "Black" : "White", _actualBoard.TurnNumber, bestMove.row, bestMove.column, GameParameters.Simulations*turnCount);
//            Console.WriteLine("Coords: {0}", nodes[maxWinIndex].Pos);
            return bestMove;
        }

        private static int ApplyHeuristics(Board board, int turnCount)
        {
            int j = 0;
            int k = 0;
            if (board.TurnNumber < 5)
            {
                while (j < turnCount)
                {
                    Move m = _availableMoves[j++];
                    if (m.row > 1 && m.row < 7 && m.column > 1 && m.column < 7)
                        _availableMoves[k++] = m;
                }
            }
            else if (board.TurnNumber < 10)
            {
                while (j < turnCount)
                {
                    Move m = _availableMoves[j++];
                    if (m.row > 0 && m.row < 8 && m.column > 0 && m.column < 8)
                        _availableMoves[k++] = m;
                }
            }
            else k = turnCount;
            return k;
        }
    }
}
