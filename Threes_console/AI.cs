using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threes_console
{
    public static class AI
    {

        // Weight constants
        const double emptycells_weight = 5.0;
        const double mergeability_weight = 1.0;
        const double trappedpenalty_weight = 1.0;

        public static double Evaluate(State state)
        {
            if (state.IsGameOver()) return -1000;
            
            return WeightSnake(state);
        }

        

        public static double EmptyCells(State state)
        {
            double numEptyCells = 0;
            for (int i = 0; i < GameEngine.COLUMNS; i++)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    if (state.Grid[i][j] == 0)
                    {
                        numEptyCells++;
                    }
                }
            }
            return numEptyCells;
        }

        public static double TrappedPenalty(State state)
        {
            double trapped = 0;

            for (int i = 0; i < GameEngine.COLUMNS; i++)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    if (state.Grid[i][j] != 0)
                    {
                        // check neighbours in vertical direction
                        int neighbourRowAbove = j + 1;
                        int neighbourRowBelow = j - 1;
                        if ((neighbourRowAbove < GameEngine.ROWS && state.Grid[i][j] > 2 && state.Grid[i][neighbourRowAbove] > state.Grid[i][j] && j == 0) // trapped between wall below and higher card above
                            || (neighbourRowBelow >= 0 && state.Grid[i][j] > 2 && state.Grid[i][neighbourRowBelow] > state.Grid[i][j] && j == 3) // trapped between wall above and higher card below
                            || (state.Grid[i][j] > 2 && neighbourRowAbove < GameEngine.ROWS && state.Grid[i][neighbourRowAbove] > state.Grid[i][j] // trapped between two higher cards
                                && neighbourRowBelow >= 0 && state.Grid[i][neighbourRowBelow] > state.Grid[i][j])) 
                        {
                            trapped++;
                        }

                        // check neighbours in horizontal direction
                        int neighbourColumnToRight = i + 1;
                        int neighbourColumnToLeft = i - 1;
                        if ((state.Grid[i][j] > 2 && neighbourColumnToRight < GameEngine.COLUMNS && state.Grid[neighbourColumnToRight][j] > state.Grid[i][j] && i == 0) // trapped between wall to the left and higher card to the right
                            || (state.Grid[i][j] > 2 && neighbourColumnToLeft >= 0 && state.Grid[neighbourColumnToLeft][j] > state.Grid[i][j] && i == 3) // trapped between wall to the right and higher card to the left
                            || (state.Grid[i][j] > 2 && neighbourColumnToRight < GameEngine.COLUMNS && state.Grid[neighbourColumnToRight][j] > state.Grid[i][j] // trapped between two higher cards
                                && neighbourColumnToLeft >= 0 && state.Grid[neighbourColumnToLeft][j] > state.Grid[i][j])) 
                        {
                            trapped++;
                        }
                    }
                }
            }
            return trapped;
        }

        // TODO: currently doesnt take into account that a merge is not a merge unless the two cards to be merged are up against a wall or another cards
        public static double Mergeability(State state)
        {
            double numMerges = 0;

            for (int i = 0; i < GameEngine.COLUMNS; i++)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    if (i < state.Grid.Length - 1 && state.Grid[i][j] != 0)
                    {
                        int k = i + 1;
                        while (k < state.Grid.Length)
                        {
                            if (state.Grid[k][j] == 0)
                            {
                                k++;
                            }
                            else if ((state.Grid[i][j] == 1 && state.Grid[k][j] == 2) ||  (state.Grid[i][j] == 2 && state.Grid[k][j] == 1) || (state.Grid[k][j] == state.Grid[i][j]))
                            {
                                numMerges++;
                                break;
                            }
                            else
                            { // other value, no more possible merges in row
                                break;
                            }
                        }
                    }
                    if (j < state.Grid.Length - 1 && state.Grid[i][j] != 0)
                    {
                        int k = j + 1;
                        while (k < state.Grid.Length)
                        {
                            if (state.Grid[i][k] == 0)
                            {
                                k++;
                            }
                            else if ((state.Grid[i][j] == 1 && state.Grid[i][k] == 2) || (state.Grid[i][j] == 2 && state.Grid[i][k] == 1) || (state.Grid[i][k] == state.Grid[i][j]))
                            {
                                numMerges++;
                                break;
                            }
                            else
                            { // other value, no more possible merges in column
                                break;
                            }
                        }
                    }
                }
            }
            return numMerges;
        }

        // Arranges the tiles in a "snake"
        // As there are 8 different ways the tiles can be arranged in a "snake" on the grid, this method
        // finds the one that fits the best, allowing the AI to adjust to a different "snake" pattern
        public static double WeightSnake(State state)
        {
            double[][] snake1 = new double[][] {
                new double[]{20,9,4,.1},
                new double[]{19,10,3,0.2},
                new double[]{18,11,2,0.3},
                new double[]{17,12,1,0.4}
            };

            double[][] snake2 = new double[][] {
                new double[]{20,19,18,17},
                new double[]{9,10,11,12},
                new double[]{4,3,2,1},
                new double[]{0.1,0.2,0.3,0.4}
            };

            double[][] snake3 = new double[][]{
                new double[]{17,12,1,0.4},
                new double[]{18,11,2,0.3},
                new double[]{19,10,3,0.2},
                new double[]{20,9,4,0.1}
            };

            double[][] snake4 = new double[][] {
                new double[]{17,18,19,20},
                new double[]{12,11,10,9},
                new double[]{1,2,3,4},
                new double[]{0.4,0.3,0.2,0.1}
            };

            double[][] snake5 = new double[][] {
                new double[]{0.1,0.2,0.3,0.4},
                new double[]{4,3,2,1},
                new double[]{9,10,11,12},
                new double[]{20,19,18,17}
            };

            double[][] snake6 = new double[][] {
                new double[]{0.1,4,9,20},
                new double[]{0.2,3,10,19},
                new double[]{0.3,2,11,18},
                new double[]{0.4,1,12,17}
            };

            double[][] snake7 = new double[][] {
                new double[]{0.4,0.3,0.2,0.1},
                new double[]{1,2,3,4},
                new double[]{12,11,10,9},
                new double[]{17,18,19,20}
            };

            double[][] snake8 = new double[][] {
                new double[]{0.4,1,12,17},
                new double[]{0.3,2,11,18},
                new double[]{0.2,3,10,19},
                new double[]{0.1,4,9,20}
            };

            List<double[][]> weightMatrices = new List<double[][]>();
            weightMatrices.Add(snake1);
            weightMatrices.Add(snake2);
            weightMatrices.Add(snake3);
            weightMatrices.Add(snake4);
            weightMatrices.Add(snake5);
            weightMatrices.Add(snake6);
            weightMatrices.Add(snake7);
            weightMatrices.Add(snake8);

            return MaxProductMatrix(state.Grid, weightMatrices);
        }


        // Helper method for the WeightSnake heuristic - finds the weight matrix that gives the greatest
        // sum when multiplied with the grid and summed up, returns this sum
        private static double MaxProductMatrix(int[][] grid, List<double[][]> weightMatrices)
        {
            List<double> sums = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid.Length; j++)
                {
                    for (int k = 0; k < weightMatrices.Count; k++)
                    {
                        double mult = weightMatrices[k][i][j] * grid[i][j];
                        weightMatrices[k][i][j] = mult;
                        sums[k] += mult;
                    }
                }
            }
            // find the largest sum
            return sums.Max();
        }
    }
}
