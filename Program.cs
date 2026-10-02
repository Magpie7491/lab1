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

// 1. ЧТЕНИЕ И ПАРСИНГ ГЕНЕТИЧЕСКИХ ДАННЫХ
List<GeneticData> database = new List<GeneticData>();
string[] seqLines = File.ReadAllLines(sequencesPath);

foreach (string line in seqLines)
{
    if (string.IsNullOrWhiteSpace(line)) continue;

    string[] parts = line.Split('\t');
    if (parts.Length >= 3)
    {
        GeneticData data = new GeneticData
        {
            protein = parts[0].Trim(),
            organism = parts[1].Trim(),
            amino_acids = RLDecoding(parts[2].Trim()) // Сразу разжимаем RLE
        };
        database.Add(data);
    }
}

// 2. ОБРАБОТКА КОМАНД И ЗАПИСЬ В ВЫХОДНОЙ ФАЙЛ
using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
{
    writer.WriteLine(studentName);
    writer.WriteLine("Genetic Searching");

    string[] commandLines = File.ReadAllLines(commandsPath);
    int commandCounter = 1;

    foreach (string line in commandLines)
    {
        if (string.IsNullOrWhiteSpace(line)) continue;

        string[] parts = line.Split('\t');
        string command = parts[0].Trim();
        string cmdNum = commandCounter.ToString("D3");
        commandCounter++;

        writer.WriteLine(new string('-', 74));

        if (command.Equals("search", StringComparison.OrdinalIgnoreCase))
        {
            string targetSeq = RLDecoding(parts[1].Trim());
            ExecuteSearch(writer, cmdNum, targetSeq, database);
        }
        else if (command.Equals("diff", StringComparison.OrdinalIgnoreCase))
        {
            ExecuteDiff(writer, cmdNum, parts[1].Trim(), parts[2].Trim(), database);
        }
        else if (command.Equals("mode", StringComparison.OrdinalIgnoreCase))
        {
            ExecuteMode(writer, cmdNum, parts[1].Trim(), database);
        }
    }
    writer.WriteLine(new string('-', 74));
}

Console.WriteLine("Файл genedata.txt успешно сформирован!");

// --- МЕТОДЫ ОБРАБОТКИ ОПЕРАЦИЙ ---

static string RLDecoding(string amino_acids)
{
    if (string.IsNullOrEmpty(amino_acids)) return "";
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < amino_acids.Length; i++)
    {
        if (char.IsDigit(amino_acids[i]))
        {
            int count = amino_acids[i] - '0';
            char repeatChar = amino_acids[i + 1];
            sb.Append(repeatChar, count);
            i++;
        }
        else
        {
            sb.Append(amino_acids[i]);
        }
    }
    return sb.ToString();
}

static void ExecuteSearch(StreamWriter writer, string cmdNum, string targetSeq, List<GeneticData> database)
{
    writer.WriteLine($"{cmdNum}   search   {targetSeq}");
    writer.WriteLine($"{"organism",-24} {"protein"}");

    bool isFound = false;
    foreach (var data in database)
    {
        if (data.amino_acids.Contains(targetSeq))
        {
            writer.WriteLine($"{data.organism,-24} {data.protein}");
            isFound = true;
        }
    }
    if (!isFound) writer.WriteLine("NOT FOUND");
}

static void ExecuteDiff(StreamWriter writer, string cmdNum, string p1Name, string p2Name, List<GeneticData> database)
{
    writer.WriteLine($"{cmdNum}   diff   {p1Name}   {p2Name}");
    writer.WriteLine("amino-acids difference:");

    var p1 = database.FirstOrDefault(d => d.protein == p1Name);
    var p2 = database.FirstOrDefault(d => d.protein == p2Name);

    if (string.IsNullOrEmpty(p1.protein) || string.IsNullOrEmpty(p2.protein))
    {
        string missingMsg = "MISSING: ";
        if (string.IsNullOrEmpty(p1.protein)) missingMsg += p1Name;
        if (string.IsNullOrEmpty(p1.protein) && string.IsNullOrEmpty(p2.protein)) missingMsg += ", ";
        if (string.IsNullOrEmpty(p2.protein)) missingMsg += p2Name;
        writer.WriteLine(missingMsg);
        return;
    }

    int diffCount = 0;
    int minLen = Math.Min(p1.amino_acids.Length, p2.amino_acids.Length);

    for (int i = 0; i < minLen; i++)
    {
        if (p1.amino_acids[i] != p2.amino_acids[i]) diffCount++;
    }
    diffCount += Math.Abs(p1.amino_acids.Length - p2.amino_acids.Length);

    writer.WriteLine(diffCount);
}

static void ExecuteMode(StreamWriter writer, string cmdNum, string proteinName, List<GeneticData> database)
{
    writer.WriteLine($"{cmdNum}   mode   {proteinName}");
    writer.WriteLine("amino-acid occurs:");

    var target = database.FirstOrDefault(d => d.protein == proteinName);
    if (string.IsNullOrEmpty(target.protein))
    {
        writer.WriteLine($"MISSING: {proteinName}");
        return;
    }

    Dictionary<char, int> counts = new Dictionary<char, int>();
    foreach (char c in target.amino_acids)
    {
        if (counts.ContainsKey(c)) counts[c]++;
        else counts[c] = 1;
    }

    var topAminoAcid = counts
        .OrderByDescending(kv => kv.Value)
        .ThenBy(kv => kv.Key)
        .FirstOrDefault();

    writer.WriteLine($"{topAminoAcid.Key,-10} {topAminoAcid.Value}");
}

// СТРУКТУРА ДАННЫХ (в самом конце файла)
struct GeneticData
{
    public string protein;
    public string organism;
    public string amino_acids;
}