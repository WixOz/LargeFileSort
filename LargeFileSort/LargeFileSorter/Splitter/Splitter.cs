using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LargeFileSorter.Splitter;

public static class Splitter
{
    public static void Split(string fileNameForSplit, int partSize)
    {
        using var reader = new StreamReader(fileNameForSplit, Encoding.UTF8);
        
        var writers = new Dictionary<string, StreamWriter>();
        var keys = new Dictionary<char, int>();
        var fileSizes = new Dictionary<string, int>();
        
        while (reader.ReadLine() is { } line)
        {
            var firstLetter = GetFirstLetter(line);
            var postfix = (GetNumber(line) / 10000) * 10000;
            keys.TryAdd(firstLetter, postfix);
            
            var fileName = GetFileName(firstLetter, keys);
            fileSizes.TryAdd(fileName, 0);

            if (!writers.ContainsKey(fileName) || !writers[fileName].BaseStream.CanWrite)
            {
                
                var writer = new StreamWriter($"{fileName}.pt", true, Encoding.UTF8);
                writers.Add(fileName, writer);
            }

            if (fileSizes[fileName] < partSize)
            {
                fileSizes[fileName] += line.Length;
                writers[fileName].WriteLine(line);
            }
            else
            {
                writers[fileName].Close();
                writers.Remove(fileName);

                keys[firstLetter]++;
                fileName = GetFileName(firstLetter, keys);

                var writer = new StreamWriter($"{fileName}.pt", true, Encoding.UTF8);
                writers.Add(fileName, writer);
                
                fileSizes.TryAdd(fileName, 0);
                fileSizes[fileName] += line.Length;
                writers[fileName].WriteLine(line);
            }
        }

        foreach (var writersKey in writers.Keys)
        {
            writers[writersKey].Close();
        }
    }
    
    private static string GetFileName(char firstLetter, Dictionary<char, int> keys)
    {
        return keys[firstLetter] switch
        {
            < 10 => $"{firstLetter}_0{keys[firstLetter]}",
            _ => $"{firstLetter}_{keys[firstLetter]}"
        };
    }

    private static char GetFirstLetter(string line)
    {
        return string.IsNullOrEmpty(line)
            ? char.MinValue
            : char.ToUpper(line[line.IndexOf(". ", StringComparison.Ordinal) + 2]);
    }
    
    private static int GetNumber(string line)
    {
        return int.Parse(line[..line.IndexOf('.', StringComparison.Ordinal)]);
    }
}