using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threes_console
{
    // Class to hold Minimax definitions
    public class Minimax
    {
        private GameEngine game;
        private int definedDepth;
        private State current;
        private Deck deck;

        public Minimax(GameEngine game, int depth)
        {
            this.game = game;
            this.definedDepth = depth;
            this.current = new State(game.currentState.Grid, GameEngine.PLAYER);
            this.deck = new Deck();
        }

        // Runs an entire game using classic Minimax to decide on moves
        internal State Run(bool print)
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

                PlayerMove action = ((PlayerMove)MinimaxAlgorithm(current, definedDepth, double.MinValue, double.MaxValue, deck.Clone()));
                if (action.Direction != (DIRECTION)(-1))
                {
                    gameOver = game.SendUserAction(action);
                    current = new State(game.currentState.Grid, GameEngine.PLAYER);
                    if(nextCard > 0) this.UpdateDeckMemory(nextCard, false);
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

        // Runs an entire game using parallelized iterative deepening minimax
        public State RunParallelIterativeDeepening(bool print, int timeLimit)
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

                PlayerMove action = ((PlayerMove)ParallelIterativeDeepening(current, timeLimit, deck.Clone()));
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

        // Runs an entire game using a parallelized version of iterative deepening minimax
        private Move ParallelIterativeDeepening(State state, int timeLimit, Deck deck)
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

        // Runs an entire game using an iterative deepening of minimax with alpha-beta pruning
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

        // Iterative deepening minimax
        private Move IterativeDeepening(State state, int timeLimit, Deck deck)
        {
            int depth = 1;
            Stopwatch timer = new Stopwatch();
            Move bestMove = null;
            // start the search
            timer.Start();
            while (timer.ElapsedMilliseconds < timeLimit)
            {
                Tuple<Move, Boolean> result = RecursiveIterativeDeepening(state, depth, Double.MinValue, Double.MaxValue, timeLimit, timer, deck);
                if (result.Item2) bestMove = result.Item1; // only update bestMove if full recursion
                depth++;
            }
            return bestMove;
        }

        // Recursive part of iterative deepening
        private Tuple<Move, bool> RecursiveIterativeDeepening(State state, int depth, double alpha, double beta, int timeLimit, Stopwatch timer, Deck deck)
        {
            Move bestMove;
            if (depth == 0 || state.IsGameOver())
            {
                if (state.Player == GameEngine.PLAYER)
                {
                    bestMove = new PlayerMove(); // default constructor creates dummy action
                    bestMove.Score = AI.Evaluate(state);
                    return new Tuple<Move, Boolean>(bestMove, true);
                }
                else if (state.Player == GameEngine.COMPUTER)
                {
                    bestMove = new ComputerMove(); // default constructor creates dummy action
                    bestMove.Score = AI.Evaluate(state);
                    return new Tuple<Move, Boolean>(bestMove, true);
                }
                else
                {
                    throw new Exception();
                }
            }
            if (state.Player == GameEngine.PLAYER){

                bestMove = new PlayerMove();
                double highestScore = Double.MinValue, currentScore = Double.MinValue;

                List<Move> moves = state.GetAllMoves();

                foreach (Move move in moves)
                {
                    State resultingState = state.ApplyMove(move);
                    currentScore = RecursiveIterativeDeepening(resultingState, depth - 1, alpha, beta, timeLimit, timer, deck).Item1.Score;
                    if (currentScore > highestScore)
                    {
                        highestScore = currentScore;
                        bestMove = (PlayerMove)move;
                    }
                    alpha = Math.Max(alpha, highestScore);
                    if (beta <= alpha)
                        break;
                    if (timer.ElapsedMilliseconds > timeLimit)
                    {
                        bestMove.Score = highestScore;
                        return new Tuple<Move, Boolean>(bestMove, false); // recursion not completed, return false
                    }
                }

                bestMove.Score = highestScore;
                return new Tuple<Move, Boolean>(bestMove, true);
            }
            else
            {
                bestMove = new ComputerMove();
                double lowestScore = Double.MaxValue, currentScore = Double.MaxValue;

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

                foreach (Move move in moves)
                {
                    deck.Remove(((ComputerMove)move).Card);
                    State resultingState = state.ApplyMove(move);
                    currentScore = RecursiveIterativeDeepening(resultingState, depth - 1, alpha, beta, timeLimit, timer, deck).Item1.Score;
                    if (currentScore < lowestScore)
                    {
                        lowestScore = currentScore;
                        bestMove = (ComputerMove)move;
                    }
                    beta = Math.Min(beta, lowestScore);
                    if (beta <= alpha)
                        break;
                    deck.Add(((ComputerMove)move).Card);
                    if (timer.ElapsedMilliseconds > timeLimit)
                    {
                        bestMove.Score = lowestScore;
                        return new Tuple<Move, Boolean>(bestMove, false); // recursion not completed, return false
                    }
                }
                bestMove.Score = lowestScore;
                return new Tuple<Move, Boolean>(bestMove, true);
            }
        }

        // Classic Minimax using alpha-beta pruning
        private Move MinimaxAlgorithm(State state, int depth, double alpha, double beta, Deck deck)
        {
            Move bestMove;
            if (depth == 0 || state.IsGameOver()) 
            {
                if (state.Player == GameEngine.PLAYER)
                {                
                    bestMove = new PlayerMove(); // default constructor creates dummy action
                    bestMove.Score = AI.Evaluate(state);
                    return bestMove;
                }
                else if (state.Player == GameEngine.COMPUTER)
                {
                    bestMove = new ComputerMove(); // default constructor creates dummy action
                    bestMove.Score = AI.Evaluate(state);
                    return bestMove;
                }
                else
                {
                    throw new Exception();
                }
            }
            if (state.Player == GameEngine.PLAYER)
                return Max(state, depth, alpha, beta);
            else
                return Min(state, depth, alpha, beta);
        }

        // MIN part of Minimax
        Move Min(State state, int depth, double alpha, double beta)
        {
            ComputerMove bestMove = new ComputerMove();
            double lowestScore = Double.MaxValue, currentScore = Double.MaxValue;

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

            foreach (Move move in moves)
            {
                deck.Remove(((ComputerMove)move).Card);
                State resultingState = state.ApplyMove(move);
                currentScore = MinimaxAlgorithm(resultingState, depth - 1, alpha, beta, deck).Score;
                if (currentScore < lowestScore)
                {
                    lowestScore = currentScore;
                    bestMove = (ComputerMove)move;
                }
                beta = Math.Min(beta, lowestScore);
                if (beta <= alpha)
                    break;
                deck.Add(((ComputerMove)move).Card);
            }
            bestMove.Score = lowestScore;
            return bestMove;
        }

        // MAX part of Minimax
        Move Max(State state, int depth, double alpha, double beta)
        {
            PlayerMove bestMove = new PlayerMove();
            double highestScore = Double.MinValue, currentScore = Double.MinValue;

            List<Move> moves = state.GetAllMoves();

            foreach (Move move in moves)
            {
                State resultingState = state.ApplyMove(move);
                currentScore = MinimaxAlgorithm(resultingState, depth - 1, alpha, beta, deck).Score;
                if (currentScore > highestScore)
                {
                    highestScore = currentScore;
                    bestMove = (PlayerMove)move;
                }
                alpha = Math.Max(alpha, highestScore);
                if (beta <= alpha)
                    break;
            }
            bestMove.Score = highestScore;
            return bestMove;
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
