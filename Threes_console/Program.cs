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
        static void Main(string[] args)
        {
            Console.SetCursorPosition(0, 0);
            ShowMenu();       
        }

        private static void ShowMenu()
        {
            Console.WriteLine("Please choose what you want to do: ");
            Console.WriteLine("0: Play Threes\n1: Let AI Minimax play Threes");
            int choice = Convert.ToInt32(Console.ReadLine());
            if (choice == 0)
            {
                StartGame();
            }
            else if (choice == 1)
            {
                RunMinimax();
            }
        }

        private static void RunMinimax()
        {
            int choice = SimulationOrGraphicRun();
            if (choice == 1) // graphic run
            {
                Console.WriteLine("Depth?");
                int depth = Convert.ToInt32(Console.ReadLine());
                GameEngine game = new GameEngine();
                Minimax minimax = new Minimax(game, depth);
                minimax.Run(true);
            }
            else if (choice == 2) // multiple simulations
            {
                Console.WriteLine("Number of runs?");
                int runs = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Depth?");
                int depth = Convert.ToInt32(Console.ReadLine());


                StreamWriter writer = new StreamWriter(@"C:\Users\Kristine\Documents\Visual Studio 2013\Projects\Threes_console\Minimax_PointsHeuristic.txt", true);
                int num192 = 0;
                int num384 = 0;
                int num768 = 0;
                int num1536 = 0;
                int num3072 = 0;
                int num6144 = 0;

                for (int i = 0; i < runs; i++)
                {
                    Console.Write(i + ": ");
                    GameEngine game = new GameEngine();
                    Minimax minimax = new Minimax(game, depth);

                    var watch = Stopwatch.StartNew();
                    State endState = minimax.Run(false);

                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Console.WriteLine("Execution time: " + elapsedMs + " ms");

                    int highestTile = GridHelper.GetHighestCard(endState.Grid);
                    int score = game.CalculateFinalScore();
                    writer.WriteLine("{0,0}{1,10}{2,15}{3,12}{4,15}", i, depth, highestTile, score, elapsedMs);
                    if (highestTile >= 192) num192++;
                    if (highestTile >= 384) num384++;
                    if (highestTile >= 768) num768++;
                    if (highestTile >= 1536) num1536++;
                    if (highestTile >= 3072) num3072++;
                    if (highestTile >= 6144) num6144++;
                }
                writer.Close();
                Console.WriteLine("192: " + (double)num192 / runs * 100 
                    + "%, 384: " + (double)num384 / runs * 100 
                    + "%, 768: " + (double)num768 / runs * 100 
                    + "%, 1536: " + (double)num1536 / runs * 100
                    + "%, 3072: " + (double)num3072 / runs * 100
                    + "%, 6144: " + (double)num6144 / runs * 100 
                    + "%");

            }
            
            Console.ReadLine(); // to avoid console closing immediately
        }

        private static int SimulationOrGraphicRun()
        {
            Console.WriteLine("1: Graphic run\n2: Simulate multiple runs");
            return Convert.ToInt32(Console.ReadLine());
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
                game.deck.PrintCardsLeft();
                Console.WriteLine(GridHelper.ToString(game.currentState.Grid));


                DIRECTION action = GetUserInput();
                PlayerMove move = new PlayerMove(action);
                gameOver = game.SendUserAction(move);
            }
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
