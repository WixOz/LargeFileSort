using System;
using System.IO;

namespace LargeFileGenerator.Services;

public static class GenerationService
{
    private static int GenerationOption { get; set; } = 1;
    private static string FileDateTime { get; } = $"{DateTime.UtcNow:ddMMyy_hhmmss}"; 

    /// <summary>
    /// Method which run before generation file.
    /// Method implements logic of files size  options choosing.
    /// </summary>
    /// <exception cref="InvalidDataException">Throws when file size option was incorrect.</exception>
    public static void PreGeneration()
    {
        Console.WriteLine("Generator started. Choose one option:\n1) 1Gb file.\n2) 10Gb file.\n3) 100Gb file.\n\nYour option:");
    
        try
        {
            var generationOption = Convert.ToInt32(Console.ReadLine());

            if (generationOption is < 1 or > 3)
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
    }

    public static void Generate()
    {
        
    }

    /// <summary>
    /// Method which run after generation file.
    /// </summary>
    public static void PostGeneration()
    {
        Console.WriteLine($"File generated successfully. File name is {GetFileName()}");
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
            3 => "100Gb",
            _ => "1Gb"
        };
    }
}