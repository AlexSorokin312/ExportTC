using System.IO;

public class RecordManager
{
    private readonly string _file1To4Path = "Records_1to4.txt";
    private readonly string _file5To9Path = "Records_5to9.txt";

    // Метод для записи строки в соответствующий файл
    public void AddRecord(string record)
    {
        if (string.IsNullOrWhiteSpace(record))
            throw new ArgumentException("Запись не может быть пустой");

        char firstChar = record[0];

        // Проверяем, начинается ли строка с цифры от 1 до 4 или от 5 до 9
        if (char.IsDigit(firstChar))
        {
            string filePath = (firstChar >= '1' && firstChar <= '4') ? _file1To4Path : _file5To9Path;

            // Записываем в файл
            File.AppendAllText(filePath, record + Environment.NewLine);
        }
        else
        {
            throw new ArgumentException("Запись должна начинаться с цифры");
        }
    }

    // Метод для проверки, есть ли строка в соответствующем файле
    public bool IsRecordExists(string record)
    {
        if (string.IsNullOrWhiteSpace(record))
            throw new ArgumentException("Запись не может быть пустой");

        char firstChar = record[0];

        if (char.IsDigit(firstChar))
        {
            string filePath = (firstChar >= '1' && firstChar <= '4') ? _file1To4Path : _file5To9Path;

            // Проверяем наличие записи в файле
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                return lines.Contains(record);
            }
        }
        else
        {
            throw new ArgumentException("Запись должна начинаться с цифры");
        }

        return false;
    }
}