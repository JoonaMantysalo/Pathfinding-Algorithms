namespace PathFindingAlgorithms.Grid
{
    public class GridManagerRandomFactory : IGridManagerFactory
    {
        public GridManager CreateGridManager(int mapSize)
        {
            return new GridManagerRandom(mapSize);
        }
    }
}
