using Newtonsoft.Json;

namespace PathFindingAlgorithms.Grid
{
    public  class DoorStates
    {
        public List<Door> changedDoors;

        public List<Door> doors;
        HashSet<ObstacleBlock> obstacleBlocks;
        Dictionary<string, List<bool>> doorStates;
        Dictionary<int, List<string>> doorStatesAlt;
        string gridType;

        int statePosition = 0;

        public DoorStates(List<Door> doors, string gridType)
        {
            this.doors = doors;
            doorStates = new Dictionary<string, List<bool>>();
            changedDoors = new List<Door>();
            this.gridType = gridType;
        }
        public DoorStates(HashSet<ObstacleBlock> obstacleBlocks, string gridType)
        {
            this.obstacleBlocks = obstacleBlocks;
            doorStates = new Dictionary<string, List<bool>>();
            doorStatesAlt = new Dictionary<int, List<string>>();
            changedDoors = new List<Door>();
            this.gridType = gridType;
        }

        public void RecordDynamicDoorStatesRooms(int count, string filePath, int changeVolume, List<Room> rooms)
        {
            RandomizedDFS rdfs = new RandomizedDFS();
            Random random = new Random();

            int ratioOfClosed = 80;
            int ratioOfOpened = 20;

            CloseDoors();
            ResetRooms(rooms);
            rdfs.CreateMaze(rooms);

            List<Door> dynamicDoors = new List<Door>();
            foreach (Door door in doors)
            {
                if (door.isObstacle) dynamicDoors.Add(door);
            }

            // Set up the inital states of the doors
            foreach (Door door in dynamicDoors)
            {
                if (random.Next(100) < ratioOfOpened)
                {
                    door.Open();
                }
            }
            RecordDoorStatesRooms(filePath);

            // Add the dynamic changes
            while (count > 0)
            {
                foreach (Door door in dynamicDoors)
                {
                    if (door.isObstacle && (random.Next(10000) < ((changeVolume / 2.0)/ratioOfClosed) * 10000)) {
                        door.Open();
                    }
                    else if (!door.isObstacle && (random.Next(10000) < ((changeVolume / 2.0)/ratioOfOpened) * 10000)) {
                        door.Close();
                    }
                }
                RecordDoorStatesRooms(filePath);
                count--;
            }

            DoorStatesToJson(filePath);
        }

        public void RecordDynamicDoorStatesBlocks(int count, string filePath, int changeVolume)
        {
            Random random = new Random();
            int initialCount = count;
            int ratioOfClosed = 80;
            int ratioOfOpened = 20;

            // Add the dynamic changes
            while (count > 0)
            {
                List<ObstacleBlock> changingBlocks = new List<ObstacleBlock>();
                foreach (ObstacleBlock obstacleBlock in obstacleBlocks)
                {
                    if (obstacleBlock.isObstacle() && (random.Next(10000) < ((changeVolume / 2.0) / ratioOfClosed) * 10000))
                    {
                        obstacleBlock.ChangeState();
                        changingBlocks.Add(obstacleBlock);
                    }
                    else if (!obstacleBlock.isObstacle() && (random.Next(10000) < ((changeVolume / 2.0) / ratioOfOpened) * 10000))
                    {
                        obstacleBlock.ChangeState();
                        changingBlocks.Add(obstacleBlock);
                    }
                }
                //RecordDoorStatesBlocks(filePath);
                RecordDoorStatesBlocks(filePath, changingBlocks, initialCount - count);
                count--;
            }

            DoorStatesToJson(filePath);
        }

        private void CloseDoors()
        {
            foreach (Door door in doors) door.Close();
        }

        private void ResetRooms(List<Room> rooms)
        {
            foreach (Room room in rooms) room.visited = false;
        }

        private void RecordDoorStatesRooms(string filePath)
        {
            foreach (var door in doors)
            {
                string doorName = door.name;
                if (!doorStates.ContainsKey(doorName))
                    doorStates[doorName] = new List<bool> { door.isObstacle };
                else
                    doorStates[doorName].Add(door.isObstacle);
            }
        }
        //private void RecordDoorStatesBlocks(string filePath)
        //{
        //    foreach (var obstacleBlock in obstacleBlocks)
        //    {
        //        string blockName = obstacleBlock.name;
        //        if (!doorStates.ContainsKey(blockName))
        //            doorStates[blockName] = new List<bool> { obstacleBlock.isObstacle() };
        //        else
        //            doorStates[blockName].Add(obstacleBlock.isObstacle());
        //    }
        //}

        private void RecordDoorStatesBlocks(string filePath, List<ObstacleBlock> changedBlocks, int currentCount)
        {
            doorStatesAlt[currentCount] = new List<string>();
            foreach (var obstacleBlock in changedBlocks)
            {
                doorStatesAlt[currentCount].Add(obstacleBlock.name);
            }
        }

        private void DoorStatesToJson(string filePath)
        {
            DoorStatesDataModel dataModel = new DoorStatesDataModel
            {
                doorStatesAlt = doorStatesAlt
            };

            string json = JsonConvert.SerializeObject(dataModel, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void JsonToDoorStates(string filePath)
        {
            using (StreamReader file = File.OpenText(filePath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                DoorStatesDataModel? dataModel = serializer.Deserialize<DoorStatesDataModel>(reader);
                if (dataModel != null)
                {
                    doorStatesAlt = dataModel.doorStatesAlt;
                    LoadNextDoorStates();
                }
            }
        }

        public void LoadNextDoorStates()
        {
            if (gridType == "Rooms") LoadNextDoorStatesRooms();
            else if (gridType == "Random") LoadNextDoorStatesBlocks();
            else throw new Exception("No correct grid type when loading next doorstates");
        }

        private void LoadNextDoorStatesRooms()
        {
            changedDoors.Clear();
            foreach (var doorState in doorStates)
            {
                Door? foundDoor = doors.FirstOrDefault(obj => obj.name == doorState.Key);

                if (foundDoor != null)
                {
                    // If door's state gets changed
                    if (foundDoor.isObstacle != doorState.Value[statePosition])
                    {
                        foundDoor.Change();

                        changedDoors.Add(foundDoor);
                    }
                }

            }
            if (statePosition < doorStates["Door 0"].Count - 1)
                statePosition++;
            else statePosition = 0;
        }

        private void LoadNextDoorStatesBlocks()
        {
            changedDoors.Clear();

            List<string> doorState = doorStatesAlt[statePosition];
            var allChangedBlocks = obstacleBlocks.Where(obj => doorState.Contains(obj.name));

            Parallel.ForEach(allChangedBlocks, block =>
            {
                foreach (Door obstacle in block.obstacles)
                {
                    obstacle.Change();
                    lock (changedDoors)
                    {
                        changedDoors.Add(obstacle);
                    }
                }
            });

            if (statePosition < doorStatesAlt.Count - 1)
                statePosition++; 
            else
                statePosition = 0;
        }
    }
}
