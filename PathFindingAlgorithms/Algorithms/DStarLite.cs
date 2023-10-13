using PathFindingAlgorithms.Grid;
using PathFindingAlgorithms.PriorityQueue;
using System;
using System.Diagnostics;

public class DStarLite
{
    private Node start;
    private Node goal;
    private PriorityQueue<Node> openSet;
    private DoorStates doorStates;
    private double k_m;

    private int expadedNodes;

    public DStarLite(Node start, Node goal, DoorStates doorStates)
    {
        this.start = start;
        this.goal = goal;
        this.doorStates = doorStates;
        openSet = new PriorityQueue<Node>(new NodeComparerKey());
        k_m = 0;

        expadedNodes = 0;

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
        return Heuristic(node1, node2); // Calculated same way using Manhattan distance
    }

    private double CostReal(Node node1, Node node2)
    {
        return Heuristic(node1, node2); // Calculated same way using Manhattan distance
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

    public void ComputeShortestPath()
    {

        while (start.RHS != start.gCost || CompareKey(openSet.Peek().key, CalculateKey(start)))
        {
            expadedNodes++;
            Node u = openSet.Peek();

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

    public void FindPathAfterGridChange(List<Door> changedDoors, Node previousStart)
    {
        k_m = k_m + Heuristic(previousStart, start);
        UpdateNodeCosts(changedDoors);
        ComputeShortestPath();
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
        return nextNode;
    }

    public string[] Main()
    {
        Stopwatch swTotal = Stopwatch.StartNew();
        TimeSpan elapsedTotal = TimeSpan.Zero;
        TimeSpan totalReCompute = TimeSpan.Zero;
        int pathLength = 0;
        int reComputeTimer = 0;

        Node lastStart = start;

        ComputeShortestPath();

        bool gridChange = false;
        int gridChangeTimer = 0;

        while (start != goal)
        {
            start = NextStep();
            pathLength++;

            // Change the grid every 20 steps
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

            if (gridChange)
            {
                Stopwatch reComputeSW = Stopwatch.StartNew();

                FindPathAfterGridChange(doorStates.changedDoors, lastStart);

                reComputeSW.Stop();
                totalReCompute += reComputeSW.Elapsed;
                reComputeTimer++;

                lastStart = start;
            }
        }
        swTotal.Stop();
        elapsedTotal += swTotal.Elapsed;
        string totalTime = elapsedTotal.TotalSeconds.ToString();

        return new string[] { totalTime, totalReCompute.TotalMilliseconds.ToString(), 
            reComputeTimer.ToString() ,pathLength.ToString(), expadedNodes.ToString() };
    }
}