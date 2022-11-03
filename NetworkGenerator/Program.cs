using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QuickGraph;
using AutomataLibrary2D;
using TEdge = QuickGraph.UndirectedEdge<string>;
using TVertex = System.String;
using TEdgeString = System.String;

namespace NetworkGenerator
{
    class Program
    {
        //general settings
        private static int _amount = 5;
        private static SmallWorldNetwork _wattsNetwork = null;
        private static NetworkSettings _networkSettings = null;

        //directories
        private static string _networkFile = string.Empty;
        private static string _dataFolder = string.Empty;
        private static string _networkId = string.Empty;

        //network settings
        private static int _gridSizeX = 750;
        private static int _gridSizeY = 375;
        private static int _gridDivision = 375;
        private static double _p = 1 * Math.Pow(10, -2);
        private static double _r = Math.Sqrt(2);
        private static bool _periodic = false;

        static void Main(string[] args)
        {
            int time_start = Environment.TickCount;
            Notification.PrintProgramLabel();
            CreateDataFolder();
            // ReadSettings();
            List<TVertex> template = MF.GetRegularNeighboursTemplate(_r);
            List<TVertex> ftemplate = MF.FilterRegularNeighboursTemplate(template);
            _networkSettings = new NetworkSettings(_gridSizeX, _gridSizeY, _gridDivision, _p, _r, false, _periodic);
            PrintSettings(template, ftemplate);
            for (int i = 0; i < _amount; i++)
            {
                Console.WriteLine("main: creating network number: " + (i + 1));
                CreateNetworkFile();
                Thread.Sleep(1000);
                StreamWriter streamWriter = new StreamWriter(_networkFile);
                WriteFileOutline(template, ftemplate, streamWriter);
                BuildNetwork();
                WriteFileData(streamWriter);
                _wattsNetwork.Dispose();
                _wattsNetwork = null;
                streamWriter.Write("\n");
                streamWriter.Write("[EOF]");
                streamWriter.Close();
                streamWriter.Dispose();
                Console.WriteLine("main: finished network number: " + (i + 1) + "\n");
            }
            int time_elapsedmiliseconds = Environment.TickCount - time_start;
            string formatted_time = Notification.TimeStamp(time_elapsedmiliseconds);
            Console.WriteLine("main: program finished" + formatted_time);
            Console.ReadLine();
        }

        private static void WriteFileData(StreamWriter streamWriter)
        {
            Console.WriteLine("main: writting network data to file...");
            int timeStart = Environment.TickCount;
            streamWriter.WriteLine("[Vertexs]");
            int count = _wattsNetwork.Vertices.Count;
            dynamic ls = _wattsNetwork.Vertices;
            bool written = false;
            Console.WriteLine("main: writting vertexs...");
            for (int i = 0; i < count; i++)
            {
                Notification.CompletionNotification(i, count, ref written, "");
                streamWriter.WriteLine(ls[i]);
            }
            Notification.FinalCompletionNotification("");
            streamWriter.Write("\n");
            streamWriter.WriteLine("[Edges]");
            Console.WriteLine("main: writting edges...");
            ls = _wattsNetwork.Edges;
            written = false;
            count = _wattsNetwork.Edges.Count;
            for (int i = 0; i < count; i++)
            {
                Notification.CompletionNotification(i, count, ref written, "");
                streamWriter.WriteLine(ls[i]);
            }
            Notification.FinalCompletionNotification("");
            ls.Clear();
            int time_elapsedmiliseconds = Environment.TickCount - timeStart;
            string formatted_time = Notification.TimeStamp(time_elapsedmiliseconds);
            Console.WriteLine("main: finished writting network data to file" + formatted_time);
        }
        private static void BuildNetwork()
        {
            Console.WriteLine("main: creating network...");
            int timeStart = Environment.TickCount;
            _wattsNetwork = new SmallWorldNetwork(_networkSettings);
            int timeElapsedMiliseconds = Environment.TickCount - timeStart;
            string formattedTime = Notification.TimeStamp(timeElapsedMiliseconds);
            Console.WriteLine("main: finished creating network" + formattedTime);
        }
        private static void WriteFileOutline(List<TVertex> template, List<TVertex> ftemplate, StreamWriter streamWriter)
        {
            Console.WriteLine("main: writting file outline...");
            streamWriter.WriteLine("[Radiant Network File]");
            streamWriter.WriteLine("networkid = " + _networkId);
            streamWriter.Write("\n");
            streamWriter.WriteLine("[Settings]");
            streamWriter.WriteLine("grid_size_x = " + _gridSizeX);
            streamWriter.WriteLine("grid_size_y = " + _gridSizeY);
            streamWriter.WriteLine("grid_division = " + _gridDivision);
            streamWriter.WriteLine("r = " + _r);
            streamWriter.WriteLine("p = " + _p);
            streamWriter.WriteLine("periodic = " + _periodic);
            streamWriter.Write("\n");
            Console.WriteLine("main: finished writting file outline");
        }
        private static void CreateNetworkFile()
        {
            DateTime time = DateTime.Now;
            string processedTime = time.Year.ToString() + time.Month.ToString() + time.Day.ToString() + time.Hour.ToString() + time.Minute.ToString() + time.Second.ToString();
            _networkFile = _dataFolder + "\\" + processedTime + ".network";
            Console.WriteLine("main: test file created - with path: \"" + _networkFile);
            _networkId = processedTime;
        }
        private static void PrintSettings(List<TVertex> template, List<TVertex> ftemplate)
        {
            Console.WriteLine("main: setttings:");
            Console.WriteLine("--grid_size_x = " + _gridSizeX);
            Console.WriteLine("--grid_size_y = " + _gridSizeY);
            Console.WriteLine("--grid_division = " + _gridDivision);
            Console.WriteLine("--r = " + _r);
            Console.WriteLine("--p = " + _p);
            Console.WriteLine("--amount = " + _amount);
            Console.WriteLine("--periodic = " + _periodic);
            Console.WriteLine("--|template|=" + template.Count);
            Console.WriteLine("--|filteredtemplate|=" + ftemplate.Count + "\n");
        }
        private static void ReadSettings()
        {
            Console.WriteLine("main: reading settings file...");
            string path_settings = Directory.GetCurrentDirectory() + "\\Settings.txt";
            if (!File.Exists(path_settings))
                path_settings = Directory.GetCurrentDirectory() + "\\..\\..\\Settings.txt";
            if (!File.Exists(path_settings))
            {
                Console.WriteLine("main: failed reading settings file");
                Console.WriteLine("main: using default parameters");
                return;
            }
            string[] textbody = File.ReadAllLines(path_settings);
            int linenumber = 0;
            if (textbody[linenumber] == "[NextGen Network Generator Settings]")
            {
                linenumber++;
                while (linenumber < textbody.Length)
                {
                    string removed_blank_spaces = MF.RemoveBlankSpaces(textbody[linenumber]);
                    string[] splitted = removed_blank_spaces.Split('=');
                    switch (splitted[0])
                    {
                        case "amount":
                            _amount = int.Parse(splitted[1]);
                            break;
                        case "grid_size_x":
                            _gridSizeX = int.Parse(splitted[1]);
                            break;
                        case "grid_size_y":
                            _gridSizeY = int.Parse(splitted[1]);
                            break;
                        case "grid_division":
                            _gridDivision = int.Parse(splitted[1]);
                            break;
                        case "p":
                            _p = double.Parse(splitted[1]);
                            break;
                        case "r":
                            _r = double.Parse(splitted[1]);
                            break;
                        case "periodic":
                            _periodic = bool.Parse(splitted[1]);
                            break;
                        default:
                            Console.WriteLine("main: failed reading settings file");
                            Console.WriteLine("main: using default parameters");
                            return;
                    }
                    linenumber++;
                }
                Console.WriteLine("main: finished reading settings file");
            }
            else
            {
                Console.WriteLine("main: failed reading settings file");
                Console.WriteLine("main: using default parameters");
            }
        }
        private static void CreateDataFolder()
        {
            _dataFolder = Directory.GetCurrentDirectory() + "\\Data";
            Directory.CreateDirectory(_dataFolder);
            Console.WriteLine("main: data folder created - with path: \"" + _dataFolder + "\"\n");
        }
    }
}
