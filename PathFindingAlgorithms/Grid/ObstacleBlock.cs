namespace PathFindingAlgorithms.Grid
{
    public class ObstacleBlock
    {
        private List<Door> obstacles;
        public string name;

        public ObstacleBlock(int blockNumber)
        {
            obstacles = new List<Door>();
            name = "ObstacleBlock" + blockNumber;
        }

        public bool isObstacle()
        {
            if (obstacles.Count == 0) throw new Exception("Obstacle block empty");
            return obstacles[0].isObstacle;
        }

        public void Add(Door newObstacle)
        {
            obstacles.Add(newObstacle);
        }

        public void ChangeState()
        {
            foreach (Door door in obstacles)
            {
                door.Change();
            }
        }
    }
}
