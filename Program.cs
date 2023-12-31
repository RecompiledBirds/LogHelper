﻿// See https://aka.ms/new-console-template for more information
using LogHelper;
using System.IO;
using System.Reflection;

internal class Program
{
    private static bool autoMode = true;

    public static string LogHelperPath
    {
        get
        {
            string res = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LogHelper/");
            if (!Directory.Exists(res)) Directory.CreateDirectory(res);
            return res;
        }
    }
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Full automatic mode?");
        Logger.Log("- Full automatic mode will attempt to identify log types itself, with no input");
        if (!Logger.GetYNInput("")) autoMode = false;
        Logger.GetYNInput("Read entire directory of log files?");
        Console.WriteLine("File path to read from:");
        string path = Console.ReadLine().Replace("\"", "");
        ReadFile(path);
    }

    private static void ReadFile(string path)
    {
        if (path == null || !Path.Exists(path))
        {
            Console.WriteLine($"That path does not exist!");
            return;
        }
        Console.WriteLine($"Loading {path}.");
        FileStream stream = File.OpenRead(path);
        Console.WriteLine($"Opened {path}.");
        string extenstion = Path.GetExtension(path);
        Logger.Debug = true;
        Logger.DBGLog($"Found extension: {extenstion}");
        FileType fileType = FileTypeReader.GetFileType(extenstion);

        Logger.Log($"File read mode is {fileType}.");
        if (!autoMode)
        {
            if (Logger.GetYNInput("Select alternative mode?"))
            {

                Console.WriteLine("Please type mode: ");
                for (FileType i = FileType.XML; i < FileType.OTHER; i++)
                {
                    Logger.Log($"- {i}");
                }
                fileType = FileTypeReader.GetFileType(Console.ReadLine());
                Logger.Log("New mode selected.");
            }

        }

        if (fileType == FileType.XML)
        {
            //Get XMLAnaylzers

        }
        if (fileType == FileType.EVTX)
        {
            //Get EVTXAnaylzers
            Logger.Log("Attempting to build EVTX anaylzer list.");
            List<EVTXAnaylzer> anaylzers=new List<EVTXAnaylzer>() { };
            foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (type.BaseType == typeof(EVTXAnaylzer))
                        try
                        {
                            anaylzers.Add((EVTXAnaylzer)Activator.CreateInstance(type));
                        }
                        catch (Exception ex)
                        {
                            Logger.DBGLog($"Exception building EVTX anaylzer: {ex}");
                        }
                }
            }
            Logger.Log($"Built {anaylzers.Count} EVTX anaylzers.");
            List<EVTXAnaylzer> anaylzersFoundData = new List<EVTXAnaylzer>();
            foreach(EVTXAnaylzer anaylzer in anaylzers)
            {
                Logger.Log($"Running analysis: {anaylzer.Name()}");

                anaylzer.DoAnaylsis(stream);
                if (anaylzer.FoundPotentialData())
                {
                    anaylzersFoundData.Add(anaylzer);
                }
               // Logger.Log($"{anaylzer.Name()} found potentially interesting data!", condition: anaylzer.FoundPotentialData());
            }

            Logger.Log($"Found {anaylzersFoundData.Count} results.");

            foreach(EVTXAnaylzer anaylzer in anaylzersFoundData)
            {
                Logger.Log("");
                Logger.Log($"=========={anaylzer.Name()}==========");
                anaylzer.DisplayData();
            }
        }
        if (fileType == FileType.KML)
        {
            //Get KMLAnaylzers
        }
        if (fileType == FileType.OTHER)
        {
            //Get FileAnaylzers
        }



        stream.Close();
    }
}