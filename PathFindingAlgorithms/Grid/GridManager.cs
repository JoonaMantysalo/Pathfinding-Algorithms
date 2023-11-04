using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PathFindingAlgorithms.Grid
{
    public interface GridManager
    {
        Dictionary<Vector2, Node> nodes { get; }
        List<Door> doors { get; }
        DoorStates doorStates { get; }

        void GenerateGrid(string mapFilePath);
        void RecordDoorStates(int count, string filePath, int changeVolume);
        void JsonToDoorStates(string filePath);
        Node GetNodeAtPosition(Vector2 pos);
    }
}
