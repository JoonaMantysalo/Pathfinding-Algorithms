namespace PathFindingAlgorithms.Grid
{
    public class Room
    {
        public int roomNumber;
        public Room parent;

        public bool visited;

        public int hCost;
        public int gCost;
        public int fCost => hCost + gCost;

        public Door rightDoor;
        public Door leftDoor;
        public Door upDoor;
        public Door downDoor;

        public Room rightNeighbour;
        public Room leftNeighbour;
        public Room upNeighbour;
        public Room downNeighbour;
    }
}
