

using System;
using System.Runtime.InteropServices;

namespace dotNetGo
{
    public class Program
    {
//        public static TimeSpan PlaceStoneSpan = TimeSpan.Zero;
//        public static TimeSpan IsConsumingSpan = TimeSpan.Zero;
//        public static TimeSpan RemoveDragonSpan = TimeSpan.Zero;
//        public static TimeSpan IsMultipleSuicideTimeSpan = TimeSpan.Zero;
//        public static TimeSpan GetLibertiesSpan = TimeSpan.Zero;
//        public static TimeSpan GetDragonLibertiesSpan = TimeSpan.Zero;
//        public static TimeSpan GetDragonSpan = TimeSpan.Zero;
//        public static TimeSpan IsGemeOverSpan = TimeSpan.Zero;
//        public static TimeSpan IsEyeSpan = TimeSpan.Zero;
//        public static TimeSpan CopyStateSpan = TimeSpan.Zero;
//        public static TimeSpan ToStringSpan = TimeSpan.Zero;

        public static void Main(string[] args)
        {
            AIvsAI(10);
        }

        public static void AIvsAI(int gameCount)
        {
            int[] winners = new int[3];
            MonteCarlo MC = new MonteCarlo();
            winners[0] = 0;
            winners[1] = 0;

            for (int i = 0; i < gameCount; i++)
            {
                Board board = new Board();
                while (board.TurnNumber < GameParameters.GameDepth && board.IsGameOver() == false)
                {
                    DateTime start = DateTime.Now;
                    Move m = MC.GetMove(board);
                    if (m.row == -2 && m.column == -2)
                    {
                        winners[3 - board.ActivePlayer]++;
                        break;
                    }
                    board.PlaceStone(m);
                    Console.WriteLine(board);
                }
                double black, white;
                winners[board.DetermineWinner(out black, out white)]++;
                Console.WriteLine(board);
                Console.WriteLine("Turn: {0}; White captured: {1}; Black captured: {2}", board.TurnNumber, board.WhiteCaptured, board.BlackCaptured);
                Console.WriteLine("Black score: {1}; White score: {0}", black, white);
            }
        }

        public static void HumanVsAi()
        {
            Console.WriteLine("Chinese rules");

            int player;
            while (true)
            {
                Console.WriteLine("Which player do you want to be? (1 - black, 2 - white)");
                var isPlayerChosen = int.TryParse(Console.ReadLine(), out player);
                if (isPlayerChosen && (player == 1 || player == 2))
                    break;
                else
                {
                    Console.WriteLine("Incorrect player choice, try again:");
                }
            }
            MonteCarlo MC = new MonteCarlo();
            Board b = new Board();
            if (player == 1)
            {
                while (b.IsGameOver() == false)
                {
                    Console.WriteLine(b);
                    Console.WriteLine("Enter move coordinates: row column (-1 -1 for pass)");

                    int row = int.Parse(Console.ReadLine());
                    int column = int.Parse(Console.ReadLine());
                    while (b.PlaceStone(new Move(row, column)) == false)
                    {
                        Console.WriteLine("Invalid move");
                        Console.WriteLine("Enter move coordinates: row column");
                        row = int.Parse(Console.ReadLine());
                        column = int.Parse(Console.ReadLine());
                    }
                    Console.WriteLine(b);
                    b.PlaceStone(MC.GetMove(b));
                }
            }
            else if (player == 2)
            {
                while (b.IsGameOver() == false)
                {
                    b.PlaceStone(MC.GetMove(b));
                    Console.WriteLine(b);
                    Console.WriteLine("Enter move coordinates: row column (-1 -1 for pass)");
                    int row = int.Parse(Console.ReadLine());
                    int column = int.Parse(Console.ReadLine());
                    while (b.PlaceStone(new Move(row, column)) == false)
                    {
                        Console.WriteLine("Invalid move");
                        Console.WriteLine("Enter move coordinates: row column");
                        row = int.Parse(Console.ReadLine());
                        column = int.Parse(Console.ReadLine());
                    }
                    Console.WriteLine(b);
                }
            }
            else throw new Exception("can't be this player");
        }

        public static void f2()
        {
            Board b = new Board();
            MonteCarlo MC = new MonteCarlo();
            MC.PlaySimulation(b);
            Console.ReadLine();

        }
        public static void f1()
        {
            Board b = new Board();
            b.PlaceStone(new Move(2, 8));
            b.PlaceStone(new Move(3, 8));
            b.PlaceStone(new Move(2, 7));
            b.PlaceStone(new Move(3, 7));
            b.PlaceStone(new Move(2, 6));
            b.PlaceStone(new Move(4, 7));
            b.PlaceStone(new Move(3, 6));
            b.PlaceStone(new Move(5, 7));
            b.PlaceStone(new Move(4, 6));
            b.PlaceStone(new Move(5, 8));
            b.PlaceStone(new Move(5, 6));
            b.PlaceStone(new Move(1, 0));
            b.PlaceStone(new Move(6, 6));
            b.PlaceStone(new Move(2, 0));
            b.PlaceStone(new Move(6, 7));
            b.PlaceStone(new Move(3, 0));
            b.PlaceStone(new Move(6, 8));

            b.PlaceStone(new Move(4, 0));
            b.PlaceStone(new Move(4, 8));
            Console.WriteLine(b);
            int a;
            Console.WriteLine(b.IsEye(new Move(3, 8), out a));
            Console.WriteLine(b);
            Console.WriteLine(a);
        }
    }

}
