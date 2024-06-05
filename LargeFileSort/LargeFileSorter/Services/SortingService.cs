using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LargeFileSorter.Services;

public static class SortingService
{
    /// <summary>
    /// Method which run before sorting file.
    /// Method implements logic of choosing file for sorting.
    /// </summary>
    /// <exception cref="InvalidDataException">Throws when file option was incorrect.</exception>
    public static void PreSorting()
    {
        try
        {
            var filesForSort = LoadFilesForSort();
        }
        catch (Exception)
        {
            Console.WriteLine("Incorrect option!");
            throw;
        }
    }

    private static IList<string> LoadFilesForSort()
    {
        var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var files = Directory.GetFiles(root, "*.txt");
        return new List<string>();
    }
}