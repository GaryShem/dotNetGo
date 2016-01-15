using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace dotNetGo
{
    internal class MonteCarloUCT : IPlayer
    {

        private const int Size = GameParameters.BoardSize;
        public UCTNode Root;
        private byte _player { get; set; }
        private readonly int _sims;
        private int _doneSims;

        private bool _randomUCT;
        private bool _resetTreeEachTurn;
        [ThreadStatic]
        private static Board _boardClone;
        [ThreadStatic]
        private static Move[] _availableMoves;

        public MonteCarloUCT(byte player, bool randomSims, bool resetTreeEachTurn)
        {
            if (player != 1 && player != 2)
                throw new ArgumentOutOfRangeException("player");
            Root = new UCTNode(null, new Move(-5, -5), new Board());
            Root.CreateChildren();
            _player = player;
            _randomUCT = randomSims;
            _resetTreeEachTurn = resetTreeEachTurn;
            _sims = GameParameters.UCTSimulations;
        }

        public UCTNode GetBestChild(UCTNode root)
        {
            UCTNode bestChild = null;
            int bestVisits = -1;

            foreach (UCTNode child in Root.Children)
            {
                if (child.IsSolved && child.SolvedWinner == _player)
                {
                    return child;
                }
                if (child.Visits > bestVisits)
                {
                    bestChild = child;
                    bestVisits = child.Visits;
                }
            }
            return bestChild;
        }

        public const double UCTK = GameParameters.UCTSimulations; // theoretically should be 0.44 = sqrt(1/5)
        // Larger values give uniform search
        // Smaller values give very selective search
        public UCTNode UCTSelect(UCTNode node)
        {
            UCTNode result = null;
            double bestUCT = 0;
            foreach (UCTNode child in node.Children)
            {
                if (child.IsSolved == true)
                    continue;
//                double uctvalue = child.Visits > 0 ? child.GetUctValue() : 111111;
                double uctvalue = child.GetUctValue();
                if (uctvalue > bestUCT)
                {
                    bestUCT = uctvalue;
                    result = child;
                }
            }
            return result;
        }

        private int PlayRandomGame(UCTNode node)
        {
            _boardClone.CopyStateFrom(node.BoardState);
            int turnsSimulated = 0;
            while (turnsSimulated < GameParameters.GameDepth && _boardClone.IsGameOver() == false)
            {
                turnsSimulated++;
                Move m = new Move(-5, -5);
                do
                {
                    m.row = RandomGen.Next(-1, GameParameters.BoardSize);
                    m.column = RandomGen.Next(-1, GameParameters.BoardSize);
                } while (_boardClone.PlaceStone(m) == false);
            }
            int winner = _boardClone.DetermineWinner();
            return winner;
        }

        private int GetAvailableMoves(Board b)
        {
            if (_availableMoves == null)
                _availableMoves = new Move[Size * Size+1];
            int moveCount = 0;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    //is on empty space on the board and not a friendly eye
                    if (b[i, j] == 0 && b.IsEye(i, j) != b.ActivePlayer)
                    {
                        _availableMoves[moveCount++] = new Move(i, j);
                    }
                }
            }
            return moveCount;
        }
        private int PlayLessRandomGame(UCTNode node)
        {
            if (_availableMoves == null)
                _availableMoves = new Move[Size*Size+1];
            _boardClone.CopyStateFrom(node.BoardState);
            int turnsSimulated = 0;
            while (turnsSimulated < GameParameters.GameDepth && _boardClone.IsGameOver() == false)
            {
                turnsSimulated++;
                int moveCount = GetAvailableMoves(_boardClone);
                _availableMoves.Shuffle(moveCount);
                Move pass = new Move(-1, -1); //add pass to possible moves
                _availableMoves[moveCount++] = pass;
                for (int i = 0; i < moveCount; i++)
                {
                    if (_boardClone.PlaceStone(_availableMoves[i]) == true)
                    {
                        break;
                    }
                }
            }
            int winner = _boardClone.DetermineWinner();
            return winner;
        }

        public bool ReceiveTurn(Move m)
        {
            if (Root.Children != null)
            {
                foreach (UCTNode child in Root.Children)
                {
                    if (child.Position.Equals(m))
                    {
                        Console.WriteLine("UCTTurbo-{0} had {1} nodes, lost {2} nodes and now has {3} nodes", _player==1?"Black":"White", Root.MeasureTree(), Root.MeasureTree()-child.MeasureTree(), child.MeasureTree());
                        Root = child;
                        Root.Parent.Children = null;
                        child.Parent = null;
                        if (child.Children == null)
                            child.CreateChildren();
                        return true;
                    }
                }
            }
            Board newBoard = Root.BoardState.Clone();
            if (newBoard.PlaceStone(m) == false)
                throw new ArgumentException("invalid turn");
            Console.WriteLine("UCTTurbo-{0} had {1} nodes, lost {1} nodes and now has {2} nodes", _player == 1 ? "Black" : "White", Root.MeasureTree(), 1);
            Root.Children = null; //break the link for garbage collection
            UCTNode newRoot = new UCTNode(null, new Move(m), newBoard);
            newRoot.CreateChildren();
            Root = newRoot;
            return true;
        }

        public string Name
        {
            get { return "UCT"; }
        }

        private int PlaySimulation(UCTNode n)
        {
            if (_boardClone == null)
                _boardClone = new Board();
            int randomWinner = 0;
            if (n.IsSolved == true) //should always be false (only for single thread! - can be true for multiple threads)
            {
                int solvedCurrentPlayerWins = n.SolvedWinner == _player ? 1 : 0;
                n.Update(solvedCurrentPlayerWins); //update node (Node-wins are associated with moves in the Nodes)
                return n.SolvedWinner;
            }
            if (n.Children == null && n.Visits < GameParameters.UCTExpansion && n.IsSolved == false)
            {
                if (_boardClone == null)
                    _boardClone = new Board();
                randomWinner = PlayMoreOrLessRandomGame(n);
            }
            else
            {
                if (n.HasChildren == false)
                    n.CreateChildren();
                UCTNode next = UCTSelect(n); // select a move
                if (next == null) //only happens in finished positions and solved nodes - we can start backpropagating ideal result
                {
                    n.IsSolved = true;
                    if (n.Children.Count == 0) //this is a terminal position - there can be no nodes after it
                    {
                        double blackScore, whiteScore;
                        n.SolvedWinner = n.BoardState.DetermineWinner(out blackScore, out whiteScore);
                        n.SolvedScore = _player == 1 ? blackScore : whiteScore;
                    }
                    else //this is a non-terminal position for which all possible subsequent moves have been checked
                    {
                        if (n.BoardState.ActivePlayer == _player)
                            //if, for this node, it's this player's turn, then we take the best result
                        {
                            bool foundWin = false;
                            foreach (UCTNode child in n.Children)
                            {
                                if (child.IsSolved == false)
                                    throw new ImpossibleException("solved node's child is not solved", "PlaySimulation");
                                if (child.SolvedWinner == _player)
                                    //if we find a choice that leads to sure win for current player, we immediately take it
                                {
                                    foundWin = true;
                                    n.SolvedWinner = _player;
                                    n.Update(1);
                                    return 1;
                                }
                            }
                            //if we don't find a node that leads to current player's victory
                            n.SolvedWinner = 3 - _player;
                            n.Update(0);
                            return 0;
                        }
                        else //if it's enemy's turn on this node, then we take the worst result
                        {
                            foreach (UCTNode child in n.Children)
                            {
                                if (child.IsSolved == false)
                                    throw new ImpossibleException("solved node's child is not solved", "PlaySimulation");
                                if (child.SolvedWinner != _player)
                                    //if we find a choice that leads to sure win for enemy, we immediately take it
                                {
                                    n.SolvedWinner = 3 - _player;
                                    n.Update(0);
                                    return 0;
                                }
                            }
                            //if we don't find a node that leads to enemy's victory, we assume that this is our winning node
                            n.SolvedWinner = _player;
                            n.Update(1);
                            return 1;
                        }
                    }
                }
                else
                {
                    randomWinner = PlaySimulation(next);
                }
            }
            int currentPlayerWins = randomWinner == _player ? 1 : 0;
            n.Update(currentPlayerWins); //update node (Node-wins are associated with moves in the Nodes)
            return randomWinner;
        }

        private int PlayMoreOrLessRandomGame(UCTNode n)
        {
            return _randomUCT ? PlayRandomGame(n) : PlayLessRandomGame(n);
        }

        private void ParallelSimulations()
        {
            while (_doneSims < _sims)
            {
                _doneSims++;
                PlaySimulation(Root);
            }
        }

        // generate a move, using the uct algorithm
        public Move GetMove()
        {
            Move bestMove;
            DateTime start = DateTime.Now;
            if (_resetTreeEachTurn == true) //EXPERIMENTAL, HAS NOT BEEN TESTED AND MIGHT NOT WORK
                Root = new UCTNode(null, new Move(-5, -5), Root.BoardState.Clone());
            Console.WriteLine("Starting Tree size == {0}", Root.MeasureTree());
            _doneSims = 0;
            if (Root.Children.Any(_x => _x.IsSolved == true && _x.SolvedWinner == _player))
            {
                bestMove = Root.Children.First(_x => _x.IsSolved == true && _x.SolvedWinner == _player).Position;
            }
            else
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                    new Task(ParallelSimulations).Start();
                while (_doneSims < _sims)
                {
                    Thread.Sleep(100);
                }
                UCTNode n = GetBestChild(Root);
                if (n.Children.TrueForAll(_x => _x.Winrate < 0.1))
                {
                    
                }
                if (n == null)
                    bestMove = new Move(-1, -1);
                else bestMove = new Move(n.Position);
            }
            TimeSpan ts = DateTime.Now - start;
            Root.Children.Sort((_x, _y) => _x.Visits.CompareTo(_y.Visits));
            foreach (UCTNode child in Root.Children)
            {
                Console.WriteLine(child);
            }
            Root.Children.Shuffle();
            Console.WriteLine("Current tree size == {0}, and there are {1} solved nodes", Root.MeasureTree(), Root.CountSolvedNodes());
            Console.WriteLine("UCTTurbo-{1} has found move {2}({3},{4}) in {0} after {5} sims", ts, Root.BoardState.ActivePlayer == 1 ? "Black" : "White", Root.BoardState.TurnNumber, bestMove.row, bestMove.column, _doneSims);
            return bestMove;
        }
    }
}
