using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threes_console
{
    // Class holding MCTS definitions
    public class MCTS
    {
        private const int NUM_THREADS = 8;

        private GameEngine gameEngine;
        private Deck deck;
        private State currentState;
        private Random random;

        public MCTS(GameEngine gameEngine) {
            this.gameEngine = gameEngine;
            this.deck = new Deck();
            this.currentState = new State(BoardHelper.CloneGrid(this.gameEngine.currentState.Grid), GameEngine.PLAYER);
            this.random = new Random();
        }

        // Runs an entire game using MCTS limited by time
        public State RunTimeLimitedMCTS(bool print, int timeLimit)
        {
            // update deck memory initially with observed cards on board
            this.UpdateDeckMemory(0, true);
            int nextCard = this.gameEngine.nextCard;

            while (true)
            {
                currentState = new State(BoardHelper.CloneGrid(this.gameEngine.currentState.Grid), GameEngine.PLAYER);

                if (print)
                {
                    Program.CleanConsole();
                    Console.WriteLine(BoardHelper.ToString(currentState.Grid));
                }

                Node result = TimeLimitedMCTS(currentState, timeLimit, deck.Clone());
                
                if (result == null)
                {
                    // game over
                    return currentState;
                }
                gameEngine.SendUserAction((PlayerMove)result.GeneratingMove);

                // update deck memory only if the new card is not a bonus card (we know what value the new card has by keeping track of nextcard
                if (nextCard > 0) this.UpdateDeckMemory(nextCard, false);
                nextCard = this.gameEngine.nextCard;
            }
        }

        // Runs an entire game using a parallelized version of MCTS limited by time
        public State RunParallelTimeLimitedMCTS(bool print, int timeLimit)
        {
            // update deck memory initially with observed cards on board
            this.UpdateDeckMemory(0, true);
            int nextCard = this.gameEngine.nextCard;

            while (true)
            {
                currentState = new State(BoardHelper.CloneGrid(this.gameEngine.currentState.Grid), GameEngine.PLAYER);

                if (print)
                {
                    Program.CleanConsole();
                    Console.WriteLine(BoardHelper.ToString(currentState.Grid));
                }

                DIRECTION result = ParallelTimeLimitedMCTS(currentState, timeLimit, deck.Clone());
                PlayerMove move = new PlayerMove();
                move.Direction = result;
                if (result == (DIRECTION)(-1))
                {
                    // game over
                    return currentState;
                }
                gameEngine.SendUserAction(move);

                // update deck memory only if the new card is not a bonus card (we know what value the new card has by keeping track of nextcard
                if (nextCard > 0) this.UpdateDeckMemory(nextCard, false);
                nextCard = this.gameEngine.nextCard;
            }
        }

        // Parallel time limited MCTS
        private DIRECTION ParallelTimeLimitedMCTS(State currentState, int timeLimit, Deck deck)
        {
            ConcurrentBag<Node> allChildren = new ConcurrentBag<Node>();
            int numOfChildren = currentState.GetMoves(deck).Count;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            Parallel.For(0, NUM_THREADS, i =>
            {
                Node resultRoot = TimeLimited(currentState, timeLimit, timer, deck);
                foreach (Node child in resultRoot.Children)
                {
                    allChildren.Add(child);
                }
            });
            timer.Stop();

            List<int> totalVisits = new List<int>(4) { 0, 0, 0, 0 };
            List<double> totalResults = new List<double>(4) { 0, 0, 0, 0 };

            foreach (Node child in allChildren)
            {
                int direction = (int)((PlayerMove)child.GeneratingMove).Direction;
                totalVisits[direction] += child.Visits;
                totalResults[direction] += child.Results;
            }

            double best = Double.MinValue;
            int bestDirection = -1;
            for (int k = 0; k < 4; k++)
            {
                double avg = totalResults[k] / totalVisits[k];
                if (avg > best)
                {
                    best = avg;
                    bestDirection = k;
                }
            }
            if (bestDirection == -1) return (DIRECTION)(-1);
            return (DIRECTION)bestDirection;
        }

        // Starts the time limited Monte Carlo Tree Search and returns the best child node
        // resulting from the search
        public Node TimeLimitedMCTS(State rootState, int timeLimit, Deck deck)
        {
            Stopwatch timer = new Stopwatch();
            Node bestNode = null;
            while (bestNode == null && !rootState.IsGameOver())
            {
                timer.Start();
                Node rootNode = TimeLimited(rootState, timeLimit, timer, deck);
                bestNode = FindBestChild(rootNode.Children);
                timeLimit += 10;
                timer.Reset();
            }
            return bestNode;
        }

        // Time limited MCTS
        private Node TimeLimited(State rootState, int timeLimit, Stopwatch timer, Deck deck)
        {
            Node rootNode = new Node(null, null, rootState, deck);
            
            while (true)
            {
                if (timer.ElapsedMilliseconds > timeLimit)
                {
                    if (FindBestChild(rootNode.Children) == null && !rootNode.state.IsGameOver())
                    {
                        timeLimit += 10;
                        timer.Restart();
                    }
                    else
                    {
                        return rootNode;
                    }

                }
                Node node = rootNode;
                State state = rootState.Clone();
                Deck clonedDeck = deck.Clone();

                // 1: Select
                while (node.UntriedMoves.Count == 0 && node.Children.Count != 0)
                {
                    node = node.SelectChild();
                    state = state.ApplyMove(node.GeneratingMove);
                    if (node.GeneratingMove is ComputerMove)
                    {
                        clonedDeck.Remove(((ComputerMove)node.GeneratingMove).Card);
                        if (clonedDeck.IsEmpty()) clonedDeck = new Deck();
                    }
                }

                // 2: Expand
                if (node.UntriedMoves.Count != 0)
                {
                    Move randomMove = node.UntriedMoves[random.Next(0, node.UntriedMoves.Count)];
                    if (randomMove is ComputerMove)
                    {
                        if (clonedDeck.IsEmpty()) clonedDeck = new Deck();
                        clonedDeck.Remove(((ComputerMove)randomMove).Card);
                        state = state.ApplyMove(randomMove);
                        node = node.AddChild(randomMove, state, clonedDeck);
                        
                    }
                    else
                    {
                        state = state.ApplyMove(randomMove);
                        node = node.AddChild(randomMove, state, clonedDeck);
                    }
                }

                // 3: Simulation
                while (state.GetMoves(clonedDeck).Count != 0)
                {
                    Move move = state.GetRandomMove(clonedDeck);
                    if (move is ComputerMove)
                    {
                        if (clonedDeck.IsEmpty()) clonedDeck = new Deck();
                        clonedDeck.Remove(((ComputerMove)move).Card);
                        state = state.ApplyMove(move);
                    }
                    else
                    {
                        state = state.ApplyMove(move);
                    }
                }

                // 4: Backpropagation
                while (node != null)
                {
                    node.Update(state.GetResult());
                    node = node.Parent;
                }
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
                        if (currentState.Grid[i][j] != 0)
                        {
                            deck.Remove(currentState.Grid[i][j]);
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

        // Called at the end of a MCTS to decide on the best child
        // Best child is the child with the highest average score
        private Node FindBestChild(List<Node> children)
        {

            double bestResults = 0;
            Node best = null;
            foreach (Node child in children)
            {
                if (child.Results / child.Visits > bestResults)
                {
                    best = child;
                    bestResults = child.Results / child.Visits;
                }
            }
            return best;
        }
    }
}
