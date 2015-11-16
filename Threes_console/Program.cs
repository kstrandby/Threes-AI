using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Threes_console;

namespace Threes_console
{
    class Program
    {
        const String MINIMAX_LOG_FILE_NAME = @"Minimax.txt";
        const String EXPECTIMAX_LOG_FILE_NAME = @"Expectimax.txt";

        enum TYPE
        {
            MINIMAX,
            EXPECTIMAX
        }
        static void Main(string[] args)
        {
            ShowMenu();       
        }

        private static void ShowMenu()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Please choose what you want to do: ");
            Console.WriteLine("0: Play Threes\n1: Let AI Minimax play Threes\n2: Let AI Expectimax play Threes");
            
            int choice = Convert.ToInt32(Console.ReadLine());
            if (choice == 0)
            {
                StartGame();
            }
            else if (choice == 1)
            {
                RunMinimax();
            }
            else if (choice == 2)
            {
                RunExpectimax();
            }
        }

        private static void RunMinimax()
        {
            int choice = TestRunsOrGraphicRun();
            if (choice == 1) 
            {
                RunGraphicAIGame(TYPE.MINIMAX);
            }
            else if (choice == 2) // multiple simulations
            {
                RunMultipleTests(TYPE.MINIMAX);
            }
            
            Console.ReadLine(); // to avoid console closing immediately
        }

        private static void RunExpectimax()
        {
            int choice = TestRunsOrGraphicRun();
            if (choice == 1)
            {
                RunGraphicAIGame(TYPE.EXPECTIMAX);
            }
            else if (choice == 2)
            {
                RunMultipleTests(TYPE.EXPECTIMAX);
            }
            Console.ReadLine();
        }

        private static void RunGraphicAIGame(TYPE AItype)
        {
            if (AItype == TYPE.MINIMAX)
            {
                int depth = GetDepth();
                GameEngine game = new GameEngine();
                Minimax minimax = new Minimax(game, depth);
                minimax.Run(true);
            }
        }

        private static void RunMultipleTests(TYPE AItype)
        {
            if (AItype == TYPE.MINIMAX)
            {
                int runs = GetRuns();
                int depth = GetDepth();

                StreamWriter writer = new StreamWriter(MINIMAX_LOG_FILE_NAME, true);
                Dictionary<int, int> highCardCount = new Dictionary<int, int>() { { 192, 0 }, { 384, 0 }, { 768, 0 }, { 1536, 0 }, { 3072, 0 }, { 6144, 0 } };
                
                for (int i = 0; i < runs; i++)
                {
                    GameEngine game = new GameEngine();
                    Minimax minimax = new Minimax(game, depth);

                    var watch = Stopwatch.StartNew();
                    State endState = minimax.Run(false);
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;

                    int highestTile = GridHelper.GetHighestCard(endState.Grid);
                    int score = game.CalculateFinalScore();
                   
                    String stats = i + "\t" + depth + "\t" + highestTile + "\t" + score + "\t" + elapsedMs;
                    Console.WriteLine(stats);
                    writer.WriteLine(stats);

                    List<int> keys = new List<int>(highCardCount.Keys);
                    for (int j = 0; j < keys.Count; j++) 
                    {

                        if (highestTile >= keys[j]) highCardCount[keys[j]]++;
                    }
                }
                writer.Close();
                Console.WriteLine(GetStatistics(highCardCount, runs));
            }

            else if (AItype == TYPE.EXPECTIMAX)
            {
                int runs = GetRuns();
                int depth = GetDepth();

                StreamWriter writer = new StreamWriter(EXPECTIMAX_LOG_FILE_NAME, true);
                Dictionary<int, int> highCardCount = new Dictionary<int, int>() { { 192, 0 }, { 384, 0 }, { 768, 0 }, { 1536, 0 }, { 3072, 0 }, { 6144, 0 } };

                for (int i = 0; i < runs; i++)
                {
                    GameEngine game = new GameEngine();
                    Expectimax expectimax = new Expectimax(game, depth);

                    var watch = Stopwatch.StartNew();
                    State endState = expectimax.Run(false);
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;

                    int highestTile = GridHelper.GetHighestCard(endState.Grid);
                    int score = game.CalculateFinalScore();

                    String stats = i + "\t" + depth + "\t" + highestTile + "\t" + score + "\t" + elapsedMs;
                    Console.WriteLine(stats);
                    writer.WriteLine(stats);

                    List<int> keys = new List<int>(highCardCount.Keys);
                    for (int j = 0; j < keys.Count; j++)
                    {

                        if (highestTile >= keys[j]) highCardCount[keys[j]]++;
                    }
                }
                writer.Close();
                Console.WriteLine(GetStatistics(highCardCount, runs));
            }
        }

        private static String GetStatistics(Dictionary<int, int> highCardCount, int runs)
        {
            return "192: " + (double)highCardCount[192] / runs * 100
                    + "%, 384: " + (double)highCardCount[384] / runs * 100
                    + "%, 768: " + (double)highCardCount[768] / runs * 100
                    + "%, 1536: " + (double)highCardCount[1536] / runs * 100
                    + "%, 3072: " + (double)highCardCount[3072] / runs * 100
                    + "%, 6144: " + (double)highCardCount[6144] / runs * 100
                    + "%";
        }

        private static int TestRunsOrGraphicRun()
        {
            Console.WriteLine("1: Graphic run\n2: Test runs");
            return Convert.ToInt32(Console.ReadLine());
        }

        private static int GetRuns()
        {
            Console.WriteLine("Number of runs?");
            int runs = Convert.ToInt32(Console.ReadLine());
            return runs;
        }

        private static int GetDepth()
        {
            Console.WriteLine("Depth?");
            int depth = Convert.ToInt32(Console.ReadLine());
            return depth;
        }

        private static void StartGame()
        {
            GameEngine game = new GameEngine();
            bool gameOver = false;
            CleanConsole();

            // main game loop
            while (!gameOver)
            {
                CleanConsole();
                String nextCard = "";
                if (game.nextCard == -1) nextCard = "BONUS CARD";
                else nextCard = game.nextCard.ToString();
                Console.WriteLine("Next card: " + nextCard);
                Console.WriteLine(GridHelper.ToString(game.currentState.Grid));


                DIRECTION action = GetUserInput();
                PlayerMove move = new PlayerMove(action);
                gameOver = game.SendUserAction(move);
            }
            CleanConsole();
            
            Console.WriteLine("Next card: ");
            Console.WriteLine(GridHelper.ToString(game.currentState.Grid));
            Console.WriteLine("GAME OVER! Final score: " + game.currentState.CalculateFinalScore());
            Console.ReadLine(); // to avoid console closing immediately
        }

        private static DIRECTION GetUserInput()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                return DIRECTION.UP;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                return DIRECTION.DOWN;
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                return DIRECTION.LEFT;
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                return DIRECTION.RIGHT;
            }
            else return (DIRECTION)(-1);
        }


        public static void CleanConsole()
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("                                                                                                                                                                                                                                                                                                   ");
            }
            Console.SetCursorPosition(0, 0);

        }
    }
}
