

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace dotNetGo
{
    public class Program
    {
        private const string alphabet = "abcdefghijklmnopqrstuvwxyz"; //for recording
        private static StringBuilder gameRecord = new StringBuilder();
        private static bool _isHumanChosen;

        public static void Main(string[] args)
        {
            IPlayer black = ChoosePlayer(1);
            if (black == null)
            {
                Console.WriteLine("User decided to exit");
                return;
            }
            IPlayer white = ChoosePlayer(2);
            if (white == null)
            {
                Console.WriteLine("User decided to exit");
                return;
            }

            PlayGame(black, white);
        }

        private static IPlayer ChoosePlayer(int playerNumber)
        {
            if (playerNumber != 1 && playerNumber != 2)
                throw new ArgumentOutOfRangeException("playerNumber");
            string playerColor = playerNumber == 1 ? "black" : "white";
            Console.WriteLine("Choose {0} player", playerColor);
            Console.WriteLine("1. UCT");
            Console.WriteLine("2. MonteCarlo");
            Console.WriteLine("3. MonteCarlo Random");
            if (_isHumanChosen == false)
                Console.WriteLine("4. Human");
            Console.WriteLine("Anything else: Exit program");
            Console.Write("-> ");
            string choiceString = Console.ReadLine();
            int choice;
            if (String.IsNullOrWhiteSpace(choiceString) == true || int.TryParse(choiceString, out choice) == false)
                return null;
            switch (choice)
            {
                case 1:
                    Console.WriteLine("Choose UCT simulation mode:");
                    Console.WriteLine("1. Random");
                    Console.WriteLine("2. Smart");
                    Console.WriteLine("Anything else: Exit program");
                    int simulationMode = 0;
                    if (int.TryParse(Console.ReadLine(), out simulationMode) == false || simulationMode < 1 || simulationMode > 2)
                        return null;
                    switch (simulationMode)
                    {
                        case 1:
                            return new MonteCarloUCT((byte)playerNumber, true, false);
                        case 2:
                            return new MonteCarloUCT((byte)playerNumber, false, false);
                        default:
                            return null;
                    }
                case 2:
                    return new MonteCarlo();
                case 3:
                    return new MonteCarloRandom();
                case 4:
                    if (_isHumanChosen == true)
                        return null;
                    _isHumanChosen = true;
                    return new HumanPlayer();
                default:
                    return null;
            }
        }

        public static void PlayGame(IPlayer blackPlayer, IPlayer whitePlayer)
        {
            gameRecord.AppendLine("(;FF[4]GM[1]SZ[9]AP[dotNetGo]");

            gameRecord.AppendLine(String.Format("PB[{0}]", blackPlayer.Name));
            gameRecord.AppendLine("HA[0]");
            gameRecord.AppendLine(String.Format("PW[{0}]", whitePlayer.Name));
            gameRecord.AppendLine("KM[6.5]");
            gameRecord.AppendLine("RU[Chinese]");
            gameRecord.AppendLine("");
            gameRecord.AppendLine("");
            Board board = new Board();
            while (board.IsGameOver() == false)
            {
                Move move;
                switch (board.ActivePlayer)
                {
                    case 1:
                        move = blackPlayer.GetMove();
                        break;
                    default: //case 2:
                        move = whitePlayer.GetMove();
                        break;
                }
                if (blackPlayer.ReceiveTurn(move) == false)
                    throw new ImpossibleException("somehow invalid turn made it through", "PlayGame");
                if (whitePlayer.ReceiveTurn(move) == false)
                    throw new ImpossibleException("somehow invalid turn made it through", "PlayGame");
                if (move.row >= 0 && move.column >= 0)
                    gameRecord.AppendFormat(";{0}[{1}{2}]", board.ActivePlayer == 1? "B": "W", alphabet[move.column], alphabet[move.row]);
                if (board.PlaceStone(move) == false)
                    throw new ImpossibleException("somehow invalid turn made it through", "PlayGame");
                Console.WriteLine(board);
                //Console.ReadLine();
            }
            switch (board.State)
            {
                case Board.GameState.BlackSurrendered:
                    Console.WriteLine("White won by resignation, last position:");
                    break;
                case Board.GameState.WhiteSurrendered:
                    Console.WriteLine("Black won by resignation, last position:");
                    break;
                case Board.GameState.DoublePass:
                    double blackScore, whiteScore;
                    board.DetermineWinner(out blackScore, out whiteScore);
                    gameRecord.AppendFormat(";RE[{0}+{1}]", blackScore > whiteScore?"B":"W", Math.Abs(blackScore-whiteScore));
                    Console.WriteLine(board);
                    Console.WriteLine("Turn: {0}", board.TurnNumber);
                    Console.WriteLine("Black score: {0}; White score: {1}", blackScore, whiteScore);
                    Console.WriteLine("last position:");
                    break;
            }
            Console.WriteLine(board);
            gameRecord.Append(")");
            DateTime dt = DateTime.Now;
            string filename = String.Format("{0}-{1}-{2}-{3}-{4}-{5}.sgf",
                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            File.WriteAllText(filename, gameRecord.ToString(), Encoding.UTF8);
        }
    }
}
