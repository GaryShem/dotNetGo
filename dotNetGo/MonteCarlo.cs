using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    //using the naive approach - no complex heuristics
    //engine uses New Zealand rules - same as Chinese except suicide is allowed
    internal class MonteCarlo
    {
        private int size = GameParameters.boardSize;
        private Board board = new Board();
        private List<Move> _availableMoves;
        
        List<Move> GetAvailableMoves()
        {
            List<Move> result = new List<Move>(size*size);
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    if (board[i, j] == 0)
                        result.Add(new Move(i, j));
                }
            return result;
        }

        int PlaySimulation(Board b)
        {
            
            return 0;
        }

        public Move GetMove(Board b)
        {
            board.CopyState(b);
            _availableMoves = GetAvailableMoves();
            foreach (Move move in _availableMoves)
            {
                
            }
            throw new NotImplementedException();
        }
    }
}
