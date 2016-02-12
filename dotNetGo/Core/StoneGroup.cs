using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    internal class StoneGroup
    {
        private int _liberties, _stoneCount;
        public int Number;

        public int Liberties
        {
            get { return _liberties; }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");
                _liberties = value;
            }
        }

        public int StoneCount
        {
            get
            {
                return _stoneCount;
            }
            private set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");
                _stoneCount = value;
            }
        }

        public StoneGroup(int n, int stoneCount, int liberties)
        {
            Number = n;
            StoneCount = stoneCount;
            Liberties = liberties;
        }

        public void UpdateGroup(int stoneCount, int liberties)
        {
            StoneCount = stoneCount;
            Liberties = liberties;
        }
    }
}
