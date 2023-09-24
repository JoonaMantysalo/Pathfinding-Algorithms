using PathFindingAlgorithms.Algorithms;
using PathFindingAlgorithms.Grid;
using PathFindingAlgorithms.DataCollection;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Xml.Serialization;

namespace PathFindingAlgorithms
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string algorithm = "";
            int mapSize = 0;
            int changeVolume = 0;
            bool correctInput = false;

            while (!correctInput)
            {
                Console.Write("Enter the parameters: ");
                string input = Console.ReadLine();
                string[] parts = input.Split(' ');

                if (parts.Length >= 3)
                {
                    // Extract each part into separate variables
                    algorithm = parts[0];
                    int.TryParse(parts[1], out mapSize);
                    int.TryParse(parts[2], out changeVolume);
                }

                correctInput = TestForCorrectInput(algorithm, mapSize, changeVolume);
                if (!correctInput)
                {
                    Console.WriteLine("Incorrect input, try again");
                }
            }

            string mapFilePath = SetFilePath("Maps\\16room" + mapSize + ".map");
            string doorStatesFilePath = SetFilePath("DoorStates\\room" + mapSize + "_change" + changeVolume);
            string dataFilePath = SetFilePath("DataCollection\\SavedFiles\\algorithm\\16room_" + algorithm + "_change" + changeVolume + ".csv");

            int runCount = 0;
            List<string[]> data = new List<string[]>
            {
                new string[] { "Total time", "Compute time", "Path length", "Memory usage" }
            };

            // Go through all the doorStates 
            string[] doorStateFiles = Directory.GetFiles(doorStatesFilePath, "*.json");
            Console.WriteLine("Processing folder: " + doorStatesFilePath);
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

                Console.WriteLine("Processing file: " + Path.GetFileName(doorStateFile));

                string[] results = new string[0];
                switch (algorithm)
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
                }

                data.Add(results);

                Console.WriteLine("Finished run " + (runCount));
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

        static bool TestForCorrectInput(string algorithm, int mapSize, int changeVolume)
        {
            if (!(algorithm == "AStar" || algorithm == "DStar-Lite" || algorithm == "RTDStar"))
                return false;
            if (!(mapSize == 128 || mapSize == 256 || mapSize == 512)) 
                return false;
            if (!(changeVolume == 2 || changeVolume == 5 || changeVolume == 10 || changeVolume == 20))
                return false;
            return true;
        }
    }
}
