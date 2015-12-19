using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace dotNetGo
{
    class Board
    {
        //for printing purposes only
        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //board size
        private static readonly int _size = GameParameters.boardSize;
        //cardinal directions
        public readonly static int[,] Directions = { { -1, 0 }, { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 1 },  { 1, 1 },  { 1, -1 }, { -1, -1 } };
        //turn number - needed for simulations
        public int TurnNumber { get; private set; }
        //prisoner count
        public int BlackCaptured { get; private set; }
        public int WhiteCaptured { get; private set; }
        public int ActivePlayer { get; private set; }
//        //needed for ko checks
//        private Board lastPosition;
//        private Board preLastPosition;

        int[,] board = new int[_size, _size];

        public int this[int i, int j]
        {
            get { return board[i, j]; }
            set { board[i, j] = value; }
        }

        public Board()
        {
            BlackCaptured = 0;
            WhiteCaptured = 0;
            ActivePlayer = 1;
            TurnNumber = 1;
        }
        public void CopyState(Board b)
        {
            BlackCaptured = b.BlackCaptured;
            WhiteCaptured = b.WhiteCaptured;
            TurnNumber = b.TurnNumber;
            ActivePlayer = b.ActivePlayer;
            for (int i = 0; i < _size; i++)
                for (int j = 0; j < _size; j++)
                    board[i, j] = b[i, j];
        }

        public bool PlaceStone(Move m)
        {
            //check if the move is on the board
            if (IsOnBoard(m) == false)
                return false;
            //check if the intersection is unoccupied
            if (board[m.row, m.column] != 0)
                return false;
            //TODO: check liberties for all neighbours - ENEMIES FIRST
            for (int i = 0; i < 4; i++)
            {
                
            }
            for (int i = 0; i < 4; i++)
            {
                
            }
            //TODO: add ko checks
            

            throw new NotImplementedException();
        }

        //counts liberties of a single stone at coordinates of m
        //to be used in combination with GetDragonLiberties only
        //return values:
        //null if the intersection is empty or outside the board
        //list of the liberty spaces if the - empty list if there are no liberties
        List<Move> GetLiberties(Move m)
        {
            if (IsOnBoard(m) == false || IsFree(m))
                return null;

            List<Move> liberties = new List<Move>();
            int owner = board[m.row, m.column];
            Move test = new Move(m);
            for (int i = 0; i < 4; i++)
            {
                test.row = m.row + Directions[i, 0];
                test.column = m.column + Directions[i, 1];
                if (IsOnBoard(test) && IsFree(test))
                {
                    liberties.Add(new Move(test));
                }
            }
            return liberties;
        }

        List<Move> GetDragonLiberties(List<Move> dragon)
        {
            throw new NotImplementedException();
        }
        //дракон - множество камней, непосредственно примыкающих друг к другу (не по диагонали)
        List<Move> GetDragon(Move m)
        {
            if (IsOnBoard(m) == false || IsFree(m)) 
                return null;

            List<Move> result = new List<Move>();
            int owner = board[m.row, m.column];
            int checkedStones = 0;
            Move test = new Move(-1, -1);
            while (checkedStones < result.Count)
            {
                Move currentStone = result[checkedStones];
                for (int i = 0; i < 4; i++)
                {
                    test.row = currentStone.row + Directions[i, 0];
                    test.column = currentStone.column + Directions[i, 1];
                    if (IsOnBoard(test) && board[test.row, test.column] == owner)
                    {
                        bool isInDragon = result.Contains(test);
                        if (isInDragon == false)
                        {
                            result.Add(new Move(test));
                        }
                    }
                }
            }
            return result;
        }
        //то же самое, но с указанием игрока, которому принадлежит дракон
        //вернёт пустой список, если клетка пуста или дракон принадлежит не тому игроку
        //по сути - просто дополнительная проверка принадлежности
        List<Move> GetOwnedDragon(Move m, int owner)
        {

            if (IsOnBoard(m) == false || board[m.row, m.column] != owner)
                return null;
            return GetDragon(m);
        }

        public bool IsFree(Move m)
        {
            return IsOnBoard(m) && board[m.row, m.column] == 0;
        }

        public bool IsOnBoard(Move m)
        {
            return m.row >= 0 & m.row < _size && m.column >= 0 && m.column < _size;
        }

        public int DetermineWinner()
        {
            if (IsGameOver() == false)
                return 0;
            throw new NotImplementedException();
        }

        //checks whether the game is over
        //returns true if there all empty spaces are 1-space eyes
        //returns false when there are potential moves left
        public bool IsGameOver()
        {
            List<Move> availableMoves = new List<Move>(_size * _size);
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    if (board[i, j] == 0)
                        availableMoves.Add(new Move(i, j));
                }
            }
            foreach (Move move in availableMoves)
            {
                Move m = new Move(0, 0);
                int eyeOwner;
                for (int i = 0; i < Directions.GetLength(0); i++)
                {
                    m.row = move.row + Directions[i, 0];
                    m.column = move.column + Directions[i, 1];
                    if (IsEye(move, out eyeOwner) == false)
                        return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("  ");
            for (int i = 0; i < _size; i++)
                sb.Append(alphabet[i]);
            sb.AppendLine();
            for (int i = 0; i < _size; i++)
            {
                sb.Append(String.Format("{0:D2}", i));
                for (int j = 0; j < _size; j++)
                {
                    switch (board[i, j])
                    {
                        case 1:
                            sb.Append("b");
                            break;
                        case 2:
                            sb.Append("w");
                            break;
                        default:
                            sb.Append(".");
                            break;
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        //return values:
        //0 - not an eye
        //1 - black eye
        //2 - white eye
        bool IsEye(Move move, out int owner) //TODO: possibly fix false eyes
        {
            owner = 0;
            if (IsOnBoard(move) == false)
            {
                return false;
            }
            Move m = new Move(0, 0);
            int black = 0;
            int white = 0;
            for (int i = 0; i < Directions.GetLength(0); i++)
            {
                m.row = move.row + Directions[i, 0];
                m.column = move.row + Directions[i, 1];
                if (IsOnBoard(m) == false)
                {
                    black++;
                    white++;
                }
                else
                {
                    switch (board[m.row, m.column])
                    {
                        case 1:
                            black++;
                            break;
                        case 2:
                            white++;
                            break;
                    }
                }
            }
            bool isSomeonesEye = false;
            if (black >= 7)
            {
                isSomeonesEye = true;
                owner = 1;
            }
            else if (white >= 7)
            {
                isSomeonesEye = true;
                owner = 2;
            }
            return isSomeonesEye;
        }

        public bool Compare(Board b)
        {
            if (b == null)
                return false;
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    if (board[i, j] != b.board[i, j])
                        return false;
                }
            }
            return true;
        }
    }
}