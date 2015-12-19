using System;

namespace dotNetGo
{
    class Move
    {
        public int row, column;

        public Move(int row, int column)
        {
            this.row = row;
            this.column = column;
        }

        public Move(Move m)
        {
            row = m.row;
            column = m.column;
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}", row, column);
        }

        public override bool Equals(object obj)
        {
            Move m2 = obj as Move;
            if (m2 == null)
                return false;
            return row == m2.row && column == m2.column;
        }

        public override int GetHashCode()
        {
            return row*GameParameters.boardSize + column;
        }
    }
}