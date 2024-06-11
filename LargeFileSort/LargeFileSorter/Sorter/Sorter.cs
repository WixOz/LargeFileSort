using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LargeFileSorter.Comparer;

namespace LargeFileSorter.Sorter;

public class Sorter
{
    private const int PartSize = 13107200;

    private string _filename;
    private readonly CustomComparer _comparer;

    public Sorter(string fileName)
    {
        _filename = fileName;
        _comparer = new CustomComparer(StringComparer.Ordinal);
    }

    public void Sort()
    {
        var cycleCount = 1;

        if(!IsFileSorted(_filename))
        {
            Splitter.Splitter.Split(_filename, PartSize*cycleCount);
            SortParts();
            ConcatParts();
            cycleCount++;
        }
    }

    private bool IsFileSorted(string fileName)
    {
        using var reader = new StreamReader(fileName);
        var firstLine = reader.ReadLine();

        if (string.IsNullOrEmpty(firstLine)) return true;
        
        while (reader.ReadLine() is { } line)
        {
            var compareRes = _comparer.Compare(firstLine, line);
            if (_comparer.Compare(firstLine, line) > 0)
            {
                return false;
            }

            firstLine = line;
        }

        return true;
    }

    private void SortParts()
    {
        var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var parts = Directory.GetFiles(root, "*.pt");

        /*var partitioner = Partitioner.Create(parts, EnumerablePartitionerOptions.NoBuffering);
        Parallel.ForEach(partitioner, SortPart);*/

        for (var i = 0; i < parts.Length; i += 20)
        {
            var tempList = new List<string>();

            for (var j = 0; j < 20 && j + i < parts.Length; j++)
            {
                tempList.Add(parts[i+j]);
            }
            
            Parallel.ForEach(tempList, SortPart);
        }
    }

    private void SortPart(string partPath)
    {
        var linesArray = File.ReadAllLines(partPath);
        Array.Sort(linesArray, _comparer);
        File.WriteAllLines(partPath.Replace(".pt", "_sorted.pt"), linesArray);
        File.Delete(partPath);
    }

    private void ConcatParts()
    {
        var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var parts = Directory.EnumerateFiles(root, "*_sorted.pt");
        var newFilename = _filename;

        if (!newFilename.Contains("_sorted"))
        {
            newFilename = _filename.Replace(".txt", "_sorted.txt");
        }
        
        File.Delete(newFilename);

        foreach (var part in parts)
        {
            File.AppendAllLines(newFilename, File.ReadAllLines(part));
            File.Delete(part);
        }

        _filename = newFilename;
    }
}