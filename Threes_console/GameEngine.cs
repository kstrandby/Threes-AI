using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threes_console
{
    // class to hold all game logic
    public class GameEngine
    {
        // constants
        public const int COLUMNS = 4, ROWS = 4;
        public const float BONUS_CARD_PROB = 1.0f / 21.0f;
        public const int PLAYER = 1, COMPUTER = 0;

        // Dictionary that translates card values to points
        public static Dictionary<int, int> TILE_TO_POINTS_DICT = new Dictionary<int, int>() {
            {3, 3},
            {6, 9},
            {12, 27},
            {24, 81},
            {48, 243},
            {96, 729},
            {192, 2187},
            {384, 6561},
            {768, 19683},
            {1536, 59049},
            {3072, 177147},
            {6144, 531441}
        };

        public Deck deck;
        public int nextCard { get; set; }
        private Random random = new Random();
        private bool nextIsBonus = false;
        public State currentState {get; set; }

        public GameEngine()
        {
            deck = new Deck();
            int[][] grid = initializeGrid();
            currentState = new State(grid, COMPUTER);
            UpdatePeekCard();
        }

        // Initializes the grid, adding the 9 initial random cards
        private int[][] initializeGrid()
        {
            int[][] grid = new int[ROWS][];
            // initialize the grid data structure
            for (int i = 0; i < COLUMNS; i++)
            {
                grid[i] = new int[] { 0, 0, 0, 0 };
            }

            // generates 9 random tiles on the grid, either 1, 2 or 3
            for (int i = 0; i < 9; i++)
            {
                // find random available position for tile
                int x = random.Next(4);
                int y = random.Next(4);

                while (grid[x][y] != 0)
                {
                    x = random.Next(4);
                    y = random.Next(4);
                }

                // draw card form deck
                int value = deck.DealCard();

                grid[x][y] = value;
            }
            return grid;
        }

       // Takes a player action and executes it, updates peek card and
        // generates a new random tile
        public bool SendUserAction(PlayerMove action)
        {
            currentState = currentState.ApplyMove(action);

            // only continue game if action was valid (if something moved on the grid)
            if (currentState.columnsOrRowsWithMovedTiles.Count != 0)
            {
                 GenerateNewCard();
                 if (CheckForGameOver())
                 {
                    return true;
                }
                UpdatePeekCard();
            }
            return false;            
        }

        // Calculates the final score of game over state
        public int CalculateFinalScore()
        {
            int score = 0;
            for (int i = 0; i < COLUMNS; i++)
            {
                for (int j = 0; j < ROWS; j++)
                {
                    if (currentState.Grid[i][j] > 2)
                    {
                        score += TILE_TO_POINTS_DICT[currentState.Grid[i][j]];
                    }
                }
            }
            return score;
        }

        // Updates the peek card
        private void UpdatePeekCard()
        {
            // should next card be a bonus card?
            double i = random.NextDouble();
            if (BoardHelper.GetHighestCard(currentState.Grid) >= 48 && i < BONUS_CARD_PROB)
            {
                nextCard = -1;
                nextIsBonus = true;
            }
            else
            {
                nextCard = deck.PeekNextCard();
                nextIsBonus = false;
            }
        }

        // Generates a new card on the board
        private void GenerateNewCard()
        {
            int numTiles = 0;

            int card = 0;
            if (nextIsBonus)
            {
                List<int> possibleBonusCards = currentState.GeneratePossibleBonusCards();
                card = possibleBonusCards[random.Next(0, possibleBonusCards.Count)];
                numTiles = possibleBonusCards.Count * currentState.columnsOrRowsWithMovedTiles.Count;
            }
            else
            {
                card = deck.DealCard();
                numTiles = currentState.columnsOrRowsWithMovedTiles.Count * 3;
            }
            
            // decide on position to put new tile
            int row = 0, column = 0;
            int index = random.Next(0, currentState.columnsOrRowsWithMovedTiles.Count); // random index of list of possible rows/columns

            if (((PlayerMove)currentState.GeneratingMove).Direction == DIRECTION.LEFT)
            {
                row = currentState.columnsOrRowsWithMovedTiles[index];
                column = 3; // right-most column
            }
            else if (((PlayerMove)currentState.GeneratingMove).Direction == DIRECTION.RIGHT)
            {
                row = currentState.columnsOrRowsWithMovedTiles[index];
                column = 0; // left-most column
            }
            else if (((PlayerMove)currentState.GeneratingMove).Direction == DIRECTION.UP)
            {
                column = currentState.columnsOrRowsWithMovedTiles[index];
                row = 0; // down-most row
            }
            else if (((PlayerMove)currentState.GeneratingMove).Direction == DIRECTION.DOWN)
            {
                column = currentState.columnsOrRowsWithMovedTiles[index];
                row = 3; // up-most row
            }
            currentState.Grid[column][row] = card;

            ComputerMove move = new ComputerMove(card, new Tuple<int, int>(column, row));
            currentState.GeneratingMove = move;
        }

        // Checks if the current game state is game over
        public bool CheckForGameOver()
        {
            int[] directions = { -1, 1 };
            for (int i = 0; i < COLUMNS; i++)
            {
                for (int j = 0; j < ROWS; j++)
                {
                    if (currentState.Grid[i][j] == 0) return false;
                    else
                    {
                        foreach (int direction in directions)
                        {
                            // check for merges in up/down direction
                            if (j + direction >= 0 && j + direction < ROWS && currentState.Grid[i][j] > 2 && currentState.Grid[i][j + direction] == currentState.Grid[i][j]) return false;
                            else if (j + direction >= 0 && j + direction < ROWS && currentState.Grid[i][j] == 1 && currentState.Grid[i][j + direction] == 2) return false;
                            else if (j + direction >= 0 && j + direction < ROWS && currentState.Grid[i][j] == 2 && currentState.Grid[i][j + direction] == 1) return false;
                            // check for merges in left/right direction
                            else if (i + direction >= 0 && i + direction < COLUMNS && currentState.Grid[i][j] > 2 && currentState.Grid[i + direction][j] == currentState.Grid[i][j]) return false;
                            else if (i + direction >= 0 && i + direction < COLUMNS && currentState.Grid[i][j] == 1 && currentState.Grid[i + direction][j] == 2) return false;
                            else if (i + direction >= 0 && i + direction < COLUMNS && currentState.Grid[i][j] == 2 && currentState.Grid[i + direction][j] == 1) return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
    