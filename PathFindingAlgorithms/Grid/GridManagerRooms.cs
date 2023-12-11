using System.Numerics;

namespace PathFindingAlgorithms.Grid
{
    public class GridManagerRooms : GridManager
    {
        const bool diagonalAllowed = false;
        const int roomSize = 16;
        int roomCount;

        public Dictionary<Vector2, Node> nodes { get; private set; }
        public List<Door> doors { get; private set; }
        public List<Room> rooms { get; private set; }
        public DoorStates doorStates { get; private set; }

        int doorCounter;

        public GridManagerRooms(int gridSize) 
        {
            roomCount = gridSize / roomSize;
            nodes = new Dictionary<Vector2, Node>();
            doors = new List<Door>();
            rooms = new List<Room>();
            doorStates = new DoorStates(doors, "Rooms");
        }

        public void GenerateGrid(string mapFilePath)
        {
            doorCounter = 0;

            int x = 0;
            int y = 0;

            // Create a StreamReader instance to read the file
            using (StreamReader reader = new StreamReader(mapFilePath))
            {
                int character;

                // Read and process characters from the file until the end
                while ((character = reader.Read()) != -1)
                {
                    char c = (char)character;

                    switch (c)
                    {
                        case '.':
                            CreateNode(x, y);
                            x++;
                            break;
                        case '@':
                            CreateNode(x, y).isObstacle = true;
                            x++;
                            break;
                        case 'D':
                            CreateDoor(x, y);
                            x++;
                            break;
                        case '\n':
                            y++;
                            x = 0;
                            break;
                    }
                }
            }
            x--;

            SetNeighbours();
            SetRooms();
            SetRoomNeighbours();
            SetRoomDoors();
        }

        Node CreateNode(int x, int y)
        {
            Node spawnedNode = new Node();
            string nodeName = $"Node {x} {y}";

            Vector2 position = new Vector2(x, y);
            nodes[position] = spawnedNode;

            spawnedNode.Init(position, nodeName);

            return spawnedNode;
        }

        Node CreateDoor(int x, int y)
        {
            Door spawnedNode = new Door();
            String doorName = $"Door {doorCounter}";
            doorCounter++;

            Vector2 position = new Vector2(x, y);
            nodes[position] = spawnedNode;
            doors.Add(spawnedNode);

            spawnedNode.Init(position, doorName);

            // Start with every door as closed
            spawnedNode.isObstacle = true;

            return spawnedNode;
        }

        void SetNeighbours()
        {
            foreach (var node in nodes)
            {
                Node nodeLeft = GetNodeAtPosition(new Vector2(node.Key.X - 1, node.Key.Y));
                Node nodeRight = GetNodeAtPosition(new Vector2(node.Key.X + 1, node.Key.Y));
                Node nodeUp = GetNodeAtPosition(new Vector2(node.Key.X, node.Key.Y + 1));
                Node nodeDown = GetNodeAtPosition(new Vector2(node.Key.X, node.Key.Y - 1));


                node.Value.AddNeighbour(nodeLeft);
                node.Value.AddNeighbour(nodeRight);
                node.Value.AddNeighbour(nodeUp);
                node.Value.AddNeighbour(nodeDown);


                if (diagonalAllowed)
                {
                    // Add diagonals
                    node.Value.AddNeighbour(GetNodeAtPosition(new Vector2(node.Key.X + 1, node.Key.Y + 1)));
                    node.Value.AddNeighbour(GetNodeAtPosition(new Vector2(node.Key.X + 1, node.Key.Y - 1)));
                    node.Value.AddNeighbour(GetNodeAtPosition(new Vector2(node.Key.X - 1, node.Key.Y + 1)));
                    node.Value.AddNeighbour(GetNodeAtPosition(new Vector2(node.Key.X - 1, node.Key.Y - 1)));
                }
            }
        }

        void SetRooms()
        {
            for (int i = 1; i <= roomCount; i++)
            {
                for (int j = 1; j <= roomCount; j++)
                {
                    Room newRoom = new Room();
                    newRoom.hCost = (roomCount - i) + (roomCount - j);
                    newRoom.roomNumber = (i - 1) * roomCount + j - 1;
                    newRoom.visited = false;
                    rooms.Add(newRoom);
                }
            }
        }

        void SetRoomNeighbours()
        {
            int counter = 0;
            foreach (Room room in rooms)
            {
                if (counter % roomCount != 0)
                {
                    room.leftNeighbour = rooms[counter - 1];
                }
                if (counter % (roomCount) != roomCount - 1)
                {
                    room.rightNeighbour = rooms[counter + 1];
                }
                if (counter >= roomCount)
                {
                    room.downNeighbour = rooms[counter - roomCount];
                }
                if (counter < roomCount * roomCount - roomCount)
                {
                    room.upNeighbour = rooms[counter + roomCount];
                }
                counter++;
            }
        }

        void SetRoomDoors()
        {
            int roomCounter = 0;
            foreach (Room room in rooms)
            {
                foreach (Door door in doors)
                {
                    if (door.position.X == ((roomCounter + 1) % roomCount) * roomSize
                        && door.position.Y > (roomCounter / roomCount) * roomSize
                        && door.position.Y < (roomCounter / roomCount) * roomSize + roomSize)
                    {
                        room.rightDoor = door;
                    }
                    if (door.position.X == (roomCounter % roomCount) * roomSize
                        && door.position.Y > (roomCounter / roomCount) * roomSize
                        && door.position.Y < (roomCounter / roomCount) * roomSize + roomSize)
                    {
                        room.leftDoor = door;
                    }
                    if (door.position.Y == ((roomCounter / roomCount) + 1) * roomSize
                        && door.position.X > (roomCounter % roomCount) * roomSize
                        && door.position.X < (roomCounter % roomCount) * roomSize + roomSize)
                    {
                        room.upDoor = door;
                    }
                    if (door.position.Y == ((roomCounter / roomCount)) * roomSize
                        && door.position.X > (roomCounter % roomCount) * roomSize
                        && door.position.X < (roomCounter % roomCount) * roomSize + roomSize)
                    {
                        room.downDoor = door;
                    }
                }
                roomCounter++;
            }
        }

        
        public void RecordDoorStates(int count, string filePath, int changeVolume)
        {
            // Comment out to not accidently record new doorstates.
            doorStates.RecordDynamicDoorStatesRooms(count, filePath, changeVolume, rooms);
        }

        public void JsonToDoorStates(string filePath)
        {
            doorStates.JsonToDoorStates(filePath);
        }

        public Node GetNodeAtPosition(Vector2 pos)
        {
            if (nodes.TryGetValue(pos, out var node)) return node;
            return null;
        }

        public void ResetGrid()
        {
            Parallel.ForEach(nodes.Values, node =>
            {
                node.Reset();
                if (node.GetType() == typeof(Door)) node.isObstacle = false;
            });
            doorStates = new DoorStates(doors, "Rooms");
        }
    }
}