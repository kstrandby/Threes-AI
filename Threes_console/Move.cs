using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Threes_console
{
    public enum DIRECTION
    {
        LEFT = 0,
        RIGHT = 1,
        UP = 2,
        DOWN = 3
    }

    public class Move
    {
        private double score;
        public double Score
        {
            get
            {
                return this.score;
            }
            set
            {
                this.score = value;
            }
        }

        public Move()
        {
        }
    }

    public class PlayerMove : Move {

        private DIRECTION direction;
        public DIRECTION Direction
        {
            get
            {
                return this.direction;
            }
            set
            {
                this.direction = value;
            }
        }

        public PlayerMove(DIRECTION direction)
        {
            this.direction = direction;
        }

        public PlayerMove()
        {
            this.direction = (DIRECTION)(-1);
        }
    }

    public class ComputerMove : Move {

        private int card;
        public int Card
        {
            get
            {
                return this.card;
            }
            set
            {
                this.card = value;
            }
        }
        private Tuple<int, int> position;
        public Tuple<int, int> Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }

        public ComputerMove(int card, Tuple<int, int> position)
        {
            this.card = card;
            this.position = position;
        }

        public ComputerMove()
        {
            this.card = -1;
            this.position = new Tuple<int, int>(-1, -1);
        }
    }
}
