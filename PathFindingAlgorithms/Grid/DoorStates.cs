using Newtonsoft.Json;

namespace PathFindingAlgorithms.Grid
{
    public  class DoorStates
    {
        public List<Door> changedDoors;

        public List<Door> doors;
        List<Room> rooms;
        Dictionary<string, List<bool>> doorStates;

        int statePosition = 0;

        public DoorStates(List<Door> doors, List<Room> rooms)
        {
            this.doors = doors;
            this.rooms = rooms;
            doorStates = new Dictionary<string, List<bool>>();
            changedDoors = new List<Door>();
        }

        public void RecordDynamicDoorStates(int count, string filePath, int changeSize)
        {
            RandomizedDFS rdfs = new RandomizedDFS();
            Random random = new Random();

            int ratioOfClosed = 80;
            int ratioOfOpened = 20;

            CloseDoors();
            ResetRooms();
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
            RecordDoorStates(filePath);

            // Add the dynamic changes
            while (count > 0)
            {
                foreach (Door door in dynamicDoors)
                {
                    if (door.isObstacle && (random.Next(10000) < ((changeSize/2.0)/ratioOfClosed) * 10000)) {
                        door.Open();
                    }
                    else if (!door.isObstacle && (random.Next(10000) < ((changeSize/2.0)/ratioOfOpened) * 10000)) {
                        door.Close();
                    }
                }
                RecordDoorStates(filePath);
                count--;
            }

            DoorStatesToJson(filePath);
        }

        private void CloseDoors()
        {
            foreach (Door door in doors) door.Close();
        }

        private void ResetRooms()
        {
            foreach (Room room in rooms) room.visited = false;
        }

        private void RecordDoorStates(string filePath)
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
            changedDoors.Clear();
            foreach (var doorState in doorStates)
            {
                Door? foundDoor = doors.FirstOrDefault(obj => obj.name == doorState.Key);

                if (foundDoor != null)
                {
                    // If door's state gets changed
                    if (foundDoor.isObstacle != doorState.Value[statePosition])
                    {
                        if (doorState.Value[statePosition])
                            foundDoor.Close();
                        else foundDoor.Open();

                        changedDoors.Add(foundDoor);
                    }
                }

            }
            if (statePosition < doorStates["Door 0"].Count - 1)
                statePosition++;
            else statePosition = 0;
        }
    }
}
