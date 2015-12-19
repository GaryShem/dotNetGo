using System;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.XPath;

namespace dotNetGo
{
    class Board
    {
        //for printing purposes only
        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //board size
        private static readonly int _size = GameParameters.boardSize;
        private static readonly double _komi = GameParameters.komi;
        //cardinal directions
        public readonly static int[,] Directions = { { -1, 0 }, { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 1 },  { 1, 1 },  { 1, -1 }, { -1, -1 } };
        //turn number - needed for simulations
        public int TurnNumber { get; private set; }
        public int Passes { get; private set; }
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

        public int this[Move m]
        {
            get { return board[m.row, m.column]; }
            set { board[m.row, m.column] = value; }
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
            Passes = b.Passes;
            ActivePlayer = b.ActivePlayer;
            for (int i = 0; i < _size; i++)
                for (int j = 0; j < _size; j++)
                    board[i, j] = b[i, j];
        }

        public bool PlaceStone(Move move)
        {
            if (move.row == -1 && move.column == -1)
            {
                Pass();
                return true;
            }
            //check if the move is on the board
            if (IsOnBoard(move) == false)
                return false;
            //check if the intersection is unoccupied
            if (board[move.row, move.column] != 0)
                return false;

            this[move] = ActivePlayer;

            //if there is an enemy dragon nearby, it won't contain newly-placed stone - have to check each one individually
            List<Move> dragon;
            for (int i = 0; i < 4; i++) //first check opponent's dragons
            {
                Move test = new Move(move.row + Directions[i,0], move.column + Directions[i,1]);
                dragon = GetOwnedDragon(test, 3-ActivePlayer);
                if (dragon.Count > 0)
                {
                    int liberties = GetDragonLiberties(dragon).Count;
                    if (liberties < 1) //if the dragon has no liberties, remove it, else do nothing
                    {
                        //we know that this is opponent's dragon - important!!!
                        int removedStones = RemoveDragon(dragon);
                    }
                }
            }
            //если рядом с новым камнем есть дракон, то он включает в себя и этот камень
            //if there is a nearby friendly dragon, it will contain newly-placed stone
            if (IsMultipleSuicide(move))
            {
                this[move] = 0;
                return false;
            }

            ActivePlayer = 3 - ActivePlayer;
            //TODO: possibly add ko checks
            Passes = 0;
            TurnNumber++;
            return true;
        }

        public bool IsConsuming(Move move)
        {
            List<Move> dragon;
            for (int i = 0; i < 4; i++) //first check opponent's dragons
            {
                Move test = new Move(move.row + Directions[i, 0], move.column + Directions[i, 1]);
                dragon = GetOwnedDragon(test, 3 - ActivePlayer);
                if (dragon.Count > 0)
                {
                    int liberties = GetDragonLiberties(dragon).Count;
                    if (liberties <= 1) //if the dragon has no liberties, remove it, else do nothing
                    {
                        //we know that this is opponent's dragon - important!!!
                        return true;
                    }
                }
            }
            return false;
        }

        int RemoveDragon(List<Move> dragon)
        {
            if (dragon.Count < 1)
                return 0;
            int owner = this[dragon[0]];
            int removedStones = 0;
            foreach (Move move in dragon)
            {
                this[move] = 0;
                removedStones++;
            }
            switch (owner)
            {
                case 1:
                    BlackCaptured+= removedStones;
                    break;
                case 2:
                    WhiteCaptured += removedStones;
                    break;
            }
            return removedStones;
        }

        bool IsSingleSuicide(Move m)
        {
            if (this[m] == 0)
                return false;
            int enemyCount = 0;
            Move test = new Move(m);
            for (int i = 0; i < 4; i++)
            {
                test.row = m.row + Directions[i, 0];
                test.column = m.column + Directions[i, 1];
                if (!IsOnBoard(test) || this[test] == 3 - ActivePlayer)
                {
                    enemyCount++;
                }
            }
            if (enemyCount == 4)
                return true;
            else return false;
        }

        public bool IsMultipleSuicide(Move move)
        {
            List<Move> dragon = GetDragon(move);
            int liberties = GetDragonLiberties(dragon).Count;
            return liberties < 1;
        }

        public void Pass()
        {
            ActivePlayer = 3 - ActivePlayer;
            TurnNumber++;
            Passes++;
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
            List<Move> result = new List<Move>();
            foreach (Move move in dragon)
            {
                foreach (Move liberty in GetLiberties(move))
                {
                    if (result.Contains(liberty) == false)
                        result.Add(liberty);
                }
            }
            return result;
        }
        //дракон - множество камней, непосредственно примыкающих друг к другу (не по диагонали)
        List<Move> GetDragon(Move m)
        {
            List<Move> result = new List<Move>();
            if (IsOnBoard(m) == false || IsFree(m)) 
                return result;
            result.Add(m);
            int owner = board[m.row, m.column];
            int checkedStones = 0;
            int totalStones = 1;
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
                            totalStones++;
                        }
                    }
                }
                checkedStones++;
            }
            return result;
        }
        //то же самое, но с указанием игрока, которому принадлежит дракон
        //вернёт пустой список, если клетка пуста или дракон принадлежит не тому игроку
        //по сути - просто дополнительная проверка принадлежности
        List<Move> GetOwnedDragon(Move m, int owner)
        {

            if (IsOnBoard(m) == false || board[m.row, m.column] != owner)
                return new List<Move>();
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

        public int DetermineWinner(out double blackScore, out double whiteScore)
        {
            whiteScore = 0;
            blackScore = 0;
            int[] scores = new int[2]{0,0};
            if (IsGameOver() == false)
                return 0;
            for (int player = 1; player < 3; player++)
            {
                for (int i = 0; i < _size; i++)
                {
                    for (int j = 0; j < _size; j++)
                    {
                        if (board[i, j] == player)
                            scores[player - 1]++;
                        else
                        {
                            int eyeOwner;
                            if (IsEye(new Move(i, j), out eyeOwner))
                            {
                                if (eyeOwner == player)
                                    scores[player - 1]++;
                            }
                        }
                    }
                }
            }
            blackScore = scores[0];
            whiteScore = scores[1] + _komi;
            if (blackScore > whiteScore)
                return 1;
            else return 2;
        }

        //checks whether the game is over
        //returns true if there all empty spaces are 1-space eyes
        //returns false when there are potential moves left
        public bool IsGameOver()
        {
            List<Move> availableMoves = new List<Move>(_size * _size);
            Move test = new Move(0,0);
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
                int eyeOwner;
                for (int i = 0; i < Directions.GetLength(0); i++)
                {
                    Move m = new Move(move.row + Directions[i, 0], move.column + Directions[i, 1]);
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
                sb.Append(i.ToString());
            sb.AppendLine();
            for (int i = 0; i < _size; i++)
            {
                sb.Append(String.Format("{0} ", i));
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
        //an intersection is an eye when all surrounding stones belong to the same dragon - also fixes false eyes
        public bool IsEye(Move move, out int owner) //false eyes fixed
        {
            owner = 0;
            if (IsOnBoard(move) == false || this[move] != 0)
            {
                return false;
            }
                List<Move> dragon = null;
            for (int i = 0; i < 4; i++)
            {
                Move m = new Move(move.row + Directions[i, 0], move.column + Directions[i, 1]);
                if (IsOnBoard(m) == false)
                {
                    continue;
                }
                if (dragon == null)
                {
                    dragon = GetDragon(m);
                    if (dragon.Count < 3) //minimum 3 stones required to surround a stone from 2 sides and stay connected
                    {
//                            XX
//                            XO
                        return false;
                    }
                }
                else
                {
                    if (dragon.Contains(m) == false)
                        return false;
                }
            }
            owner = this[dragon[0]];
            return true;
//            for (int i = 4; i < 8; i++)
//            {
//                m.row = move.row + Directions[i, 0];
//                m.column = move.column + Directions[i, 1];
//                if (IsOnBoard(m) == false)
//                {
//                    black++;
//                    white++;
//                }
//                else
//                {
//                    switch (board[m.row, m.column])
//                    {
//                        case 1:
//                            black++;
//                            break;
//                        case 2:
//                            white++;
//                            break;
//                    }
//                }
//            }
//            if (black >= 7)
//            {
//                owner = 1;
//            }
//            else if (white >= 7)
//            {
//                owner = 2;
//            }
//            return owner != 0;
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