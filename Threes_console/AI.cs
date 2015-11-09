using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threes_console
{
    public static class AI
    {
        public static double Evaluate(State state)
        {
            return CalculateFinalScore(state.Grid);
        }

        public static int CalculateFinalScore(int[][] grid)
        {
            int score = 0;
            for (int i = 0; i < GameEngine.COLUMNS; i++)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    if (grid[i][j] != 1 && grid[i][j] != 2 && grid[i][j] != 0)
                    {
                        score += GameEngine.TILE_TO_POINTS_DICT[grid[i][j]];
                    }
                }
            }
            return score;
        }
    }
}
