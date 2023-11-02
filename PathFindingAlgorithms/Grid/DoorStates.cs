using Newtonsoft.Json;

namespace PathFindingAlgorithms.Grid
{
    public  class DoorStates
    {
        public List<Door> changedDoors;
        public List<ObstacleBlock> changedBlocks;

        public List<Door> doors;
        List<ObstacleBlock> obstacleBlocks;
        Dictionary<string, List<bool>> doorStates;
        string gridType;

        int statePosition = 0;

        public DoorStates(List<Door> doors, string gridType)
        {
            this.doors = doors;
            doorStates = new Dictionary<string, List<bool>>();
            changedDoors = new List<Door>();
            changedBlocks = new List<ObstacleBlock>();
            this.gridType = gridType;
        }
        public DoorStates(List<ObstacleBlock> obstacleBlocks, string gridType)
        {
            this.obstacleBlocks = obstacleBlocks;
            doorStates = new Dictionary<string, List<bool>>();
            changedDoors = new List<Door>();
            changedBlocks = new List<ObstacleBlock>();
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

        public void RecordDynamicDoorStatesBlocks(int count, List<ObstacleBlock> obstacleBlocks, string filePath, int changeVolume)
        {
            this.obstacleBlocks = obstacleBlocks;
            Random random = new Random();
            int ratioOfClosed = 80;
            int ratioOfOpened = 20;

            // Set up the inital states of the doors
            foreach (ObstacleBlock obstacleBlock in obstacleBlocks)
            {
                if (random.Next(100) < ratioOfOpened)
                {
                    obstacleBlock.ChangeState();
                }
            }
            RecordDoorStatesBlocks(filePath, obstacleBlocks);

            // Add the dynamic changes
            while (count > 0)
            {
                foreach (ObstacleBlock obstacleBlock in obstacleBlocks)
                {
                    if (obstacleBlock.isObstacle() && (random.Next(10000) < ((changeVolume / 2.0) / ratioOfClosed) * 10000))
                    {
                        obstacleBlock.ChangeState();
                    }
                    else if (!obstacleBlock.isObstacle() && (random.Next(10000) < ((changeVolume / 2.0) / ratioOfOpened) * 10000))
                    {
                        obstacleBlock.ChangeState();
                    }
                }
                RecordDoorStatesBlocks(filePath, obstacleBlocks);
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
        private void RecordDoorStatesBlocks(string filePath, List<ObstacleBlock> blocks)
        {
            foreach (var obstacleBlock in blocks)
            {
                string blockName = obstacleBlock.name;
                if (!doorStates.ContainsKey(blockName))
                    doorStates[blockName] = new List<bool> { obstacleBlock.isObstacle() };
                else
                    doorStates[blockName].Add(obstacleBlock.isObstacle());
            }
        }

        private void DoorStatesToJson(string filePath)
        {
            DoorStatesDataModel dataModel = new DoorStatesDataModel
            {
                doorStates = doorStates
            };

            string json = JsonConvert.SerializeObject(dataModel, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void JsonToDoorStates(string filePath)
        {
            string json = File.ReadAllText(filePath);
            DoorStatesDataModel? dataModel = JsonConvert.DeserializeObject<DoorStatesDataModel>(json);
            if (dataModel != null)
            {
                doorStates = dataModel.doorStates;
                LoadNextDoorStates();
            }
        }

        public void LoadNextDoorStates()
        {
            if (gridType == "Rooms") LoadNextDoorStatesRooms();
            if (gridType == "Random") LoadNextDoorStatesBlocks();
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
            changedBlocks.Clear();
            foreach (var doorState in doorStates)
            {
                ObstacleBlock? foundBlock = obstacleBlocks.FirstOrDefault(obj => obj.name == doorState.Key);

                if (foundBlock != null)
                {
                    // If door's state gets changed
                    if (foundBlock.isObstacle() != doorState.Value[statePosition])
                    {
                        foundBlock.ChangeState();

                        changedBlocks.Add(foundBlock);
                    }
                }
            }
            if (statePosition < doorStates["ObstacleBlock0"].Count - 1)
                statePosition++;
            else statePosition = 0;
        }
    }
}
