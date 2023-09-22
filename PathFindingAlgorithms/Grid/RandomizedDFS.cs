namespace PathFindingAlgorithms.Grid
{
    public class RandomizedDFS
    {
        public void CreateMaze(List<Room> rooms)
        {
            Random random = new Random();
            Room startRoom = rooms[random.Next(rooms.Count)];
            startRoom.visited = true;

            Stack<Room> stack = new Stack<Room>();
            stack.Push(startRoom);

            while (stack.Count > 0)
            {
                Room currentRoom = stack.Pop();
                List<Room> neighbours = GetUnvisitedNeighbors(currentRoom);

                if (neighbours.Count > 0)
                {
                    Room randomNeighbour = neighbours[random.Next(neighbours.Count)];
                    OpenDoor(randomNeighbour, currentRoom);
                    randomNeighbour.visited = true;
                    stack.Push(currentRoom);
                    stack.Push(randomNeighbour);
                }
            }
        }

        private void OpenDoor(Room randomNeighbour, Room currentRoom)
        {
            if (currentRoom.leftNeighbour == randomNeighbour)
                currentRoom.leftDoor.Open();

            if (currentRoom.rightNeighbour == randomNeighbour)
                currentRoom.rightDoor.Open();

            if (currentRoom.upNeighbour == randomNeighbour)
                currentRoom.upDoor.Open();

            if (currentRoom.downNeighbour == randomNeighbour)
                currentRoom.downDoor.Open();
        }

        private List<Room> GetUnvisitedNeighbors(Room currentRoom)
        {
            List<Room> visited = new List<Room>();
            if (currentRoom.leftNeighbour != null)
                if (!currentRoom.leftNeighbour.visited)
                    visited.Add(currentRoom.leftNeighbour);

            if (currentRoom.rightNeighbour != null)
                if (!currentRoom.rightNeighbour.visited)
                    visited.Add(currentRoom.rightNeighbour);

            if (currentRoom.upNeighbour != null)
                if (!currentRoom.upNeighbour.visited)
                    visited.Add(currentRoom.upNeighbour);

            if (currentRoom.downNeighbour != null)
                if (!currentRoom.downNeighbour.visited)
                    visited.Add(currentRoom.downNeighbour);

            return visited;
        }
    }
}
