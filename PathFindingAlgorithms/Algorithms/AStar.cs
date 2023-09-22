using PathFindingAlgorithms.Grid;
using PathFindingAlgorithms.PriorityQueue;

namespace PathFindingAlgorithms.Algorithms
{
    public class AStar
    {
        private List<Node> FindPath(Node startNode, Node targetNode)
        {
            // Create open and closed sets to track visited nodes
            PriorityQueue<Node> openSet = new PriorityQueue<Node>(new NodeComparer());
            HashSet<Node> closedSet = new HashSet<Node>();

            startNode.gCost = 0;
            startNode.hCost = Heuristic(startNode, targetNode);

            // Add the start node to the open set
            openSet.Enqueue(startNode);

            while (openSet.Count > 0)
            {
                // Find the node with the lowest F cost in the open set
                Node currentNode = openSet.Dequeue();
                closedSet.Add(currentNode);

                // If we have reached the target node, reconstruct and return the path
                if (currentNode == targetNode)
                {
                    return ReconstructPath(startNode, targetNode);
                }

                // Explore the neighbors of the current node
                foreach (var neighbour in currentNode.neighbours)
                {
                    // Skip neighbors that are already in the closed set
                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    // Calculate the G cost of reaching the neighbor from the current node
                    double newGCost = currentNode.gCost + Cost(currentNode, neighbour);

                    // If the new G cost is lower than the neighbor's G cost or the neighbor is not in the open set
                    if (openSet.Contains(neighbour))
                    {
                        if (newGCost < neighbour.gCost)
                        {
                            // Update the neighbor's G cost
                            neighbour.gCost = newGCost;
                            openSet.UpdatePriority(neighbour);

                            // Set the neighbor's parent to the current node
                            neighbour.parent = currentNode;
                        }
                    }
                    else
                    {
                        // Add the neighbor to the open set
                        neighbour.hCost = Heuristic(neighbour, targetNode);
                        neighbour.gCost = newGCost;
                        neighbour.parent = currentNode;
                        openSet.Enqueue(neighbour);
                    }
                }
            }

            Console.WriteLine("No path ");
            return new List<Node>();
        }

        // Manhattan heuristic
        private double Heuristic(Node from, Node to)
        {
            return Math.Abs(from.position.X - to.position.X) + Math.Abs(from.position.Y - to.position.Y);
        }

        private double Cost(Node node1, Node node2)
        {
            if (node1.isObstacle || node2.isObstacle) return double.PositiveInfinity;
            return Heuristic(node1, node2); // Calculated same way using Manhattan distance
        }

        private List<Node> ReconstructPath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            // Traverse the path in reverse from the end node to the start node
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            // Reverse the path to get the correct order
            path.Reverse();

            return path;
        }
        private Node NextStep(List<Node> path)
        {
            Node next = path[0];
            path.RemoveAt(0);
            return next;
        }

        public void Main(Node start, Node goal, DoorStates doorStates)
        {
            bool gridHasChanged = false;
            List<Node> path = FindPath(start, goal);
            int changeGridTimer = 0;
            while (start != goal)
            {
                start = NextStep(path);
                if (start.isObstacle)
                {
                    Console.WriteLine("Stepped on an obstacle " + start.name);
                }
                if (path.Count > 2 && path[0] == path[2])
                {
                    Console.WriteLine("Loop in path-----------------------------------------------------");
                }
                if (changeGridTimer >= 20)
                {
                    gridHasChanged = true;
                    changeGridTimer = 0;
                }
                else
                {
                    gridHasChanged = true;
                    changeGridTimer++;
                }

                if (gridHasChanged)
                {
                    List<Node> newPath = FindPath(start, goal);
                    path = newPath;
                }
            }
        }
    }
}
