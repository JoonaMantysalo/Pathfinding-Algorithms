using PathFindingAlgorithms.Grid;

namespace PathFindingAlgorithms.PriorityQueue
{
    public class NodeComparerKey : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            int keyFirstPart = x.key[0].CompareTo(y.key[0]);
            if (keyFirstPart != 0)
                return keyFirstPart;

            int keySecondPart = x.key[1].CompareTo(y.key[1]);
            return keySecondPart;
        }

    }
}
