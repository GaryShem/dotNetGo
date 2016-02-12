using Engine.Core;

namespace Engine.Interface
{
    public interface IPlayer
    {
        Move GetMove();
        bool ReceiveTurn(Move m);
        string Name { get; }
    }
}
