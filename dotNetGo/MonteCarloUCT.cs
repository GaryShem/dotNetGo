using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dotNetGo
{
    internal class MonteCarloUCT
    {

        private const int Size = GameParameters.BoardSize;
        public UCTNode Root;
        private byte _player;
        private int _sims;
        [ThreadStatic] private Board boardClone;

        public MonteCarloUCT(byte player, double uctk = GameParameters.UCTK, int sims = GameParameters.UCTSimulations)
        {
            if (sims <= 0)
                throw new ArgumentOutOfRangeException("sims");
            this._sims = sims;
            Root = new UCTNode(null, new Move(-5, -5), new Board());
            UCTK = uctk;
            if (player != 1 && player != 2)
                throw new ArgumentOutOfRangeException("player");
            _player = player;
        }

        public UCTNode GetBestChild(UCTNode root)
        {
            UCTNode bestChild = null;
            int bestVisits = -1;

            foreach (UCTNode child in Root.Children)
            {
                if (child.Visits > bestVisits)
                {
                    bestChild = child;
                    bestVisits = child.Visits;
                }
            }
            return bestChild;
        }

        public double UCTK; // theoretically should be 0.44 = sqrt(1/5)
        // Larger values give uniform search
        // Smaller values give very selective search
        public UCTNode UCTSelect(UCTNode node)
        {
            UCTNode result = null;
            double best_uct = 0;
            foreach (UCTNode child in node.Children)
            {
                double uctvalue;
                if (child.Visits > 0)
                {
                    uctvalue = child.GetUctValue();
                }
                else
                {
                    uctvalue = 11000;
                }
                if (uctvalue > best_uct)
                {
                    best_uct = uctvalue;
                    result = child;
                }
            }
            return result;
        }

        private int PlayRandomGame(UCTNode node)
        {
            if (boardClone == null)
                boardClone = (Board)node.BoardState.Clone();
            else
            {
                boardClone.CopyStateFrom(node.BoardState);
            }
            int turnsSimulated = 0;
            while (turnsSimulated < GameParameters.GameDepth && boardClone.IsGameOver() == false)
            {
                turnsSimulated++;
                Move m = new Move(-5, -5);
                do
                {
                    m.row = RandomGen.Next(-1, GameParameters.BoardSize);
                    m.column = RandomGen.Next(-1, GameParameters.BoardSize);
                } while (boardClone.PlaceStone(m) == false);
            }
            int winner = boardClone.DetermineWinner();
            return winner;
        }

        public bool ReceiveTurn(Move m)
        {
            if (Root.Children == null)
                CreateChildren(Root);
            foreach (UCTNode child in Root.Children)
            {
                if (child.Position.Equals(m))
                {
                    Root = child;
                    Root.Parent.Children = null;
                    return true;
                }
            }
            Board newBoard = (Board)Root.BoardState.Clone();
            if (newBoard.PlaceStone(m) == false)
                throw new ArgumentException("invalid turn");
            {
                UCTNode newRoot = new UCTNode(Root, new Move(m), newBoard);
                Root.Children = null; //break the link for garbage collection
                Root = newRoot;
                return true;
            } 
        }

        private void CreateChildren(UCTNode node)
        {
            Board b = node.BoardState;
            if (node.Parent == null || node.Parent.Children == null)
            {
                node.Children = new List<UCTNode>(Size*Size);
            }
            else
            {
                node.Children = new List<UCTNode>(node.Parent.Children.Count);
            }
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    //is on empty space on the board
                    if (b[i, j] == 0 && b.IsEye(i, j) != b.ActivePlayer)
                    {
                        Board anotherCloneBoard = (Board)b.Clone();
                        Move m = new Move(i, j);
                        if (anotherCloneBoard.PlaceStone(m) == true)
                            new UCTNode(node, m, anotherCloneBoard);
                    }
                }
            }
            if (node.BoardState.TurnNumber < 5)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    if (node.Children[i].Position.row <= 1 || node.Children[i].Position.row >= 7 || node.Children[i].Position.column <= 1 ||
                        node.Children[i].Position.column >= 7)
                    {
                        node.Children.RemoveAt(i--);
                    }
                }
            }
            else if (node.BoardState.TurnNumber < 10)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    if (node.Children[i].Position.row < 1 | node.Children[i].Position.row > 7 || node.Children[i].Position.column < 1 ||
                        node.Children[i].Position.column > 7)
                    {
                        node.Children.RemoveAt(i--);
                    }
                }
            }
            node.Children.Shuffle();
        }

        private int PlaySimulation(UCTNode n)
        {
            int randomWinner = 0;
            if (n.IsSolved == true)
            {
                randomWinner = n.SolvedWinner;
                if (randomWinner == _player)
                    n.Update(1);
                else
                    n.Update(0);
                return n.SolvedWinner;
            }
            if (n.Children == null && n.Visits < GameParameters.UCTExpansion && n.IsSolved == false)
            {
                randomWinner = PlayRandomGame(n);
            }
            else
            {
                if (n.Children == null)
                    CreateChildren(n);
                UCTNode next = UCTSelect(n); // select a move
                if (next == null) //only happens in finished positions - we can count winrate again
                {
                    n.IsSolved = true;
                }
                else
                {
                    randomWinner = PlaySimulation(next);
                }
            }
            randomWinner = randomWinner == _player ? 1 : 0;
            n.Update(randomWinner); //update node (Node-wins are associated with moves in the Nodes)
            return randomWinner;
        }

        // generate a move, using the uct algorithm
        public Move GetMove()
        {
            DateTime start = DateTime.Now;
            if (Root.Children == null)
                CreateChildren(Root);
            for (int i = 0; i < _sims; i++)
            {
                PlaySimulation(Root);
            }
            UCTNode n = GetBestChild(Root);
            Move bestMove;
            if (n == null)
                bestMove = new Move(-1, -1);
            else bestMove = new Move(n.Position);
            TimeSpan ts = DateTime.Now - start;
            Console.WriteLine("UCTTurbo-{1} has found move {2}({3},{4}) in {0} after {5} sims", ts, Root.BoardState.ActivePlayer == 1 ? "Black" : "White", Root.BoardState.TurnNumber, bestMove.row, bestMove.column, GameParameters.UCTSimulations);
            return bestMove;
        }
    }
}
