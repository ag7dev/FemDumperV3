using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;



class Program
{

    static string[] AnticheatKeywords = {
            "Anticheat", "Godmode", "Noclip", "Eulen", "Detection", "Shield",
            "Fiveguard", "Noclip", "deltax", "waveshield", "spaceshield",
            "mixas", "protected", "cheater", "cheat", "banNoclip", "Detects",
            "blacklisted", "CHEATER BANNED:", "core_shield", "freecam"
        };


    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();
    static Random _random = new Random();
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly Regex webhookPattern = new Regex(@"https://\w+.discord\.com/api/webhooks/\w+/\w+", RegexOptions.Compiled);

    static void ClearScreen()
    {
        // check os
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            // (Linux/Mac)
            Console.Clear();
        }
        else if (Environment.OSVersion.Platform == PlatformID.Win32NT ||
                 Environment.OSVersion.Platform == PlatformID.Win32S ||
                 Environment.OSVersion.Platform == PlatformID.WinCE)
        {
            // Windows
            Console.Clear();
        }
    }

    static void TypeWriterAnimation(string text, double delay = 0.05)
    {
        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep((int)(delay * 600));
        }
        Console.WriteLine();
    }

    static void LoadingScreen()
    {
        ClearScreen();
        Console.WriteLine("Loading:");

        string[] animation = {
            "[■□□□□□□□□□]", "[■■□□□□□□□□]", "[■■■□□□□□□□]", "[■■■■□□□□□□]",
            "[■■■■■□□□□□]", "[■■■■■■□□□□]", "[■■■■■■■□□□]", "[■■■■■■■■□□]",
            "[■■■■■■■■■□]", "[■■■■■■■■■■]"
        };

        // loading animation. doesnt do anything is just for good design. and i was bored
        for (int i = 0; i < animation.Length; i++)
        {
            Thread.Sleep(200);
            Console.Write("\r" + animation[i % animation.Length]);
        }

        Console.WriteLine();
        ClearScreen();
    }

    static void GenerateRandomTitle()
    {
        while (true)
        {
            string randomTitle = new string(Enumerable.Range(0, 10)
                .Select(_ => (char)_random.Next(65, 91)) // ASCII-letters A-Z
                .ToArray());


            string randomTitleDone = (randomTitle + " made by literally.ag7 " + randomTitle);
            Console.Title = randomTitleDone;
            Thread.Sleep(50);
        }
    }

    static void Terminate()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Programm will terminate in 3 Secs...");
        Console.ResetColor();

        Thread.Sleep(3000);

        Process.GetCurrentProcess().Kill();
    }

    static void DisplayTitle()
    {
        ConsoleColor[] colors = {
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.Cyan,
            ConsoleColor.White
        };

        ConsoleColor randomColor = colors[_random.Next(colors.Length)];

        string title = @"
###################################################################### 
  ______             _____                                  
 |  ____|           |  __ \                                 
 | |__ ___ _ __ ___ | |  | |_   _ _ __ ___  _ __   ___ _ __ 
 |  __/ _ \ '_ ` _ \| |  | | | | | '_ ` _ \| '_ \ / _ \ '__| 
 | | |  __/ | | | | | |__| | |_| | | | | | | |_) |  __/ |   
 |_|  \___|_| |_| |_|_____/ \__,_|_| |_| |_| .__/ \___|_|   
                                           | |              
                                           |_|              

                                        Made by ag7-dev.de with <3
THIS PROJECT IS OPEN SOURCE AND FREE!
IF YOU PAYD FOR THIS YOU GOT SCAMMED! 
######################################################################                                                      
        ";

        Console.ForegroundColor = randomColor;
        Console.WriteLine(title);
        Console.ResetColor();
    }

    static async Task Main(string[] args)
    {
        //set variables and init logic
        IntPtr hwnd = GetConsoleWindow();
        string DumpPath = "None";
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string femdumperFolder = Path.Combine(desktopPath, "FemDumper");
        try
        {
            if (Directory.Exists(femdumperFolder))
            {
                Directory.Delete(femdumperFolder, true);
            }

            Directory.CreateDirectory(femdumperFolder);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[ + ] Directory created successfully.");
        }
        catch (UnauthorizedAccessException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ ! ] Permission denied. You don't have the necessary permissions to create or delete the directory.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ ! ] Error: " + ex.Message);
        }
        finally
        {
            Console.ResetColor();
        }

        //file paths
        string WebhookFilePath = Path.Combine(femdumperFolder, "discord_webhooks.txt");
        string VarFilePath = Path.Combine(femdumperFolder, "variables.txt");
        string TriggerFilePath = Path.Combine(femdumperFolder, "trigger_events.txt");
        string AcKeywordsFilePath = Path.Combine(femdumperFolder, "anticheat_keywords.txt");
        string AcsFoundsFilePath = Path.Combine(femdumperFolder, "acs_founds.txt");
        string CoordsFoundsFilePath = Path.Combine(femdumperFolder, "coords_founds.txt");


        string[] foldersToIgnore = { "monitor", "easyadmin" };
        string[] extensionsToSearch = { ".lua", ".html", ".js", ".json" };


        // Random Title.
        Thread randomTitleThread = new Thread(GenerateRandomTitle);
        randomTitleThread.Start();

        //loadingscreen
        LoadingScreen();

        while (true)
        {
            ClearScreen();
            DisplayTitle();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Current Path: " + DumpPath);
            Console.ResetColor();
            Console.WriteLine();

            // Menu
            Console.WriteLine("######################################");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Select Path");
            Console.WriteLine("2. Find Triggers");
            Console.WriteLine("3. Find Discord Webhooks");
            Console.WriteLine("4. Try to Find Anticheat");
            Console.WriteLine("5. Find Variables");
            Console.WriteLine("6. Find Coordinates");
            Console.WriteLine("7. Run All Scans");
            Console.WriteLine("8. Delete Webhooks");
            Console.WriteLine("9. Get Webhook Informations");
            Console.WriteLine("10. Exit");
            Console.WriteLine("######################################");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[>] ");
            Console.ResetColor();
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ClearScreen();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($"\r[ i ] Set Path");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("Input Server dump folder: ");
                    Console.ResetColor();
                    DumpPath = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.ResetColor();

                    // Path check
                    CheckDirectory(DumpPath);
                    break;

                case "2":
                    if (DumpPath == "None")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ ! ] Please set the server dump folder path first.");
                        Console.ResetColor();
                    }
                    else
                    {
                        ClearScreen();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\r[ i ] You choose Find Triggers");
                        Console.ResetColor();
                        FindAndListTriggerEvents(DumpPath, TriggerFilePath);
                    }
                    break;

                case "3":
                    if (DumpPath == "None")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ ! ] Please set the server dump folder path first.");
                        Console.ResetColor();
                    }
                    else
                    {
                        ClearScreen();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\r[ i ] You choose Find Discord Webhooks");
                        Console.ResetColor();
                        await FindDiscordWebhooks(DumpPath, WebhookFilePath);
                    }
                    break;

                case "4":
                    if (DumpPath == "None")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ ! ] Please set the server dump folder path first.");
                        Console.ResetColor();
                    }
                    else
                    {
                        ClearScreen();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\r[ i ] You choose Find ACS");
                        Console.ResetColor();
                        CheckForACS(DumpPath, AcKeywordsFilePath, AcsFoundsFilePath);
                    }
                    break;

                case "5":
                    if (DumpPath == "None")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ ! ] Please set the server dump folder path first.");
                        Console.ResetColor();
                    }
                    else
                    {
                        ClearScreen();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\r[ i ] You choose Find Variables");
                        Console.ResetColor();
                        FindAndListVariables(DumpPath, VarFilePath);
                    }
                    break;

                case "6":
                    if (DumpPath == "None")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ ! ] Please set the server dump folder path first.");
                        Console.ResetColor();
                    }
                    else
                    {
                        ClearScreen();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\r[ i ] You choose To search for Coordinates.");
                        Console.ResetColor();
                        FindAndListCoords(DumpPath, CoordsFoundsFilePath);
                    }
                    break;

                case "7":
                    if (DumpPath == "None")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ ! ] Please set the server dump folder path first.");
                        Console.ResetColor();
                    }
                    else
                    {
                        ClearScreen();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\r[ i ] You choose To Run all Scans, this can take a while.");
                        Console.ResetColor();
                        FindAndListTriggerEvents(DumpPath, TriggerFilePath);
                        await FindDiscordWebhooks(DumpPath, WebhookFilePath);
                        CheckForACS(DumpPath, AcKeywordsFilePath, AcsFoundsFilePath);
                        FindAndListVariables(DumpPath, VarFilePath);
                    }
                    break;

                case "8":
                    if (DumpPath == "None")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ ! ] Please set the server dump folder path first.");
                        Console.ResetColor();
                    }
                    else
                    {
                        ClearScreen();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\r[ i ] You choose to Delete Webhooks");
                        Console.ResetColor();
                        await LoadAndDeleteWebhooksAsync(WebhookFilePath);
                    }
                    break;

                case "9":
                    if (DumpPath == "None")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ ! ] Please set the server dump folder path first.");
                        Console.ResetColor();
                    }
                    else
                    {
                        ClearScreen();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write($"\r[ i ] You choose show Webhook info");
                        Console.ResetColor();
                        await GetWebhookInfoAsync(WebhookFilePath);
                    }
                    break;

                case "10":
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($"\r[ i ] Exiting");
                    Console.ResetColor();
                    Thread.Sleep(1000);
                    Terminate();
                    Environment.Exit(0);
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ ! ] Invalid Input");
                    Console.ResetColor();
                    break;
            }
            Thread.Sleep(1000);
        }
    }

    static void CheckDirectory(string path)
    {
        if (Directory.Exists(path))
        {

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[ + ] Directory exists: {path}");
            Console.ResetColor();
        }
        else
        {

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ ! ] Directory does not exist: {path}");
            Console.ResetColor();
            Console.ReadLine();
        }
    }

    public static void FindAndListTriggerEvents(string path, string outputFile)
    {
        List<Tuple<string, int, string>> triggerEvents = new List<Tuple<string, int, string>>();
        int totalFiles = 0;
        int processedFiles = 0;


        foreach (var file in Directory.EnumerateFiles(path, "*.lua", SearchOption.AllDirectories))
        {
            totalFiles++;
        }

        try
        {
            using (StreamWriter output = new StreamWriter(outputFile, false, System.Text.Encoding.UTF8))
            {
                foreach (var filePath in Directory.EnumerateFiles(path, "*.lua", SearchOption.AllDirectories))
                {
                    string folderName = Path.GetFileName(Path.GetDirectoryName(filePath));
                    try
                    {
                        foreach (var line in File.ReadLines(filePath, System.Text.Encoding.GetEncoding("ISO-8859-1")))
                        {
                            if (Regex.IsMatch(line, @"\b(TriggerServerEvent|TriggerEvent)\b"))
                            {
                                int lineNumber = Array.IndexOf(File.ReadAllLines(filePath), line) + 1; // line index
                                triggerEvents.Add(new Tuple<string, int, string>(folderName, lineNumber, line.Trim()));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ ! ] Error reading file {filePath}: {e.Message}");
                        Console.ResetColor();
                    }

                    processedFiles++;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($"\r[ i ] Processing files: {processedFiles}/{totalFiles}");
                    Console.ResetColor();
                }


                foreach (var eventInfo in triggerEvents)
                {
                    output.WriteLine($"\n{'=' * 25} [{eventInfo.Item1} - Line {eventInfo.Item2}] {'=' * 25}");
                    output.WriteLine(eventInfo.Item3);
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[ + ] Processing complete.");
            Console.ResetColor();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ ! ] Error opening output file: {e.Message}");
            Console.ResetColor();
        }
    }

    public static async Task FindDiscordWebhooks(string path, string outputFile)
    {
        List<Tuple<string, string>> webhookUrls = new List<Tuple<string, string>>();
        int totalFiles = 0;
        int processedFiles = 0;

        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
        {
            totalFiles++;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\r[ i ] Searching for Webhooks...");
        Console.ResetColor();


        foreach (var filePath in Directory.EnumerateFiles(path, "*.lua", SearchOption.AllDirectories))
        {
            try
            {
                string content = File.ReadAllText("beispiel.txt");
                var matches = webhookPattern.Matches(content);

                foreach (Match match in matches)
                {
                    if (await IsWebhookValid(match.Value))
                    {
                        webhookUrls.Add(new Tuple<string, string>(filePath, match.Value));
                    }
                }

                processedFiles++;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($"\r[ i ] Processing files: {processedFiles}/{totalFiles}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ ! ] Error processing file: {filePath}. Error: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n-------------------------------------------------------------------------------");
        Console.WriteLine("Webhook search complete.");
        Console.WriteLine("-------------------------------------------------------------------------------");
        Console.ResetColor();


        using (StreamWriter output = new StreamWriter(outputFile, false, System.Text.Encoding.UTF8))
        {
            output.WriteLine($"{"File Path",-60} | Webhook URL");
            output.WriteLine(new string('-', 80));
            foreach (var webhook in webhookUrls)
            {
                output.WriteLine($"{webhook.Item1,-60} | {webhook.Item2}");
            }
        }
    }


    private static async Task<bool> IsWebhookValid(string url)
    {
        try
        {
            var response = await httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }


    private static readonly HttpClient _httpClient = new HttpClient();
    static async Task DeleteWebhookAsync(string webhookUrl)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(webhookUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[ + ] Webhook successfully deleted: " + webhookUrl);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ ! ] Error deleting webhook {0}: {1}", webhookUrl, response.StatusCode);
            }
        }
        catch (HttpRequestException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ ! ] Error deleting webhook {0}: {1}", webhookUrl, e.Message);
        }
        finally
        {
            Console.ResetColor();
        }
    }

    static Task LoadAndDeleteWebhooksAsync(string webhookFilePath)
    {
        try
        {
            List<string> webhooks = new List<string>();

            string[] lines = File.ReadAllLines(webhookFilePath); 

            for (int i = 1; i < lines.Length; i++) 
            {
                var parts = lines[i].Split('|'); 
                if (parts.Length > 1)
                {
                    webhooks.Add(parts[1].Trim());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ ! ] Warning: Skipping line due to unexpected format: " + lines[i]);
                    Console.ResetColor();
                }
            }

        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ ! ] Error loading webhooks from file: " + e.Message);
            Console.ResetColor();
        }

        return Task.CompletedTask;
    }

    static async Task GetWebhookInfoAsync(string webhookFilePath)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("-------------------------------------------------------------------------------");
        Console.ResetColor();

        try
        {
            var lines = File.ReadAllLines(webhookFilePath);

            List<string> webhookUrls = new List<string>();
            foreach (var line in lines.Skip(1)) 
            {
                var parts = line.Split('|');
                if (parts.Length > 1)
                {
                    webhookUrls.Add(parts[1].Trim());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ ! ] Warning: Skipping line due to unexpected format: " + line);
                    Console.ResetColor();
                }
            }

            foreach (var url in webhookUrls)
            {
                await ValidateAndDisplayWebhookInfoAsync(url);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Press Enter to continue to the next webhook...");
                Console.ResetColor();
                Console.ReadLine();
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ ! ] Error loading webhooks from file: " + e.Message);
            Console.ResetColor();
            await Task.Delay(3000);
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("-------------------------------------------------------------------------------");
        Console.ResetColor();
    }


    private static async Task ValidateAndDisplayWebhookInfoAsync(string webhookUrl)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Checking Webhook: {webhookUrl}");
        Console.ResetColor();

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(webhookUrl);

            if (response.IsSuccessStatusCode)
            {

                var webhookData = await response.Content.ReadAsStringAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Webhook information:");
                Console.WriteLine(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(webhookData), Newtonsoft.Json.Formatting.Indented));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ ! ] Error retrieving webhook information: {response.StatusCode}");
                Console.ResetColor();
                await Task.Delay(3000);
            }
        }
        catch (HttpRequestException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ ! ] Error retrieving webhook information: {e.Message}");
            Console.ResetColor();
            await Task.Delay(3000);
        }
    }

    public static void FindAndListVariables(string path, string outputFile)
    {
        List<Tuple<string, int, string>> variablesList = new List<Tuple<string, int, string>>();

        foreach (var filePath in Directory.EnumerateFiles(path, "*.lua", SearchOption.AllDirectories))
        {
            string folderName = Path.GetFileName(Path.GetDirectoryName(filePath));

            try
            {
                foreach (var line in File.ReadLines(filePath, System.Text.Encoding.GetEncoding("ISO-8859-1")))
                {
                    if (Regex.IsMatch(line, @"\bvar_\w+\b"))
                    {
                        int lineNumber = Array.IndexOf(File.ReadAllLines(filePath), line) + 1;

                        variablesList.Add(new Tuple<string, int, string>(folderName, lineNumber, line.Trim()));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ ! ] Error reading file {filePath}: {ex.Message}");
                Console.ResetColor();
            }
        }

        try
        {
            using (StreamWriter output = new StreamWriter(outputFile, true, System.Text.Encoding.UTF8))
            {
                output.WriteLine("\nVariables:");
                foreach (var variable in variablesList)
                {
                    output.WriteLine($"[{variable.Item1}] - [Line {variable.Item2}] {variable.Item3}");
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[ + ] Variables successfully listed and saved.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ ! ] Error writing to the output file: {ex.Message}");
            Console.ResetColor();
        }
    }


    public static void FindAndListCoords(string path, string outputFile)
    {
        List<Tuple<string, int, string>> coordsList = new List<Tuple<string, int, string>>();

        foreach (var filePath in Directory.EnumerateFiles(path, "*.lua", SearchOption.AllDirectories))
        {
            string folderName = Path.GetFileName(Path.GetDirectoryName(filePath));

            try
            {
                foreach (var line in File.ReadLines(filePath, System.Text.Encoding.GetEncoding("ISO-8859-1")))
                {
                    // Regex für Vector3 und Vector4 Koordinaten
                    if (Regex.IsMatch(line, @"\bvector[34]\s*\(\s*-?\d+(\.\d+)?\s*,\s*-?\d+(\.\d+)?\s*,\s*-?\d+(\.\d+)?(\s*,\s*-?\d+(\.\d+)?)?\s*\)"))
                    {
                        int lineNumber = Array.IndexOf(File.ReadAllLines(filePath), line) + 1;

                        coordsList.Add(new Tuple<string, int, string>(folderName, lineNumber, line.Trim()));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ ! ] Error reading file {filePath}: {ex.Message}");
                Console.ResetColor();
            }
        }

        try
        {
            using (StreamWriter output = new StreamWriter(outputFile, true, System.Text.Encoding.UTF8))
            {
                output.WriteLine("\nCoordinates:");
                foreach (var coord in coordsList)
                {
                    output.WriteLine($"[{coord.Item1}] - [Line {coord.Item2}] {coord.Item3}");
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[ + ] Coordinates successfully listed and saved.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ ! ] Error writing to the output file: {ex.Message}");
            Console.ResetColor();
        }
    }



    // defining ac keywords.
    static readonly Dictionary<string, string> FilesToCheck = new Dictionary<string, string>
    {
        { "shared_fg-obfuscated.lua", "FiveGuard" },
        { "fini_events.lua", "FiniAC" },
        { "c-bypass.lua", "Reaper-AC" },
        { "waveshield.lua", "WaveShield" }
    };


    //combinng both. 
    public static void CheckForACS(string path, string outputFileKeywords, string outputFileAC)
    {
        //check for known acs. 
        CheckForACFiles(path, outputFileAC);

        // anticheat keywords
        CheckForAnticheatKeywords(path, outputFileKeywords);
    }

    // find ac files (known) (waveshield, fini, fg, reaper.) in the dump files. 
    private static void CheckForACFiles(string path, string outputFile)
    {
        using (var output = new StreamWriter(outputFile, true, System.Text.Encoding.UTF8))
        {
            foreach (var fileToCheck in FilesToCheck)
            {
                output.WriteLine("#######################################################################################");
                output.WriteLine($"Checking for {fileToCheck.Key}:");
                output.WriteLine("#######################################################################################");

                bool fileFound = false;
                foreach (var file in Directory.GetFiles(path, fileToCheck.Key, SearchOption.AllDirectories))
                {
                    string folderName = Path.GetFileName(Path.GetDirectoryName(file));
                    output.WriteLine("#######################################################################################");
                    output.WriteLine($"{fileToCheck.Value} Detected AC: ( {folderName} )");
                    output.WriteLine("#######################################################################################");
                    fileFound = true;
                }

                if (!fileFound)
                {
                    output.WriteLine($"No {fileToCheck.Value} AC file found.");
                }
            }
        }
    }

    // funktion to check for anticheat keywords
    private static void CheckForAnticheatKeywords(string path, string outputFile)
    {
        bool anticheatFound = false;
        int totalFiles = Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories).Length;
        int processedFiles = 0;

        using (var output = new StreamWriter(outputFile, false, System.Text.Encoding.UTF8))
        {
            output.WriteLine("#######################################################################################");
            output.WriteLine("Searching for Anti-Cheat keywords...");
            output.WriteLine("#######################################################################################");

            foreach (var filePath in Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories))
            {
                bool keywordFoundInFile = false;
                string folderName = Path.GetFileName(Path.GetDirectoryName(filePath));

                foreach (var line in File.ReadLines(filePath))
                {
                    foreach (var keyword in AnticheatKeywords)
                    {
                        if (line.Contains(keyword))
                        {
                            output.WriteLine($"[{folderName}] - [Line {Array.IndexOf(File.ReadAllLines(filePath), line) + 1}] {line.Trim()}");
                            keywordFoundInFile = true;
                            anticheatFound = true;
                            break;
                        }
                    }

                    if (keywordFoundInFile) break; // stop searching for anticheats in this file. || thinking about deleting this. 
                }

                processedFiles++;
                Console.Write($"\r[ i ] Processing files: {processedFiles}/{totalFiles}");

                if (anticheatFound)
                    break;
            }

            if (!anticheatFound)
            {
                output.WriteLine("[ ! ] No Anti-Cheat keywords found. But that does not mean there is none.");
            }
        }
    }
}