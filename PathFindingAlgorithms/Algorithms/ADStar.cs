using PathFindingAlgorithms.Grid;
using PathFindingAlgorithms.PriorityQueue;
using System.Diagnostics;

namespace PathFindingAlgorithms.Algorithms
{
    public class ADStar
    {
        Node start;
        Node goal;
        PriorityQueue<Node> openSet;
        HashSet<Node> closedSet;
        HashSet<Node> incons;
        double epsilon;

        private int expadedNodes;


        // Manhattan heuristic
        private double Heuristic(Node from, Node to)
        {
            return Math.Abs(from.position.X - to.position.X) + Math.Abs(from.position.Y - to.position.Y);
        }

        private double Cost(Node node1, Node node2)
        {
            if (node1.isObstacle || node2.isObstacle) return double.PositiveInfinity;
            return 2*Heuristic(node1, node2); // Calculated same way using Manhattan distance
        }

        double[] Key(Node s)
        {
            if (s.gCost > s.RHS)
            {
                return new double[2] { s.RHS + epsilon * Heuristic(s, start), s.RHS };
            }
            else return new double[2] { s.gCost + epsilon * Heuristic(s, start), s.gCost };
        }

        private bool CompareKey(double[] key1, double[] key2)
        {
            if (key1[0] < key2[0])
                return true;
            if (key1[0] > key2[0])
                return false;
            if (key1[1] < key2[1])
                return true;
            return false;
        }

        void UpdateState(Node s)
        {
            if (!s.hasBeenExpanded)
            {
                s.gCost = double.PositiveInfinity;
                s.hasBeenExpanded = true;
            }
            if (s != goal)
            {
                s.RHS = double.PositiveInfinity;
                foreach (Node succ in s.neighbours)
                {
                    s.RHS = Math.Min(s.RHS, Cost(s, succ) + succ.gCost);
                }
            }
            if (openSet.Contains(s))
            {
                openSet.Remove(s);
            }
            if (s.gCost != s.RHS)
            {
                if (!closedSet.Contains(s))
                {
                    s.key = Key(s);
                    openSet.Enqueue(s);
                }
                else
                {
                    incons.Add(s);
                }
            }
        }

        void ComputeorImprovePath()
        {
            while (CompareKey(Key(openSet.Peek()), Key(start)) || start.RHS != start.gCost)
            {
                expadedNodes++;

                Node s = openSet.Dequeue();
                if (s.gCost > s.RHS)
                {
                    s.gCost = s.RHS;
                    closedSet.Add(s);
                    foreach (Node pred in s.neighbours)
                    {
                        UpdateState(pred);
                    }
                }
                else
                {
                    s.gCost = double.PositiveInfinity;
                    foreach (Node pred in s.neighbours)
                    {
                        UpdateState(pred);
                    }
                    UpdateState(s);
                }
            }
        }

        private Node NextStep(Node current)
        {
            Node nextNode = null;
            double min = double.PositiveInfinity;
            foreach (Node v in current.neighbours)
            {
                double v_min = Cost(current, v) + v.gCost;
                if (min > v_min)
                {
                    nextNode = v;
                    min = v_min;
                }
            }
            if (nextNode.isObstacle) Console.WriteLine("Stepped on an obstacle----------------");
            return nextNode;
        }

        public string[] Main(Node startNode, Node goalNode, DoorStates doorStates)
        {
            Stopwatch swTotal = Stopwatch.StartNew();
            TimeSpan elapsedTotal = TimeSpan.Zero;
            TimeSpan elapsedCompute = TimeSpan.Zero;
            int pathLength = 0;
            expadedNodes = 0;

            start = startNode;
            goal = goalNode;

            goal.RHS = 0;
            epsilon = 2.5;
            openSet = new PriorityQueue<Node>(new NodeComparerKey());
            closedSet = new HashSet<Node>();
            incons = new HashSet<Node>();
            bool gridChange = false;
            int gridChangeTimer = 0;

            goal.key = Key(goal);
            openSet.Enqueue(goal);
            goal.hasBeenExpanded = true;

            Stopwatch swCompute = Stopwatch.StartNew();

            ComputeorImprovePath();

            swCompute.Stop();
            elapsedCompute += swCompute.Elapsed;

            while (start != goal)
            {
                // Currently a new path is calculated with a lower epsilon value.
                // If a path is already calculated with epsilon of 1, and no grid hasn't changed
                // no new path is calculated.
                if (epsilon > 1 || gridChange)
                {
                    swCompute.Restart();

                    if (gridChange)
                    {
                        foreach (Node changedDoor in doorStates.changedDoors)
                        {
                            UpdateState(changedDoor);
                            foreach (Node neighbour in changedDoor.neighbours)
                            {
                                UpdateState(neighbour);
                            }
                        }
                        epsilon += 1;
                    }
                    else if (epsilon > 1)
                    {
                        epsilon -= 0.25;
                    }

                    foreach (Node node in incons) // Add incons to openset
                    {
                        node.key = Key(node);
                        openSet.Enqueue(node);
                    }
                    foreach (Node node in openSet.itemIndices.Keys.ToList()) // Update the keys and priorities of nodes in openset 
                    {
                        node.key = Key(node);
                        openSet.UpdatePriority(node);
                    }
                    
                    incons.Clear();
                    closedSet.Clear();
                    
                    ComputeorImprovePath();

                    swCompute.Stop();
                    elapsedCompute += swCompute.Elapsed;
                }

                start = NextStep(start);
                pathLength++;

                if (start.GetType() != typeof(Door) && gridChangeTimer >= 20)
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
            }
            swTotal.Stop();
            string totalTime = (elapsedTotal + swTotal.Elapsed).TotalSeconds.ToString();
            string computeTime = elapsedCompute.TotalSeconds.ToString();

            return new string[] { totalTime, computeTime, pathLength.ToString(), expadedNodes.ToString() };
        }

    }
}
