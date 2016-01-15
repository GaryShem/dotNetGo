using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    class buffer
    {
//        public bool ReceiveTurn(Move m)
//        {
//            if (Root != null && Root.Children != null)
//            {
//                foreach (UCTNode child in Root.Children)
//                {
//                    if (child.Position.Equals(m))
//                    {
//                        Console.WriteLine("UCTTurbo-{0} had {1} nodes, lost {2} nodes and now has {3} nodes", _player == 1 ? "Black" : "White", Root.MeasureTree(), Root.MeasureTree() - child.MeasureTree(), child.MeasureTree());
//                        Root = child;
//                        Root.Parent.Children = null;
//                        return true;
//                    }
//                }
//            }
//            Board newBoard = Root.BoardState.Clone();
//            if (newBoard.PlaceStone(m) == false)
//                throw new ArgumentException("invalid turn");
//            Console.WriteLine("UCTTurbo-{0} had {1} nodes, lost {1} nodes and now has {2} nodes", _player == 1 ? "Black" : "White", Root.MeasureTree(), 1);
//            if (Root != null)
//                Root.Children = null; //break the link for garbage collection
//            UCTNode newRoot = new UCTNode(Root, new Move(m), newBoard);
//            Root = newRoot;
//            return true;
//        }
//        public Move GetMove()
//        {
//            DateTime start = DateTime.Now;
//            int doneSims;
//            if (Root == null)
//            {
//                Root = new UCTNode(null, new Move(-5, -5), new Board());
//            }
//            Console.WriteLine("Starting Tree size == {0}", Root != null ? Root.MeasureTree() : 0);
//            for (doneSims = 0; doneSims < _sims; doneSims++)
//            {
//                if (Root.IsSolved == true)
//                {
//                    break;
//                }
//                PlaySimulation(Root);
//            }
//            UCTNode n = GetBestChild(Root);
//            Move bestMove;
//            if (n == null)
//                bestMove = new Move(-1, -1);
//            else bestMove = new Move(n.Position);
//            TimeSpan ts = DateTime.Now - start;
//            Root.Children.Sort((_x, _y) => _x.Visits.CompareTo(_y.Visits));
//            foreach (UCTNode child in Root.Children)
//            {
//                Console.WriteLine(child);
//            }
//            Root.Children.Shuffle();
//            Console.WriteLine("UCTTurbo-{1} has found move {2}({3},{4}) in {0} after {5} sims", ts, Root.BoardState.ActivePlayer == 1 ? "Black" : "White", Root.BoardState.TurnNumber, bestMove.row, bestMove.column, doneSims);
//            Console.WriteLine("Current tree size == {0}, and there are {1} solved nodes", Root.MeasureTree(), Root.CountSolvedNodes());
//            return bestMove;
//        }
    }
}
