using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    static class GameParameters
    {
        public const int BoardSize = 9;
        public const double Komi = 6.5;
        public const int GameDepth = 500; //maximum turns simulated
        public const int TurnTime = 10; //not used
        public const int UCTSimulations = 50000; //25k+ for proper play
        public const int UCTExpansion = 2; //lesser values require more memory but provide deeper search
        public const double UCTK = 0.44; //0.44 = sqrt(1/5) - lesser values focus more on successful moves
    }
}
