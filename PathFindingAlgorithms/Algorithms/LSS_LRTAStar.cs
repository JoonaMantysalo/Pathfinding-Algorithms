using PathFindingAlgorithms.PriorityQueue;

namespace PathFindingAlgorithms.Algorithms
{

    public class LSS_LRTAStar
    {
        PriorityQueue<Node> openSet;
        PriorityQueue<Node> openSetHCosts;
        HashSet<Node> closedSet;

        public Node FindPath(Node start, Node goal, int lookahead)
        {
            AStar(start, goal, lookahead);
            if (openSet.Count == 0) return null;

            Node localGoal = openSet.Peek();
            Dijkstra();

            if (localGoal != start) return NextStep(start, localGoal);
            else return NextStep(start, goal);
        }

        private void AStar(Node start, Node goal, int lookahead)
        {
            start.gCostLSS_LRTA = 0;
            if (!start.hCostAssigned)
            {
                start.hCost = Heuristic(start, goal);
                start.hCostAssigned = true;
            }

            // Create open and closed sets to track visited nodes
            openSet = new PriorityQueue<Node>(new NodeComparerLSS_LRTAStar());
            openSetHCosts = new PriorityQueue<Node>(new NodeComparerHCost());
            closedSet = new HashSet<Node>();

            openSet.Enqueue(start);
            openSetHCosts.Enqueue(start);
            int expansions = 0;

            while (goal.gCostLSS_LRTA > openSet.Peek().fCostLSS_LRTA && expansions < lookahead)
            {
                expansions++;

                Node current = openSet.Dequeue();
                openSetHCosts.Remove(current);
                closedSet.Add(current);

                // Stop if we have reached the goal
                if (current == goal)
                {
                    return;
                }

                foreach (Node neighbour in current.neighbours)
                {
                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    // Calculate the G cost of reaching the neighbor from the current node
                    double newGCost = current.gCostLSS_LRTA + Cost(current, neighbour);

                    // If the new G cost is lower than the neighbor's G cost or the neighbor is not in the open set
                    if (openSet.Contains(neighbour))
                    {
                        if (newGCost < neighbour.gCostLSS_LRTA)
                        {
                            // Update the neighbor's G cost
                            neighbour.gCostLSS_LRTA = newGCost;
                            openSet.UpdatePriority(neighbour);

                            // Set the neighbor's parent to the current node
                            neighbour.parent = current;
                        }
                    }
                    else
                    {
                        if (!neighbour.hCostAssigned)
                        {
                            neighbour.hCost = Heuristic(neighbour, goal);
                            neighbour.hCostAssigned = true;
                        }
                        neighbour.gCostLSS_LRTA = newGCost;
                        neighbour.parent = current;
                        openSet.Enqueue(neighbour);
                        openSetHCosts.Enqueue(neighbour);
                    }
                }
            }
            if (openSetHCosts.Count != openSet.Count)
                Console.WriteLine("OpenSet not correct");
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

        private void Dijkstra()
        {
            foreach (Node node in closedSet)
            {
                node.hCost = double.PositiveInfinity;
            }
            while (closedSet.Count > 0)
            {
                Node current = openSetHCosts.Dequeue();
                if (closedSet.Contains(current)) closedSet.Remove(current);

                foreach (Node neighbour in current.neighbours)
                {
                    if (closedSet.Contains(neighbour)
                        && neighbour.hCost > Cost(current, neighbour) + current.hCost)
                    {
                        neighbour.hCost = Cost(neighbour, current) + current.hCost;
                        if (!openSetHCosts.Contains(neighbour)) openSetHCosts.Enqueue(neighbour);
                    }
                }
            }
        }

        private Node NextStep(Node currentNode, Node endNode)
        {
            return ReconstructPath(currentNode, endNode)[0];
        }

        private List<Node> ReconstructPath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            // Traverse the path in reverse from the end node to the start node
            while (currentNode != startNode)
            {
                //currentNode.NodeOnPath();
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            // Reverse the path to get the correct order
            path.Reverse();
            return path;
        }
    }

}
