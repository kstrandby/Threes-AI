using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threes_console;

namespace Threes_console
{
    // Static class providing methods for dealing wih the game board
    static class BoardHelper
    {
        // Clones the grid, returns the clone
        public static int[][] CloneGrid(int[][] grid)
        {
            int[][] newGrid = new int[grid.Length][];
            for (int i = 0; i < grid.Length; i++)
            {
                newGrid[i] = (int[])grid[i].Clone();
            }
            return newGrid;
        }

        // Returns string representation of the board
        public static string ToString(int[][] array)
        {
            string representation = "";
            for (int y = GameEngine.ROWS - 1; y >= 0; y--)
            {
                for (int x = 0; x < GameEngine.COLUMNS; x++)
                {

                    string append = " " + array[x][y] + " ";
                    representation += append;

                    if (x != 3)
                    {
                        representation += "|";
                    }
                    else
                    {
                        representation += "\n";
                    }
                }
                if (y != 0)
                {
                    representation += "-------------\n";
                }
            }
            return representation;
        }

        // Finds and returns the highest card on the board
        internal static int GetHighestCard(int[][] grid)
        {
            int highestCard = 0;
            for (int i = 0; i < GameEngine.COLUMNS; i++)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    if (grid[i][j] > highestCard) highestCard = grid[i][j];
                }
            }
            return highestCard;
        }

        // Checks if the board is in a game over state
        internal static bool IsGameOver(int[][] grid)
        {
            int[] directions = { -1, 1 };
            for (int i = 0; i < GameEngine.COLUMNS; i++)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    if (grid[i][j] == 0) return false;
                    else
                    {
                        foreach (int direction in directions)
                        {
                            // check for merges in up/down direction
                            if (j + direction >= 0 && j + direction < GameEngine.ROWS && grid[i][j] > 2 && grid[i][j + direction] == grid[i][j]) return false;
                            else if (j + direction >= 0 && j + direction < GameEngine.ROWS && grid[i][j] == 1 && grid[i][j + direction] == 2) return false;
                            else if (j + direction >= 0 && j + direction < GameEngine.ROWS && grid[i][j] == 2 && grid[i][j + direction] == 1) return false;
                            // check for merges in left/right direction
                            else if (i + direction >= 0 && i + direction < GameEngine.COLUMNS && grid[i][j] > 2 && grid[i + direction][j] == grid[i][j]) return false;
                            else if (i + direction >= 0 && i + direction < GameEngine.COLUMNS && grid[i][j] == 1 && grid[i + direction][j] == 2) return false;
                            else if (i + direction >= 0 && i + direction < GameEngine.COLUMNS && grid[i][j] == 2 && grid[i + direction][j] == 1) return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
