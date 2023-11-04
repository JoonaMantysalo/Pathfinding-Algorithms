namespace PathFindingAlgorithms.Grid
{
    public class GridManagerRoomsFactory : IGridManagerFactory
    {
        public GridManager CreateGridManager(int mapSize)
        {
            return new GridManagerRooms(mapSize);
        }
    }
}
