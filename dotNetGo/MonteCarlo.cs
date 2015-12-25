using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace dotNetGo
{
    //using the naive approach - no complex heuristics
    internal class MonteCarlo
    {
        private const int Size = GameParameters.boardSize;
        [ThreadStatic] private static Move[] _availableMoves;

        int GetAvailableMoves(Board b)
        {
            if (_availableMoves == null)
                _availableMoves = new Move[Size*Size + 1];
            int moveCount = 0;
            int eyeOwner;
            bool f;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    //is on empty space on the board
                    if (b.IsOnBoard(i, j) && b[i, j] == 0)
                    {
                        eyeOwner = 0;
                        f = b.IsEye(i, j, out eyeOwner);
                        //is not a friendly eye
                        if (f == false || eyeOwner != b.ActivePlayer)
                        {
                            _availableMoves[moveCount++] = new Move(i, j);
                        }
                    }
                }
            }
            return moveCount;
        }

        public int PlaySimulation(Board startingBoard)
        {
            Board board = new Board();
            board.CopyState(startingBoard);
            int turnsSimulated = 0;
            while (turnsSimulated < GameParameters.GameDepth && board.IsGameOver() == false)
            {
                turnsSimulated++;
                int turnCount = GetAvailableMoves(board);
                Move pass = new Move(-1, -1); //добавить в список возможных ходов пас
                _availableMoves[turnCount++] = pass;
                _availableMoves.Shuffle();
//                foreach (Move move in availableMoves)
                    for (int i = 0; i < turnCount; i++)
                {
                    if (board.PlaceStone(_availableMoves[i]) == true)
                    {
                        break;
                    }
                }
            }
            double white, black;
            int winner = board.DetermineWinner(out black, out white);
            return winner;
        }

        double GetWinrate(Move move, Board startingBoard)
        {
            UInt64 simulations;
            checked
            {
                simulations = GameParameters.Simulations + (UInt64) Math.Pow(GameParameters.growthFactor, startingBoard.TurnNumber);
            }
            simulations = Math.Min(simulations, GameParameters.MaxSimulations);
            int player = startingBoard.ActivePlayer;
            Board b = new Board();
            b.CopyState(startingBoard);
            if (b.PlaceStone(move) == false)
                return 0;
            UInt64 sim = 0;
            int wins = 0;
            while (sim < simulations)
            {
                int winner = PlaySimulation(b);
                if (winner != 0)
                {
                    sim++;
                    if (winner == player)
                        wins++;
                }
            }
            return sim > 0 ? (double)wins / sim : 0;
        }


        public Move GetMove(Board board)
        {
            Board b = new Board();
            b.CopyState(board);
            DateTime start = DateTime.Now;
            int turnCount = GetAvailableMoves(b);
            //most simple logic for the first couple of turns
            //reduces required computations and forbids AI from making stupid turns (should not do them anyway)
            int k = 0;
            int j = 0;
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
            turnCount = k;
            //add pass
            _availableMoves[turnCount++] = new Move(-1, -1);
            Node[] nodes = new Node[turnCount];
            for (int i = 0; i < turnCount; i++)
                nodes[i] = new Node(_availableMoves[i]);
            Parallel.For(0, turnCount, (i) =>
            {
                if (_availableMoves == null)
                    _availableMoves = new Move[Size*Size + 1];
                nodes[i].Winrate = GetWinrate(nodes[i].Pos, b);
            });
            double maxWin = -1;
            int maxWinIndex = -1;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Winrate > maxWin)
                {
                    maxWin = nodes[i].Winrate;
                    maxWinIndex = i;
                }
            }
            DateTime end = DateTime.Now;
            TimeSpan ts = end - start;
            if (maxWin < 0.05)
            {
                return new Move(-2, -2);
            }
            if (maxWinIndex == -1)
            {
//                Console.WriteLine("Turbo has passed");
                return new Move(-1, -1);
            }
            Move bestMove = nodes[maxWinIndex].Pos;
            Console.WriteLine("Turbo-{1} has found move {2}({3},{4}) in {0} after {5} sims", ts, board.ActivePlayer == 1 ? "Black" : "White", board.TurnNumber, bestMove.row, bestMove.column, GameParameters.Simulations + Math.Pow(GameParameters.growthFactor, board.TurnNumber));
//            Console.WriteLine("Coords: {0}", nodes[maxWinIndex].Pos);
            return nodes[maxWinIndex].Pos;
        }
    }
}
