using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// --- НАСТРОЙКА ИМЕН И ПУТЕЙ ---
string studentName = "Егор Сороко";
string sequencesPath = "sequences.txt";
string commandsPath = "commands.txt";
string outputPath = "genedata.txt";

if (!File.Exists(sequencesPath) || !File.Exists(commandsPath))
{
    Console.WriteLine("Ошибка: Входные файлы sequences.txt или commands.txt не найдены.");
    return;
}

List<GeneticData> database = new List<GeneticData>();
string[] seqLines = File.ReadAllLines(sequencesPath);

foreach (string line in seqLines)
{
    if (string.IsNullOrWhiteSpace(line))
        continue;

    string[] parts = line.Split('\t');
    if (parts.Length >= 3)
    {
        GeneticData data = new GeneticData
        {
            protein = parts[0].Trim(),
            organism = parts[1].Trim(),
            amino_acids = RLDecoding(parts[2].Trim())
        };
        database.Add(data);
    }
}