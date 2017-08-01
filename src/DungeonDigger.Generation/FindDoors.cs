using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDigger.Generation
{
    class FindDoors
    {
        private Tile[,] Map;
        private Dictionary<int, HashSet<Tuple<int,int>>> RoomHashMap; //dictionary with integers to list rooms 1,2,3... and two integers to list where it is in map [1,1],[1,2]...
        private Dictionary<Tuple<int, int>,int> DoorCandidates;
        private Random rnd;
        public FindDoors(Tile[,] inputMap,Random rndseed)
        {
            int h = inputMap.GetLength(0);
            int w = inputMap.GetLength(1);
            RoomHashMap = new Dictionary<int, HashSet<Tuple<int,int>>>();
            //import same Random seed used in room generator
            rnd = rndseed;
            Map = new Tile[h,w];
            for (int i = 0; i < h; i++)
            {
                for (int k = 0; k < w; k++)
                {
                    Map[i, k] = inputMap[i, k];
                }
            }

            IdentifyRooms(h,w);
            SelectDoors();
        }   

        private void IdentifyRooms(int h, int w)
        {
            var coordIsPartOfRoom = false;
            var coordToCheck = new Tuple<int, int>(99, 99);
            var nextRoomNumber = 1;
            for (int i = 0; i < h; i++)
            {
                for (int k = 0; k < w; k++)
                {
                    //go through all squares
                    if (Map[i,k] == Tile.Room)
                    {
                        coordIsPartOfRoom = false; //reset to false default
                        //square is room
                        coordToCheck = new Tuple<int, int>(i, k);
                        foreach (var roomCoordPair in RoomHashMap.Values)
                        {
                            if (roomCoordPair.Contains(coordToCheck))
                            {
                                coordIsPartOfRoom = true;
                                break;
                                //square is already inside hashmap
                            }
                        }

                        if (!coordIsPartOfRoom)
                        {
                            //square was not found in room
                            HashSet<Tuple<int, int>> set = new HashSet<Tuple<int, int>>();
                            Search(i, k, set);
                            RoomHashMap.Add(nextRoomNumber, set);
                            nextRoomNumber++;
                            //add a room and add this square to it and search for all other squares close to it and add them
                        }
                    }
                }
            }
        }

        private void Search(int h, int w, HashSet<Tuple<int, int>> set)
        {
            var right = false;
            var left = false;
            var top = false;
            var bottom = false;


            if (set.Contains(new Tuple<int,int>(h,w+1)))
            {
                right = true;
            }
            if (set.Contains(new Tuple<int, int>(h, w-1)))
            {
                left = true;
            }
            if (set.Contains(new Tuple<int, int>(h+1, w)))
            {
                bottom = true;
            }
            if (set.Contains(new Tuple<int, int>(h-1, w)))
            {
                top = true;
            }

            if (w == Map.GetLength(1) - 1)
            {
                right = true;
            }
            if (w == 0)
            {
                left = true;
            }
            if (h == Map.GetLength(0) - 1)
            {
                bottom = true;
            }
            if (h == 0)
            {
                top = true;
            }

            if (Map[h,w+1] == Tile.Room && !right)
            {
                set.Add(new Tuple<int, int>(h, w + 1));
                Search(h, w + 1, set);
            }
            if (Map[h, w-1] == Tile.Room && !left)
            {
                set.Add(new Tuple<int, int>(h, w-1));
                Search(h, w-1, set);
            }
            if (Map[h+1, w] == Tile.Room && !bottom)
            {
                set.Add(new Tuple<int, int>(h+1, w));
                Search(h+1, w, set);
            }
            if (Map[h-1, w] == Tile.Room && !top)
            {
                set.Add(new Tuple<int, int>(h-1, w));
                Search(h-1, w, set);
            }
        }


        private void SelectDoors()
        {
            int NumberOfDoors = 0;
            List<Tuple<int, int>> Keys;
            int PotentialDoors = 0;
            int SelectedDoor = 0;
            Tuple<int, int> SelectedDoorCoord;
            Dictionary<Tuple<int, int>, int> DoorCandidates;
            List<Tuple<int, int>> CornerKeys;

            for (int i = 1; i < RoomHashMap.Count; i++)
            {
                //go through all the rooms
                DoorCandidates = new Dictionary<Tuple<int, int> ,int>();
                foreach (var coordpoint in RoomHashMap[i])
                {
                    //go through each tile in room
                    if (Map[coordpoint.Item1-1,coordpoint.Item2] == Tile.Unassigned && coordpoint.Item1-1 != 0)
                    {
                        //add 1 to value of item above unless it's the top of the array
                        addOrUpdate(DoorCandidates, new Tuple<int, int>(coordpoint.Item1 - 1, coordpoint.Item2), 1);
                    }
                    if (Map[coordpoint.Item1+1,coordpoint.Item2] == Tile.Unassigned && coordpoint.Item1+1 != Map.GetLength(0)-1)
                    {
                        //add 1 to value of item bellow unless it's the bottom of the array
                        addOrUpdate(DoorCandidates, new Tuple<int, int>(coordpoint.Item1 + 1, coordpoint.Item2), 1);
                    }
                    if (Map[coordpoint.Item1,coordpoint.Item2-1] == Tile.Unassigned && coordpoint.Item2-1 != 0)
                    {
                        //add 1 to value of item to the left unless it's the first column of the array
                        addOrUpdate(DoorCandidates, new Tuple<int, int>(coordpoint.Item1, coordpoint.Item2 - 1), 1);
                    }
                    if (Map[coordpoint.Item1,coordpoint.Item2+1] == Tile.Unassigned && coordpoint.Item2+1 !=Map.GetLength(1)-1)
                    {
                        //add 1 to the value of item to the right unless it's the last column of the array
                        addOrUpdate(DoorCandidates, new Tuple<int, int>(coordpoint.Item1, coordpoint.Item2 + 1), 1);
                    }
                }

                CornerKeys = new List<Tuple<int, int>>();
                //remove corners where you don't want doors
                foreach (var PotentialDoor in DoorCandidates)
                {
                    if (PotentialDoor.Value > 1)
                    {
                        CornerKeys.Add(PotentialDoor.Key);
                    }
                }
                for (int l = 0; l < CornerKeys.Count; l++)
                {
                    DoorCandidates.Remove(CornerKeys[l]);
                }

                //calculate doors needed
                PotentialDoors = DoorCandidates.Count;
                NumberOfDoors = (int)Math.Round(PotentialDoors / (double)(10 + RoomHashMap[i].Count/10));
                //Add doors randomly and remove the spots that become ineligable because of it until we have enough doors
                while (NumberOfDoors > 0)
                {
                    Keys = new List<Tuple<int, int>>(DoorCandidates.Keys);
                    SelectedDoor = rnd.Next(0, PotentialDoors);
                    SelectedDoorCoord = Keys[SelectedDoor];

                    Map[SelectedDoorCoord.Item1, SelectedDoorCoord.Item2] = Tile.DoorOpen;

                    DoorCandidates.Remove(SelectedDoorCoord);
                    DoorCandidates.Remove(new Tuple<int, int>(SelectedDoorCoord.Item1 - 1, SelectedDoorCoord.Item2));
                    DoorCandidates.Remove(new Tuple<int, int>(SelectedDoorCoord.Item1 + 1, SelectedDoorCoord.Item2));
                    DoorCandidates.Remove(new Tuple<int, int>(SelectedDoorCoord.Item1, SelectedDoorCoord.Item2 - 1));
                    DoorCandidates.Remove(new Tuple<int, int>(SelectedDoorCoord.Item1, SelectedDoorCoord.Item2 + 1));

                    NumberOfDoors--;
                    PotentialDoors = DoorCandidates.Count;
                }
            }
        }

        private void addOrUpdate(Dictionary<Tuple<int,int>, int> dic, Tuple<int, int> key, int newValue)
        {
            int val;
            if (dic.TryGetValue(key, out val))
            {
                // yay, value exists!
                dic[key] = val + newValue;
            }
            else
            {
                // darn, lets add the value
                dic.Add(key, newValue);
            }
        }

        public Tile[,] GetMapWithDoor()
        {
            return Map;
        }
    }
}
