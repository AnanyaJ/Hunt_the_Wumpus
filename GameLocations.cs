using System;
using System.Collections.Generic;
using System.Linq;

public class GameLocations
{
    // constants
    private readonly string[] MESSAGES = { "Bats nearby.", "I feel a draft.", "I smell a Wumpus!", "You found a treasure chest!" };
    private readonly string[] SECRETS = { "The Wumpus is currently located in room ", "One of the hazards is in room ", "You can find a treasure chest in room " };

    // for locations variable
    private readonly int NORMAL_INDEX = 0;
    private readonly int BAT_INDEX = 1;
    private readonly int PIT_INDEX = 2;
    private readonly int WUMPUS_INDEX = 3;

    // instance variables
    private int numRooms;
    private int numBats;
    private int numPits;
    private int[] locations;
    private int[] goldLocations;
    private int[] treasureChestLocations; // room numbers of treasure chests
    private int playerLocation;
    private int[] obstacleLocations;
    private int wumpusLocation;
    private int constantGold;
    private bool treasureChestFound;

    private Wumpus wumpus;
    private Cave cave;
    private Random rand;

    // initialize variables and set up obstacles/coins
    // int totalCoins: total number of coins in cave
    // bool wumpusBehavior: true if Wumpus is lazy, false if active
    // int constantGold: number of coins in a regular room
    public GameLocations(int numRooms, int numNeighbors, int startingRoom, int totalCoins, Random rand, bool wumpusBehavior, int maxAdjacent=3, int constantGold=2, int numberOfTreasures=3, int numBats=2, int numPits=2)
    {
        this.rand = rand;
        this.numRooms = numRooms;
        this.numBats = numBats;
        this.numPits = numPits;
        this.constantGold = constantGold;
        playerLocation = startingRoom;
        treasureChestFound = false;

        cave = new Cave(numRooms, numNeighbors, maxAdjacent, rand);
        locations = new int[numRooms];
        goldLocations = new int[numRooms];
        treasureChestLocations = new int[numberOfTreasures];
        obstacleLocations = new int[numBats + numPits + 1];

        GenerateObstacles(wumpusBehavior);
        GenerateGoldCoins(totalCoins, constantGold, numberOfTreasures);
    }

    // helper methods

    // distribute gold coins in rooms
    // int numberOfTreasures: number of rooms with more coins than rest
    private void GenerateGoldCoins(int totalCoins, int constantGold, int numberOfTreasures)
    {
        for (int i = 0; i < numberOfTreasures; i++)
        {
            int pos = rand.Next(numRooms);
            if (goldLocations[pos] == 0) // not already a treasure
            {
                goldLocations[pos] = (totalCoins - constantGold * (numRooms - numberOfTreasures)) / numberOfTreasures;
                treasureChestLocations[i] = pos; // store locations of treasures
            }
            else
                i--;
        }

        for (int i = 0; i < numRooms; i++) // rest of the rooms have an average number of coins (constantGold)
        {
            if (goldLocations[i] == 0)
            {
                goldLocations[i] = constantGold;
            }
        }
    }

    // place bats, pits, and Wumpus in unique locations in cave
    private void GenerateObstacles(bool wumpusBehavior)
    {
        if (wumpusBehavior)
        {
            wumpus = new LazyWumpus(1); // always create wumpus as awake
        }
        else
        {
            wumpus = new ActiveWumpus(1);
        }

        for (int i = 0; i < obstacleLocations.Length; i++)
        {
            int location = rand.Next(numRooms);
            if (obstacleLocations[i] == 0) // not already an obstacle
            {
                obstacleLocations[i] = location;
            }
            else
            {
                i--;
            }
        }

        // keep track of locations
        for (int i = 0; i < numBats; i++)
        {
            locations[obstacleLocations[i]] = BAT_INDEX; 
        }
        for (int i = numBats; i < numBats + numPits; i++)
        {
            locations[obstacleLocations[i]] = PIT_INDEX;
        }

        wumpusLocation = obstacleLocations[obstacleLocations.Length - 1];
        locations[wumpusLocation] = WUMPUS_INDEX;
    }

    // returns a new, random location for obstacle
    // where there are no other obstacles
    private int GetRandomLocationForObstacle()
    {
        // newLocation - new index of obstacle ignoring other obstacles
        int newLocation = rand.Next(numRooms - obstacleLocations.Length + 1);
        int currentIndex = 0;
        for (int i = 0; i < numRooms; i++)
        {
            if (currentIndex == newLocation)
            {
                return currentIndex; // actual new index of obstacle
            }

            if (locations[i] == NORMAL_INDEX)
            {
                currentIndex++;
            }
        }

        return -1;
    }

    // mutator methods

    // player moved to new room
    public int Update(int newLocation)
    {
        playerLocation = newLocation;

        int addGold = goldLocations[newLocation]; // update gold
        goldLocations[newLocation] = 0;

        treasureChestFound = addGold > constantGold;

        return addGold; // still take gold from room even if immediately teleport to another room right after
    }

    // move wumpus
    public void MoveWumpus()
    {
        int moves = wumpus.Turn(rand); // number of rooms to move Wumpus

        if (moves == -1) // random location
        {
            locations[wumpusLocation] = NORMAL_INDEX;
            wumpusLocation = GetRandomLocationForObstacle();
            locations[wumpusLocation] = WUMPUS_INDEX;
            obstacleLocations[numBats + numPits] = wumpusLocation;
        }
        else
        {
            for (int i = 0; i < moves; i++)
            {
                locations[wumpusLocation] = NORMAL_INDEX;
                List<int> options = new List<int>(); // rooms Wumpus can move to on this turn
                foreach (int adjacent in cave.GetAdjacentRooms(wumpusLocation))
                {
                    if (!obstacleLocations.Contains(adjacent))
                    {
                        options.Add(adjacent);
                    }
                }

                if (options.Count > 0)
                {
                    wumpusLocation = options[rand.Next(options.Count)];
                    locations[wumpusLocation] = WUMPUS_INDEX;
                    obstacleLocations[numBats + numPits] = wumpusLocation;
                }
                else // Wumpus can't move - surrounded by existing hazards
                {
                    break;
                }
            }
        }
    }

    // change movement of Wumpus since player shot arrow and missed
    public void ShotAttempted()
    {
        wumpus.ShotAttempted();
    }

    // change movement of Wumpus since player defeated it in trivia
    public void DefeatedInTrivia()
    {
        wumpus.DefeatedInTrivia(rand);
    }

    // moves bat to random location once player runs into it
    public void MoveBat()
    {
        locations[playerLocation] = NORMAL_INDEX;
        int newBatLocation = GetRandomLocationForObstacle();
        locations[newBatLocation] = BAT_INDEX;
        for (int obstacleIndex = 0; obstacleIndex < numBats; obstacleIndex++)
        {
            if (obstacleLocations[obstacleIndex] == playerLocation)
            {
                obstacleLocations[obstacleIndex] = newBatLocation;
                break;
            }
        }
    }

    // accessor methods

    // returns current location of player       
    public int GetPlayerLocation()
    {
        return playerLocation;
    }

    // locations of treasure chests
    public int[] GetTreasureChestLocations()
    {
        return treasureChestLocations;
    }

    // wumpus location
    public int GetWumpusLocation()
    {
        return wumpusLocation;
    }

    // returns list of messages (if treasure chest found + warnings) for current room
    public List<string> GetMessages()
    {
        bool[] areNearby = new bool[MESSAGES.Length-1]; // index 0 represents bats, index 1 represents pit, index 2 represents wumpus

        foreach (int room in cave.GetAdjacentRooms(playerLocation))
        {
            if (locations[room] == BAT_INDEX)
            {
                areNearby[BAT_INDEX-1] = true;
            } else if (locations[room] == PIT_INDEX)
            {
                areNearby[PIT_INDEX-1] = true;
            } else if (locations[room] == WUMPUS_INDEX)
            {
                areNearby[WUMPUS_INDEX-1] = true;
            }
        }

        List<string> messages = new List<string>();

        if (treasureChestFound)
        {
            messages.Add(MESSAGES[MESSAGES.Length - 1]);
        }

        for (int i = 0; i < MESSAGES.Length-1; i++)
        {
            if (areNearby[i])
            {
                messages.Add(MESSAGES[i]);
            }
        }

        return messages;
    }

    // Constructs a secret as a String based on locations of hazards, Wumpus, etc.
    public string GetSecret()
    {
        int num = rand.Next(SECRETS.Length);
        int loc = -1;
        switch (num)
        {
            case 0: // 1/3 chance of getting current location of Wumpus
                loc = wumpusLocation + 1;
                break;
            case 1: // 1/3 chance of getting location of hazard
                loc = obstacleLocations[rand.Next(numBats + numPits)] + 1;
                break;
            case 2: // 1/3 chance of getting location of treasure chest
                loc = treasureChestLocations[rand.Next(treasureChestLocations.Length)] + 1;
                break;
        }

        return SECRETS[num] + loc + ".";
    }

    // return true if bats in current room, false otherwise
    public bool BatsInRoom()
    {
        return locations[playerLocation] == BAT_INDEX;
    }

    // returns true if pit in current room, false otherwise
    public bool PitInRoom()
    {
        return locations[playerLocation] == PIT_INDEX;
    }

    // returns true if Wumpus is current room, false otherwise
    public bool WumpusInRoom()
    {
        return locations[playerLocation] == WUMPUS_INDEX;
    }

    // returns Cave object
    public Cave GetCave() // must use this method to access GetAdjacentRooms method from cave to tell player which rooms can access from current location
    {
        return cave;
    }

}
