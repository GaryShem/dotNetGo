using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    class Node
    {
        public static double UCTK = GameParameters.UCTK;
        public Board BoardState { get; set; }
        public Node Parent { get; set; }
        public List<Node> Children { get; set; }
        public Move Position { get; set; } // position of move
        public int Wins { get; private set; }
        public int Visits { get; private set; }
        public bool IsSolved { get; set; }
        public double SolvedScore { get; set; }
        public int SolvedWinner { get; set; }
        public bool HasChildren { get; set; }
        public override bool Equals(object obj)
        {
            Node un = obj as Node;
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

        public Node(Node parent, Move m, Board boardState)
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
                    Children = new List<Node>(size*size);
                }
                else
                {
                    Children = new List<Node>(Parent.Children.Count);
                }
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        //is on empty space on the board
                        if (b[i, j] == 0 && b.CheckEye(i, j) != b.ActivePlayer)
                        {
                            Board anotherCloneBoard = b.Clone();
                            Move m = new Move(i, j);
                            if (anotherCloneBoard.PlaceStone(m) == true)
                                Children.Add(new Node(this, m, anotherCloneBoard));
                        }
                    }
                }
                Children.Shuffle();
                HasChildren = true;
            }
        }

        public void Update(int wins)
        {
            if (wins < 0)
                throw new ArgumentOutOfRangeException("wins");
            Visits++;
            Wins += wins;
        }

        public int MeasureTree()
        {
            int result = 1;
            if (Children == null) return result;
            foreach (Node child in Children)
            {
                result += child.MeasureTree();
            }
            return result;
        }

        public int CountSolvedNodes()
        {
            int result = IsSolved ? 1 : 0;
            if (Children == null) return result;
            foreach (Node child in Children)
            {
                result += child.CountSolvedNodes();
            }
            return result;
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}; {2}: {3}/{4} : {5:F5}", Position.row, Position.column, Winrate, Wins, Visits, GetUctValue());
        }

        public void Clear()
        {
            Children = null;
            Parent = null;
            Wins = 0;
            Visits = 0;
        }
    }
}
