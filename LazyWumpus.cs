using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class LazyWumpus : Wumpus
    {
    // constants
    private readonly int DORMANT_TURNS_TO_SLEEP = 2;
    private readonly int MAX_NUMBER_ROOMS_TO_MOVE = 1;
    private readonly int NUMBER_OF_TURNS_TO_MOVE = 1;
    private readonly int ARROW_MAX_NUMBER_OF_ROOMS_TO_MOVE = 2;
    private readonly int TRIVIA_MAX_NUMBER_OF_ROOMS_TO_MOVE = 3;

        // instance variables
        private int numberOfTurnsDormant;

        // set state
        public LazyWumpus(int startingBehavior) : base(startingBehavior) { }

        // new turn
        // returns number of rooms to move
        public override int Turn(Random rand)
        {
            // currently moving
            if (numberOfTurnsToMove > 0)
            {
                numberOfTurnsToMove--;
                state = MOVING;
                return rand.Next(maxNumberOfRoomsToMovePerTurn + 1); // up to maxNumberOfRoomsToMovePerTurn rooms
            }

            // not currently moving
            state = AWAKE;
            maxNumberOfRoomsToMovePerTurn = MAX_NUMBER_ROOMS_TO_MOVE;
            numberOfTurnsToMove = NUMBER_OF_TURNS_TO_MOVE;

            // dormant
            if (maxNumberOfRoomsToMovePerTurn == 0)
            {
                numberOfTurnsDormant++;
            }

            // falls asleep
            if(numberOfTurnsDormant == DORMANT_TURNS_TO_SLEEP)
            {
                state = ASLEEP;
            }

            return 0;
        }

        // Wumpus defeated in trivia
        public override void DefeatedInTrivia(Random rand)
        {
            maxNumberOfRoomsToMovePerTurn = TRIVIA_MAX_NUMBER_OF_ROOMS_TO_MOVE;
            numberOfTurnsToMove = NUMBER_OF_TURNS_TO_MOVE;
        }

        // player shot arrow at Wumpus and missed
        public override void ShotAttempted()
        {
            if (GetState() == ASLEEP)
            {
                state = AWAKE;
                numberOfTurnsToMove = NUMBER_OF_TURNS_TO_MOVE;
                maxNumberOfRoomsToMovePerTurn = ARROW_MAX_NUMBER_OF_ROOMS_TO_MOVE;
            }
        }

    }
