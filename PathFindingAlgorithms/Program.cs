using PathFindingAlgorithms.Algorithms;
using PathFindingAlgorithms.Grid;
using PathFindingAlgorithms.DataCollection;
using System.Numerics;

namespace PathFindingAlgorithms
{
    public class Program
    {
        static readonly string[] algorithmNames = new string[] { "AStar", "DStar-Lite", "RTDStar", "ADStar" };
        static readonly int[] mapSizes = new int[] { 128, 256, 512 };
        static readonly int[] changeVolumes = new int[] { 2, 5, 10, 20 };

        public static void Main(string[] args)
        {

            foreach (int size in mapSizes)
            {
                //DoTests("DStar-Lite", size, 2);
            }
            DoTests("ADStar", 256, 5);
        }

        static void DoTests(string algorithmName, int mapSize, int changeVolume)
        {
            string mapFilePath = SetFilePath("Maps\\16room" + mapSize + ".map");
            string doorStatesFilePath = SetFilePath("DoorStates\\room" + mapSize + "_change" + changeVolume);
            string dataFilePath = SetFilePath("DataCollection\\SavedFiles\\" + algorithmName + "\\16room_" + mapSize + "map_" + algorithmName + "_change" + changeVolume + ".csv");

            int runCount = 0;
            List<string[]> data = new List<string[]>
            {
                new string[] { "Total time", "Compute time", "Path length", "Expanded nodes" }
            };

            // Go through all the doorStates 
            string[] doorStateFiles = Directory.GetFiles(doorStatesFilePath, "*.json");
            foreach (string doorStateFile in doorStateFiles)
            {
                runCount++;
                GridManager gridManager = new GridManager(mapSize);
                gridManager.GenerateGrid(mapFilePath);

                List<Room> rooms = gridManager.rooms;
                List<Door> doors = gridManager.doors;
                Node start = gridManager.GetNodeAtPosition(new Vector2(1, 1));
                Node goal = gridManager.GetNodeAtPosition(new Vector2(mapSize - 1, mapSize - 1));

                DoorStates doorStates = new DoorStates(doors, rooms);
                doorStates.JsonToDoorStates(doorStateFile);

                string[] results = new string[0];
                switch (algorithmName)
                {
                    case "AStar":
                        AStar aStar = new AStar();
                        results = aStar.Main(start, goal, doorStates);
                        break;
                    case "DStar-Lite":
                        DStarLite dStarLite = new DStarLite(start, goal, doorStates);
                        results = dStarLite.Main();
                        break;
                    case "RTDStar":
                        RTDStar rtdStar = new RTDStar();
                        rtdStar.Main(100, start, goal, 0.5, doorStates); // No result yet implemented
                        break;
                    case "ADStar":
                        ADStar aDStar = new ADStar();
                        aDStar.Main(start, goal, doorStates, doors); // No result yet implemented
                        break;
                }

                data.Add(results);

                Console.WriteLine("Finished run " + runCount);
            }

            //SaveAsCSV saveCSV = new SaveAsCSV();
            //saveCSV.SaveData(dataFilePath, data);
        }

        static string SetFilePath(string fileName)
        {
            string saveDirectory = Path.Combine(Environment.CurrentDirectory, fileName);

            string debugDirectory = "bin\\Debug\\net6.0\\";
            if (saveDirectory.Contains(debugDirectory))
            {
                saveDirectory = saveDirectory.Replace(debugDirectory, "");
            }

            return saveDirectory;
        }

        static bool TestForCorrectInput(string algorithmName, int mapSize, int changeVolume)
        {
            if (!algorithmNames.Contains(algorithmName))
                return false;
            if (!mapSizes.Contains(mapSize)) 
                return false;
            if (!changeVolumes.Contains(changeVolume))
                return false;
            return true;
        }
    }
}
