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
        public Move Position { get; set; } // position of move
        public int Wins { get; private set; }
        public int Visits { get; private set; }
        public bool IsSolved { get; set; }
        public double SolvedScore { get; set; }
        public int SolvedWinner { get; set; }
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
            if (IsSolved)
                return -1;
            return Visits > 0 ? Winrate + UCTK*Math.Sqrt(Math.Log(Parent.Visits)/Visits) : 11111;
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
            BoardState = boardState.Clone();
            Parent = parent;
            Children = null;
            Position = new Move(m);
            Wins = 0;
            Visits = 0;
        }

        public void CreateChildren()
        {
            lock (this)
            {
                int size = Board.Size;
                Board b = BoardState;
                if (Children != null)
                    return;
                if (Parent == null || Parent.Children == null)
                {
                    Children = new List<UCTNode>(size*size);
                }
                else
                {
                    Children = new List<UCTNode>(Parent.Children.Count);
                }
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        //is on empty space on the board
                        if (b[i, j] == 0 && b.IsEye(i, j) != b.ActivePlayer)
                        {
                            Board anotherCloneBoard = b.Clone();
                            Move m = new Move(i, j);
                            if (anotherCloneBoard.PlaceStone(m) == true)
                                Children.Add(new UCTNode(this, m, anotherCloneBoard));
                        }
                    }
                }
                Children.Shuffle();
            }
        }

        public void Update(int wins)
        {
            if (wins < 0)
                throw new ArgumentOutOfRangeException("wins");
            Visits++;
            Wins += wins;
        }

        public UInt64 MeasureTree()
        {
            UInt64 result = 1;
            if (Children == null) return result;
            foreach (UCTNode child in Children)
            {
                result += child.MeasureTree();
            }
            return result;
        }

        public UInt64 CountSolvedNodes()
        {
            UInt64 result;
            result = IsSolved ? 1ul : 0ul;
            if (Children == null) return result;
            foreach (UCTNode child in Children)
            {
                result += child.CountSolvedNodes();
            }
            return result;
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}; {2}: {3}/{4} : {5:F5}", Position.row, Position.column, Winrate, Wins, Visits, GetUctValue());
        }
    }
}
