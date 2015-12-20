using System;

namespace dotNetGo
{
    class Node {
//        public int Wins { get; set; }
//        public int Simulations { get; set; }
        public Move Pos; // position of move
        public double Winrate { get; set; }

        public Node(Move m)
        {
            Pos = new Move(m);
            Winrate = 0;
//            Wins = 0;
//            Simulations = 0;
        }

        public override string ToString()
        {
            return String.Format("{0}; {1}", Pos, Winrate);
        }
    }
}