using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    static class GameParameters
    {
        public const int boardSize = 9;
        public const double komi = 6.5;
        public const int GameDepth = 500;
        public const int TurnTime = 120;
        public const UInt64 Simulations = 200;
        public const UInt64 MaxSimulations = 10000;
        public const double growthFactor = 1.3;
    }
}
