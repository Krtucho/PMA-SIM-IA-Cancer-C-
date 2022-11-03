using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using AutomataLibrary2D;
using TEdge = QuickGraph.UndirectedEdge<string>;
using TVertex = System.String;
using TEdgeString = System.String;

namespace NetworkTest
{
    class Program
    {
        //general settings
        private static int _tests = 5;
        private static NetworkSettings _networkSettings = null;

        //directories
        private static string _testFile = string.Empty;
        private static string _dataFolder = string.Empty;
        private static string _testId = string.Empty;

        //network settings
        private static int _gridSizeX = 40;
        private static int _gridSizeY = 20;
        private static double _p0 = 0.0001;
        private static double _pf = 1;
        private static double _r = Math.Sqrt(2);
        private static bool _periodic = false;

        static void Main(string[] args)
        {
            int time_start = Environment.TickCount;
            Notification.PrintProgramLabel();
            CreateDataFolder();
            ReadSettings();
            List<TVertex> template = MF.GetRegularNeighboursTemplate(_r);
            List<TVertex> ftemplate = MF.FilterRegularNeighboursTemplate(template);
            PrintSettings(template, ftemplate);
            CreateTestFile();
            StreamWriter stream_writer = new StreamWriter(_testFile);
            WriteFileOutline(template, ftemplate, stream_writer);
            List<double> ps = new List<double>() { 0 };
            for (double i = _p0; i != _pf * 10; i *= 10)
                ps.Add(i);
            for (int j = 0; j < ps.Count; j++)
            {
                stream_writer.Write(ps[j] + ":");
                Console.WriteLine("main: testing network with p=" + ps[j]);
                for (int i = 0; i < _tests; i++)
                {
                    Console.WriteLine("main: starting test number: " + (i + 1));
                    _networkSettings = new NetworkSettings(_gridSizeX, _gridSizeY, -1, ps[j], _r, true, _periodic);
                    double pathlenght = 0;
                    double clustering = 0;
                    BuildNetwork(out pathlenght, out clustering);
                    stream_writer.Write("(" + pathlenght + "/" + clustering + ")");
                    if (i != _tests - 1) stream_writer.Write(" ");
                    Console.WriteLine("main: finished test number: " + (i + 1));
                }
                Console.WriteLine("main: finished network test with p=" + ps[j] + "\n");
                stream_writer.Write("\n");
            }
            stream_writer.Write("\n");
            stream_writer.Write("[EOF]");
            stream_writer.Close();
            int time_elapsedmiliseconds = Environment.TickCount - time_start;
            string formatted_time = Notification.TimeStamp(time_elapsedmiliseconds);
            Console.WriteLine("main: program finished" + formatted_time);
            Console.ReadLine();
        }

        private static void BuildNetwork(out double pathlenght, out double clustering)
        {
            Console.WriteLine("main: creating network...");
            int time_start = Environment.TickCount;
            SmallWorldNetwork wattsnetwork = new SmallWorldNetwork(_networkSettings);
            int time_elapsedmiliseconds = Environment.TickCount - time_start;
            string formatted_time = Notification.TimeStamp(time_elapsedmiliseconds);
            Console.WriteLine("main: finished creating network" + formatted_time);
            pathlenght = wattsnetwork.AveragePathLength;
            clustering = wattsnetwork.ClusteringCoefficient;
            wattsnetwork.Dispose();
        }
        private static void WriteFileOutline(List<TVertex> template, List<TVertex> ftemplate, StreamWriter stream_writer)
        {
            stream_writer.WriteLine("[Radiant Network Test]");
            stream_writer.WriteLine("testid = " + _testId);
            stream_writer.Write("\n");
            stream_writer.WriteLine("[Settings]");
            stream_writer.WriteLine("grid_size_x = " + _gridSizeX);
            stream_writer.WriteLine("grid_size_y = " + _gridSizeY);
            stream_writer.WriteLine("r = " + _r);
            stream_writer.WriteLine("p0 = " + _p0);
            stream_writer.WriteLine("pf = " + _pf);
            stream_writer.WriteLine("tests = " + _tests);
            stream_writer.WriteLine("periodic = " + _periodic);
            stream_writer.Write("\n");
            stream_writer.WriteLine("[Tests]");
        }
        private static void CreateTestFile()
        {
            DateTime time = DateTime.Now;
            string processed_time = time.Year.ToString() + time.Month.ToString() + time.Day.ToString() + time.Hour.ToString() + time.Minute.ToString() + time.Second.ToString();
            _testFile = _dataFolder + "\\" + processed_time + ".nettest";
            Console.WriteLine("main: test file created - with path: \"" + _testFile + "\"\n");
            _testId = processed_time;
        }
        private static void PrintSettings(List<TVertex> template, List<TVertex> ftemplate)
        {
            Console.WriteLine("main: setttings:");
            Console.WriteLine("--grid_size_x = " + _gridSizeX);
            Console.WriteLine("--grid_size_y = " + _gridSizeY);
            Console.WriteLine("--r = " + _r);
            Console.WriteLine("--p0 = " + _p0);
            Console.WriteLine("--pf = " + _pf);
            Console.WriteLine("--tests = " + _tests);
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
            if (textbody[linenumber] == "[NextGen Network Test Settings]")
            {
                linenumber++;
                while (linenumber < textbody.Length)
                {
                    string removed_blank_spaces = MF.RemoveBlankSpaces(textbody[linenumber]);
                    string[] splitted = removed_blank_spaces.Split('=');
                    switch (splitted[0])
                    {
                        case "tests":
                            _tests = int.Parse(splitted[1]);
                            break;
                        case "grid_size_x":
                            _gridSizeX = int.Parse(splitted[1]);
                            break;
                        case "grid_size_y":
                            _gridSizeY = int.Parse(splitted[1]);
                            break;
                        case "p0":
                            _p0 = double.Parse(splitted[1]);
                            break;
                        case "r":
                            _r = double.Parse(splitted[1]);
                            break;
                        case "periodic":
                            _periodic = bool.Parse(splitted[1]);
                            break;
                        case "pf":
                            _pf = double.Parse(splitted[1]);
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
