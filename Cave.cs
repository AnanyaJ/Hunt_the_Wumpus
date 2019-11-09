using System;
using System.Collections.Generic;

    public class Cave
    {
        // constants
        private readonly int[][] NEIGHBOR_PATTERNS = { new int[] { -6, -5, 1, 6, -1, -7 }, new int[] { -6, 1, 7, 6, 5, -1 }, // how room numbers relate to the
                                                                                        new int[] { -6, -5, 1, 6, 5, -1 } }; // rooms around them
        // instance variables
        private int numRooms;
        private int numNeighbors;
        private int maxAdjacent;
        private List<int>[] adjacents;
        private int[][] neighbors;
        private Random rand;

        // creates cave
        // int numNeighbors: number of rooms touching each room
        // int maxAdjacent: maximum number of connections out of a room
        public Cave(int numRooms, int numNeighbors, int maxAdjacent, Random rand)
        {
            this.numRooms = numRooms;
            this.numNeighbors = numNeighbors;
            this.maxAdjacent = maxAdjacent;
            this.rand = rand;
            neighbors = new int[numRooms][];
            adjacents = new List<int>[numRooms];

            for (int i = 0; i < numRooms; i++)
            {
                neighbors[i] = new int[numNeighbors];
                adjacents[i] = new List<int>();
            }

            FindNeighbors();
            CreateCave();
        }

        // helper methods

        // find 6 neighboring rooms for each room, in clockwise order, starting from top
        private void FindNeighbors()
        {
            for (int i = 0; i < numRooms; i++)
            {
                for (int j = 0; j < numNeighbors; j++)
                {
                    if (i % numNeighbors != 0 && i % numNeighbors != numNeighbors - 1) // not an edge room
                    {
                        neighbors[i][j] = GetPositiveMod(i + NEIGHBOR_PATTERNS[i % 2][j], numRooms); // different pattern for odd and even rooms
                    }
                    else // edge room
                    {
                        neighbors[i][j] = GetPositiveMod(i + NEIGHBOR_PATTERNS[2][j], numRooms);
                    }
                }
            }
        }

        // creates caves until valid one is reached
        private void CreateCave()
        {
            while (!IsValidCave())
            {
                for (int i = 0; i < numRooms; i++) // reset adjacents
                {
                    adjacents[i] = new List<int>();
                }
                CreatePossibleCave();
            }
        }

        // breadth first search
        // returns true if all rooms are reachable from every other room, false otherwise
        private bool IsValidCave()
        {
            HashSet<int> exploredRooms = new HashSet<int>();
            Queue<int> roomsToCheck = new Queue<int>();
            exploredRooms.Add(0); // start at arbitrary room
            roomsToCheck.Enqueue(0);

            while (roomsToCheck.Count > 0) // while queue not empty
            {
                int room = roomsToCheck.Dequeue();
                foreach (int adjacent in adjacents[room]) // find all adjacent rooms
                {
                    if (!exploredRooms.Contains(adjacent))
                    {
                        exploredRooms.Add(adjacent);
                        roomsToCheck.Enqueue(adjacent); // add unexplored adjacent rooms to queue
                    }
                }
            }

            return exploredRooms.Count == numRooms; // all rooms found - is valid
        }

        // randomly generate tunnels
        private void CreatePossibleCave()
        {
            for (int i = 0; i < numRooms; i++)
            {
                List<int> options = new List<int>();
                foreach (int neighbor in neighbors[i])
                {
                    // neighbor is a valid candidate if it's not already connected to current room
                    // and has less than maxAdjacent existing connections
                    if (adjacents[neighbor].Count < maxAdjacent && !adjacents[i].Contains(neighbor))
                    {
                        options.Add(neighbor);
                    }
                }

                int numAdjacent = rand.Next(1, 1 + maxAdjacent);
                int numAdjacentToAdd = Math.Min(Math.Max(0, numAdjacent - adjacents[i].Count), options.Count); // number connections to add from options

                for (int newConnection = 0; newConnection < numAdjacentToAdd; newConnection++)
                {
                    int selection = options[rand.Next(options.Count)];
                    adjacents[i].Add(selection);
                    adjacents[selection].Add(i);
                    options.Remove(selection);
                }
            }
        }

        // accessor methods

        // get all adjacent rooms (ones connected by tunnels)
        public List<int> GetAdjacentRooms(int roomNumber)
        {
            return adjacents[roomNumber];
        }

        // get ALL neighboring rooms (even if there's no tunnel)
        public int[] GetAllNeighbors(int roomNumber)
        {
            return neighbors[roomNumber];
        }

        // get the positive modulus of first when divided by second
        private int GetPositiveMod(int first, int second)
        {
            return (first + second) % second;
        }

    }  