﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Threes_console
{
    public class Minimax
    {
        private GameEngine game;
        private int definedDepth;
        private State current;
        private Deck deck;

        private bool countCards;

        public Minimax(GameEngine game, int depth)
        {
            this.game = game;
            this.definedDepth = depth;
            this.current = new State(game.currentState.Grid, GameEngine.PLAYER);
            this.deck = new Deck();
            this.countCards = false;
        }

        public void CountCards(bool b)
        {
            this.countCards = b;
        }

        internal State Run(bool print)
        {
            bool gameOver = false;
            while (!gameOver)
            {
                if (print)
                {
                    Program.CleanConsole();
                    Console.Write(GridHelper.ToString(current.Grid));
                    Thread.Sleep(500);
                }
                current = new State(game.currentState.Grid, GameEngine.PLAYER);

                if (countCards) this.UpdateDeckMemory();

                PlayerMove action = ((PlayerMove)MinimaxAlgorithm(current, definedDepth, double.MinValue, double.MaxValue, GameEngine.PLAYER, deck.Clone()));
                if (action.Direction != (DIRECTION)(-1))
                {
                    gameOver = game.SendUserAction(action);
                }
                else
                {
                    gameOver = true;
                }
            }
            return current;
        }

        private Move MinimaxAlgorithm(State state, int depth, double alpha, double beta, int player, Deck deck)
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
            if (player == GameEngine.PLAYER)
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
                moves = state.GetAllComputerMoves(deck);
            }

            foreach (Move move in moves)
            {
                deck.Remove(((ComputerMove)move).Card);
                State resultingState = state.ApplyMove(move);
                currentScore = MinimaxAlgorithm(resultingState, depth - 1, alpha, beta, GameEngine.PLAYER, deck).Score;

                if (currentScore < lowestScore)
                {
                    lowestScore = currentScore;
                    bestMove = (ComputerMove)move;
                }
                beta = Math.Min(beta, lowestScore);
                if (beta <= alpha)
                    break;
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
                currentScore = MinimaxAlgorithm(resultingState, depth - 1, alpha, beta, GameEngine.COMPUTER, deck).Score;

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
        private void UpdateDeckMemory()
        {
            deck.Remove(((ComputerMove)current.GeneratingMove).Card); // remove the observed card from our deck memory
            if (deck.IsEmpty())
            {
                deck = new Deck();
            }
        }
    }
}