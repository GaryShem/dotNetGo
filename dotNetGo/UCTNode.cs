using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    class UCTNode
    {
        public static double UCTK = GameParameters.UCTK;
        public Board BoardState { get; set; }
        public UCTNode Parent { get; set; }
        public List<UCTNode> Children { get; set; }
        public Move Position; // position of move
        public int Wins { get; private set; }
        public int Visits { get; private set; }
        public bool IsSolved { get; set; }
        public int SolvedWinner { get; set; }
        public int TurnNumber { get; set; }
        public override bool Equals(object obj)
        {
            UCTNode un = obj as UCTNode;
            if (un == null)
                return false;
            return Position.row == un.Position.row && Position.column == un.Position.column;
        }

        public override int GetHashCode()
        {
            return Position.row*GameParameters.BoardSize + Position.column;
        }

        public double GetUctValue()
        {
            return Winrate + UCTK*Math.Sqrt(Math.Log(Parent.Visits)/Visits);
        }

        public double Winrate
        {
            get
            {
                if (Visits > 0)
                    return (double) Wins/Visits;
                return -1;
            }
        }

        public UCTNode(UCTNode parent, Move m, Board boardState)
        {
            if (m == null || boardState == null)
                throw new ArgumentNullException("m");
            BoardState = boardState.Clone() as Board;
            if (parent != null)
                parent.Children.Add(this);
            Parent = parent;
            Children = null;
            Position = new Move(m);
            Wins = 0;
            Visits = 0;
        }

        public void Update(int wins)
        {
            if (wins < 0)
                throw new ArgumentOutOfRangeException("wins");
            Visits++;
            Wins += wins;
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}; {2}: {3}/{4}", Position.row, Position.column, Winrate, Wins, Visits);
        }
    }
}
