namespace PathFindingAlgorithms.Grid
{
    public interface IGridManagerFactory
    {
        GridManager CreateGridManager(int mapSize);
    }
}
