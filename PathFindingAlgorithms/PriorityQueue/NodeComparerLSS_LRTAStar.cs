namespace PathFindingAlgorithms.PriorityQueue
{
    public class NodeComparerLSS_LRTAStar : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            return x.fCostLSS_LRTA.CompareTo(y.fCostLSS_LRTA);
        }
    }
}
