namespace dotNetGo
{
    class Node {
        public int Wins { get; set; }
        public int Visits { get; set; }
        public Move Pos; // position of move
        //public Node parent; //optional
        public Node Child { get; set; }
        public Node Parent { get; set; }

        public Node(Node parent, int row, int column)
        {
            Parent = parent;
            Pos.row = row;
            Pos.column = column;
        }
        public void Update(int val) {
            Visits++;
            Wins+=val;
        }
        public double GetWinRate() {
            if (Visits>0) return (double)Wins / Visits;
            else return 0; /* should not happen */
        }
    }
}