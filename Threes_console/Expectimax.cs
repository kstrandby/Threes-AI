using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threes_console
{
    // Class to hold expectimax definitions
    public class Expectimax
    {
        private const int SIM_FREQUENCY = 500;

        private GameEngine game;
        private int definedDepth;
        private State current;
        private Deck deck;

        public Expectimax(GameEngine game, int depth)
        {
            this.game = game;
            this.current = new State(game.currentState.Grid, GameEngine.PLAYER);
            this.deck = new Deck();
            this.definedDepth = depth;
        }

        // Runs an entire game using classic expectimax search to decide on moves
        public State Run(bool print)
        {
            bool gameOver = false;

            // update deck memory initially with observed cards on board
            this.UpdateDeckMemory(0, true);
            current = new State(game.currentState.Grid, GameEngine.PLAYER);
            int nextCard = game.nextCard;

            while (!gameOver)
            {
                // print mode
                if (print)
                {
                    Program.CleanConsole();
                    Console.WriteLine(BoardHelper.ToString(current.Grid));
                }

                PlayerMove move = ((PlayerMove)ExpectimaxAlgorithm(current, definedDepth, deck.Clone()));
                if (move.Direction != (DIRECTION)(-1))
                {
                    gameOver = game.SendUserAction(move);
                    current = new State(game.currentState.Grid, GameEngine.PLAYER);

                    // update deck memory if next card is not a bonus card
                    if (nextCard > 0) this.UpdateDeckMemory(nextCard, false);
                    nextCard = game.nextCard;
                }
                else // game over
                {
                    gameOver = true;
                    Program.CleanConsole();
                    Console.Write(BoardHelper.ToString(current.Grid));
                }
            }
            return current;
        }

        // Runs an entire game using a parallel iterative deepening version of expectimax
        public State RunParallelIterativeDeepeningExpectimax(bool print, int timeLimit)
        {
            bool gameOver = false;

            // update deck memory initially with observed cards on board
            this.UpdateDeckMemory(0, true);
            current = new State(game.currentState.Grid, GameEngine.PLAYER);
            int nextCard = game.nextCard;

            while (!gameOver)
            {
                // print mode
                if (print)
                {
                    Program.CleanConsole();
                    Console.WriteLine(BoardHelper.ToString(current.Grid));
                }

                PlayerMove move = ((PlayerMove)ParallelIterativeDeepeningExpectimax(current, timeLimit, deck.Clone()));
                if (move.Direction != (DIRECTION)(-1))
                {
                    gameOver = game.SendUserAction(move);
                    current = new State(game.currentState.Grid, GameEngine.PLAYER);

                    // update deck memory if next card is not a bonus card
                    if (nextCard > 0) this.UpdateDeckMemory(nextCard, false);
                    nextCard = game.nextCard;
                }
                else // game over
                {
                    gameOver = true;
                    Program.CleanConsole();
                    Console.Write(BoardHelper.ToString(current.Grid));
                }
            }
            return current;
        }


        // Runs an entire game using a parallel version of Expectimax
        public State RunParallelExpectimax(bool print)
        {
            bool gameOver = false;

            // update deck memory initially with observed cards on board
            this.UpdateDeckMemory(0, true);
            current = new State(game.currentState.Grid, GameEngine.PLAYER);
            int nextCard = game.nextCard;

            while (!gameOver)
            {
                // print mode
                if (print)
                {
                    Program.CleanConsole();
                    Console.WriteLine(BoardHelper.ToString(current.Grid));
                }

                PlayerMove move = ((PlayerMove)ParallelExpectimax(current, definedDepth, deck.Clone()));
                if (move.Direction != (DIRECTION)(-1))
                {
                    gameOver = game.SendUserAction(move);
                    current = new State(game.currentState.Grid, GameEngine.PLAYER);

                    // update deck memory if next card is not a bonus card
                    if (nextCard > 0) this.UpdateDeckMemory(nextCard, false);
                    nextCard = game.nextCard;
                }
                else // game over
                {
                    gameOver = true;
                    Program.CleanConsole();
                    Console.Write(BoardHelper.ToString(current.Grid));
                }
            }
            return current;
        }

        // Parallel iterative deepening Expectimax
        private Move ParallelIterativeDeepeningExpectimax(State state, int timeLimit, Deck deck)
        {
            Move bestMove = new PlayerMove();

            List<Move> moves = state.GetMoves(deck);
            ConcurrentBag<Tuple<double, Move>> scores = new ConcurrentBag<Tuple<double, Move>>();

            if (moves.Count == 0)
            {
                // game over
                return bestMove;
            }

            // create the resulting states before starting the threads
            List<State> resultingStates = new List<State>();
            foreach (Move move in moves)
            {
                State resultingState = state.ApplyMove(move);
                resultingStates.Add(resultingState);
            }

            Parallel.ForEach(resultingStates, resultingState =>
            {
                double score = IterativeDeepening(resultingState, timeLimit, deck.Clone()).Score;
                scores.Add(new Tuple<double, Move>(score, resultingState.GeneratingMove));
            });
            // find the best score
            double highestScore = Double.MinValue;
            foreach (Tuple<double, Move> score in scores)
            {
                PlayerMove move = (PlayerMove)score.Item2;
                if (score.Item1 > highestScore)
                {
                    highestScore = score.Item1;
                    bestMove = score.Item2;
                }
            }
            return bestMove;
        }

        
        // Parallel Expectimax
        private Move ParallelExpectimax(State state, int depth, Deck deck)
        {
            Move bestMove = new PlayerMove();

            List<Move> moves = state.GetMoves(deck);
            ConcurrentBag<Tuple<double, Move>> scores = new ConcurrentBag<Tuple<double, Move>>();

            if (moves.Count == 0)
            {
                // game over
                return bestMove;
            }

            // create the resulting states before starting the threads
            List<State> resultingStates = new List<State>();
            foreach (Move move in moves)
            {
                State resultingState = state.ApplyMove(move);
                resultingStates.Add(resultingState);
            }

            // start a thread for each child
            Parallel.ForEach(resultingStates, resultingState =>
            {
                double score = ExpectimaxAlgorithm(resultingState, depth - 1,deck.Clone()).Score;
                scores.Add(new Tuple<double, Move>(score, resultingState.GeneratingMove));
            });
            // find the best score
            double highestScore = Double.MinValue;
            foreach (Tuple<double, Move> score in scores)
            {
                PlayerMove move = (PlayerMove)score.Item2;
                if (score.Item1 > highestScore)
                {
                    highestScore = score.Item1;
                    bestMove = score.Item2;
                }
            }
            return bestMove;

        }

        // Runs an entire game using an iterative deepening version of expectimax
        public State RunIterativeDeepening(bool print, int timeLimit)
        {
            bool gameOver = false;

            // update deck memory initially with observed cards on board
            this.UpdateDeckMemory(0, true);
            current = new State(game.currentState.Grid, GameEngine.PLAYER);
            int nextCard = game.nextCard;

            while (!gameOver)
            {
                if (print)
                {
                    Program.CleanConsole();
                    Console.Write(BoardHelper.ToString(current.Grid));
                }

                PlayerMove action = ((PlayerMove)IterativeDeepening(current, timeLimit, deck.Clone()));
                if (action.Direction != (DIRECTION)(-1))
                {
                    gameOver = game.SendUserAction(action);
                    current = new State(game.currentState.Grid, GameEngine.PLAYER);
                    if (nextCard > 0) this.UpdateDeckMemory(nextCard, false);
                    nextCard = game.nextCard;
                }
                else
                {
                    gameOver = true;
                    Program.CleanConsole();
                    Console.Write(BoardHelper.ToString(current.Grid));
                }
            }
            return current;
        }

        // Iterative deepening expectimax
        private Move IterativeDeepening(State state, int timeLimit, Deck deck)
        {
            int depth = 1;
            Stopwatch timer = new Stopwatch();
            Move bestMove = null;
            // start the search
            timer.Start();
            while (true) 
            {
                if (timeLimit < timer.ElapsedMilliseconds)
                {
                    if (bestMove == null)
                    {
                        timeLimit += 10;
                        timer.Restart();
                    }
                    else
                    {
                        return bestMove;
                    }
                }
                Tuple<Move, Boolean> result = RecursiveIterativeDeepening(state, depth, timeLimit, timer, deck);
                if (result.Item2) bestMove = result.Item1; // only update bestMove if full recursion
                depth++;
            }
        }

        // Recursive part of iterative deepening
        private Tuple<Move, bool> RecursiveIterativeDeepening(State state, int depth, int timeLimit, Stopwatch timer, Deck deck)
        {
            Move bestMove;
            if (depth == 0 || state.IsGameOver())
            {
                if (state.Player == GameEngine.PLAYER)
                {
                    bestMove = new PlayerMove();
                    bestMove.Score = AI.Evaluate(state);
                    return new Tuple<Move, Boolean>(bestMove, true);
                }
                else if (state.Player == GameEngine.COMPUTER)
                {
                    bestMove = new ComputerMove();
                    bestMove.Score = AI.Evaluate(state);
                    return new Tuple<Move, Boolean>(bestMove, true);
                }
                else throw new Exception();
            }
            else
            {
                if (state.Player == GameEngine.PLAYER)
                {
                    bestMove = new PlayerMove();
                    double highestScore = Double.MinValue, currentScore = Double.MinValue;

                    List<Move> moves = state.GetAllMoves();

                    foreach (Move move in moves)
                    {
                        State resultingState = state.ApplyMove(move);
                        currentScore = RecursiveIterativeDeepening(resultingState, depth - 1, timeLimit, timer, deck).Item1.Score;
                        if (currentScore > highestScore)
                        {
                            highestScore = currentScore;
                            bestMove = (PlayerMove)move;
                        }
                        if (timer.ElapsedMilliseconds > timeLimit)
                        {
                            bestMove.Score = highestScore;
                            return new Tuple<Move, Boolean>(bestMove, false); // recursion not completed, return false
                        }
                    }

                    bestMove.Score = highestScore;
                    return new Tuple<Move, Boolean>(bestMove, true);
                }
                else if (state.Player == GameEngine.COMPUTER)
                {
                    bestMove = new ComputerMove();

                    List<Move> moves = null;
                    if (depth == definedDepth - 1)
                    {
                        int nextCard = game.nextCard;
                        moves = state.GetAllComputerMoves(nextCard);
                    }
                    else
                    {
                        if (deck.IsEmpty()) deck = new Deck();
                        moves = state.GetAllComputerMoves(deck);
                    }

                    double average = 0;
                    int moveCheckedSoFar = 0;
                    foreach (Move move in moves)
                    {
                        deck.Remove(((ComputerMove)move).Card);
                        State resultingState = state.ApplyMove(move);
                        average += RecursiveIterativeDeepening(resultingState, depth - 1, timeLimit, timer, deck).Item1.Score;
                        deck.Add(((ComputerMove)move).Card);
                        moveCheckedSoFar++;
                        if (timer.ElapsedMilliseconds > timeLimit)
                        {
                            bestMove.Score = average / moveCheckedSoFar;
                            return new Tuple<Move, Boolean>(bestMove, false); // recursion not completed, return false
                        }
                    }
                    bestMove.Score = average / moves.Count;
                    return new Tuple<Move, Boolean>(bestMove, true);
                }
                else throw new Exception();
            }
        }

        // Classic expectimax
        private Move ExpectimaxAlgorithm(State state, int depth, Deck deck)
        {
            Move bestMove;
            if (depth == 0 || state.IsGameOver())
            {
                if (state.Player == GameEngine.PLAYER)
                {
                    bestMove = new PlayerMove();
                    bestMove.Score = AI.Evaluate(state);
                    return bestMove;
                }
                else if (state.Player == GameEngine.COMPUTER)
                {
                    bestMove = new ComputerMove();
                    bestMove.Score = AI.Evaluate(state);
                    return bestMove;
                }
                else throw new Exception();
            }
            else
            {
                if (state.Player == GameEngine.PLAYER)
                {
                    bestMove = new PlayerMove();
                    double highestScore = Double.MinValue, currentScore = Double.MinValue;

                    List<Move> moves = state.GetAllMoves();

                    foreach (Move move in moves)
                    {
                        State resultingState = state.ApplyMove(move);
                        currentScore = ExpectimaxAlgorithm(resultingState, depth - 1, deck).Score;
                        if (currentScore > highestScore)
                        {
                            highestScore = currentScore;
                            bestMove = (PlayerMove)move;
                        }
                    }

                    bestMove.Score = highestScore;
                    return bestMove;
                }
                else if (state.Player == GameEngine.COMPUTER)
                {
                    bestMove = new ComputerMove();

                    List<Move> moves = null;
                    if (depth == definedDepth - 1)
                    {
                        int nextCard = game.nextCard;
                        moves = state.GetAllComputerMoves(nextCard);
                    }
                    else
                    {
                        if (deck.IsEmpty()) deck = new Deck();
                        moves = state.GetAllComputerMoves(deck);
                    }

                    double average = 0;

                    foreach (Move move in moves)
                    {
                        deck.Remove(((ComputerMove)move).Card);
                        State resultingState = state.ApplyMove(move);
                        average += ExpectimaxAlgorithm(resultingState, depth - 1, deck).Score;
                        deck.Add(((ComputerMove)move).Card);
                    }
                    bestMove.Score = average / moves.Count;
                    return bestMove;
                }
                else throw new Exception();
            }
        }

        
         // To take advantage of the fact that we can count cards
        private void UpdateDeckMemory(int card, bool firstMove)
        {
            if (firstMove)
            {
                for (int i = 0; i < GameEngine.COLUMNS; i++)
                {
                    for (int j = 0; j < GameEngine.ROWS; j++)
                    {
                        if (current.Grid[i][j] != 0)
                        {
                            deck.Remove(current.Grid[i][j]);
                        }
                    }
                }
            }
            else
            {
                deck.Remove(card); // remove the observed card from our deck memory
                if (deck.IsEmpty())
                {
                    deck = new Deck();
                }
            }
        }
    }
}
