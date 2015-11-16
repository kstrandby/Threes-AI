using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Threes_console
{
    public class Minimax
    {
        private const int SIM_FREQUENCY = 500;

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
                    Console.Write(GridHelper.ToString(current.Grid));
                    Thread.Sleep(SIM_FREQUENCY);
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
                    Console.Write(GridHelper.ToString(current.Grid));
                }
            }
            return current;
        }

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

        /*
         * To take advantage of the fact that we can count cards
         */
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
