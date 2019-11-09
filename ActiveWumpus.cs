using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class ActiveWumpus : Wumpus
    {
        // constants
        private readonly int RANDOM_TELEPORT_PROBABILITY = 20;
        private readonly int NUMBER_TURNS_TO_WAKE_UP = 5;
        private readonly int NUMBER_OF_TURNS_TO_MOVE = 3;
        private readonly int MAX_NUMBER_OF_ROOMS_TO_MOVE = 1;
        private readonly int TRIVIA_MAX_NUMBER_OF_ROOMS_TO_MOVE = 2;
        
        // set state
        public ActiveWumpus(int startingState) : base(startingState) { }

        // new turn
        // returns number of rooms to move
        public override int Turn(Random rand)
        {
            // currently needs to move
            if(numberOfTurnsToMove > 0)
            {
                numberOfTurnsToMove--;
                state = AWAKE;
                return rand.Next(maxNumberOfRoomsToMovePerTurn+1); // up to maxNumberOfRoomsToMovePerTurn rooms
            }

            state = ASLEEP; // not currently moving

            if(rand.Next(RANDOM_TELEPORT_PROBABILITY) == 0) // teleport to random room with 5% probability
            {
                return -1;
            }
            else if(numberOfTurns % NUMBER_TURNS_TO_WAKE_UP == 0) // every NUMBER_TURNS_TO_WAKE_UP turns...
            {
                maxNumberOfRoomsToMovePerTurn = MAX_NUMBER_OF_ROOMS_TO_MOVE;
                numberOfTurnsToMove = rand.Next(NUMBER_OF_TURNS_TO_MOVE+1);  // up to NUMBER_OF_TURNS_TO_MOVE turns
                state = AWAKE;
            }

            return 0;
        }

        // Wumpus defeated in trivia
        public override void DefeatedInTrivia(Random rand)
        {
            maxNumberOfRoomsToMovePerTurn = TRIVIA_MAX_NUMBER_OF_ROOMS_TO_MOVE;
            numberOfTurnsToMove = rand.Next(NUMBER_OF_TURNS_TO_MOVE+1);
            state = AWAKE;
        }
    }