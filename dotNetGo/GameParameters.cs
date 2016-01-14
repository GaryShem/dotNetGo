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
        public const int TurnTime = 20; //not used
        public const Int64 RandomSimulations = 10000; //simulations for random MC
        public const Int64 Simulations = 5000; //simulations for proper MC for eyes
        public const int UCTSimulations = 100;
        public const int UCTExpansion = 2; //lesser values require more memory but provide deeper search
        public const double UCTK = 0.5; //0.44 = sqrt(1/5) - lesser values focus more on successful moves
    }
}
