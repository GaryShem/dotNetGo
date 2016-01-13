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
        public const Int64 StupidSimulations = 10000; //simulations for random MC
        public const Int64 Simulations = 5000; //simulations for proper MC for eyes
        public const int UCTSimulations = 100000;
        public const int UCTExpansion = 2; //lesser values require more memory but provide better search
        public const Int64 MaxSimulations = 10000;
        public const double GrowthFactor = 0;
        public const double UCTK = 0.44; //0.44 = sqrt(1/5) - lesser values focus more on successful moves
    }
}
