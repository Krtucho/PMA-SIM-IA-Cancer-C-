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

namespace ExperimentBatch
{
    class Program
    {
        //general settings
        private static bool _loadNetwork = false;
        private static SmallWorldNetwork _wattsNetwork = null;
        private static NetworkSettings _networkSettings = null;
        private static ModelSettings _modelSettings = null;
        private static ParametersSettings _parametersSettings = null;
        private static NutrientsSettings _nutrientSettings = null;
        private static CellularAutomata1_5 _automata = null;

        //directories
        private static string _experimentFolder = string.Empty;
        private static string _dataFolder = string.Empty;
        private static string _experimentID = string.Empty;
        private static string _networksFolder = string.Empty;
        private static List<string> _generationsFiles = null;
        private static List<string> _compressedFiles = null;

        //network settings
        private static int _gridSizeX = 1000;
        private static int _gridSizeY = 500;
        private static int _gridDivision = 500;
        private static double _p = 1 * Math.Pow(10, -2);
        private static double _r = Math.Sqrt(2);
        private static bool _periodic = false;
        
        //model settings
        private static int _runs = 10;

        // fastavasc min: ra = 0.05797, deltata = 16.93, na = 9, generations = 10, K_a = 69 !!!
        // fastavasc pro: ra = 0.01802, deltata = 29.96, na = 20, generations = 21, K_a = 222 !!!
        // fastavasc max: ra = 0.008097, deltata = 45.58, na = 34, generations = 35, K_a = 494 !!!

        // slowavasc min: ra = 0.005797, deltata = 28.54, na = 51, generations = 51 + 1 = 52, K_a = 69 !!!
        // slowavasc pro: ra = 0.001802, deltata = 54.97, na = 109, generations = 109 + 1 = 110, K_a = 222 !!!
        // slowavasc max: ra = 0.0008097, deltata = 86.04, na = 178, generations = 178 + 1 = 179, K_a = 494 !!!

        // fastvasc min: rv = 0.000144, deltatv = 540.1, na = 21, generations = 21 + 120 + 1 = 142, K_v = 27780, P0v = 222 !!!
        // fastvasc pro: rv = 0.00007199, deltatv = 766.5, na = 21, generations = 21 + 200 + 1 = 222, K_v = 55560, P0v = 222 !!!
        // fastvasc max: rv = 0.000036, deltatv = 1079, na = 21, generations = 21 + 320 + 1 = 342, K_v = 111100, P0v = 222 !!!

        // slowvasc min: rv = 0.0000144, deltatv = 1187, na = 21, generations = 21 + 564 + 1 = 586, K_v = 27780, P0v = 222 !!!
        // slowvasc pro: rv = 0.000007199, deltatv = 1659, na = 21, generations = 21 + 924 + 1 = 946, K_v = 55560, P0v = 222 !!!
        // slowvasc max: rv = 0.0000036, deltatv = 2332, na = 21, generations = 21 + 1480 + 1 = 1502, K_v = 111100, P0v = 222 !!!

        private static double _P0a = 1;
        private static double _Ka = 222;
        private static double _ra = 0.01802;
        private static double _deltata = 29.96;
        private static double _na = 21;
        private static int _generations = 322;
        
        private static double _P0v = 222;
        private static double _Kv = 55560;
        private static double _rv = 0.00007199;
        private static double _deltatv = 766.5;

        //parameters settings

        // fast high apparition : max prob = 0.1, eta_mig = 1, K_mig = 500040
        // slow low apparition : max prob = 0.01, eta_mig = 0.1, K_mig = 32491
        // params for apparition testing : mu_mig = 1, eta_mig_prima = 0.1, mu_max = 55
        private static double _eta_mig = 1;
        private static double _K_mig = 500040;

        // long migration : mu_max = 100
        // short migration : mu_max = 10
        // params for migration : max prob = 0.05, eta_mig = 0.5, K_mig = 192920
        private static double _mu_mig = 1;
        private static double _eta_mig_prima = 0.1;
        private static double _mu_max = 100;

        private static double _xi_sc = 0.0005; 

        // high metastasis potential : mu_max = 100, eta_mig = 1, K_mig = 500040
        // low metastasis potential : mu_max = 10, eta_mig = 0.1, K_mig = 32491
        // organs values: bones = 0.6, lungs = 0.34, liver = 0.2, breast = 0.1

        private static double _xi_mic0 = 1 - ((1 - 0.1) / _na); // 0.9571
        private static double _psi_mic0 = 0.1 / (double)100; // 0.001

        private static double _xi_mic1 = 1 - ((1 - 0.6) / _na);
        private static double _psi_mic1 = 0.6 / (double)100;

        private static int _simScale = 9;
        
        static void Main(string[] args)
        {
            int totalTimeStart = Environment.TickCount;
            _compressedFiles = new List<string>();
            Notification.PrintProgramLabel();
            CreateDataFolder();
            CreateBatchFolder();
            //ReadSettings();
            List<TVertex> template = MF.GetRegularNeighboursTemplate(_r);
            List<TVertex> ftemplate = MF.FilterRegularNeighboursTemplate(template);
            //PrintSettings(template, ftemplate);
            LoadOrBuildNetwork();
            for (int j = 0; j < _runs; j++)
            {               
                CreateExperimentFolder();

                // Tumor Settings

                // Basic Sim - Organ 1
                //int tumorPosX = 250; // _gridSizex/4
                //int tumorPosY = 250;
                //double tumorRadius = 0;
                //int organ1Lumen = 250; // _gridSizeY/2
                //int organ1Epith = 1;
                //int organ1Stroma = 249;
                //int organ1Epsilon = 25;
                //OrganScheme _organ0 = new OrganScheme1(organ1Lumen, organ1Epith, organ1Stroma, tumorPosX, tumorPosY, tumorRadius);
                //int[] region01 = new int[] { 0, _gridDivision, 0, organ1Lumen + organ1Epith + organ1Epsilon };
                //int[] region02 = new int[] { 0, _gridDivision, organ1Lumen + organ1Epith + organ1Epsilon, _gridSizeY };
                //List<int[]> nutrients01 = new List<int[]>() { MF.BuildVector(new[] { 0, 0 }, new[] { 0, 1 }) };
                //List<int[]> nutrients02 = new List<int[]>();

                // Conduct Sim - Organ 1
                int epithLayers = 1;
                int centralDuct = _gridSizeY / 2;
                int centralDuctRadius = 5;
                int organEpsilon = 25;
                int tumorPosX = 250; // _gridSizex/4
                int tumorPosY = centralDuct + epithLayers + centralDuctRadius - 1;
                double tumorRadius = 0;
                OrganScheme _organ0 = new OrganScheme2(epithLayers, centralDuct, centralDuctRadius, tumorPosX, tumorPosY, tumorRadius);
                int[] region01 = new int[] { 0, _gridDivision, 0, centralDuct - centralDuctRadius - epithLayers - organEpsilon };
                int[] region02 = new int[] { 0, _gridDivision, centralDuct - centralDuctRadius - epithLayers - organEpsilon, centralDuct };
                int[] region03 = new int[] { 0, _gridDivision, centralDuct, centralDuct + centralDuctRadius + epithLayers + organEpsilon };
                int[] region04 = new int[] { 0, _gridDivision, centralDuct + centralDuctRadius + epithLayers + organEpsilon, _gridSizeY };
                List<int[]> nutrients01 = new List<int[]>();
                List<int[]> nutrients02 = new List<int[]>() { MF.BuildVector(new[] { 0, 1 }, new[] { 0, 0 }) };
                List<int[]> nutrients03 = new List<int[]>() { MF.BuildVector(new[] { 0, 0 }, new[] { 0, 1 }) };
                List<int[]> nutrients04 = new List<int[]>();

                // Organ 2
                OrganScheme _organ1 = new OrganScheme3();
                int[] region11 = new int[] { _gridDivision, _gridSizeX, 0, _gridSizeY };
                List<int[]> nutrients11 = new List<int[]>();
                
                _modelSettings = new ModelSettings(_Ka, _Kv, _P0a, _P0v, _ra, _rv, _deltata, _deltatv, _na);
                _parametersSettings = new ParametersSettings(_mu_mig, _eta_mig, _eta_mig_prima, _mu_max, _xi_sc, _xi_mic0, _psi_mic0, _xi_mic1, _psi_mic1, _K_mig, _simScale);
                //_nutrientSettings = new NutrientsSettings(new List<int[]> { region01, region02, region11 }, new List<List<int[]>> { nutrients01, nutrients02, nutrients11 });
                _nutrientSettings = new NutrientsSettings(new List<int[]> { region01, region02, region03, region04, region11 }, new List<List<int[]>> { nutrients01, nutrients02, nutrients03, nutrients04, nutrients11 });
                //_automata = new CellularAutomata(_wattsNetwork, _modelSettings, _organ1, _organ2, _nutrientSettings, _parametersSettings, ExecutionMode.Optimized, HeavisideMode.Generation, NeighboursAmountInfluence.No);
                _automata = new CellularAutomata1_5(_wattsNetwork, _modelSettings, _organ0, _organ1, _nutrientSettings, _parametersSettings);
                _generationsFiles = new List<string>();

                Console.WriteLine("main: writing original states file...");
                WriteOriginalStatesFile();
                Console.WriteLine("main: finished writing original states file");
                Console.WriteLine();

                for (int i = 0; i < _generations; i++)
                {
                    Console.WriteLine("main: writing generation file " + i + "...");
                    int innerTimeStart = Environment.TickCount;
                    WriteGenerationFile(i);
                    int innerElapsedMiliseconds = Environment.TickCount - innerTimeStart;
                    string innerFormattedTime = Notification.TimeStamp(innerElapsedMiliseconds);
                    Console.WriteLine("main: finished writing generation file " + i + innerFormattedTime);
                    Console.WriteLine();

                    Console.WriteLine("main: computing generation " + j + ":" + (i + 1) + "...");
                    innerTimeStart = Environment.TickCount;
                    _automata.UpdateProcedure();
                    innerElapsedMiliseconds = Environment.TickCount - innerTimeStart;
                    innerFormattedTime = Notification.TimeStamp(innerElapsedMiliseconds);
                    Console.WriteLine("main: finished computing generation " + (i + 1) + innerFormattedTime);
                }
                Console.WriteLine();
                Console.WriteLine("main: writing raw report file...");
                int reportTimeStart = Environment.TickCount;
                WriteRawReport();
                int reportElapsedMiliseconds = Environment.TickCount - reportTimeStart;
                string reportFormattedTime = Notification.TimeStamp(reportElapsedMiliseconds);
                Console.WriteLine("main: finished writing raw report file" + reportFormattedTime);
                Console.WriteLine();

                Console.WriteLine("main: writing report file...");
                int report2TimeStart = Environment.TickCount;
                WriteCompressedReport();
                int report2ElapsedMiliseconds = Environment.TickCount - report2TimeStart;
                string report2FormattedTime = Notification.TimeStamp(reportElapsedMiliseconds);
                Console.WriteLine("main: finished writing report file" + report2FormattedTime);
                Console.WriteLine();
            }
            WriteFullCompressedReport();

            int totalElapsedMiliseconds = Environment.TickCount - totalTimeStart;
            string totalFormattedTime = Notification.TimeStamp(totalElapsedMiliseconds);
            Console.WriteLine("main: program finished" + totalFormattedTime);
            Console.ReadLine();
        }

        private static void WriteFullCompressedReport()
        {
            string fullCompressedReport = _dataFolder + "\\full-resume.fcmpr";
            List<double> mainTumorData = new List<double>();
            List<double> mainMigraData = new List<double>();
            List<double> bloodStreamTumor = new List<double>();
            List<double> bloodStreamMigra = new List<double>();
            List<double> migraGenerated = new List<double>();
            Dictionary<int, int> wanderedInfoMetas = new Dictionary<int, int>();
            Dictionary<int, int> wanderedInfoDeath = new Dictionary<int, int>();
            Dictionary<int, int> failedMicro0 = new Dictionary<int, int>();
            Dictionary<int, int> successMicro0 = new Dictionary<int, int>();
            Dictionary<int, int> failedMicro1 = new Dictionary<int, int>();
            Dictionary<int, int> successMicro1 = new Dictionary<int, int>();
            for (int i = 0; i < _compressedFiles.Count; i++)
            {
                string[] textBody = File.ReadAllLines(_compressedFiles[i]);
                int lineNumber = 0;
                if (textBody[lineNumber] == "[Compressed Report Data]")
                {
                    lineNumber += 3;
                    while (lineNumber < textBody.Length && textBody[lineNumber] != "[EOF]")
                    {
                        switch (textBody[lineNumber])
                        {
                            case "[Tumors]":
                                GatherFullCompressedReportAuxiliarList(mainTumorData, textBody, ref lineNumber);
                                break;
                            case "[Migra]":
                                GatherFullCompressedReportAuxiliarList(mainMigraData, textBody, ref lineNumber);
                                break;
                            case "[BloodstreamTumor]":
                                GatherFullCompressedReportAuxiliarList(bloodStreamTumor, textBody, ref lineNumber);
                                break;
                            case "[BloodstreamMigra]":
                                GatherFullCompressedReportAuxiliarList(bloodStreamMigra, textBody, ref lineNumber);
                                break;
                            case "[MigraGenerated]":
                                GatherFullCompressedReportAuxiliarList(migraGenerated, textBody, ref lineNumber);
                                break;
                            case "[WanderedDistanceMetas]":
                                GatherFullCompressedReportAuxiliarDict(wanderedInfoMetas, textBody, ref lineNumber);
                                break;
                            case "[WanderedDistanceDeath]":
                                GatherFullCompressedReportAuxiliarDict(wanderedInfoDeath, textBody, ref lineNumber);
                                break;
                            case "[FailedMicro0]":
                                GatherFullCompressedReportAuxiliarDict(failedMicro0, textBody, ref lineNumber);
                                break;
                            case "[SuccessMicro0]":
                                GatherFullCompressedReportAuxiliarDict(successMicro0, textBody, ref lineNumber);
                                break;
                            case "[FailedMicro1]":
                                GatherFullCompressedReportAuxiliarDict(failedMicro1, textBody, ref lineNumber);
                                break;
                            case "[SuccessMicro1]":
                                GatherFullCompressedReportAuxiliarDict(successMicro1, textBody, ref lineNumber);
                                break;
                            default:
                                lineNumber++;
                                break;
                        }
                    }
                }
                else Console.WriteLine("main: failed reading compressed report.");
            }
            for (int i = 0; i < mainTumorData.Count; i++)
                mainTumorData[i] = mainTumorData[i] / _compressedFiles.Count;
            for (int i = 0; i < mainMigraData.Count; i++)
                mainMigraData[i] = mainMigraData[i] / _compressedFiles.Count;
            for (int i = 0; i < bloodStreamTumor.Count; i++)
                bloodStreamTumor[i] = bloodStreamTumor[i] / _compressedFiles.Count;
            for (int i = 0; i < bloodStreamMigra.Count; i++)
                bloodStreamMigra[i] = bloodStreamMigra[i] / _compressedFiles.Count;
            for (int i = 0; i < migraGenerated.Count; i++)
                migraGenerated[i] = migraGenerated[i] / _compressedFiles.Count;

            StreamWriter compressedReportWriter = new StreamWriter(fullCompressedReport);
            compressedReportWriter.WriteLine("[Full Compressed Report Data]");
            compressedReportWriter.WriteLine(_experimentID);
            compressedReportWriter.WriteLine();
                        
            WriteCompressedReportAuxiliar2List(compressedReportWriter, "[Tumors]", mainTumorData);
            WriteCompressedReportAuxiliar2List(compressedReportWriter, "[Migra]", mainMigraData);
            WriteCompressedReportAuxiliar2List(compressedReportWriter, "[BloodstreamTumor]", bloodStreamTumor);
            WriteCompressedReportAuxiliar2List(compressedReportWriter, "[BloodstreamMigra]", bloodStreamMigra);
            WriteCompressedReportAuxiliar2List(compressedReportWriter, "[MigraGenerated]", migraGenerated);

            compressedReportWriter.WriteLine("[WanderedDistanceMetas]");
            List<KeyValuePair<int, int>> values = wanderedInfoMetas.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i].Value / (double)_compressedFiles.Count;
                compressedReportWriter.WriteLine(value);
            }
            compressedReportWriter.WriteLine();

            compressedReportWriter.WriteLine("[WanderedDistanceDeath]");
            values = wanderedInfoDeath.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i].Value / (double)_compressedFiles.Count;
                compressedReportWriter.WriteLine(value);
            }
            compressedReportWriter.WriteLine();

            compressedReportWriter.WriteLine("[FailedMicro0]");
            values = failedMicro0.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i].Value / (double)_compressedFiles.Count;
                compressedReportWriter.WriteLine(value);
            }
            compressedReportWriter.WriteLine();

            compressedReportWriter.WriteLine("[FailedMicro1]");
            values = failedMicro1.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i].Value / (double)_compressedFiles.Count;
                compressedReportWriter.WriteLine(value);
            }
            compressedReportWriter.WriteLine();

            compressedReportWriter.WriteLine("[SuccessMicro0]");
            values = successMicro0.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i].Value / (double)_compressedFiles.Count;
                compressedReportWriter.WriteLine(value);
            }
            compressedReportWriter.WriteLine();

            compressedReportWriter.WriteLine("[SuccessMicro1]");
            values = successMicro1.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i].Value / (double)_compressedFiles.Count;
                compressedReportWriter.WriteLine(value);
            }
            compressedReportWriter.WriteLine();

            compressedReportWriter.Write("[EOF]");
            compressedReportWriter.Close();
        }
        private static void GatherFullCompressedReportAuxiliarDict(Dictionary<int, int> dataToFill, string[] textBody, ref int lineNumber)
        {
            int counter = 0;
            lineNumber++;
            string[] splitted = textBody[lineNumber].Split(':');
            if (splitted[0] == "")
            {
                lineNumber++;
                return;
            }
            while (textBody[lineNumber] != "")
            {
                int value = int.Parse(textBody[lineNumber]);
                if (dataToFill.ContainsKey(counter))
                    dataToFill[counter] += value;
                else dataToFill.Add(counter, value);
                counter++;
                lineNumber++;
            }
        }
        private static void WriteCompressedReportAuxiliar2List(StreamWriter compressedReportWriter, string label, List<double> dataToWrite)
        {
            compressedReportWriter.WriteLine(label);
            bool written = false;
            int count = dataToWrite.Count;
            for (int i = 0; i < count; i++)
            {
                Notification.CompletionNotification(i, count, ref written, "");
                compressedReportWriter.WriteLine(dataToWrite[i]);
            }
            Notification.FinalCompletionNotification("");
            compressedReportWriter.WriteLine();
        }
        private static void GatherFullCompressedReportAuxiliarList(List<double> dataToFill, string[] textBody, ref int lineNumber)
        {
            lineNumber++;
            string[] splitted = textBody[lineNumber].Split(':');
            if (splitted[0] == "")
            {
                lineNumber++;
                return;
            }
            if (splitted.Length == 2)
                lineNumber++;
            int counter = 0;
            do
            {
                double value = 0;
                bool converted = double.TryParse(textBody[lineNumber], out value);
                if (converted)
                {
                    if (dataToFill.Count == counter) dataToFill.Add(value);
                    else dataToFill[counter] += int.Parse(textBody[lineNumber]);
                    counter++;
                    lineNumber++;
                }
                splitted = textBody[lineNumber].Split(':');
            }
            while (splitted.Length == 1 && textBody[lineNumber] != "");
            lineNumber++;
        }
        private static void WriteOriginalStatesFile()
        {
            string originalStatesFile = _experimentFolder + "\\original-states.originals";
            StreamWriter originalStatesWriter = new StreamWriter(originalStatesFile);
            originalStatesWriter.WriteLine("[Original States Data]");
            originalStatesWriter.WriteLine(_experimentID);
            originalStatesWriter.WriteLine();
            originalStatesWriter.WriteLine("[Grid]");
            originalStatesWriter.WriteLine(_gridSizeX + ":" + _gridSizeY + ":" + _gridDivision);
            originalStatesWriter.WriteLine();
            originalStatesWriter.WriteLine("[States]");
            List<KeyValuePair<TVertex, CellState>> cellPairs = _automata.DictOriginalStates;
            int cellPairsCount = cellPairs.Count;
            for (int i = 0; i < cellPairsCount; i++)
                originalStatesWriter.WriteLine(cellPairs[i].Key + ":" + MF.ConvertCellStateToInt(cellPairs[i].Value));
            cellPairs.Clear();
            originalStatesWriter.WriteLine();
            originalStatesWriter.Write("[EOF]");
            originalStatesWriter.Close();
        }
        private static void WriteCompressedReport()
        {
            string rawReportFile = _experimentFolder + "\\gens-resume.rawr";
            string compressedReportFile = _experimentFolder + "\\gens-resume.cmpr";
            Dictionary<int, List<KeyValuePair<int, int>>> tumorData = new Dictionary<int, List<KeyValuePair<int, int>>>();
            Dictionary<int, List<KeyValuePair<int, int>>> microData = new Dictionary<int, List<KeyValuePair<int, int>>>();
            Dictionary<int, List<KeyValuePair<int, int>>> migraData = new Dictionary<int, List<KeyValuePair<int, int>>>();
            List<int> bloodTumorData = new List<int>();
            List<int> bloodMigraData = new List<int>();
            List<int> migraGeneratedData = new List<int>();
            string[] textBody = File.ReadAllLines(rawReportFile);
            int lineNumber = 0;
            Console.WriteLine("main: compressing raw report...");
            if (textBody[lineNumber] == "[Raw Report Data]")
            {
                lineNumber += 3;
                bool written = false;
                while (lineNumber < textBody.Length && textBody[lineNumber] != "[EOF]")
                {
                    Notification.CompletionNotification(lineNumber, textBody.Length, ref written, "");
                    switch (textBody[lineNumber])
                    {
                        case "[Tumors]":
                            GatherCompressedReportAuxiliar1Dict(tumorData, textBody, ref lineNumber);
                            break;
                        case "[Micros]":
                            GatherCompressedReportAuxiliar1Dict(microData, textBody, ref lineNumber);
                            break;
                        case "[Migra]":
                            GatherCompressedReportAuxiliar1Dict(migraData, textBody, ref lineNumber);
                            break;
                        case "[BloodstreamTumor]":
                            GatherCompressedReportAuxiliar1List(bloodTumorData, textBody, ref lineNumber);
                            break;
                        case "[BloodstreamMigra]":
                            GatherCompressedReportAuxiliar1List(bloodMigraData, textBody, ref lineNumber);
                            break;
                        case "[MigraGenerated]":
                            GatherCompressedReportAuxiliar1List(migraGeneratedData, textBody, ref lineNumber);
                            break;
                        default:
                            Console.WriteLine("main: failed reading raw report.");
                            break;
                    }
                }
                Notification.FinalCompletionNotification("");
                Console.WriteLine("main: writing compressed report file...");
                StreamWriter compressedReportWriter = new StreamWriter(compressedReportFile);
                compressedReportWriter.WriteLine("[Compressed Report Data]");
                compressedReportWriter.WriteLine(_experimentID);
                compressedReportWriter.WriteLine();

                WriteCompressedReportAuxiliar2Dict(compressedReportWriter, "[Tumors]", tumorData);
                WriteCompressedReportAuxiliar2Dict(compressedReportWriter, "[Micros]", microData);
                WriteCompressedReportAuxiliar2Dict(compressedReportWriter, "[Migra]", migraData);
                WriteCompressedReportAuxiliar2List(compressedReportWriter, "[BloodstreamTumor]", bloodTumorData);
                WriteCompressedReportAuxiliar2List(compressedReportWriter, "[BloodstreamMigra]", bloodMigraData);
                WriteCompressedReportAuxiliar2List(compressedReportWriter, "[MigraGenerated]", migraGeneratedData);

                List<KeyValuePair<int, int>> values = _automata.DictWanderedDistanceMeasurementsMetas;
                compressedReportWriter.WriteLine("[WanderedDistanceMetas]");
                for (int i = 0; i < values.Count; i++)
                    compressedReportWriter.WriteLine(values[i].Value);
                compressedReportWriter.WriteLine();

                values = _automata.DictWanderedDistanceMeasurementsDeath;
                compressedReportWriter.WriteLine("[WanderedDistanceDeath]");
                for (int i = 0; i < values.Count; i++)
                    compressedReportWriter.WriteLine(values[i].Value);
                compressedReportWriter.WriteLine();

                values = _automata.FailedMicrometastasis[0];
                compressedReportWriter.WriteLine("[FailedMicro0]");
                for (int i = 0; i < values.Count; i++)
                    compressedReportWriter.WriteLine(values[i].Value);
                compressedReportWriter.WriteLine();

                values = _automata.FailedMicrometastasis[1];
                compressedReportWriter.WriteLine("[FailedMicro1]");
                for (int i = 0; i < values.Count; i++)
                    compressedReportWriter.WriteLine(values[i].Value);
                compressedReportWriter.WriteLine();

                values = _automata.SuccessfulMicrometastasis[0];
                compressedReportWriter.WriteLine("[SuccessMicro0]");
                for (int i = 0; i < values.Count; i++)
                    compressedReportWriter.WriteLine(values[i].Value);
                compressedReportWriter.WriteLine();

                values = _automata.SuccessfulMicrometastasis[1];
                compressedReportWriter.WriteLine("[SuccessMicro1]");
                for (int i = 0; i < values.Count; i++)
                    compressedReportWriter.WriteLine(values[i].Value);
                compressedReportWriter.WriteLine();

                compressedReportWriter.Write("[EOF]");
                compressedReportWriter.Close();
                _compressedFiles.Add(compressedReportFile);
            }
            else Console.WriteLine("main: failed reading raw report.");
        }
        private static void WriteCompressedReportAuxiliar2Dict(StreamWriter compressedReportWriter, string label, Dictionary<int, List<KeyValuePair<int, int>>> dataToWrite)
        {
            compressedReportWriter.WriteLine(label);
            bool written = false;
            List<KeyValuePair<int, List<KeyValuePair<int, int>>>> pairs = dataToWrite.ToList();
            int pairsCount = pairs.Count;
            for (int i = 0; i < pairsCount; i++)
            {
                Notification.CompletionNotification(i, pairsCount, ref written, "");
                List<KeyValuePair<int, int>> cellValues = pairs[i].Value;
                int valuesCount = cellValues.Count;
                compressedReportWriter.WriteLine(pairs[i].Key + ":" + valuesCount);
                for (int j = 0; j < valuesCount; j++)
                    compressedReportWriter.WriteLine(pairs[i].Value[j].Key);
                //compressedReportWriter.WriteLine(pairs[i].Key + ": Exteriors");
                //for (int j = 0; j < valuesCount; j++)
                    //compressedReportWriter.WriteLine(pairs[i].Value[j].Value);
            }
            Notification.FinalCompletionNotification("");
            pairs.Clear();
            compressedReportWriter.WriteLine();
        }
        private static void WriteCompressedReportAuxiliar2List(StreamWriter compressedReportWriter, string label, List<int> dataToWrite)
        {
            compressedReportWriter.WriteLine(label);
            bool written = false;
            int count = dataToWrite.Count;
            for (int i = 0; i < count; i++)
            {
                Notification.CompletionNotification(i, count, ref written, "");
                compressedReportWriter.WriteLine(dataToWrite[i]);
            }
            Notification.FinalCompletionNotification("");
            compressedReportWriter.WriteLine();
        }
        private static void GatherCompressedReportAuxiliar1Dict(Dictionary<int, List<KeyValuePair<int, int>>> dataToFill, string[] textBody, ref int lineNumber)
        {
            lineNumber++;
            while (textBody[lineNumber] != "")
            {
                string[] gensplitted = textBody[lineNumber].Split(':');
                string gen = gensplitted[0];
                if (gen != "Gen")
                    throw new Exception("Unexpected error.");
                int generationNumber = int.Parse(gensplitted[1]);
                lineNumber++;
                while (true)
                {
                    string[] splitted = textBody[lineNumber].Split(':');
                    if (splitted[0] == "Gen" || splitted[0] == "")
                    {
                        break;
                        //if (dataToFill.Count == 0)
                        //    break;
                        //else
                        //{
                        //    List<int> keys = dataToFill.Keys.ToList();
                        //    for (int i = 0; i < keys.Count; i++)
                        //        dataToFill[keys[i]].Add(new KeyValuePair<int, int>(0, 0));
                        //    break;
                        //}
                    }
                    int ID = int.Parse(splitted[0]);
                    int cellsCount = int.Parse(splitted[1]);
                    int exteriorCellsCount = int.Parse(splitted[2]);
                    if (dataToFill.ContainsKey(ID))
                        dataToFill[ID].Add(new KeyValuePair<int, int>(cellsCount, exteriorCellsCount));
                    else
                    {
                        List<KeyValuePair<int, int>> ls = new List<KeyValuePair<int, int>>();
                        for (int i = 0; i < generationNumber; i++)
                            ls.Add(new KeyValuePair<int, int>(0, 0));
                        ls.Add(new KeyValuePair<int, int>(cellsCount, exteriorCellsCount));
                        dataToFill.Add(ID, ls);
                    }
                    lineNumber++;
                }
            }
            lineNumber++;
        }
        private static void GatherCompressedReportAuxiliar1List(List<int> dataToFill, string[] textBody, ref int lineNumber)
        {
            lineNumber++;
            while (textBody[lineNumber] != "")
            {
                string[] gensplitted = textBody[lineNumber].Split(':');
                string gen = gensplitted[0];
                if (gen != "Gen")
                    throw new Exception("Unexpected error.");
                int generationNumber = int.Parse(gensplitted[1]);
                lineNumber++;
                while (true)
                {
                    string[] splitted = textBody[lineNumber].Split(':');
                    if (splitted[0] == "Gen" || splitted[0] == "")
                        break;
                    int cells = int.Parse(splitted[0]);
                    dataToFill.Add(cells);
                    lineNumber++;
                }
            }
            lineNumber++;
        }
        private static void WriteRawReport()
        {
            string rawReportFile = _experimentFolder + "\\gens-resume.rawr";
            StreamWriter reportWriter = new StreamWriter(rawReportFile);
            reportWriter.WriteLine("[Raw Report Data]");
            reportWriter.WriteLine(_experimentID);
            reportWriter.WriteLine();

            WriteRawReportAuxiliar(reportWriter, "[Tumors]");
            WriteRawReportAuxiliar(reportWriter, "[Micros]");
            WriteRawReportAuxiliar(reportWriter, "[Migra]");
            WriteRawReportAuxiliar(reportWriter, "[BloodstreamTumor]");
            WriteRawReportAuxiliar(reportWriter, "[BloodstreamMigra]");
            WriteRawReportAuxiliar(reportWriter, "[MigraGenerated]");

            reportWriter.Write("[EOF]");
            reportWriter.Close();
        }
        private static void WriteRawReportAuxiliar(StreamWriter reportWriter, string label)
        {
            reportWriter.WriteLine(label);
            bool written = false;
            for (int i = 0; i < _generationsFiles.Count; i++)
            {
                Notification.CompletionNotification(i, _generationsFiles.Count, ref written, "");
                reportWriter.WriteLine("Gen:" + i);
                int linenumber = 0;
                string[] textbody = File.ReadAllLines(_generationsFiles[i]);
                while (textbody[linenumber] != label)
                    linenumber++;
                if (textbody[linenumber] == "[EOF]")
                    throw new Exception("EOF reached.");
                linenumber++;
                while (textbody[linenumber] != "")
                {
                    string[] splitted = textbody[linenumber].Split(':');
                    if (splitted.Length == 1)
                        reportWriter.WriteLine(splitted[0]);
                    else reportWriter.WriteLine(splitted[0] + ":" + splitted[1] + ":" + splitted[2]);
                    linenumber++;
                }
            }
            Notification.FinalCompletionNotification("");
            reportWriter.WriteLine();
        }
        private static void WriteGenerationFile(int generation)
        {
            string generationFile = _experimentFolder + "\\" + generation + ".generation";
            _generationsFiles.Add(generationFile);
            StreamWriter generationWriter = new StreamWriter(generationFile);
            generationWriter.WriteLine("[Generation Data]");
            generationWriter.WriteLine(_experimentID + ":" + generation);
            generationWriter.WriteLine();

            generationWriter.WriteLine("[Grid]");
            generationWriter.WriteLine(_gridSizeX + ":" + _gridSizeY + ":" + _gridDivision);
            generationWriter.WriteLine();

            var tumors = _automata.DictTumors;
            var micros = _automata.DictMicro;
            var migras = _automata.DictMigra;
            
            WriteGenerationFileAuxiliar(generationWriter, "[Tumors]", tumors);
            WriteGenerationFileAuxiliar(generationWriter, "[Micros]", micros);
            WriteGenerationFileAuxiliar(generationWriter, "[Migra]", migras);

            generationWriter.WriteLine("[BloodstreamTumor]");
            generationWriter.WriteLine(_automata.CellsInBloodstreamFromTumors);
            generationWriter.WriteLine();

            generationWriter.WriteLine("[BloodstreamMigra]");
            generationWriter.WriteLine(_automata.CellsInBloodstreamFromMigration);
            generationWriter.WriteLine();

            generationWriter.WriteLine("[MigraGenerated]");
            generationWriter.WriteLine(_automata.MigrationCellsGenerated);
            generationWriter.WriteLine();

            generationWriter.WriteLine("[MigraGenerated]");
            generationWriter.WriteLine(_automata.MigrationCellsGenerated);
            generationWriter.WriteLine();

            generationWriter.Write("[EOF]");
            generationWriter.Close();
        }
        private static void WriteGenerationFileAuxiliar(StreamWriter generationWriter, string label, List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>> dict)
        {
            generationWriter.WriteLine(label);
            List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>> pairs = dict;
            Dictionary<int, int> exteriorDict = _automata.DictExteriorCells(label);
            int pairsCount = pairs.Count;
            for (int i = 0; i < pairsCount; i++)
            {
                int cellsCount = pairs[i].Value.Count;
                int exteriorCellsCount = 0;
                if (exteriorDict.ContainsKey(pairs[i].Key))
                    exteriorCellsCount = exteriorDict[pairs[i].Key];
                generationWriter.Write(pairs[i].Key + ":" + cellsCount + ":" + exteriorCellsCount + ":");
                for (int j = 0; j < cellsCount; j++)
                {
                    if (j != cellsCount - 1)
                        generationWriter.Write(pairs[i].Value[j].Key + "," + pairs[i].Value[j].Value + ":");
                    else
                    {
                        generationWriter.Write(pairs[i].Value[j].Key + "," + pairs[i].Value[j].Value);
                        break;
                    }
                }
                generationWriter.WriteLine();
            }
            pairs.Clear();
            generationWriter.WriteLine();
        }
        private static void LoadOrBuildNetwork()
        {
            if (_loadNetwork)
            {
                Console.WriteLine("main: \"loadnetwork\" set to true, loading network...");
                LoadNetwork();
            }
            else
            {
                Console.WriteLine("main: \"loadnetwork\" set to false, creating network...");
                BuildNetwork();
            }
        }
        private static void LoadNetwork()
        {
            _networksFolder = Directory.GetCurrentDirectory() + "\\Networks";
            if (Directory.Exists(_networksFolder))
            {
                List<string> allfiles = Directory.EnumerateFiles(_networksFolder).ToList();
                if (allfiles.Count != 0)
                {
                    List<string> filteredfiles = new List<string>();
                    for (int i = 0; i < allfiles.Count; i++)
                    {
                        string extension = Path.GetExtension(allfiles[i]);
                        if (extension == ".network")
                            filteredfiles.Add(allfiles[i]);
                    }
                    if (filteredfiles.Count != 0)
                    {
                        if (filteredfiles.Count == 1)
                        {
                            Console.WriteLine("main: networks folder contains one network file, loading file...");
                            LoadNetwork(filteredfiles[0]);
                        }
                        else
                        {
                            Console.WriteLine("main: networks folder contains various network files, loading one randomly...");
                            int index = new Random().Next(filteredfiles.Count);
                            LoadNetwork(filteredfiles[index]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("main: networks folder does not contain network files, failed loading network, creating network...");
                        BuildNetwork();
                    }
                }
                else
                {
                    Console.WriteLine("main: networks folder does not contain files, failed loading network, creating network...");
                    BuildNetwork();
                }
            }
            else
            {
                Console.WriteLine("main: networks folder does not exist, failed loading network, creating network...");
                BuildNetwork();
            }
        }
        private static void LoadNetwork(string networkpath)
        {
            Console.WriteLine("main: loading network...");
            Console.WriteLine("main: the program is using the network file settings for network settings");
            int time_start = Environment.TickCount;
            IEnumerable<string> textbody = File.ReadLines(networkpath);
            bool mark = false;
            foreach (var line in textbody)
            {
                if (mark)
                {
                    string removed_blank_spaces = MF.RemoveBlankSpaces(line);
                    string[] splitted = removed_blank_spaces.Split('=');
                    switch (splitted[0])
                    {
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
                    }
                }
                if (line == "[Settings]") mark = true;
                if (line == "[Vertexs]") break;
            }
            _networkSettings = new NetworkSettings(_gridSizeX, _gridSizeY, _gridDivision, _p, _r, false, _periodic);
            _wattsNetwork = new SmallWorldNetwork(_networkSettings, networkpath);
            int time_elapsedmiliseconds = Environment.TickCount - time_start;
            string formatted_time = Notification.TimeStamp(time_elapsedmiliseconds);
            Console.WriteLine("main: finished loading network" + formatted_time);
        }
        private static void BuildNetwork()
        {
            Console.WriteLine("main: creating network...");
            int time_start = Environment.TickCount;
            _networkSettings = new NetworkSettings(_gridSizeX, _gridSizeY, _gridDivision, _p, _r, false, _periodic);
            _wattsNetwork = new SmallWorldNetwork(_networkSettings);
            int time_elapsedmiliseconds = Environment.TickCount - time_start;
            string formatted_time = Notification.TimeStamp(time_elapsedmiliseconds);
            Console.WriteLine("main: finished creating network" + formatted_time);
        }
        private static void CreateExperimentFolder()
        {
            DateTime time = DateTime.Now;
            _experimentID = time.Year.ToString() + time.Month.ToString() + time.Day.ToString() + time.Hour.ToString() + time.Minute.ToString() + time.Second.ToString();
            _experimentFolder = _dataFolder + "\\" + _experimentID;
            Directory.CreateDirectory(_experimentFolder);
            Console.WriteLine("main: experiment folder created - with path: \"" + _experimentFolder + "\"\n");
        }
        
        private static void CreateBatchFolder()
        {
            DateTime time = DateTime.Now;
            string batchID = time.Year.ToString() + time.Month.ToString() + time.Day.ToString() + time.Hour.ToString() + time.Minute.ToString() + time.Second.ToString();
            _dataFolder = _dataFolder + "\\" + batchID;
            Directory.CreateDirectory(_dataFolder);
            Console.WriteLine("main: batch folder created - with path: \"" + _dataFolder + "\"\n");
        }
        private static void CreateDataFolder()
        {
            _dataFolder = Directory.GetCurrentDirectory() + "\\Data";
            Directory.CreateDirectory(_dataFolder);
            Console.WriteLine("main: data folder created - with path: \"" + _dataFolder + "\"\n");
        }
    }
}