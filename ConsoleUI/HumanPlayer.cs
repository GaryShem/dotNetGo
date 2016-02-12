using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    class HumanPlayer : IPlayer
    {
        Board b = new Board();

        public Move GetMove()
        {
            Board cloneBoard = b.Clone();
            while (true)
            {
                Console.WriteLine("Enter row and column for move coordinates, split by space");
                string moveCoordsString = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(moveCoordsString))
                    continue;
                string[] coords = moveCoordsString.Split(' ');
                if (coords.Length != 2)
                    continue;
                int row;
                int column;
                if (int.TryParse(coords[0], out row))
                {
                    if (int.TryParse(coords[1], out column))
                    {
                        Move result = new Move(row, column);
                        if (cloneBoard.PlaceStone(result))
                            return result;
                    }
                }
            }
        }

        public bool ReceiveTurn(Move m)
        {
            return b.PlaceStone(m);
        }

        public string Name
        {
            get { return "Human"; }
        }
    }
}
