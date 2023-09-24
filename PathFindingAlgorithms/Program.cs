﻿using PathFindingAlgorithms.Algorithms;
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
        const int mapSize = 256;

        public static void Main(string[] args)
        {
            string mapFilePath = SetFilePath("Maps\\16room256.map");
            string doorStatesFilePath = SetFilePath("DoorStates\\room256_change20");
            string dataFilePath = SetFilePath("DataCollection\\SavedFiles\\testFile2.csv");

            int runCount = 0;
            List<string[]> data = new List<string[]>
            {
                new string[] { "Total time", "Compute time", "Path length", "Memory usage" }
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

                //AStar aStar = new AStar();
                //string[] results = aStar.Main(start, goal, doorStates);

                DStarLite dStarLite = new DStarLite(start, goal, doorStates);
                string[] results = dStarLite.Main();

                //RTDStar rtdStar = new RTDStar();
                //rtdStar.Main(100, start, goal, 0.5, doorStates);


                //data.Add(results);
                
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
    }
}
