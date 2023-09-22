using PathFindingAlgorithms.Algorithms;
using PathFindingAlgorithms.Grid;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;

namespace PathFindingAlgorithms
{
    public class Program
    {
        const int mapSize = 256;
        const string mapFilePath = "C:/Users/Joona/source/repos/Path Finding Algorithms/PathFindingAlgorithms/Maps/Test16room256.map";
        const string doorStatesFilePath = "C:/Users/Joona/source/repos/Path Finding Algorithms/PathFindingAlgorithms/DoorStates/Test16room256.json";


        public static void Main(string[] args)
        {
            int runCount = 100;

            while (runCount > 0)
            {
                GridManager gridManager = new GridManager(mapSize);
                Console.WriteLine("Loading grid...");
                gridManager.GenerateGrid(mapFilePath);

                List<Room> rooms = gridManager.rooms;
                List<Door> doors = gridManager.doors;
                Node start = gridManager.GetNodeAtPosition(new Vector2(1, 1));
                Node goal = gridManager.GetNodeAtPosition(new Vector2(mapSize - 1, mapSize - 1));

                DoorStates doorStates = new DoorStates(doors, rooms);

                doorStates.RecordDynamicDoorStates(20, doorStatesFilePath);
                //doorStates.JsonToDoorStates(doorStatesFilePath);
                //doorStates.Reformat(doorStatesFilePath);

                Console.WriteLine("Starting pathfinding");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //AStar aStar = new AStar();
                //aStar.Main(start, goal, doorStates);

                //DStarLite dStarLite = new DStarLite(start, goal, doorStates);
                //dStarLite.Main();

                RTDStar rtdStar = new RTDStar();
                rtdStar.Main(100, start, goal, 0.5, doorStates);

                stopwatch.Stop();
                Console.WriteLine("Elapsed time: " + stopwatch.Elapsed);


                runCount--;
                Console.WriteLine("Finished run " + (100-runCount));
            }
        }
    }

}
