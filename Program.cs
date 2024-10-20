// See https://aka.ms/new-console-template for more information
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
    private static (bool,int) GetAnyArg(string val)
    {
        if(arguments==null)return (false, -1);
        if (arguments.Length == 0) return (false,-1);
        for(int i = 0; i < arguments.Length; i++)
        {
            if (GetArgIsVal(i,val)) return (true,i);
        }
        return (false,-1);
    }
    private static bool GetArgIsVal(int index, string val)
    {
        return GetArg(index).ToLower()== val.ToLower();
    }
    private static string[]? arguments;
    private static string GetArg(int index)
    {
        if (arguments == null) return string.Empty;
        if (arguments.Length == 0) return string.Empty;
        if (arguments.Length < index - 1) return string.Empty;
        return arguments[index];
    }
    private static void Main(string[] args)
    {
        arguments = args;
        if (!GetAnyArg("a").Item1 && !GetAnyArg("m:").Item1)
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("Full automatic mode?");
            Logger.Log("- Full automatic mode will attempt to identify log types itself, with no input");
            if (!Logger.GetYNInput("")) autoMode = false;
           
        }
        else
        {
            autoMode = true;
           
        }
        (bool,int) fileArg=GetAnyArg("f:");

        string? path = null;
        if (fileArg.Item1)
        {
            path = GetArg(fileArg.Item2+1);

        }
        if (path == null)
        {
           // Logger.GetYNInput("Read entire directory of log files?");
            Console.WriteLine("File path to read from:");
            path = ReadLine().Replace("\"", "");
        }
        ReadFile(path);
    }
    private static string ReadLine()
    {
       return Console.ReadLine()??string.Empty;
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
        (bool, int) modeArg = GetAnyArg("m:");
        if (modeArg.Item1)
        {
            autoMode = true;
            extenstion = GetArg(modeArg.Item2 + 1);
        }
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
                fileType = FileTypeReader.GetFileType(ReadLine());
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
            List<EVTXAnaylzer?> anaylzers=new() { };
            foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (type.BaseType == typeof(EVTXAnaylzer))
                    {
                        try
                        {
                            anaylzers.Add((EVTXAnaylzer?)Activator.CreateInstance(type));
                        }
                        catch (Exception ex)
                        {
                            Logger.DBGLog($"Exception building EVTX anaylzer: {ex}");
                        }
                    }
                }
            }
            Logger.Log($"Built {anaylzers.Count} EVTX anaylzers.");
            List<EVTXAnaylzer> anaylzersFoundData = new();
            foreach(EVTXAnaylzer? anaylzer in anaylzers)
            {
                if(anaylzer == null) { continue; }
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