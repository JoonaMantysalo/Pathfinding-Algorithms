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
        static readonly string gridType = "Rooms";

        public static void Main(string[] args)
        {
            //foreach (int size in mapSizes)
            //{
            //    foreach (int volume in changeVolumes)
            //    {
            //        RunTests("AStar", size, volume, gridType);
            //        RunTests("DStar-Lite", size, volume, gridType);
            //    }
            //}
            RunTests("RTDStar", 128, 5, gridType, 256);

            Console.WriteLine("All done");
        }

        static void RunTests(string algorithmName, int mapSize, int changeVolume, string gridType, int extraVariable)
        {
            IGridManagerFactory gridManagerFactory;
            string mapFilePath = string.Empty;
            string doorStatesFilePath = string.Empty;
            string dataFilePath = string.Empty;
            Vector2 startNodePos = Vector2.Zero;

            // RTD* and AD* use an extra variable that needs to be marked in the files seperately
            // A* and D* Lite don't use extra variables and it should be set to 0 
            string additionalVariable = string.Empty;
            if (extraVariable != 0) additionalVariable = "_variable" + extraVariable.ToString();

            switch (gridType)
            {
                case "Rooms":
                    gridManagerFactory = new GridManagerRoomsFactory();
                    mapFilePath = SetFilePath("Maps\\16room" + mapSize + ".map");
                    doorStatesFilePath = SetFilePath("DoorStates\\room" + mapSize + "_change" + changeVolume);
                    dataFilePath = SetFilePath("DataCollection\\SavedFiles\\" + algorithmName + "\\16room_" 
                        + mapSize + "map_" + algorithmName + "_change" + changeVolume + additionalVariable + ".csv");
                    startNodePos = new Vector2(1, 1);
                    break;
                case "Random":
                    gridManagerFactory = new GridManagerRandomFactory();
                    mapFilePath = SetFilePath("Maps\\random" + mapSize + ".map");
                    doorStatesFilePath = SetFilePath("DoorStates\\random" + mapSize + "_change" + changeVolume);
                    dataFilePath = SetFilePath("DataCollection\\SavedFiles\\" + algorithmName + "\\random_" 
                        + mapSize + "map_" + algorithmName + "_change" + changeVolume + additionalVariable + ".csv");
                    startNodePos = new Vector2(0, 0);
                    break;
                default:
                    throw new Exception("No valid gridtype given");
            }

            int runCount = 0;
            List<string[]> data = new List<string[]>
            {
                new string[] { "Total time (s)", "Total recompute time (ms)", "Amount of recomputes", "Average recompute time (ms)", "Path length", "Expanded nodes" },
                new string[] {"=AVERAGE(A4:A103)", "=AVERAGE(B4:B103)", "=AVERAGE(C4:C103)", "=AVERAGE(D4:D103)", "=AVERAGE(E4:E103)", "=AVERAGE(F4:F103)" },
                new string[] { "---------------------------------------------------------------------------------------------" }
            };

            // Record the doorstates
            //for (int i = 1; i <= 100; i++)
            //{
            //    GridManager gridManager = gridManagerFactory.CreateGridManager(mapSize);
            //    gridManager.GenerateGrid(mapFilePath);
            //    string doorstatefile = doorStatesFilePath + "\\doorstates" + i + ".json";
            //    gridManager.RecordDoorStates(50, doorstatefile, changeVolume);
            //    Console.WriteLine("Finished recording for " + mapSize + " " + changeVolume + ". Count " + i);
            //}

            GridManager gridManager = gridManagerFactory.CreateGridManager(mapSize);
            gridManager.GenerateGrid(mapFilePath);

            Node start = gridManager.GetNodeAtPosition(startNodePos);
            Node goal = gridManager.GetNodeAtPosition(new Vector2(mapSize - 1, mapSize - 1));

            // Go through all the doorStates 
            string[] doorStateFiles = Directory.GetFiles(doorStatesFilePath, "*.json");
            foreach (string doorStateFile in doorStateFiles)
            {
                gridManager.ResetGrid();

                gridManager.JsonToDoorStates(doorStateFile);
                DoorStates doorStates = gridManager.doorStates; // ?

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
                        results = rtdStar.Main(extraVariable, start, goal, 0.5, doorStates);
                        break;
                    case "ADStar":
                        ADStar aDStar = new ADStar();
                        results = aDStar.Main(start, goal, doorStates);
                        break;
                }
                data.Add(results);

                runCount++;
                Console.WriteLine("Finished run " + runCount);
            }

            SaveAsCSV saveCSV = new SaveAsCSV();
            saveCSV.SaveData(dataFilePath, data);
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
    }
}
