using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotNetGo
{
    public interface IPlayer
    {
        Move GetMove();
        bool ReceiveTurn(Move m);
        string Name { get; }
    }
}
