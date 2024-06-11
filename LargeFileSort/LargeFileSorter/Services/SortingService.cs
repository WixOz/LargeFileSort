using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using LargeFileSorter.Sorter;

namespace LargeFileSorter.Services;

public static class SortingService
{
    private static readonly Stopwatch Watcher = new();

    /// <summary>
    /// Method which run before sorting file.
    /// Method implements logic of choosing file for sorting.
    /// </summary>
    /// <exception cref="InvalidDataException">Throws when file option was incorrect.</exception>
    public static void PreSorting()
    {
        try
        {
            
        }
        catch (Exception)
        {
            Console.WriteLine("Incorrect option!");
            throw;
        }
        
        Watcher.Start();
    }

    /// <summary>
    /// Method for sorting file.
    /// </summary>
    public static void Sort()
    {
        var sorter = new Sorter.Sorter("LargeFile_1Gb_100624_035753.txt");
        sorter.Sort();
    }
    
    /// <summary>
    /// Method which run after sorting file.
    /// </summary>
    public static void PostSorting()
    {
        Watcher.Stop();
        Console.WriteLine($"File sorted successfully.");
        Console.WriteLine($"Execution Time: {Watcher.ElapsedMilliseconds / 1000f} s.");
    }

    private static IList<string> LoadFilesForSort()
    {
        var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var files = Directory.EnumerateFiles(root, "*.txt");
        return new List<string>();
    }
}