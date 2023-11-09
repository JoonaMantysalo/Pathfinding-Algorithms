using System.Numerics;

public class Node
{
    public List<Node> neighbours { get; private set; }
    public Vector2 position;
    public string name;

    public double hCost;
    public double gCost;
    public double fCost => hCost + gCost;

    public double gCostLSS_LRTA;
    public double fCostLSS_LRTA => hCost + gCostLSS_LRTA;

    public double RHS;
    public double[] key;

    public Node? parent;

    public bool hCostAssigned = false;
    public bool isObstacle = false;
    public bool hasBeenExpanded = false;

    public void Init(Vector2 pos, string name)
    {
        this.position = pos;
        this.name = name;

        gCost = double.PositiveInfinity;
        gCostLSS_LRTA = double.PositiveInfinity;
        RHS = double.PositiveInfinity;
        key = new double[2];

        neighbours = new List<Node>();
    }

    public void AddNeighbour(Node neighbour)
    {
        if (neighbour == null) return;
        neighbours.Add(neighbour);
    }

    public void Reset()
    {
        hCost = double.PositiveInfinity;
        gCost = double.PositiveInfinity;
        gCostLSS_LRTA = double.PositiveInfinity;
        RHS = double.PositiveInfinity;
        key = new double[2];
        parent = null;
        hCostAssigned = false;
        hasBeenExpanded = false;
    }
}