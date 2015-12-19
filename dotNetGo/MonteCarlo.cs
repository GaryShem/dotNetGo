using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            List<Move> availableMoves;
            while (board.TurnNumber < GameParameters.GameDepth && board.IsGameOver() == false && board.Passes < 2)
            {
                availableMoves = GetAvailableMoves(board);
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
                Console.WriteLine("Turn {0}", board.TurnNumber);
                if (f == true)
                    Console.WriteLine(m);
                else
                {
                    Console.WriteLine(board.ActivePlayer == 1 ? "Black passed" : "White passed");
                    board.Pass();
                }
                Console.WriteLine(board);
                Console.WriteLine("Black prisoners == {0}", board.BlackCaptured);
                Console.WriteLine("White prisoners == {0}", board.WhiteCaptured);
//                Console.ReadLine();
            }
            double white, black;
            int winner = board.DetermineWinner(out black, out white);
            switch (winner)
            {
                case 1:
                    Console.WriteLine("Black won by {0}", black-white);
                    break;
                case 2:
                    Console.WriteLine("White won by {0}", white-black);
                    break;
            }
            return 0;
        }


//        
//        public Move GetMove(Board b)
//        {
//            
//        }
    }
}
