namespace PathFindingAlgorithms.PriorityQueue
{
    public class NodeComparerHCost : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            return x.hCost.CompareTo(y.hCost);
        }
    }
}
