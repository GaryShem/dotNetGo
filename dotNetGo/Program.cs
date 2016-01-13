

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace dotNetGo
{
    public class Program
    {
        public static void Main(string[] args)
        {
//            UCTvsUCT(1);
//            AINvsAIN(1);
//            AINvsAIS(1);
            UCTvsAIN(1);
        }

        public static void AINvsAIN(int gameCount = 1)
        {
            int[] winners = new int[3];
            MonteCarlo black = new MonteCarlo();
            MonteCarlo white = new MonteCarlo();
            winners[0] = 0;
            winners[1] = 0;

            for (int i = 0; i < gameCount; i++)
            {
                Board board = new Board();
                while (board.IsGameOver() == false)
                {
//                    if (board.TurnNumber > 2)
//                        return;
                    Move move;
                    switch (board.ActivePlayer)
                    {
                        case 1:
                            move = black.GetMove();
                            break;
                        default: //case 2:
                            move = white.GetMove();
                            break;
                    }
                    black.ReceiveTurn(move);
                    white.ReceiveTurn(move);
                    board.PlaceStone(move);
                    Console.WriteLine(board);
                    Console.ReadLine();
                }
                switch (board.State)
                {
                    case Board.GameState.BlackSurrendered:
                        Console.WriteLine("White won by resignation, last position:");
                        break;
                    case Board.GameState.WhiteSurrendered:
                        Console.WriteLine("Black won by resignation, last position:");
                        break;
                    case Board.GameState.DoublePass:
                        double blackScore, whiteScore;
                        winners[board.DetermineWinner(out blackScore, out whiteScore)]++;
                        Console.WriteLine(board);
                        Console.WriteLine("Turn: {0}", board.TurnNumber);
                        Console.WriteLine("Black score: {1}; White score: {0}", blackScore, whiteScore);
                        Console.WriteLine("last position:");
                        break;
                }
                Console.WriteLine(board);
            }
        }

        public static void AINvsAIS(int gameCount = 1)
        {
            int[] winners = new int[3];
            MonteCarlo black = new MonteCarlo();
            MonteCarloStupid white = new MonteCarloStupid();
            winners[0] = 0;
            winners[1] = 0;

            for (int i = 0; i < gameCount; i++)
            {
                Board board = new Board();
                while (board.IsGameOver() == false)
                {
                    //                    if (board.TurnNumber > 2)
                    //                        return;
                    Move move;
                    switch (board.ActivePlayer)
                    {
                        case 1:
                            move = black.GetMove();
                            break;
                        default: //case 2:
                            move = white.GetMove();
                            break;
                    }
                    black.ReceiveTurn(move);
                    white.ReceiveTurn(move);
                    board.PlaceStone(move);
                    Console.WriteLine(board);
                    //Console.ReadLine();
                }
                switch (board.State)
                {
                    case Board.GameState.BlackSurrendered:
                        Console.WriteLine("White won by resignation, last position:");
                        break;
                    case Board.GameState.WhiteSurrendered:
                        Console.WriteLine("Black won by resignation, last position:");
                        break;
                    case Board.GameState.DoublePass:
                        double blackScore, whiteScore;
                        winners[board.DetermineWinner(out blackScore, out whiteScore)]++;
                        Console.WriteLine(board);
                        Console.WriteLine("Turn: {0}", board.TurnNumber);
                        Console.WriteLine("Black score: {1}; White score: {0}", blackScore, whiteScore);
                        Console.WriteLine("last position:");
                        break;
                }
                Console.WriteLine(board);
            }
        }

        public static void UCTvsUCT(int gameCount = 1)
        {
            int[] winners = new int[3];
            MonteCarloUCT black = new MonteCarloUCT(1);
            MonteCarloUCT white = new MonteCarloUCT(2);
            winners[0] = 0;
            winners[1] = 0;

            for (int i = 0; i < gameCount; i++)
            {
                Board board = new Board();
                while (board.IsGameOver() == false)
                {
//                    if (board.TurnNumber > 2)
//                        return;
                    Move move;
                    switch (board.ActivePlayer)
                    {
                        case 1:
                            move = black.GetMove();
                            break;
                        default: //case 2:
                            move = white.GetMove();
                            break;
                    }
                    black.ReceiveTurn(move);
                    white.ReceiveTurn(move);
                    board.PlaceStone(move);
                    Console.WriteLine(board);
//                    Console.ReadLine();
                }
                switch (board.State)
                {
                    case Board.GameState.BlackSurrendered:
                        Console.WriteLine("White won by resignation, last position:");
                        break;
                    case Board.GameState.WhiteSurrendered:
                        Console.WriteLine("Black won by resignation, last position:");
                        break;
                    case Board.GameState.DoublePass:
                        double blackScore, whiteScore;
                        winners[board.DetermineWinner(out blackScore, out whiteScore)]++;
                        Console.WriteLine(board);
                        Console.WriteLine("Turn: {0}", board.TurnNumber);
                        Console.WriteLine("Black score: {1}; White score: {0}", blackScore, whiteScore);
                        Console.WriteLine("last position:");
                        break;
                }
                Console.WriteLine(board);
            }
        }

        public static void UCTvsAIN(int gameCount = 1)
        {
            int[] winners = new int[3];
            MonteCarloUCT black = new MonteCarloUCT(1);
            MonteCarlo white = new MonteCarlo();
            winners[0] = 0;
            winners[1] = 0;

            for (int i = 0; i < gameCount; i++)
            {
                Board board = new Board();
                while (board.IsGameOver() == false)
                {
//                    if (board.TurnNumber > 2)
//                        return;
                    Move move;
                    switch (board.ActivePlayer)
                    {
                        case 1:
                            move = black.GetMove();
                            break;
                        default: //case 2:
                            move = white.GetMove();
                            break;
                    }
                    black.ReceiveTurn(move);
                    white.ReceiveTurn(move);
                    board.PlaceStone(move);
                    Console.WriteLine(board);
//                    Console.ReadLine();
                }
                switch (board.State)
                {
                    case Board.GameState.BlackSurrendered:
                        Console.WriteLine("White won by resignation, last position:");
                        break;
                    case Board.GameState.WhiteSurrendered:
                        Console.WriteLine("Black won by resignation, last position:");
                        break;
                    case Board.GameState.DoublePass:
                        double blackScore, whiteScore;
                        winners[board.DetermineWinner(out blackScore, out whiteScore)]++;
                        Console.WriteLine(board);
                        Console.WriteLine("Turn: {0}", board.TurnNumber);
                        Console.WriteLine("Black score: {1}; White score: {0}", blackScore, whiteScore);
                        Console.WriteLine("last position:");
                        break;
                }
                Console.WriteLine(board);
            }
        }
    }

}
