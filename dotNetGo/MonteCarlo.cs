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
        private int size = GameParameters.boardSize;

        List<Move> GetAvailableMoves(Board b)
        {
            List<Move> result = new List<Move>(size * size);
            int eyeOwner;
            bool f;
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    //is on empty space on the board
                    if (b.IsOnBoard(i, j) && b[i, j] == 0)
                    {
                        eyeOwner = 0;
                        f = b.IsEye(i, j, out eyeOwner);
                        //is not a friendly eye
                        if (f == false || eyeOwner != b.ActivePlayer)
                        {
                            result.Add(new Move(i, j));
                        }
                    }
                }
            return result;
        }

        public int PlaySimulation(Board startingBoard)
        {
            Board board = new Board();
            board.CopyState(startingBoard);
            int turnsSimulated = 0;
            while (turnsSimulated < GameParameters.GameDepth && board.IsGameOver() == false)
            {
                turnsSimulated++;
                List<Move> availableMoves = GetAvailableMoves(board);
                Move pass = new Move(-1, -1); //добавить в список возможных ходов пас
                availableMoves.Add(pass);
                availableMoves.Shuffle();
                foreach (Move move in availableMoves)
                {
                    if (board.PlaceStone(move) == true)
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
            List<Move> availableMoves = GetAvailableMoves(b);
            //most simple logic for the first couple of turns
            //reduces required computations and forbids AI from making stupid turns (should not do them anyway)
            if (board.TurnNumber < 5)
            {
                availableMoves = availableMoves.Where(_move => _move.row > 1 && _move.row < 7 
                                                    && _move.column > 1 && _move.column < 7).ToList();
            }
            else if (board.TurnNumber < 10)
                availableMoves = availableMoves.Where(_move => _move.row > 0 && _move.row < 8
                                                    && _move.column > 0 && _move.column < 8).ToList();
            //add pass
            availableMoves.Add(new Move(-1, -1));
            Node[] nodes = new Node[availableMoves.Count];
            for (int i = 0; i < availableMoves.Count; i++)
                nodes[i] = new Node(availableMoves[i]);
            Parallel.For(0, availableMoves.Count, (i) =>
            {
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
