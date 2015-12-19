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
        Random r = new Random();
        
        List<Move> GetAvailableMoves(Board b)
        {
            List<Move> result = new List<Move>(size*size);
            int eyeOwner;
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    Move m = new Move(i,j);
                    eyeOwner = 0;
                    bool f = b.IsEye(m, out eyeOwner);
                    //is on empty space on the board
                    if (b.IsOnBoard(m) && b[i, j] == 0)
                    {
                        //is not a friendly eye
                        if (f == false || eyeOwner != b.ActivePlayer)
                        {
                            b[i, j] = b.ActivePlayer;
                            //is not a suicide
                            if (b.IsMultipleSuicide(m) == false || b.IsConsuming(m) == true)
                                result.Add(m);
                            b[i, j] = 0;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            return result;
        }

        public int PlaySimulation(Board startingBoard)
        {
            Board board = new Board();
            board.CopyState(startingBoard);
            while (board.TurnNumber < GameParameters.GameDepth && board.IsGameOver() == false && board.Passes < 2)
            {
                List<Move> availableMoves = GetAvailableMoves(board);
                availableMoves.Shuffle(r);
                bool f = false;
                Move m = new Move(-1,-1);
                foreach (Move move in availableMoves)
                {
                    if (board.PlaceStone(move) == true)
                    {
                        m = move;
                        f = true;
                        break;
                    }
                }
//                Console.WriteLine("Turn {0}", board.TurnNumber);
                if (f == false)
                {
//                    Console.WriteLine(board.ActivePlayer == 1 ? "Black passed" : "White passed");
                    board.Pass();
                }
                else
                {
//                    Console.WriteLine(m);
                }
//                Console.WriteLine(board);
//                Console.WriteLine("Black prisoners == {0}", board.BlackCaptured);
//                Console.WriteLine("White prisoners == {0}", board.WhiteCaptured);
//                Console.ReadLine();
            }
            double white, black;
            int winner = board.DetermineWinner(out black, out white);
//            switch (winner)
//            {
//                case 1:
//                    Console.WriteLine("Black won by {0}", black-white);
//                    break;
//                case 2:
//                    Console.WriteLine("White won by {0}", white-black);
//                    break;
//            }
            return winner;
        }

        double GetWinrate(Move move, Board startingBoard)
        {
            int simulations = GameParameters.Simulations;
            int player = startingBoard.ActivePlayer;
            Board b = new Board();
            b.CopyState(startingBoard);
            if (b.PlaceStone(move) == false)
                return 0;
            int sim = 0;
            int wins = 0;
            while (sim < simulations)
            {
                sim++;
                int winner = PlaySimulation(b);
                if (winner == player)
                    wins++;
            }
            return (double)wins/sim;
        }

        
        public Move GetMove(Board board)
        {
            Board b = new Board();
            b.CopyState(board);
            DateTime start = DateTime.Now;
            List<Move> availableMoves = GetAvailableMoves(b);
            double[] winrates = new double[availableMoves.Count];
            Move[] moves = new Move[availableMoves.Count];
            for (int i = 0; i < availableMoves.Count; i++)
            {
                moves[i] = new Move(availableMoves[i]);
                winrates[i] = GetWinrate(moves[i], b);
            }
            double maxWin = -1;
            int maxWinIndex = -1;
            for (int i = 0; i < winrates.Length; i++)
            {
                if (winrates[i] > maxWin)
                {
                    maxWin = winrates[i];
                    maxWinIndex = i;
                }
            }
            DateTime end = DateTime.Now;
            TimeSpan ts = end - start;
            Console.WriteLine("Found a turn in {0}", ts);
            return moves[maxWinIndex];
        }
    }
}
