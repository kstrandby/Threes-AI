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


        public static double HighestCard(State state)
        {
            return GridHelper.GetHighestCard(state.Grid);
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

        public static double Points(State state)
        {
            return state.CalculateFinalScore();
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
                        if (neighbourRowAbove < GameEngine.ROWS && j == 0) // trapped between wall below and higher card above
                        {
                            if (state.Grid[i][j] < 3 && state.Grid[i][neighbourRowAbove] >= 3)
                            {
                                trapped++;
                            }
                            else if (state.Grid[i][j] >= 3 && state.Grid[i][neighbourRowAbove] >= 3 && state.Grid[i][neighbourRowAbove] > state.Grid[i][j])
                            {
                                trapped++;
                            }
                        }
                        else if (neighbourRowBelow >= 0 && j == 3) // trapped between wall above and higher card below
                        {
                            if (state.Grid[i][j] < 3 && state.Grid[i][neighbourRowBelow] >= 3)
                            {
                                trapped++;
                            }
                            else if (state.Grid[i][j] >= 3 && state.Grid[i][neighbourRowBelow] >= 3 && state.Grid[i][neighbourRowBelow] > state.Grid[i][j])
                            {
                                trapped++;
                            }
                        }
                        else if (neighbourRowAbove < GameEngine.ROWS && neighbourRowBelow >= 0) // trapped between two higher cards
                        {
                            if (state.Grid[i][j] < 3 && state.Grid[i][neighbourRowBelow] >= 3 && state.Grid[i][neighbourRowAbove] >= 3)
                            {
                                trapped++;
                            }
                            else if (state.Grid[i][j] >= 3 && state.Grid[i][neighbourRowBelow] >= 3 && state.Grid[i][neighbourRowAbove] >= 3 
                                && state.Grid[i][neighbourRowBelow] > state.Grid[i][j] && state.Grid[i][neighbourRowAbove] > state.Grid[i][j])
                            {
                                trapped++;
                            }
                        }


                        // check neighbours in horizontal direction
                        int neighbourColumnToRight = i + 1;
                        int neighbourColumnToLeft = i - 1;
                        if (neighbourColumnToRight < GameEngine.COLUMNS && i == 0) // trapped between wall to the left and higher card to the right
                        {
                            if (state.Grid[i][j] < 3 && state.Grid[neighbourColumnToRight][j] >= 3)
                            {
                                trapped++;
                            }
                            else if (state.Grid[i][j] >= 3 && state.Grid[neighbourColumnToRight][j] >= 3 && state.Grid[neighbourColumnToRight][j] > state.Grid[i][j])
                            {
                                trapped++;
                            }
                        }
                        else if (neighbourColumnToLeft >= 0 && i == 3) // trapped between wall to the right and higher card to the left
                        {
                            if (state.Grid[i][j] < 3 && state.Grid[neighbourColumnToLeft][j] >= 3)
                            {
                                trapped++;
                            }
                            else if (state.Grid[i][j] >= 3 && state.Grid[neighbourColumnToLeft][j] >= 3 && state.Grid[neighbourColumnToLeft][j] > state.Grid[i][j])
                            {
                                trapped++;
                            }
                        }
                        else if (neighbourColumnToRight < GameEngine.COLUMNS && neighbourColumnToLeft >= 0) // trapped between two higher cards
                        {
                            if (state.Grid[i][j] < 3 && state.Grid[neighbourColumnToRight][j] >= 3 && state.Grid[neighbourColumnToLeft][j] >= 3)
                            {
                                trapped++;
                            } 
                            else if(state.Grid[i][j] >= 3 && state.Grid[neighbourColumnToRight][j] >= 3 && state.Grid[neighbourColumnToLeft][j] >= 3
                                && state.Grid[neighbourColumnToRight][j] > state.Grid[i][j] && state.Grid[neighbourColumnToLeft][j] > state.Grid[i][j])
                            {
                                trapped++;
                            }
                        }
                    }
                }
            }
            return trapped;
        }


        public static double Mergeability(State state)
        {
            double numMerges = 0;

            for (int i = 0; i < GameEngine.COLUMNS; i++)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    // check merges horizontally
                    if (i + 1 < state.Grid.Length && state.Grid[i][j] != 0 && state.Grid[i + 1][j] != 0)
                    {
                        if (state.Grid[i][j] == 1 && state.Grid[i + 1][j] == 2
                            || state.Grid[i][j] == 2 && state.Grid[i + 1][j] == 1
                            || state.Grid[i][j] == state.Grid[i + 1][j])
                        {
                            if (i == 0) numMerges++;
                            else if (state.Grid[i - 1][j] != 0) numMerges++;
                        }

                    }
                    // check merges vertically
                    if (j + 1 < state.Grid.Length && state.Grid[i][j] != 0 && state.Grid[i][j + 1] != 0)
                    {
                        if (state.Grid[i][j] == 1 && state.Grid[i][j + 1] == 2
                            || state.Grid[i][j] == 2 && state.Grid[i][j + 1] == 1
                            || state.Grid[i][j] == state.Grid[i][j])
                        {
                            if (j == 0) numMerges++;
                            else if (state.Grid[i][j - 1] != 0) numMerges++;
                        }
                    }
                }
            }
            return numMerges;
        }

        // returns a score ranking a state according to how the tiles are increasing/decreasing in all directions
        // increasing/decreasing in the same directions (for example generally increasing in up and right direction) will return higher score 
        // than a state increasing in one row and decreasing in another row

        // range: {-192, 0}
        public static double Monotonicity(State state)
        {
            double left = 0;
            double right = 0;
            double up = 0;
            double down = 0;

            // up/down direction
            for (int i = 0; i < state.Grid.Length; i++)
            {
                int current = 0;
                int next = current + 1;
                while (next < state.Grid.Length)
                {
                    // skip empty cells
                    while (next < state.Grid.Length && state.Grid[i][next] == 0)
                        next++;
                    // check boundaries
                    if (next >= state.Grid.Length)
                        next--;

                    // only count instances where both cells are occupied
                    if (state.Grid[i][current] != 0 && state.Grid[i][next] != 0)
                    {
                        double currentValue = 0;
                        if (state.Grid[i][current] == 1 || state.Grid[i][current] == 2) currentValue = 1;
                        else currentValue = Math.Log(state.Grid[i][current] / 3) / Math.Log(2) + 2;

                        double nextValue  = 0;
                        if(state.Grid[i][next] == 1 || state.Grid[i][next] == 2) nextValue = 1;
                        else nextValue = Math.Log(state.Grid[i][next] / 3) / Math.Log(2) + 2;

                        if (currentValue > nextValue) // increasing in down direction
                            down += nextValue - currentValue;
                        else if (nextValue > currentValue) // increasing in up direction
                            up += currentValue - nextValue;
                    }

                    current = next;
                    next++;
                }
            }

            // left/right direction
            for (int j = 0; j < state.Grid.Length; j++)
            {
                int current = 0;
                int next = current + 1;
                while (next < state.Grid.Length)
                {
                    // skip empty cells
                    while (next < state.Grid.Length && state.Grid[next][j] == 0)
                        next++;
                    // check boundaries
                    if (next >= state.Grid.Length)
                        next--;

                    // only consider instances where both cells are occupied
                    if (state.Grid[current][j] != 0 && state.Grid[next][j] != 0)
                    {
                        double currentValue = 0;
                        if (state.Grid[current][j] == 1 || state.Grid[current][j] == 2) currentValue = 1;
                        else currentValue = Math.Log(state.Grid[current][j] / 3) / Math.Log(2) + 2;

                        double nextValue = 0;
                        if (state.Grid[next][j] == 1 || state.Grid[next][j] == 2) nextValue = 1;
                        else nextValue = Math.Log(state.Grid[next][j] / 3) / Math.Log(2) + 2;

                        if (currentValue > nextValue) // increasing in left direction
                            left += nextValue - currentValue;
                        else if (nextValue > currentValue) // increasing in right direction
                            right += currentValue - nextValue;
                    }

                    current = next;
                    next++;
                }
            }
            return Math.Max(up, down) + Math.Max(left, right);
        }


        public static double Smoothness(State state)
        {
            double smoothness = 0;
            for (int i = 0; i < state.Grid.Length; i++)
            {
                for (int j = 0; j < state.Grid.Length; j++)
                {
                    if (state.Grid[i][j] != 0)
                    {
                        double currentValue = 0;
                        if (state.Grid[i][j] == 1 || state.Grid[i][j] == 2) currentValue = 1;
                        else currentValue = Math.Log(state.Grid[i][j] / 3) / Math.Log(2) + 2;


                        // we only check right and up for each tile
                        Cell nearestTileRight = FindNearestTile(new Cell(i, j), DIRECTION.RIGHT, state.Grid);
                        Cell nearestTileUp = FindNearestTile(new Cell(i, j), DIRECTION.UP, state.Grid);

                        // check that we found a tile (do not take empty cells into account)
                        if (nearestTileRight.IsValid() && state.Grid[nearestTileRight.x][nearestTileRight.y] != 0)
                        {
                            double neighbourValue = 0;
                            if (state.Grid[nearestTileRight.x][nearestTileRight.y] == 1 || state.Grid[nearestTileRight.x][nearestTileRight.y] == 2) neighbourValue = 1;
                            else neighbourValue = Math.Log(state.Grid[nearestTileRight.x][nearestTileRight.y] / 3) / Math.Log(2) + 2;
                            smoothness += Math.Abs(currentValue - neighbourValue);
                        }

                        if (nearestTileUp.IsValid() && state.Grid[nearestTileUp.x][nearestTileUp.y] != 0)
                        {
                            double neighbourValue = 0;
                            if (state.Grid[nearestTileUp.x][nearestTileUp.y] == 1 || state.Grid[nearestTileUp.x][nearestTileUp.y] == 2) neighbourValue = 1;
                            else neighbourValue = Math.Log(state.Grid[nearestTileUp.x][nearestTileUp.y] / 3) / Math.Log(2) + 2;
                            smoothness += Math.Abs(currentValue - neighbourValue);
                        }
                    }
                }
            }
            return -smoothness;
        }


        public static Cell FindNearestTile(Cell from, DIRECTION dir, int[][] grid)
        {
            int x = from.x, y = from.y;
            if (dir == DIRECTION.LEFT)
            {
                x -= 1;
                while (x >= 0 && grid[x][y] == 0)
                {
                    x--;
                }
            }
            else if (dir == DIRECTION.RIGHT)
            {
                x += 1;
                while (x < grid.Length && grid[x][y] == 0)
                {
                    x++;
                }
            }
            else if (dir == DIRECTION.UP)
            {
                y += 1;
                while (y < grid.Length && grid[x][y] == 0)
                {
                    y++;
                }
            }
            else if (dir == DIRECTION.DOWN)
            {
                y -= 1;
                while (y >= 0 && grid[x][y] == 0)
                {
                    y--;
                }
            }
            return new Cell(x, y);
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


        public static double Corner(State state)
        {
            double[][] corner1 = new double[][] {
                new double[]{20,12,4,0.4},
                new double[]{19,11,3,0.3},
                new double[]{18,10,2,0.2},
                new double[]{17,9,1,0.1}
            };

            double[][] corner2 = new double[][] {
                new double[]{0.4,4,12,20},
                new double[]{0.3,3,11,19},
                new double[]{0.2,2,10,18},
                new double[]{0.1,1,9,17}
            };

            double[][] corner3 = new double[][] {
                new double[]{17,9,1,0.1},
                new double[]{18,10,2,0.2},
                new double[]{19,11,3,0.3},
                new double[]{20,12,4,0.4}
            };

            double[][] corner4 = new double[][] {
                new double[]{0.1,1,9,17},
                new double[]{0.2,2,10,18},
                new double[]{0.3,3,11,19},
                new double[]{0.4,4,12,20}
            };

            double[][] corner5 = new double[][] {
                new double[]{20,19,18,17},
                new double[]{12,11,10,9},
                new double[]{4,3,2,1},
                new double[]{0.4,0.3,0.2,0.1}
            };

            double[][] corner6 = new double[][] {
                new double[]{17,18,19,20},
                new double[]{9,10,11,12},
                new double[]{1,2,3,4},
                new double[]{0.1,0.2,0.3,0.4}
            };

            double[][] corner7 = new double[][] {
                new double[]{0.4,0.3,0.2,0.1},
                new double[]{4,3,2,1},
                new double[]{12,11,10,9},
                new double[]{20,19,18,17}
            };

            double[][] corner8 = new double[][] {
                new double[]{0.1,0.2,0.3,0.4},
                new double[]{1,2,3,4},
                new double[]{9,10,11,12},
                new double[]{17,18,19,20}
            };
            List<double[][]> weightMatrices = new List<double[][]>();

            weightMatrices.Add(corner1);
            weightMatrices.Add(corner2);
            weightMatrices.Add(corner3);
            weightMatrices.Add(corner4);
            weightMatrices.Add(corner5);
            weightMatrices.Add(corner6);
            weightMatrices.Add(corner7);
            weightMatrices.Add(corner8);

            return MaxProductMatrix(state.Grid, weightMatrices);
        }
    }
}
