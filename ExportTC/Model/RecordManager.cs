using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class RecordManager
{
    // Метод для записи строки с ревизией в указанный файл (с подчеркиваниями в имени файла)
    public void AddRecord(string fileName, string record, string revision)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(record) || string.IsNullOrWhiteSpace(revision))
            throw new ArgumentException("Имя файла, запись и ревизия не могут быть пустыми");

        // Формируем имя файла с подчеркиваниями, если оно не в нужном формате
        fileName = "__" + fileName + "__.txt";

        // Формируем строку для записи с подчеркиваниями
        string fullRecord = $"_{record}-{revision}_";

        // Проверяем, существует ли запись в этом файле
        if (!IsRecordExistsInFile(fileName, record, revision))
        {
            // Если записи нет, добавляем её
            File.AppendAllText(fileName, fullRecord + Environment.NewLine);
            Console.WriteLine($"Запись '{record}' с ревизией '{revision}' добавлена в файл '{fileName}'.");
        }
        else
        {
            Console.WriteLine($"Запись '{record}' с ревизией '{revision}' уже существует в файле '{fileName}'.");
        }
    }

    // Метод для проверки, есть ли строка с ревизией в указанном файле
    private bool IsRecordExistsInFile(string fileName, string record, string revision)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(record) || string.IsNullOrWhiteSpace(revision))
            throw new ArgumentException("Имя файла, запись и ревизия не могут быть пустыми");

        if (File.Exists(fileName))
        {
            // Формируем строку для проверки
            string fullRecord = $"_{record}-{revision}_";

            // Проверяем наличие записи в файле
            string[] lines = File.ReadAllLines(fileName);
            return lines.Contains(fullRecord);
        }

        return false;
    }

    // Метод для проверки, существует ли запись с ревизией во всех подходящих файлах
    public bool IsRecordExists(string fileName, string record, string revision)
    {
        if (string.IsNullOrWhiteSpace(record) || string.IsNullOrWhiteSpace(revision))
            throw new ArgumentException("Запись и ревизия не могут быть пустыми");

        // Получаем список всех файлов в формате __<имя>__.txt
        string[] allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "__*__.txt");

        foreach (var filePath in allFiles)
        {
            if (filePath.Contains(fileName))
                continue;
            if (IsRecordExistsInFile(filePath, record, revision))
            {
                return true; // Если нашли в каком-то файле, возвращаем true
            }
        }

        return false; // Если не нашли, возвращаем false
    }
}
