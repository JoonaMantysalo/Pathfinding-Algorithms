namespace PathFindingAlgorithms.DataCollection
{
    public class SaveAsCSV
    {
        public void SaveData(string filePath, List<string[]> data)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    foreach (var row in data)
                    {
                        // Join the elements in the row with semicolons and write to the file
                        writer.WriteLine(string.Join(";", row));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
