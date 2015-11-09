using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Threes_console
{
    public class State
    {
        private int[][] grid;
        public int[][] Grid
        {
            get
            {
                return this.grid;
            }
            set
            {
                this.grid = value;
            }
        }

        private int player;
        public int Player
        {
            get
            {
                return this.player;
            }
            set
            {
                this.player = value;
            }
        }

        private Move generatingMove;
        public Move GeneratingMove
        {
            get
            {
                return this.generatingMove;
            }

            set
            {
                this.generatingMove = value;
            }
        }
        internal List<int> columnsOrRowsWithMovedTiles;


        public State(int[][] grid, int player)
        {
            this.grid = grid;
            this.player = player;
            if (player == GameEngine.COMPUTER)
            {
                columnsOrRowsWithMovedTiles = new List<int>();
            }
            else
            {
                columnsOrRowsWithMovedTiles = null;
            }
            
        }

        public List<Move> GetAllMoves()
        {
            if (player == GameEngine.PLAYER)
            {
                return GetAllPlayerMoves();
            }
            else if (player == GameEngine.COMPUTER)
            {
                return GetAllComputerMoves();
            }
            else
            {
                throw new Exception();
            }
        }

        // used for upcoming computer moves, where we know the value of the next card
        public List<Move> GetAllComputerMoves(int nextCard)
        {
            List<Move> moves = new List<Move>();

            switch(((PlayerMove)generatingMove).Direction) {
                
                case DIRECTION.LEFT:
                    return AddComputerMovesAfterLeft(moves, nextCard);
                
                case DIRECTION.RIGHT:
                    return AddComputerMovesAfterRight(moves, nextCard);

                case DIRECTION.UP:
                    return AddComputerMovesAfterUp(moves, nextCard);

                case DIRECTION.DOWN:
                    return AddComputerMovesAfterDown(moves, nextCard);

                default:
                    throw new Exception();
            }
        }

        public List<Move> GetAllComputerMoves(Deck deck)
        {
            IEnumerable<int> cardsLeft = deck.GetAllCards().Distinct(); // get all distinct cards left
            List<Move> moves = new List<Move>();

            foreach (int card in cardsLeft)
            {
                switch (((PlayerMove)generatingMove).Direction)
                {

                    case DIRECTION.LEFT:
                        return AddComputerMovesAfterLeft(moves, card);

                    case DIRECTION.RIGHT:
                        return AddComputerMovesAfterRight(moves, card);

                    case DIRECTION.UP:
                        return AddComputerMovesAfterUp(moves, card);

                    case DIRECTION.DOWN:
                        return AddComputerMovesAfterDown(moves, card);

                    default:
                        throw new Exception();
                }
            }
            return moves;

        }

        private List<Move> AddComputerMovesAfterLeft(List<Move> moves, int cardToAdd)
        {
            foreach (int i in columnsOrRowsWithMovedTiles)
            {
                moves.Add(new ComputerMove(cardToAdd, new Tuple<int, int>(3, i)));
            }
            return moves;
        }

        private List<Move> AddComputerMovesAfterRight(List<Move> moves, int cardToAdd)
        {
            foreach (int i in columnsOrRowsWithMovedTiles)
            {
                moves.Add(new ComputerMove(cardToAdd, new Tuple<int,int>(0, i)));
            }
            return moves;
        }

        private List<Move> AddComputerMovesAfterUp(List<Move> moves, int cardToAdd)
        {
            foreach (int i in columnsOrRowsWithMovedTiles)
            {
                moves.Add(new ComputerMove(cardToAdd, new Tuple<int, int>(i, 0)));
            }
            return moves;
        }

        private List<Move> AddComputerMovesAfterDown(List<Move> moves, int cardToAdd)
        {
            foreach (int i in columnsOrRowsWithMovedTiles)
            {
                moves.Add(new ComputerMove(cardToAdd, new Tuple<int, int>(i, 3)));
            }
            return moves;
        }

        private List<Move> GetAllComputerMoves()
        {
            List<Move> moves = new List<Move>(); 
            if (((PlayerMove)generatingMove).Direction == DIRECTION.LEFT)
            {
                moves = AddComputerMovesAfterLeft(moves, 1);
                moves = AddComputerMovesAfterLeft(moves, 2);
                moves = AddComputerMovesAfterLeft(moves, 3);
                List<int> bonusCards = this.GeneratePossibleBonusCards();
                foreach (int card in bonusCards)
                {
                    moves = AddComputerMovesAfterLeft(moves, card);
                }
            }
            else if (((PlayerMove)generatingMove).Direction == DIRECTION.RIGHT)
            {
                moves = AddComputerMovesAfterRight(moves, 1);
                moves = AddComputerMovesAfterRight(moves, 2);
                moves = AddComputerMovesAfterRight(moves, 3);
                
                List<int> bonusCards = this.GeneratePossibleBonusCards();
                foreach (int card in bonusCards)
                {
                    moves = AddComputerMovesAfterRight(moves, card);
                }
                
            }
            else if (((PlayerMove)generatingMove).Direction == DIRECTION.DOWN)
            {
                moves = AddComputerMovesAfterDown(moves, 1);
                moves = AddComputerMovesAfterDown(moves, 2);
                moves = AddComputerMovesAfterDown(moves, 3);
                
                List<int> bonusCards = this.GeneratePossibleBonusCards();
                foreach (int card in bonusCards)
                {
                    moves = AddComputerMovesAfterDown(moves, card);
                }
                
            }
            else if (((PlayerMove)generatingMove).Direction == DIRECTION.UP)
            {
                moves = AddComputerMovesAfterUp(moves, 1);
                moves = AddComputerMovesAfterUp(moves, 2);
                moves = AddComputerMovesAfterUp(moves, 3);
                
                List<int> bonusCards = this.GeneratePossibleBonusCards();
                foreach (int card in bonusCards)
                {
                    moves = AddComputerMovesAfterUp(moves, card);
                }
            }
            else
            {
                throw new Exception();
            }

            return moves;
        }

        private List<Move> GetAllPlayerMoves()
        {
            List<Move> moves = new List<Move>();
            foreach (DIRECTION direction in Enum.GetValues(typeof(DIRECTION)))
            {
                if (IsValidMove(direction))
                {
                    PlayerMove move = new PlayerMove(direction);
                    moves.Add(move);
                }
            }
            return moves;
        }

        internal State ApplyMove(Move move)
        {
            if (move is PlayerMove)
            {
                State result = new State(GridHelper.CloneGrid(this.grid), GameEngine.COMPUTER);
                if (((PlayerMove)move).Direction == DIRECTION.LEFT)
                {
                    return result.ApplyLeft();
                }
                else if (((PlayerMove)move).Direction == DIRECTION.RIGHT)
                {
                    return result.ApplyRight();
                }
                else if (((PlayerMove)move).Direction == DIRECTION.UP)
                {
                    return result.ApplyUp();
                }
                else if (((PlayerMove)move).Direction == DIRECTION.DOWN)
                {
                    return result.ApplyDown();
                }
                return result;
            }
            else if (move is ComputerMove)
            {
                State result = new State(GridHelper.CloneGrid(this.grid), GameEngine.PLAYER);
                result.grid[((ComputerMove)move).Position.Item1][((ComputerMove)move).Position.Item2] = ((ComputerMove)move).Card;
                result.generatingMove = (ComputerMove)move;
                return result;
            }
            else
            {
                throw new Exception();
            }
        }

        private State ApplyDown()
        {
            for (int j = 1; j < GameEngine.ROWS; j++)
            {
                for (int i = 0; i < GameEngine.COLUMNS; i++)
                {
                    if (grid[i][j] != 0)
                    {
                        if (grid[i][j - 1] == 0)
                        { // next cell is empty, just move the tile
                            MoveTile(i, j, i, j - 1);
                            this.columnsOrRowsWithMovedTiles.Add(i);
                        }
                        else if ((grid[i][j] == 1 && grid[i][j - 1] == 2)
                          || (grid[i][j] == 2 && grid[i][j - 1] == 1)
                          || (grid[i][j] > 2 && grid[i][j] == grid[i][j - 1]))
                        { // tiles are mergeable
                            MergeTiles(i, j, i, j - 1);
                            this.columnsOrRowsWithMovedTiles.Add(i);
                        }
                    }
                }
            }
            this.generatingMove = new PlayerMove(DIRECTION.DOWN);
            return this;
        }

        private State ApplyUp()
        {
            for (int j = GameEngine.ROWS - 2; j >= 0; j--)
            {
                for (int i = 0; i < GameEngine.COLUMNS; i++)
                {
                    if (grid[i][j] != 0)
                    {
                        if (grid[i][j + 1] == 0)
                        { // next cell is empty, just move the tile
                            MoveTile(i, j, i, j + 1);
                            this.columnsOrRowsWithMovedTiles.Add(i);
                        }
                        else if ((grid[i][j] == 1 && grid[i][j + 1] == 2)
                          || (grid[i][j] == 2 && grid[i][j + 1] == 1)
                          || (grid[i][j] > 2 && grid[i][j] == grid[i][j + 1]))
                        { // tiles are mergeable
                            MergeTiles(i, j, i, j + 1);
                            this.columnsOrRowsWithMovedTiles.Add(i);
                        }
                    }
                }
            }
            this.generatingMove = new PlayerMove(DIRECTION.UP);
            return this;
        }

        private State ApplyRight()
        {
            for (int i = GameEngine.COLUMNS - 2; i >= 0; i--)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    if (grid[i][j] != 0)
                    {
                        if (grid[i + 1][j] == 0)
                        { // next cell is empty, just move the tile
                            MoveTile(i, j, i + 1, j);
                            this.columnsOrRowsWithMovedTiles.Add(j);
                        }
                        else if ((grid[i][j] == 1 && grid[i + 1][j] == 2)
                          || (grid[i][j] == 2 && grid[i + 1][j] == 1)
                          || (grid[i][j] > 2 && grid[i][j] == grid[i + 1][j]))
                        { // tiles are mergeable
                            MergeTiles(i, j, i + 1, j);
                            this.columnsOrRowsWithMovedTiles.Add(j);
                        }
                    }
                }
            }
            this.generatingMove = new PlayerMove(DIRECTION.RIGHT);
            return this;
        }

        private State ApplyLeft()
        {
            for (int i = 1; i < GameEngine.COLUMNS; i++)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    if (grid[i][j] != 0)
                    {
                        if (grid[i - 1][j] == 0)
                        { // next cell is empty, just move the tile
                            MoveTile(i, j, i - 1, j);
                            this.columnsOrRowsWithMovedTiles.Add(j);
                        }
                        else if ((grid[i][j] == 1 && grid[i - 1][j] == 2)
                          || (grid[i][j] == 2 && grid[i - 1][j] == 1)
                          || (grid[i][j] > 2 && grid[i][j] == grid[i - 1][j]))
                        { // tiles are mergeable
                            MergeTiles(i, j, i - 1, j);
                            this.columnsOrRowsWithMovedTiles.Add(j);
                        }
                    }
                }
            }
            this.generatingMove = new PlayerMove(DIRECTION.LEFT);
            return this;
        }

        private void MergeTiles(int i1, int j1, int i2, int j2)
        {
            int newValue = grid[i1][j1] + grid[i2][j2];
            grid[i1][j1] = 0;
            grid[i2][j2] = newValue;
        }

        private void MoveTile(int old_i, int old_j, int new_i, int new_j)
        {
            int value = grid[old_i][old_j];
            grid[old_i][old_j] = 0;
            grid[new_i][new_j] = value;
        }

        public List<int> GeneratePossibleBonusCards()
        {
            int highestCard = GridHelper.GetHighestCard(grid);
            List<int> bonusCards = new List<int>();
            for (int i = 6; i <= highestCard / 8; i *= 2)
            {
                bonusCards.Add(i);
            }
            return bonusCards;
        }

        public bool IsGameOver()
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

        private bool IsValidMove(DIRECTION direction)
        {
            int occupied = 0;

            if (direction == DIRECTION.LEFT)
            {
                for (int i = 0; i < GameEngine.COLUMNS; i++)
                {
                    for (int j = 0; j < GameEngine.ROWS; j++)
                    {
                        if (i == 0 && grid[i][j] != 0)
                        {
                            occupied++;
                        }
                        else if (grid[i][j] != 0 && grid[i - 1][j] == 0) // empty cell to the left
                        {
                            return true;
                        }
                        else if ((grid[i][j] == 1 && grid[i - 1][j] == 2)
                         || (grid[i][j] == 2 && grid[i - 1][j] == 1)
                         || (grid[i][j] > 2 && grid[i][j] == grid[i - 1][j])) // tiles are mergeable
                        {
                            return true;
                        }
                    }
                    if (i == 0 && occupied == 0) return true; // no cards in left-most column must mean left is possible
                }
                return false;
            }
            else if (direction == DIRECTION.RIGHT)
            {
                for (int i = GameEngine.COLUMNS - 1; i >= 0; i--)
                {
                    for (int j = 0; j < GameEngine.ROWS; j++)
                    {
                        if (i == GameEngine.COLUMNS - 1 && grid[i][j] != 0)
                        {
                            occupied++;
                        } 
                        else if(grid[i][j] != 0 && grid[i + 1][j] == 0) // empty cell to the right
                        {
                            return true;
                        }
                        else if((grid[i][j] == 1 && grid[i +1][j] == 2)
                            || (grid[i][j] == 2 && grid[i + 1][j] == 1)
                            || (grid[i][j] > 2 && grid[i][j] == grid[i + 1][j])) // tiles are mergeable
                        {
                            return true;
                        }
                    }
                    if (i == GameEngine.COLUMNS - 1 && occupied == 0) return true; // no card in the right-most column must mean right is possible
                }
                return false;
            }
            else if (direction == DIRECTION.UP)
            {
                for (int j = GameEngine.ROWS - 1; j >= 0; j--)
                {  
                    for (int i = 0; i < GameEngine.COLUMNS; i++)
                    {
                        if (j == GameEngine.ROWS - 1 && grid[i][j] != 0)
                        {
                            occupied++;
                        } 
                        else if(grid[i][j] != 0 && grid[i][j + 1] == 0) // empty cell above
                        {
                            return true;
                        }
                        else if((grid[i][j] == 1 && grid[i][j + 1] == 2)
                            || (grid[i][j] == 2 && grid[i][j + 1] == 1)
                            || (grid[i][j] > 2 && grid[i][j] == grid[i][j + 1])) // tiles are mergeable
                        {
                            return true;
                        }
                    }
                    if (j == GameEngine.ROWS - 1 && occupied == 0) return true; // no cards in the top row must mean up is possible
                }
                return false;
            }
            else if (direction == DIRECTION.DOWN)
            {
                for (int j = 0; j < GameEngine.ROWS; j++)
                {
                    for (int i = 0; i < GameEngine.COLUMNS; i++)
                    {
                        if (j == 0 && grid[i][j] != 0)
                        {
                            occupied++;
                        }
                        else if(grid[i][j] != 0 && grid[i][j - 1] == 0) // empty cell below
                        {
                            return true;
                        }
                        else if((grid[i][j] == 1 && grid[i][j - 1] == 2)
                            || (grid[i][j] == 2 && grid[i][j - 1] == 1)
                            || (grid[i][j] > 2 && grid[i][j] == grid[i][j - 1])) // tiles are mergeable
                        {
                            return true;
                        }
                    }
                    if (j == 0 && occupied == 0) return true; // no cards in the bottom row must mean down is possible
                }
                return false;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
