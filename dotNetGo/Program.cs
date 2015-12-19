/* same as on main page, but translated to java */
 // CHANGES:
 // * best = child with max number of visits (instead of max winrate)
 // * UCTK outside of sqrt(...) in uct formula
 // * randomresult non-global

using System;

namespace dotNetGo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
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
            Board b = new Board();
            Console.WriteLine(b);
            Console.ReadLine();
        }
    }
}

/* END: class Board */