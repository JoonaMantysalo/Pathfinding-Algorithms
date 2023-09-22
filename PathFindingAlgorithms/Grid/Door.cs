namespace PathFindingAlgorithms.Grid
{
    public class Door : Node
    {
        public void Open()
        {
            isObstacle = false;
        }
        public void Close()
        {
            isObstacle = true;
        }
        public void Change()
        {
            if (isObstacle) Open();
            else Close();
        }

    }
}
