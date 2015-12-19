namespace dotNetGo
{
    class Node {
        public int Wins { get; set; }
        public int Simulations { get; set; }
        public Move Pos; // position of move

        public Node(Move m)
        {
            Pos = new Move(m);
            Wins = 0;
            Simulations = 0;
        }
    }
}