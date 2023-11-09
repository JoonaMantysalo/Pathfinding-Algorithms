using PathFindingAlgorithms.Grid;
using PathFindingAlgorithms.PriorityQueue;
using System.Diagnostics;

namespace PathFindingAlgorithms.Algorithms
{
    public class RTDStar
    {
        private Node start;
        private Node goal;
        private PriorityQueue<Node> openSet;
        private double k_m;

        private int expadedNodes;

        public void InitializeDStar(Node start, Node goal)
        {
            this.start = start;
            this.goal = goal;
            openSet = new PriorityQueue<Node>(new NodeComparerKey());
            k_m = 0;

            goal.RHS = 0;
            goal.key = CalculateKey(goal);

            openSet.Enqueue(goal);
        }

        private double[] CalculateKey(Node node)
        {
            double minG_RHS = Math.Min(node.gCost, node.RHS);
            double heuristic = Heuristic(node, start);
            return new double[2] { minG_RHS + heuristic + k_m, Math.Min(node.gCost, node.RHS) };
        }

        // Manhattan heuristic
        private double Heuristic(Node from, Node to)
        {
            return Math.Abs(from.position.X - to.position.X) + Math.Abs(from.position.Y - to.position.Y);
        }

        private double Cost(Node node1, Node node2)
        {
            if (node1.isObstacle || node2.isObstacle) return double.PositiveInfinity;
            return Heuristic(node1, node2); // Calculated same way using Euclidean distance
        }
        private double CostReal(Node node1, Node node2)
        {
            return Heuristic(node1, node2); // Calculated same way using Euclidean distance
        }

        private void UpdateVertex(Node node)
        {
            if (node.gCost != node.RHS && openSet.Contains(node))
            {
                node.key = CalculateKey(node);
                openSet.UpdatePriority(node);
            }
            else if (node.gCost != node.RHS && !openSet.Contains(node))
            {
                node.key = CalculateKey(node);
                openSet.Enqueue(node);
            }
            else if (node.gCost == node.RHS && openSet.Contains(node))
            {
                openSet.Remove(node);
            }
        }

        private Status ComputeShortestPath(int globalLimit)
        {
            while (start.RHS != start.gCost || CompareKey(openSet.Peek().key, CalculateKey(start)))
            {
                if (globalLimit == 0) return Status.EXPANSION_LIMIT_REACHED;
                globalLimit--;

                Node u = openSet.Peek();

                expadedNodes++;

                double[] k_old = u.key;
                double[] k_new = CalculateKey(u);

                if (CompareKey(k_old, k_new))
                {
                    u.key = k_new;
                    openSet.UpdatePriority(u);
                }
                else if (u.gCost > u.RHS)
                {
                    u.gCost = u.RHS;
                    openSet.Remove(u);
                    foreach (Node s in u.neighbours)
                    {
                        s.RHS = Math.Min(s.RHS, Cost(s, u) + u.gCost);
                        UpdateVertex(s);
                    }
                }
                else
                {
                    double g_old = u.gCost;
                    u.gCost = double.PositiveInfinity;
                    foreach (Node s in u.neighbours.Concat(new[] { u })) // for all s ∈ Neighbors(u) ∪ {u}
                    {
                        if (s.RHS == Cost(s, u) + g_old)
                        {
                            if (s != goal)
                            {
                                s.RHS = double.PositiveInfinity;
                                foreach (Node s_neighbor in s.neighbours)
                                {
                                    s.RHS = Math.Min(s.RHS, Cost(s, s_neighbor) + s_neighbor.gCost);
                                }
                            }
                        }
                        UpdateVertex(s);
                    }
                }
            }
            if (start.RHS == double.PositiveInfinity) return Status.NO_PATH_EXISTS;
            else return Status.COMPLETE_PATH_FOUND;
        }

        bool CompareKey(double[] key1, double[] key2)
        {
            if (key1[0] < key2[0]) return true;
            if (key1[0] > key2[0]) return false;
            if (key1[1] < key2[1]) return true;
            return false;
        }

        public void UpdateNodeCosts(List<Door> chagedDoors)
        {
            foreach (Door u in chagedDoors)
            {
                foreach (Node v in u.neighbours)
                {
                    if (!u.isObstacle)
                    {
                        u.RHS = Math.Min(u.RHS, Cost(u, v) + v.gCost);
                    }
                    else if (u.RHS == CostReal(u, v) + v.gCost)
                    {
                        u.RHS = double.PositiveInfinity; // Is obstacle now

                    }
                }
                UpdateNodeCostsNeighbours(u.neighbours, u);
                UpdateVertex(u);
            }
        }

        public void UpdateNodeCostsNeighbours(List<Node> changedDoorNeighbours, Door u)
        {
            foreach (Node v in u.neighbours)
            {
                if (!u.isObstacle)
                {
                    v.RHS = Math.Min(v.RHS, Cost(v, u) + u.gCost);
                }
                else if (v.RHS == CostReal(u, v) + u.gCost)
                {
                    v.RHS = double.PositiveInfinity;

                    foreach (Node s in v.neighbours)
                    {
                        v.RHS = Math.Min(v.RHS, Cost(v, s) + s.gCost);
                    }
                }
                UpdateVertex(v);
            }
        }

        private Node NextStep()
        {
            Node nextNode = null;
            double min = double.PositiveInfinity;
            foreach (Node v in start.neighbours)
            {
                double v_min = Cost(start, v) + v.gCost;
                if (min > v_min)
                {
                    nextNode = v;
                    min = v_min;
                }
            }
            try
            {
                if (nextNode.isObstacle) Console.WriteLine("Stepped on an obstacle----------------");
            }
            catch
            {
                throw new Exception("Next node is null. Current node is " + start.name);
            }
            return nextNode;
        }

        private Node ChooseStep(int localLimit, Status status)
        {
            if (status == Status.EXPANSION_LIMIT_REACHED)
            {
                LSS_LRTAStar localSearch = new LSS_LRTAStar();
                return localSearch.FindPath(start, goal, localLimit);
            }
            else if (status == Status.COMPLETE_PATH_FOUND)
            {
                return NextStep();
            }
            else if (status == Status.NO_PATH_EXISTS)
            {
                return start;
            }
            else throw new Exception("No valid status");
        }

        public string[] Main(int expandLimit, Node startNode, Node goalNode, double localRatio, DoorStates doorStates)
        {
            Stopwatch swTotal = Stopwatch.StartNew();
            Stopwatch reComputeSW = new Stopwatch();
            TimeSpan elapsedTotal = TimeSpan.Zero;
            TimeSpan totalReCompute = TimeSpan.Zero;
            int pathLength = 0;
            int reComputeTimer = 0;

            int localLimit = (int)(localRatio * expandLimit);
            int globalLimit = expandLimit - localLimit;
            Node last = startNode;
            InitializeDStar(startNode, goalNode);
            Status status = ComputeShortestPath(globalLimit);

            bool gridChange = false;
            int gridChangeTimer = 0;

            while (start != goal)
            {
                if (gridChange)
                {
                    // Since ChooseStep can run LLS-LRTA after a change in the grid, need to measure it
                    reComputeSW.Restart();

                    start = ChooseStep(localLimit, status);

                    reComputeSW.Stop();
                    totalReCompute += reComputeSW.Elapsed;
                }
                else
                {
                    // Still get the next step when no grid change 
                    start = ChooseStep(localLimit, status);
                }

                k_m = k_m + Heuristic(last, start);
                pathLength++;

                if (start.isObstacle)
                    Console.WriteLine("Moved to an obsacle at " + start.name + "\n"
                        + "Current status: " + status);
                if (status == Status.COMPLETE_PATH_FOUND)
                {
                    if (last == NextStep() && !gridChange && start != goal)
                    {
                        Console.WriteLine("Repeated node at " + last.name + " and " + start.name);
                    }
                }

                last = start;

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
                    // Measure the time it takes to compute a new path after a change in the grid
                    reComputeSW.Restart();

                    UpdateNodeCosts(doorStates.changedDoors);

                    status = ComputeShortestPath(globalLimit);

                    reComputeSW.Stop();
                    totalReCompute += reComputeSW.Elapsed;
                    reComputeTimer++;
                }
                else
                {
                    // Still run ComputeShortestPath every step 
                    status = ComputeShortestPath(globalLimit);
                }
            }
            swTotal.Stop();
            elapsedTotal += swTotal.Elapsed;
            string totalTime = elapsedTotal.TotalSeconds.ToString();
            string totalReComputeTime = totalReCompute.TotalMilliseconds.ToString();
            string totalReComputes = reComputeTimer.ToString();
            string averageReComputeTime = (totalReCompute.TotalMilliseconds / reComputeTimer).ToString();

            return new string[] { totalTime, totalReComputeTime, totalReComputes, averageReComputeTime,
                pathLength.ToString(), expadedNodes.ToString() };
        }

        enum Status
        {
            EXPANSION_LIMIT_REACHED,
            COMPLETE_PATH_FOUND,
            NO_PATH_EXISTS
        }
    }
}
