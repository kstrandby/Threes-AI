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
    // Main program
    public class Program
    {
        const String MINIMAX_LOG_FILE_NAME = @"MINIMAX_LOG.txt";
        const String EXPECTIMAX_LOG_FILE_NAME = @"EXPECTIMAX_LOG.txt";
        const String MCTS_LOG_FILE_NAME = @"MCTS_LOG.txt";

        // enum to hold AI type
        enum TYPE
        {
            MINIMAX,
            MINIMAX_ITERATIVE_DEEPENING,
            MINIMAX_PARALLEL_ITERATIVE_DEEPENING,
            EXPECTIMAX_CLASSIC,
            EXPECTIMAX_ITERATIVE_DEEPENING,
            EXPECTIMAX_PARALLEL,
            EXPECTIMAX_PARALLEL_ITERATIVE_DEEPENING,
            MCTS_CLASSIC,
            MCTS_PARALLEL
        }

        static void Main(string[] args)
        {
            ShowMenu();       
        }

        // Presents a menu in the console to the user, letting the user choose 
        // which AI to run (or to play the game himself)
        private static void ShowMenu()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Please choose what you want to do: ");
            Console.WriteLine("0: Play Threes\n1: Minimax\n2: Expectimax\n3: MCTS");
            
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
            else if (choice == 3)
            {
                RunMCTS();
            }
            Console.ReadLine(); // to avoid console closing immediately
        }

        // Runs games played by MCTS agent
        private static void RunMCTS()
        {
            int type = GetChoice("1: Classic MCTS\n2: Parallel MCTS");
            int choice = TestRunsOrGraphicRun();
            
            if (type == 1)
            {
                if (choice == 1)
                {
                    RunGraphicAIGame(TYPE.MCTS_CLASSIC);
                }
                else if (choice == 2)
                {
                    RunMultipleTests(TYPE.MCTS_CLASSIC);
                }
                else RunMCTS();
            }
            else if (type == 2)
            {
                if (choice == 1)
                {
                    RunGraphicAIGame(TYPE.MCTS_PARALLEL);
                }
                else if (choice == 2)
                {
                    RunMultipleTests(TYPE.MCTS_PARALLEL);
                }
                else RunMCTS();
            }
        }

        // Runs games played by Minimax agent
        private static void RunMinimax()
        {
            int type = GetChoice("1: Classic Alpha-Beta Minimax\n2: Parallel Iterative Deepening Alpha-Beta Minimax\n3: Iterative Deepening Alpha-Beta Minimax");
            int choice = TestRunsOrGraphicRun();
            if (type == 1)
            {
                if (choice == 1)
                {
                    RunGraphicAIGame(TYPE.MINIMAX);
                }
                else if (choice == 2) // multiple simulations
                {
                    RunMultipleTests(TYPE.MINIMAX);
                }
            }
            else if (type == 2)
            {
                if (choice == 1)
                {
                    RunGraphicAIGame(TYPE.MINIMAX_PARALLEL_ITERATIVE_DEEPENING);
                }
                else if (choice == 2) // multiple simulations
                {
                    RunMultipleTests(TYPE.MINIMAX_PARALLEL_ITERATIVE_DEEPENING);
                }
            }
            else if (type == 3)
            {
                if (choice == 1)
                {
                    RunGraphicAIGame(TYPE.MINIMAX_ITERATIVE_DEEPENING);
                }
                else if (choice == 2) // multiple simulations
                {
                    RunMultipleTests(TYPE.MINIMAX_ITERATIVE_DEEPENING);
                }
            }
        }

        // Runs games played by Expectimax agent
        private static void RunExpectimax()
        {
            int type = GetChoice("1: Classic Expectimax\n2: Parallel Expectimax\n3: Parallel iterative deepening Expectimax\n4: Iterative deepening Expectimax");
            if (type == 1)
            {
                int choice = TestRunsOrGraphicRun();
                if (choice == 1)
                {
                    RunGraphicAIGame(TYPE.EXPECTIMAX_CLASSIC);
                }
                else if (choice == 2)
                {
                    RunMultipleTests(TYPE.EXPECTIMAX_CLASSIC);
                }
            }
            else if (type == 2)
            {
                int choice = TestRunsOrGraphicRun();
                if (choice == 1)
                {
                    RunGraphicAIGame(TYPE.EXPECTIMAX_PARALLEL);
                }
                else if (choice == 2)
                {
                    RunMultipleTests(TYPE.EXPECTIMAX_PARALLEL);
                }
            }
            else if (type == 3)
            {
                int choice = TestRunsOrGraphicRun();
                if (choice == 1)
                {
                    RunGraphicAIGame(TYPE.EXPECTIMAX_PARALLEL_ITERATIVE_DEEPENING);
                }
                else if (choice == 2)
                {
                    RunMultipleTests(TYPE.EXPECTIMAX_PARALLEL_ITERATIVE_DEEPENING);
                }
            }
            else if (type == 4)
            {
                int choice = TestRunsOrGraphicRun();
                if (choice == 1)
                {
                    RunGraphicAIGame(TYPE.EXPECTIMAX_ITERATIVE_DEEPENING);
                }
                else if (choice == 2)
                {
                    RunMultipleTests(TYPE.EXPECTIMAX_ITERATIVE_DEEPENING);
                }
            }
            
            Console.ReadLine();
        }

        // Runs a game by given AI showing it in the console
        private static void RunGraphicAIGame(TYPE AItype)
        {
            if (AItype == TYPE.MINIMAX || AItype == TYPE.MINIMAX_PARALLEL_ITERATIVE_DEEPENING || AItype == TYPE.MINIMAX_ITERATIVE_DEEPENING)
            {
                int depth = 0;
                int timeLimit = 0;
                GameEngine game = new GameEngine();
                Minimax minimax = new Minimax(game, depth);

                if (AItype == TYPE.MINIMAX_PARALLEL_ITERATIVE_DEEPENING)
                {
                    timeLimit = GetChoice("Time limit?");
                    minimax.RunParallelIterativeDeepening(true, timeLimit);
                }
                else if (AItype == TYPE.MINIMAX_ITERATIVE_DEEPENING)
                {
                    timeLimit = GetChoice("Time limit?");
                    minimax.RunIterativeDeepening(true, timeLimit);
                }
                else
                {
                    depth = GetDepth();
                    minimax.Run(true);
                }
            }
            else if (AItype == TYPE.EXPECTIMAX_CLASSIC || AItype == TYPE.EXPECTIMAX_PARALLEL || AItype == TYPE.EXPECTIMAX_PARALLEL_ITERATIVE_DEEPENING || AItype == TYPE.EXPECTIMAX_ITERATIVE_DEEPENING)
            {
                int depth = 0;
                if (AItype != TYPE.EXPECTIMAX_PARALLEL_ITERATIVE_DEEPENING) depth = GetDepth();
                GameEngine game = new GameEngine();
                Expectimax expectimax = new Expectimax(game, depth);
                if (AItype == TYPE.EXPECTIMAX_CLASSIC)
                {
                    expectimax.Run(true);
                }
                else if (AItype == TYPE.EXPECTIMAX_PARALLEL)
                {
                    int timeLimit = GetChoice("Time limit?");
                    expectimax.RunParallelExpectimax(true);
                }
                else if (AItype == TYPE.EXPECTIMAX_PARALLEL_ITERATIVE_DEEPENING)
                {
                    int timeLimit = GetChoice("Time limit?");
                    expectimax.RunParallelIterativeDeepeningExpectimax(true, timeLimit);
                }
                else if (AItype == TYPE.EXPECTIMAX_ITERATIVE_DEEPENING)
                {
                    int timeLimit = GetChoice("Time limit?");
                    expectimax.RunIterativeDeepening(true, timeLimit);
                }
            }
            else if (AItype == TYPE.MCTS_CLASSIC || AItype == TYPE.MCTS_PARALLEL)
            {
                int timeLimit = GetChoice("Time limit?");
                GameEngine game = new GameEngine();
                MCTS mcts = new MCTS(game);
                if (AItype == TYPE.MCTS_CLASSIC) mcts.RunTimeLimitedMCTS(true, timeLimit);
                else if (AItype == TYPE.MCTS_PARALLEL) mcts.RunParallelTimeLimitedMCTS(true, timeLimit);
            }
        }

        // Runs a number of games with the given AI agents
        private static void RunMultipleTests(TYPE AItype)
        {
            if (AItype == TYPE.MINIMAX || AItype == TYPE.MINIMAX_PARALLEL_ITERATIVE_DEEPENING || AItype == TYPE.MINIMAX_ITERATIVE_DEEPENING)
            {
                int runs = GetRuns();
                int depth = 0;
                int timeLimit = 0;
                if (AItype == TYPE.MINIMAX)
                {
                    depth = GetDepth();
                }
                else
                {
                    timeLimit = GetChoice("Time limit?");
                }

                StreamWriter writer = new StreamWriter(MINIMAX_LOG_FILE_NAME, true);
                Dictionary<int, int> highCardCount = new Dictionary<int, int>() { { 192, 0 }, { 384, 0 }, { 768, 0 }, { 1536, 0 }, { 3072, 0 }, { 6144, 0 } };
                
                for (int i = 0; i < runs; i++)
                {
                    GameEngine game = new GameEngine();
                    Minimax minimax = new Minimax(game, depth);

                    var watch = Stopwatch.StartNew();
                    State endState = null;
                    if (AItype == TYPE.MINIMAX) endState = minimax.Run(false);
                    else if (AItype == TYPE.MINIMAX_PARALLEL_ITERATIVE_DEEPENING) endState = minimax.RunParallelIterativeDeepening(false, timeLimit);
                    else if (AItype == TYPE.MINIMAX_ITERATIVE_DEEPENING) endState = minimax.RunIterativeDeepening(false, timeLimit);
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;

                    int highestTile = BoardHelper.GetHighestCard(endState.Grid);
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

            else if (AItype == TYPE.EXPECTIMAX_CLASSIC || AItype == TYPE.EXPECTIMAX_PARALLEL || AItype == TYPE.EXPECTIMAX_PARALLEL_ITERATIVE_DEEPENING || AItype == TYPE.EXPECTIMAX_ITERATIVE_DEEPENING)
            {
                int runs = GetRuns();
                int depth = 0;
                int timeLimit = 0;
                if (AItype != TYPE.EXPECTIMAX_PARALLEL_ITERATIVE_DEEPENING && AItype != TYPE.EXPECTIMAX_ITERATIVE_DEEPENING)
                {
                    depth = GetDepth();
                }
                else
                {
                    timeLimit = GetChoice("Time limit?");
                }

                StreamWriter writer = new StreamWriter(EXPECTIMAX_LOG_FILE_NAME, true);
                Dictionary<int, int> highCardCount = new Dictionary<int, int>() { { 192, 0 }, { 384, 0 }, { 768, 0 }, { 1536, 0 }, { 3072, 0 }, { 6144, 0 } };

                for (int i = 0; i < runs; i++)
                {
                    GameEngine game = new GameEngine();
                    Expectimax expectimax = new Expectimax(game, depth);

                    var watch = Stopwatch.StartNew();
                    State endState = null;
                    if (AItype == TYPE.EXPECTIMAX_CLASSIC)
                    {
                        endState = expectimax.Run(false);
                    }
                    else if (AItype == TYPE.EXPECTIMAX_PARALLEL)
                    {
                        endState = expectimax.RunParallelExpectimax(false);
                    }
                    else if (AItype == TYPE.EXPECTIMAX_PARALLEL_ITERATIVE_DEEPENING)
                    {
                        endState = expectimax.RunParallelIterativeDeepeningExpectimax(false, timeLimit);
                    }
                    else if (AItype == TYPE.EXPECTIMAX_ITERATIVE_DEEPENING)
                    {
                        endState = expectimax.RunIterativeDeepening(false, timeLimit);
                    }
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;

                    int highestTile = BoardHelper.GetHighestCard(endState.Grid);
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
            else if (AItype == TYPE.MCTS_CLASSIC || AItype == TYPE.MCTS_PARALLEL)
            {
                int runs = GetRuns();
                int timeLimit = GetChoice("Time limit?");
                StreamWriter writer = new StreamWriter(MCTS_LOG_FILE_NAME, true);
                Dictionary<int, int> highCardCount = new Dictionary<int, int>() { { 192, 0 }, { 384, 0 }, { 768, 0 }, { 1536, 0 }, { 3072, 0 }, { 6144, 0 } };

                for (int i = 0; i < runs; i++)
                {
                    GameEngine game = new GameEngine();
                    MCTS mcts = new MCTS(game);

                    var watch = Stopwatch.StartNew();
                    State endState = null;
                    if (AItype == TYPE.MCTS_CLASSIC)
                    {
                        endState = mcts.RunTimeLimitedMCTS(false, timeLimit);
                    }
                    else if (AItype == TYPE.MCTS_PARALLEL)
                    {
                        endState = mcts.RunParallelTimeLimitedMCTS(false, timeLimit);
                    }

                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;

                    int highestTile = BoardHelper.GetHighestCard(endState.Grid);
                    int score = game.CalculateFinalScore();

                    String stats = i + "\t" + highestTile + "\t" + score + "\t" + elapsedMs;
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

        // Returns a string with stats
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

        // Methods asking for user input
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

        // Starts a game for the user to play
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
                Console.WriteLine(BoardHelper.ToString(game.currentState.Grid));


                DIRECTION action = GetUserInput();
                PlayerMove move = new PlayerMove(action);
                gameOver = game.SendUserAction(move);
            }
            CleanConsole();
            
            Console.WriteLine("Next card: ");
            Console.WriteLine(BoardHelper.ToString(game.currentState.Grid));
            Console.WriteLine("GAME OVER! Final score: " + game.currentState.CalculateFinalScore());
            Console.ReadLine(); // to avoid console closing immediately
        }

        // Retrieves user key info
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

        // Clear console output
        public static void CleanConsole()
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("                                                                                                                                                                                                                                                                                                   ");
            }
            Console.SetCursorPosition(0, 0);

        }

        // Get user input
        private static int GetChoice(String options)
        {
            Console.WriteLine("Please choose an option:");
            Console.WriteLine(options);
            int choice = Convert.ToInt32(Console.ReadLine());
            return choice;
        }
    }
}
