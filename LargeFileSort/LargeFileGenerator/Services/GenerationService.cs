using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LargeFileGenerator.Services;

public static class GenerationService
{
    private const int EolLength = 2;
    
    private static IList<string>? _phrases;

    private static Stopwatch _watcher = new Stopwatch();
    private static int GenerationOption { get; set; } = 1;
    private static string FileDateTime { get; } = $"{DateTime.UtcNow:ddMMyy_hhmmss}";

    /// <summary>
    /// Method which run before generation file.
    /// Method implements logic of files size  options choosing.
    /// </summary>
    /// <exception cref="InvalidDataException">Throws when file size option was incorrect.</exception>
    public static void PreGeneration()
    {
        Console.WriteLine("Generator started. Choose one option:\n1) 1Gb file.\n2) 10Gb file.\n\nYour option:");
    
        try
        {
            var generationOption = Convert.ToInt32(Console.ReadLine());

            if (generationOption is < 1 or > 2)
            {
                throw new InvalidDataException();
            }

            GenerationOption = generationOption;
        }
        catch (Exception)
        {
            Console.WriteLine("Incorrect option!");
            throw;
        }
        
        _watcher.Start();
    }

    /// <summary>
    /// Method for generating large file with selected option.
    /// </summary>
    public static void Generate()
    {
        LoadPhrases();

        var rnd = new Random();
        using var writer = new StreamWriter(GetFileName(), false);
        long fileSize = 0;
        
        while (fileSize < GetLongSize())
        {
            var line = $"{rnd.Next(500000)}. {_phrases?[rnd.Next(0, _phrases.Count - 1)]}";
            fileSize += line.Length+EolLength;
            writer.WriteLine(line);
        }
    }

    /// <summary>
    /// Method which run after generation file.
    /// </summary>
    public static void PostGeneration()
    {
        _watcher.Stop();
        Console.WriteLine($"File generated successfully. File name is {GetFileName()}");
        Console.WriteLine($"Execution Time: {_watcher.ElapsedMilliseconds / 1000f} s.");
    }
    
    private static string GetFileName()
    {
        return $"LargeFile_{GetFileSize()}_{FileDateTime}.txt";
    }

    private static string GetFileSize()
    {
        return GenerationOption switch
        {
            2 => "10Gb",
            _ => "1Gb"
        };
    }
    
    private static long GetLongSize()
    {
        const long oneGb = 1024 * 1024 * 1024;
        return GenerationOption switch
        {
            2 => 10*oneGb,
            _ => oneGb
        };
    }

    private static void LoadPhrases()
    {
        using var reader = new StreamReader("Phrases.txt");
        _phrases = new List<string>();

        while (reader.ReadLine() is { } line)
        {
            _phrases.Add(line);
        }
    }
}