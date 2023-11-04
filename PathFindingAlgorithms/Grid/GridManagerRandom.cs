using System.Numerics;

namespace PathFindingAlgorithms.Grid
{
    public class GridManagerRandom : GridManager
    {
        const bool diagonalAllowed = false;

        public Dictionary<Vector2, Node> nodes { get; private set; }
        public List<Door> doors { get; private set; }
        public  HashSet<ObstacleBlock> obstacleBlocks { get; private set; }
        public DoorStates doorStates { get; private set; }

        int doorCounter;

        public GridManagerRandom(int gridSize)
        {
            nodes = new Dictionary<Vector2, Node>();
            doors = new List<Door>();
            obstacleBlocks = new HashSet<ObstacleBlock>();
            doorStates = new DoorStates(obstacleBlocks, "Random");
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
            GetConnectedObstacles(doors);
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

        void GetConnectedObstacles(List<Door> obstacles)
        {
            List<ObstacleBlock> connectedObstacles = new List<ObstacleBlock>();
            HashSet<Door> visited = new HashSet<Door>();
            int blockCount = 0;

            foreach (Door obstacle in obstacles)
            {
                if (!visited.Contains(obstacle))
                {
                    ObstacleBlock obstacleBlock = new ObstacleBlock(blockCount++);
                    Queue<Door> queue = new Queue<Door>();
                    queue.Enqueue(obstacle);
                    visited.Add(obstacle);

                    while (queue.Count > 0)
                    {
                        Door current = queue.Dequeue();
                        obstacleBlock.Add(current);

                        // Add to the queue if neighbour is an obstacle and has not been visited
                        foreach (Node neighbor in current.neighbours)
                        {
                            if (neighbor.GetType() == typeof(Door) && !visited.Contains(neighbor))
                            {
                                queue.Enqueue((Door)neighbor);
                                visited.Add((Door)neighbor);
                            }
                        }
                    }

                    obstacleBlocks.Add(obstacleBlock);
                }
            }
        }

        public void RecordDoorStates(int count, string filePath, int changeVolume)
        {
            doorStates.RecordDynamicDoorStatesBlocks(count, filePath, changeVolume);
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
    }
}