using System;

    public class Wumpus
    {
        // constants
        protected readonly int ASLEEP = 0;
        protected readonly int AWAKE = 1;
        protected readonly int MOVING = 2;

        // instance variables
        protected int state;
        protected int numberOfTurns;
        protected int maxNumberOfRoomsToMovePerTurn;
        protected int numberOfTurnsToMove;

        // int state: starting state of Wumpus
        public Wumpus(int state)
        {
            this.state = state;
            numberOfTurns = 0;
        }

        // accessor methods

        // returns state ( 0 - asleep, 1 - awake, 2 - moving )
        public int GetState()
        {
            return state;
        }

        // returns number of turns
        public int GetNumberOfTurns()
        {
            return numberOfTurns;
        }

        // mutator methods

        // change state ( 0 - asleep, 1 - awake, 2 - moving )
        public void SetState(int state)
        {
            this.state = state;
        }
        
        // every time player moves, Wumpus turn executed as well
        // returns number of rooms Wumpus must move on this turn
        // -1: move to random room
        // anything else: number of rooms to move
        public virtual int Turn(Random rand) { return -1; }

        // called when Wumpus defeated in trivia
        public virtual void DefeatedInTrivia(Random rand) { }

        // called when player shoots arrow and misses   
        public virtual void ShotAttempted() { }
    }