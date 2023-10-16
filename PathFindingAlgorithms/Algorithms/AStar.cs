using PathFindingAlgorithms.Grid;
using PathFindingAlgorithms.PriorityQueue;
using System.Diagnostics;

namespace PathFindingAlgorithms.Algorithms
{
    public class AStar
    {
        private int expadedNodes;

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
                expadedNodes++;

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
                    // Skip neighbors that are already in the closed set or obstacles
                    if (closedSet.Contains(neighbour) || neighbour.isObstacle)
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

            // No path
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

        public string[] Main(Node start, Node goal, DoorStates doorStates)
        {
            Stopwatch swTotal = Stopwatch.StartNew();
            TimeSpan elapsedTotal = TimeSpan.Zero;
            TimeSpan totalReCompute = TimeSpan.Zero;
            int pathLength = 0;
            int reComputeTimer = 0;
            expadedNodes = 0;

            bool gridChange = false;

            List<Node> path = FindPath(start, goal);

            int gridChangeTimer = 0;
            while (start != goal)
            {
                start = NextStep(path);
                pathLength++;

                // Change the grid every 10 steps
                if (start.GetType() != typeof(Door) && gridChangeTimer >= 10)
                {
                    // Let's not include the time it takes to load the doors as it is not part of the algorithm
                    swTotal.Stop();
                    elapsedTotal += swTotal.Elapsed;

                    doorStates.LoadNextDoorStates();
                    gridChange = true;
                    gridChangeTimer = 0;

                    swTotal.Restart();
                }
                else
                {
                    gridChange = false;
                    gridChangeTimer++;
                }

                if (gridChange)
                {
                    Stopwatch reComputeSW = Stopwatch.StartNew();

                    path = FindPath(start, goal);

                    reComputeSW.Stop();
                    totalReCompute += reComputeSW.Elapsed;
                    reComputeTimer++;
                }
            }
            swTotal.Stop();
            elapsedTotal += swTotal.Elapsed;
            string totalTime = elapsedTotal.TotalSeconds.ToString();

            return new string[] { totalTime, totalReCompute.TotalMilliseconds.ToString(),
                reComputeTimer.ToString() ,pathLength.ToString(), expadedNodes.ToString() };

        }
    }
}
