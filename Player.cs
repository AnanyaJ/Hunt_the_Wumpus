using System;

    public class Player
    {
        // constants
        private readonly int POINTS_PER_ARROW = 5;
        private readonly int POINTS_FOR_KILLING_WUMPUS = 50;
        private readonly double POINTS_PER_SECOND_LEFT = 0.25;

        // instance variables
        private int totalCoins;
        private int numberOfGoldCoins;
        private int numberOfTurns;
        private int numberOfArrows;
        
        // initializes variables
        // int totalCoins: total available coins in game
        public Player(int totalCoins, int startingGoldCoins=0, int startingArrows=2) {
            this.totalCoins = totalCoins;
            numberOfGoldCoins = startingGoldCoins;
            numberOfTurns = 0;
            numberOfArrows = startingArrows;
        }

        // mutator methods

        // increments number of turns
        public void NewTurn()
        {
            numberOfTurns++;
        }
        
        // adds coins to inventory
        // int goldCoins: number of coins to add
        public void AddGoldCoins(int goldCoins)
        {
            numberOfGoldCoins += goldCoins;
        }

        // subtracts coins from inventory (when playing trivia)
        // int goldCoins: number of coins to subtract
        public void SubGoldCoins(int goldCoins)
        {
            numberOfGoldCoins -= goldCoins;
        }

        
        // adds arrows to inventory
        // int arrows; number of arrows to add
        public void AddArrows(int arrows)
        {
            numberOfArrows += arrows;
        }

        // subtracts arrows from inventory
        // int arrows: number of arrows to subtract
        public void SubArrows(int arrows)
        {
            numberOfArrows -= arrows;
        }

        // accessor methods

        // computes and returns score
        // bool killedWumpus: whether or not player defeated Wumps
        // int secondsRemaining: how many seconds player had left when game ended
        public int GetScore(bool killedWumpus, int secondsRemaining)
        {
            int timePoints = 0;
            if (killedWumpus) timePoints = (int)(secondsRemaining * POINTS_PER_SECOND_LEFT);
            return totalCoins - numberOfTurns + numberOfGoldCoins + POINTS_PER_ARROW * numberOfArrows + (killedWumpus ? POINTS_FOR_KILLING_WUMPUS : 0) + timePoints;
        }

        // returns number of gold coins in inventory
        public int GetNumberOfGoldCoins()
        {
            return numberOfGoldCoins;
        }

        // returns number of arrows in inventory
        public int GetNumberOfArrows()
        {
            return numberOfArrows;
        }

        // returns number of turns
        public int GetNumberOfTurns()
        {
            return numberOfTurns;
        }
    }