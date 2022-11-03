//using System;
//using System.Collections.Generic;
//using System.Linq;
//using QuickGraph;
//using TEdge = QuickGraph.UndirectedEdge<string>;
//using TVertex = System.String;

//namespace AutomataLibrary2D
//{
//    public class CellularAutomata1
//    {
//        private SmallWorldNetwork _smallWorldNetwork = null;
//        private Dictionary<TVertex, CellState> _dictStatesOriginal = null;
//        private Dictionary<TVertex, CellData> _dictStates = null;
//        private Dictionary<TVertex, int>[] _dictAsyncMig = null;
//        private Dictionary<int, TVertex> _dictAsyncCS = null;
//        private List<TVertex>[] _dictAsyncTum = null;
//        private Dictionary<int, List<TVertex>> _dictTumor = null;
//        private Dictionary<int, List<TVertex>> _dictMicro = null;
//        private Dictionary<int, int> _dictTumorData = null;
//        private Random _random = null;
//        private ModelSettings _modelSettings = null;
//        private OrganScheme _organ1Scheme = null;
//        private OrganScheme _organ2Scheme = null;
//        private NutrientsSettings _nutrientsSettings = null;
//        private ParametersSettings _parametersSettings = null;
//        private int _generation = 0;
//        private int _cellInBloodstreamIDGenerator = 0;
//        private int _tumorIDGenerator = 0;
//        private HeavisideMode _heavisideMode = HeavisideMode.Population;
//        private NeighboursAmountInfluence _neighboursAmountInfluence = NeighboursAmountInfluence.No;

//        private ExecutionMode _executionMode = ExecutionMode.Optimized;
//        private Dictionary<TVertex, bool> _synchronousCellsToUpdate = null;
        
//        public List<KeyValuePair<TVertex, CellState>> DictOriginalStates { get { return _dictStatesOriginal.ToList(); } }
//        public List<KeyValuePair<int, List<TVertex>>> DictTumors { get { return _dictTumor.ToList(); } }
//        public List<KeyValuePair<int, List<TVertex>>> DictMicro { get { return _dictMicro.ToList(); } }
//        public List<KeyValuePair<int, List<TVertex>>> DictMigra
//        {
//            get
//            {
//                Dictionary<int, List<TVertex>> migratoryDictionay = new Dictionary<int, List<TVertex>>();
//                List<TVertex> migratoryCells = _dictAsyncMig[0].Keys.ToList();
//                for (int i = 0; i < migratoryCells.Count; i++)
//                {
//                    int migratoryCellOriginalTumor = _dictStates[migratoryCells[i]].CurrentTumor;
//                    if (migratoryDictionay.ContainsKey(migratoryCellOriginalTumor))
//                        migratoryDictionay[migratoryCellOriginalTumor].Add(migratoryCells[i]);
//                    else
//                    {
//                        migratoryDictionay.Add(migratoryCellOriginalTumor, new List<TVertex>());
//                        migratoryDictionay[migratoryCellOriginalTumor].Add(migratoryCells[i]);
//                    }
//                }
//                return migratoryDictionay.ToList();
//            }
//        }
//        public int CellsInBloodStream { get { return _cellInBloodstreamIDGenerator; } }

//        public CellularAutomata1(SmallWorldNetwork network, ModelSettings model, 
//            OrganScheme organ1, OrganScheme organ2, NutrientsSettings nutrients,
//            ParametersSettings parameters, ExecutionMode mode, HeavisideMode mode1,
//            NeighboursAmountInfluence neighbours)
//        {
//            _smallWorldNetwork = network;
//            _generation = 0;
//            _cellInBloodstreamIDGenerator = 0;
//            _tumorIDGenerator = 0;
//            _random = new Random();
//            _modelSettings = model;
//            _parametersSettings = parameters;
//            _nutrientsSettings = nutrients;
//            _organ1Scheme = organ1;
//            _organ2Scheme = organ2;
//            _executionMode = mode;
//            _heavisideMode = mode1;
//            _neighboursAmountInfluence = neighbours;
//            InitializeDictionaries();
//            InitializeGrid();
//            InitializeTumor();
//        }
//        private void InitializeDictionaries()
//        {
//            _dictStates = new Dictionary<TVertex, CellData>();
//            _dictAsyncMig = new Dictionary<TVertex, int>[2];
//            _dictAsyncMig[0] = new Dictionary<TVertex, int>();
//            _dictAsyncMig[1] = new Dictionary<TVertex, int>();
//            _dictTumor = new Dictionary<int, List<TVertex>>();
//            _dictTumorData = new Dictionary<int, int>();
//            _dictAsyncCS = new Dictionary<int, TVertex>();
//            _dictAsyncTum = new List<TVertex>[2];
//            _dictAsyncTum[0] = new List<TVertex>();
//            _dictAsyncTum[1] = new List<TVertex>();
//            _dictStatesOriginal = new Dictionary<TVertex, CellState>();
//            _dictMicro = new Dictionary<int, List<TVertex>>();
//            List<TVertex> vertices = _smallWorldNetwork.Vertices;
//            int verticesCount = vertices.Count;
//            for (int i = 0; i < verticesCount; i++)
//            {
//                _dictStates.Add(vertices[i], new CellData(false, CellState.NULL, CellState.NULL, -1, -1, -1, -1));
//                _dictStatesOriginal.Add(vertices[i], CellState.NULL);
//            }
//        }
//        private void InitializeGrid()
//        {
//            switch (_organ1Scheme.SelectedScheme)
//            {
//                case SelectedScheme.Scheme1:
//                    InitializeScheme1(0, 0, _smallWorldNetwork.NetworkDivision, _smallWorldNetwork.NetworkSizeY, 0);
//                    break;
//                case SelectedScheme.Scheme2:
//                    InitializeScheme2(0, 0, _smallWorldNetwork.NetworkDivision, _smallWorldNetwork.NetworkSizeY, 0);
//                    break;
//                case SelectedScheme.Scheme3:
//                    InitializeScheme3(0, 0, _smallWorldNetwork.NetworkDivision, _smallWorldNetwork.NetworkSizeY, 0);
//                    break;
//            }
//            switch (_organ2Scheme.SelectedScheme)
//            {
//                case SelectedScheme.Scheme1:
//                    InitializeScheme1(_smallWorldNetwork.NetworkDivision, 0, _smallWorldNetwork.NetworkSizeX, _smallWorldNetwork.NetworkSizeY, 1);
//                    break;
//                case SelectedScheme.Scheme2:
//                    InitializeScheme2(_smallWorldNetwork.NetworkDivision, 0, _smallWorldNetwork.NetworkSizeX, _smallWorldNetwork.NetworkSizeY, 1);
//                    break;
//                case SelectedScheme.Scheme3:
//                    InitializeScheme3(_smallWorldNetwork.NetworkDivision, 0, _smallWorldNetwork.NetworkSizeX, _smallWorldNetwork.NetworkSizeY, 1);
//                    break;
//            }
//        }
//        private void InitializeScheme1(int x0, int y0, int xf, int yf, int organ)
//        {
//            OrganScheme1 current = null;
//            if (organ == 0)
//                current = (OrganScheme1)_organ1Scheme;
//            else if (organ == 1)
//                current = (OrganScheme1)_organ2Scheme;
//            else throw new Exception("Invalid organ identifier.");
//            CellState state = CellState.NULL;
//            for (int i = y0; i < yf; i++)
//            {
//                if (0 <= i && i < current.Lumen)
//                    state = CellState.Lumen;
//                else if (current.Lumen <= i && i < current.Lumen + current.Epithelium)
//                    state = CellState.Epith;
//                else state = CellState.Strom;
//                for (int j = x0; j < xf; j++)
//                {
//                    TVertex vertex = MF.BuildTVertexID(j, i);
//                    _dictStates[vertex].CurrentState = state;
//                    _dictStates[vertex].Organ = organ;
//                    _dictStatesOriginal[vertex] = state;
//                }
//            }
//        }
//        private void InitializeScheme2(int x0, int y0, int xf, int yf, int organ)
//        {
//            throw new NotImplementedException();
//        }
//        private void InitializeScheme3(int x0, int y0, int xf, int yf, int organ)
//        {
//            CellState state = CellState.Strom;
//            for (int i = y0; i < yf; i++)
//            {
//                for (int j = x0; j < xf; j++)
//                {
//                    TVertex vertex = MF.BuildTVertexID(j, i);
//                    _dictStates[vertex].CurrentState = state;
//                    _dictStates[vertex].Organ = organ;
//                    _dictStatesOriginal[vertex] = state;
//                }
//            }
//        }
//        private void InitializeTumor()
//        {
//            List<TVertex> tumorTemplate = MF.GetRegularNeighboursTemplate(_organ1Scheme.TumorRadius);
//            List<TVertex> clusterTumorCells = new List<TVertex>();
//            for (int i = 0; i < tumorTemplate.Count; i++)
//            {
//                int[] templatePos = MF.GetTVertexID(tumorTemplate[i]);
//                TVertex tumorCell = MF.BuildTVertexID(templatePos[0] + _organ1Scheme.TumorPosX, templatePos[1] + _organ1Scheme.TumorPosY);
//                clusterTumorCells.Add(tumorCell);
//            }
//            _dictTumor.Add(_tumorIDGenerator, new List<TVertex>());
//            _dictTumorData.Add(_tumorIDGenerator, 0);
//            for (int i = 0; i < clusterTumorCells.Count; i++)
//            {
//                _dictStates[clusterTumorCells[i]].CurrentState = CellState.Tumor;
//                _dictStates[clusterTumorCells[i]].CurrentTumor = _tumorIDGenerator;
//                _dictTumor[_tumorIDGenerator].Add(clusterTumorCells[i]);
//                List<TVertex> neigh = GetN(clusterTumorCells[i]);
//                List<TVertex> dneigh = GetDN(clusterTumorCells[i], neigh);
//                if (dneigh.Count > 0 && !_dictAsyncTum[0].Contains(clusterTumorCells[i]))
//                    _dictAsyncTum[0].Add(clusterTumorCells[i]);
//            }
//            _tumorIDGenerator++;
//        }

//        private List<TVertex> GetN(TVertex focalVertex)
//        {
//            List<TVertex> neighbours = new List<TVertex>();
//            List<TEdge> adjacentedges = _smallWorldNetwork.AdjacentEdges(focalVertex);
//            for (int i = 0; i < adjacentedges.Count; i++)
//                neighbours.Add((adjacentedges[i].Target == focalVertex) ? adjacentedges[i].Source : adjacentedges[i].Target);
//            return neighbours;
//        }
//        private List<TVertex> GetDN(TVertex focalVertex, List<TVertex> neighbours)
//        {
//            List<TVertex> distantNeighbours = new List<TVertex>();
//            for (int i = 0; i < neighbours.Count; i++)
//                if (MF.EuclideanDistance(focalVertex, neighbours[i]) > _smallWorldNetwork.NeighbourhoodRadius)
//                    distantNeighbours.Add(neighbours[i]);
//            return distantNeighbours;
//        }
//        private List<TVertex> GetNN(TVertex focalVertex, List<TVertex> neighbours)
//        {
//            List<TVertex> nearNeighbours = new List<TVertex>();
//            for (int i = 0; i < neighbours.Count; i++)
//                if (MF.EuclideanDistance(focalVertex, neighbours[i]) <= _smallWorldNetwork.NeighbourhoodRadius)
//                    nearNeighbours.Add(neighbours[i]);
//            return nearNeighbours;
//        }
//        private List<TVertex> GetNNE(TVertex focalVertex, List<TVertex> nearNeighbours, List<CellState> E)
//        {
//            int[] vertexpos = MF.GetTVertexID(focalVertex);
//            List<TVertex> filteredNeighbours = new List<TVertex>();
//            int focalVertexOrgan = _dictStates[focalVertex].Organ;
//            for (int i = 0; i < nearNeighbours.Count; i++)
//            {
//                int neighbourOrgan = _dictStates[nearNeighbours[i]].Organ;
//                if (focalVertexOrgan == neighbourOrgan && E.Contains(_dictStates[nearNeighbours[i]].CurrentState))
//                    filteredNeighbours.Add(nearNeighbours[i]);
//            }
//            return filteredNeighbours;
//        }
//        private List<TVertex> GetDNE(TVertex focalVertex, List<TVertex> distantNeighbours, List<CellState> E)
//        {
//            int[] vertexpos = MF.GetTVertexID(focalVertex);
//            List<TVertex> filteredNeighbours = new List<TVertex>();
//            for (int i = 0; i < distantNeighbours.Count; i++)
//                if (E.Contains(_dictStates[distantNeighbours[i]].CurrentState))
//                    filteredNeighbours.Add(distantNeighbours[i]);
//            return filteredNeighbours;
//        }
//        private List<int> GetCompitingTumorsID(List<TVertex> nearNeighbours)
//        {
//            List<int> tumorIdsList = new List<int>();
//            for (int i = 0; i < nearNeighbours.Count; i++)
//                if (!tumorIdsList.Contains(_dictStates[nearNeighbours[i]].CurrentTumor) && _dictStates[nearNeighbours[i]].CurrentTumor != -1)
//                    tumorIdsList.Add(_dictStates[nearNeighbours[i]].CurrentTumor);
//            return tumorIdsList;
//        }

//        private double GetGrowthProbAvascular(double n)
//        {
//            double num = _modelSettings.P0a * _modelSettings.Ka * _modelSettings.ra * Math.Pow(Math.E, _modelSettings.ra * n * _modelSettings.deltata) * (_modelSettings.Ka - _modelSettings.P0a);
//            double den = Math.Pow(((_modelSettings.P0a * Math.Pow(Math.E, _modelSettings.ra * n * _modelSettings.deltata)) - _modelSettings.P0a + _modelSettings.Ka), 2);
//            double result = num / den;
//            return result;
//        }
//        private double GetGrowthProbVascular(double n)
//        {
//            double num = (_modelSettings.P0v * _modelSettings.Kv * _modelSettings.rv) * (Math.Pow(Math.E, _modelSettings.rv * n * _modelSettings.deltatv)) * (_modelSettings.Kv - _modelSettings.P0v);
//            double den = Math.Pow(((_modelSettings.P0v * Math.Pow(Math.E, _modelSettings.rv * n * _modelSettings.deltatv)) - _modelSettings.P0v + _modelSettings.Kv), 2);
//            double result = num / den;
//            return result;
//        }
//        private double GetMigrantCellApparitionProb(int tumorCellsAmount)
//        {
//            return Math.Pow(tumorCellsAmount / (double)(_modelSettings.Kv + _parametersSettings.K_mig), 1.0 / _parametersSettings.eta_mig);
//        }

//        private TVertex GetMetastasisDestiny(List<TVertex> distantFilteredNeighbours)
//        {
//            int random = _random.Next(distantFilteredNeighbours.Count);
//            return distantFilteredNeighbours[random];
//        }
//        private List<TVertex> GetAvailableDestinies(TVertex migrantCell, TVertex tumorCentroid, List<TVertex> nearNeighbours2)
//        {
//            List<TVertex> possibleMoves = new List<TVertex>();
//            for (int i = 0; i < nearNeighbours2.Count; i++)
//            {
//                double distanceCellTumorCentroid = MF.EuclideanDistance(migrantCell, tumorCentroid);
//                double distanceNeighbourCentroid = MF.EuclideanDistance(nearNeighbours2[i], tumorCentroid);
//                if (distanceNeighbourCentroid > distanceCellTumorCentroid)
//                    possibleMoves.Add(nearNeighbours2[i]);
//            }
//            return possibleMoves;
//        }
//        private int[] GetTumorCentroid(int tumorId)
//        {
//            int[] centroid = new int[2];
//            List<TVertex> tumorCells = _dictTumor[tumorId];
//            int count = tumorCells.Count;
//            for (int i = 0; i < tumorCells.Count; i++)
//            {
//                int[] pos = MF.GetTVertexID(tumorCells[i]);
//                centroid[0] += pos[0];
//                centroid[1] += pos[1];
//            }
//            centroid[0] = (int)Math.Round(centroid[0] / (double)count);
//            centroid[1] = (int)Math.Round(centroid[1] / (double)count);
//            return centroid;
//        }
//        private List<int[]> GetRegionNutrients(TVertex cell)
//        {
//            int[] cellpos = MF.GetTVertexID(cell);
//            for (int i = 0; i < _nutrientsSettings.Regions.Count; i++)
//            {
//                int[] regionLimits = _nutrientsSettings.Regions[i];
//                if ((regionLimits[0] <= cellpos[0] && cellpos[0] < regionLimits[1]) &&
//                    (regionLimits[2] <= cellpos[1] && cellpos[1] < regionLimits[3]))
//                    return _nutrientsSettings.Vectors[i];
//            }
//            throw new Exception("Vertex not contained in any region.");
//        }

//        public void UpdateProcedure()
//        {
//            int timeStart = Environment.TickCount;
//            Console.WriteLine("   cellularautomata library: updating migratory cells in bloodstream...");
//            UpdateMigratoryCellsInBloodstream();
//            int elapsedMiliseconds = Environment.TickCount - timeStart;
//            string formattedTime = Notification.TimeStamp(elapsedMiliseconds);
//            Console.WriteLine("   cellularautomata library: finished updating migratory cells in bloodstream" + formattedTime);

//            timeStart = Environment.TickCount;
//            Console.WriteLine("   cellularautomata library: updating migratory cells...");
//            UpdateMigratoryCells();
//            elapsedMiliseconds = Environment.TickCount - timeStart;
//            formattedTime = Notification.TimeStamp(elapsedMiliseconds);
//            Console.WriteLine("   cellularautomata library: finished migratory cells" + formattedTime);

//            timeStart = Environment.TickCount;
//            Console.WriteLine("   cellularautomata library: updating tumor migratory cells...");
//            UpdateTumorMigratoryCells();
//            elapsedMiliseconds = Environment.TickCount - timeStart;
//            formattedTime = Notification.TimeStamp(elapsedMiliseconds);
//            Console.WriteLine("   cellularautomata library: finished tumor migratory cells" + formattedTime);

//            timeStart = Environment.TickCount;
//            Console.WriteLine("   cellularautomata library: checking micrometastasis survival...");
//            CheckMicrometastasisSurvival();
//            elapsedMiliseconds = Environment.TickCount - timeStart;
//            formattedTime = Notification.TimeStamp(elapsedMiliseconds);
//            Console.WriteLine("   cellularautomata library: finished checking micrometastasis survival" + formattedTime);

//            timeStart = Environment.TickCount;
//            Console.WriteLine("   cellularautomata library: updating synchronous cells...");
//            if (_executionMode == ExecutionMode.Optimized)
//            {
//                SetSynchronousUpdateList();
//                UpdateSynchronousCellsOptimized();
//            }
//            else UpdateSynchronousCells();
//            elapsedMiliseconds = Environment.TickCount - timeStart;
//            formattedTime = Notification.TimeStamp(elapsedMiliseconds);
//            Console.WriteLine("   cellularautomata library: finished updating synchronous cells" + formattedTime);

//            timeStart = Environment.TickCount;
//            Console.WriteLine("   cellularautomata library: checking micrometastasis colonization...");
//            CheckMicrometastasisColonization();
//            elapsedMiliseconds = Environment.TickCount - timeStart;
//            formattedTime = Notification.TimeStamp(elapsedMiliseconds);
//            Console.WriteLine("   cellularautomata library: finished checking micrometastasis colonization" + formattedTime);

//            timeStart = Environment.TickCount;
//            Console.WriteLine("   cellularautomata library: setting up next iteration...");
//            SetupIteration();
//            elapsedMiliseconds = Environment.TickCount - timeStart;
//            formattedTime = Notification.TimeStamp(elapsedMiliseconds);
//            Console.WriteLine("   cellularautomata library: finished setting up next iteration" + formattedTime);
//        }
//        private void SetupIteration()
//        {
//            _generation++;
//            List<TVertex> statesKeys = _dictStates.Keys.ToList();
//            int keysCount = statesKeys.Count;
//            bool written = false;
//            for (int i = 0; i < keysCount; i++)
//            {
//                Notification.CompletionNotification(i, keysCount, ref written, "   ");
//                _dictStates[statesKeys[i]].Updated = false;
//                if (_dictStates[statesKeys[i]].FutureState == CellState.Tumor)
//                {
//                    // update future and current info
//                    _dictStates[statesKeys[i]].CurrentState = CellState.Tumor;
//                    _dictStates[statesKeys[i]].FutureState = CellState.NULL;
//                    int tumorID = _dictStates[statesKeys[i]].FutureTumor;
//                    _dictStates[statesKeys[i]].FutureTumor = -1;
//                    _dictStates[statesKeys[i]].CurrentTumor = tumorID;
//                    _dictTumor[tumorID].Add(statesKeys[i]);
//                    // update tumor async set
//                    List<TVertex> neighbours = GetN(statesKeys[i]);
//                    List<TVertex> distantNeighbours = GetDN(statesKeys[i], neighbours);
//                    if (distantNeighbours.Count > 0 && !_dictAsyncTum[0].Contains(statesKeys[i]))
//                        _dictAsyncTum[0].Add(statesKeys[i]);
//                }
//                else if (_dictStates[statesKeys[i]].FutureState == CellState.Micro)
//                {
//                    _dictStates[statesKeys[i]].CurrentState = CellState.Micro;
//                    _dictStates[statesKeys[i]].FutureState = CellState.NULL;
//                    int microID = _dictStates[statesKeys[i]].FutureTumor;
//                    _dictStates[statesKeys[i]].FutureTumor = -1;
//                    _dictStates[statesKeys[i]].CurrentTumor = microID;
//                    _dictMicro[microID].Add(statesKeys[i]);
//                }
//                else if (_dictStates[statesKeys[i]].FutureState == CellState.Migra)
//                {
//                    _dictStates[statesKeys[i]].CurrentState = CellState.Migra;
//                    _dictStates[statesKeys[i]].FutureState = CellState.NULL;
//                    var tumorID = _dictStates[statesKeys[i]].FutureTumor;
//                    _dictStates[statesKeys[i]].FutureTumor = -1;
//                    _dictStates[statesKeys[i]].CurrentTumor = tumorID;
//                    _dictAsyncMig[0].Add(statesKeys[i], 0);
//                }
//            }
//            Notification.FinalCompletionNotification("   ");
//            List<TVertex> keys = _dictAsyncMig[1].Keys.ToList();
//            int count = keys.Count;
//            for (int i = 0; i < count; i++)
//                _dictAsyncMig[0].Add(keys[i], _dictAsyncMig[1][keys[i]]);
//            _dictAsyncMig[1].Clear();
//            for (int i = 0; i < _dictAsyncTum[1].Count; i++)
//                _dictAsyncTum[0].Add(_dictAsyncTum[1][i]);
//            _dictAsyncTum[1].Clear();
//        }
//        private void CheckMicrometastasisColonization()
//        {
//            List<int> micrometastasisToUpdate = new List<int>();
//            List<int> micrometastasisIDs = _dictMicro.Keys.ToList();
//            bool written = false;
//            int count = micrometastasisIDs.Count;
//            for (int i = 0; i < count; i++)
//            {
//                Notification.CompletionNotification(i, count, ref written, "   ");
//                double prob = _random.NextDouble();
//                if (prob <= _parametersSettings.psi_mic)
//                {
//                    // satisfactory colonization
//                    _dictTumor.Add(micrometastasisIDs[i], new List<TVertex>());
//                    List<TVertex> cellsInMicrometastasis = _dictMicro[micrometastasisIDs[i]];
//                    for (int j = 0; j < cellsInMicrometastasis.Count; j++)
//                    {
//                        _dictStates[cellsInMicrometastasis[j]].CurrentState = CellState.Tumor;
//                        _dictTumor[micrometastasisIDs[i]].Add(cellsInMicrometastasis[j]);
//                    }
//                    micrometastasisToUpdate.Add(micrometastasisIDs[i]);
//                }
//            }
//            for (int i = 0; i < micrometastasisToUpdate.Count; i++)
//                _dictMicro.Remove(micrometastasisToUpdate[i]);
//            Notification.FinalCompletionNotification("   ");
//        }
//        private void UpdateSynchronousCells()
//        {
//            List<TVertex> cells = _dictStates.Keys.ToList();
//            int cellsCount = cells.Count;
//            bool written = false;
//            for (int i = 0; i < cellsCount; i++)
//            {
//                Notification.CompletionNotification(i, cellsCount, ref written, "   ");
//                if (_dictStates[cells[i]].Updated == false)
//                {
//                    _dictStates[cells[i]].Updated = true;
//                    if (_dictStates[cells[i]].CurrentState == CellState.Lumen ||
//                        _dictStates[cells[i]].CurrentState == CellState.Epith ||
//                        _dictStates[cells[i]].CurrentState == CellState.Strom)
//                    {
//                        List<TVertex> neighbours = GetN(cells[i]);
//                        List<TVertex> nearNeighbours = GetNN(cells[i], neighbours);
//                        List<TVertex> nearNeighbours3 = GetNNE(cells[i], nearNeighbours, new List<CellState> { CellState.Tumor });
//                        if (nearNeighbours3.Count > 0)
//                        {
//                            // tumor growth
//                            List<int> tumorCompitingIDs = GetCompitingTumorsID(nearNeighbours3);
//                            List<int[]> nutrientsVectorsRegion = GetRegionNutrients(cells[i]);
//                            List<double> individualTumoralCellApparitionProbs = new List<double>();
//                            // all expanding tumors growth probabilities
//                            for (int j = 0; j < tumorCompitingIDs.Count; j++)
//                            {
//                                int[] tumorCentroid = GetTumorCentroid(tumorCompitingIDs[j]);
//                                int[] cellPos = MF.GetTVertexID(cells[i]);
//                                int[] expansionVector = MF.BuildVector(cellPos, tumorCentroid);
//                                List<double> simValues = new List<double>();
//                                if (nutrientsVectorsRegion.Count != 0)
//                                {
//                                    for (int k = 0; k < nutrientsVectorsRegion.Count; k++)
//                                    {
//                                        double sim = MF.GetSim(expansionVector, nutrientsVectorsRegion[k]);
//                                        double simAlt = Math.Cos(Math.Acos(sim) / 3);
//                                        simValues.Add(simAlt);
//                                    }
//                                }
//                                else simValues.Add(1);
//                                int maxSimIndex = MF.GetMaxValueIndex(simValues);
//                                double beta = simValues[maxSimIndex];
//                                List<TVertex> tumorCells = _dictTumor[tumorCompitingIDs[j]];
//                                bool mode = true;
//                                if (_heavisideMode == HeavisideMode.Population)
//                                {
//                                    if (tumorCells.Count > _modelSettings.P0v)
//                                        mode = false;
//                                }
//                                else
//                                {
//                                    if (_generation > _modelSettings.N_l)
//                                        mode = false;
//                                }
//                                if (mode)
//                                {
//                                    //avascular
//                                    CellState healthyCellState = _dictStates[cells[i]].CurrentState;
//                                    if (healthyCellState == CellState.Strom)
//                                        individualTumoralCellApparitionProbs.Add(0);
//                                    else
//                                    {
//                                        int relativetime = _dictTumorData[tumorCompitingIDs[j]];
//                                        double avascularProb = GetGrowthProbAvascular(_generation - relativetime);
//                                        double prob = avascularProb * beta;
//                                        individualTumoralCellApparitionProbs.Add(prob);
//                                    }
//                                }
//                                else
//                                {
//                                    //vascular
//                                    int relativetime = _dictTumorData[tumorCompitingIDs[j]];
//                                    double vascularProb = GetGrowthProbVascular(_generation - (relativetime + _modelSettings.N_l));
//                                    double prob = vascularProb * beta;
//                                    individualTumoralCellApparitionProbs.Add(prob);
//                                }
//                            }
//                            int maxProbIndexGrowth = MF.GetMaxValueIndex(individualTumoralCellApparitionProbs);
//                            int expandingTumor = tumorCompitingIDs[maxProbIndexGrowth];
//                            double rho3 = individualTumoralCellApparitionProbs[maxProbIndexGrowth];
//                            if (_dictStates[cells[i]].CurrentState == CellState.Strom)
//                            {
//                                // checking migrant cell apparition probability
//                                List<double> migrantApparitionProbs = new List<double>();
//                                for (int j = 0; j < tumorCompitingIDs.Count; j++)
//                                {
//                                    List<TVertex> tumorCurrentCells = _dictTumor[tumorCompitingIDs[j]];
//                                    if (tumorCurrentCells.Count <= _modelSettings.P0v)
//                                        migrantApparitionProbs.Add(0);
//                                    else
//                                    {
//                                        double prob = GetMigrantCellApparitionProb(tumorCurrentCells.Count);
//                                        migrantApparitionProbs.Add(prob);
//                                    }
//                                }
//                                int maxProbIndexApparition = MF.GetMaxValueIndex(migrantApparitionProbs);
//                                int tumorSpawningMigrantCell = tumorCompitingIDs[maxProbIndexApparition];
//                                double rho4 = migrantApparitionProbs[maxProbIndexApparition];
//                                if (rho3 + rho4 > 1) throw new Exception("Probabilities sums more than 1.");
//                                double X = _random.NextDouble();
//                                // deciding if the tumor grows or throw a new migrant cell
//                                if (X <= rho3)
//                                {
//                                    _dictStates[cells[i]].FutureState = CellState.Tumor;
//                                    _dictStates[cells[i]].FutureTumor = expandingTumor;
//                                }
//                                else if (rho3 < X && X <= rho3 + rho4)
//                                {
//                                    _dictStates[cells[i]].FutureState = CellState.Migra;
//                                    _dictStates[cells[i]].FutureTumor = tumorSpawningMigrantCell;
//                                }
//                            }
//                            else
//                            {
//                                // applying tumor growth rule, updating states
//                                double X = _random.NextDouble();
//                                if (X <= rho3)
//                                {
//                                    _dictStates[cells[i]].FutureState = CellState.Tumor;
//                                    _dictStates[cells[i]].FutureTumor = expandingTumor;
//                                }
//                            }
//                        }
//                        else
//                        {
//                            List<TVertex> nearNeighbours5 = GetNNE(cells[i], nearNeighbours, new List<CellState> { CellState.Micro });
//                            if (nearNeighbours5.Count > 0)
//                            {
//                                // micrometastasis growth
//                                List<int> microCompitingIDs = GetCompitingTumorsID(nearNeighbours5);
//                                List<int[]> nutrientsVectorsRegion = GetRegionNutrients(cells[i]);
//                                List<double> individualMicroCellApparitionProbs = new List<double>();
//                                for (int j = 0; j < microCompitingIDs.Count; j++)
//                                {
//                                    int[] microCentroid = GetTumorCentroid(microCompitingIDs[j]);
//                                    int[] cellPos = MF.GetTVertexID(cells[i]);
//                                    int[] expansionVector = MF.BuildVector(cellPos, microCentroid);
//                                    List<double> simValues = new List<double>();
//                                    if (nutrientsVectorsRegion.Count != 0)
//                                    {
//                                        for (int k = 0; k < nutrientsVectorsRegion.Count; k++)
//                                        {
//                                            double sim = MF.GetSim(expansionVector, nutrientsVectorsRegion[k]);
//                                            double simAlt = Math.Cos(Math.Acos(sim) / 3);
//                                            simValues.Add(simAlt);
//                                        }
//                                    }
//                                    else simValues.Add(1);
//                                    int maxSimIndex = MF.GetMaxValueIndex(simValues);
//                                    double beta = simValues[maxSimIndex];
//                                    List<TVertex> tumorCells = _dictTumor[microCompitingIDs[j]];
//                                    bool mode = true;
//                                    if (_heavisideMode == HeavisideMode.Population)
//                                    {
//                                        if (tumorCells.Count > _modelSettings.P0v)
//                                            mode = false;
//                                    }
//                                    else
//                                    {
//                                        if (_generation > _modelSettings.N_l)
//                                            mode = false;
//                                    }
//                                    if (mode)
//                                    {
//                                        int relativetime = _dictTumorData[microCompitingIDs[j]];
//                                        double avascularProb = GetGrowthProbAvascular(_generation - relativetime);
//                                        double prob = avascularProb * beta;
//                                        individualMicroCellApparitionProbs.Add(prob);
//                                    }
//                                    else individualMicroCellApparitionProbs.Add(0);
//                                }
//                                int maxProbIndexGrowth = MF.GetMaxValueIndex(individualMicroCellApparitionProbs);
//                                int expandingMicro = microCompitingIDs[maxProbIndexGrowth];
//                                double rho5 = individualMicroCellApparitionProbs[maxProbIndexGrowth];
//                                double X = _random.NextDouble();
//                                // applying rule, updating states
//                                if (X <= rho5)
//                                {
//                                    _dictStates[cells[i]].FutureState = CellState.Micro;
//                                    _dictStates[cells[i]].FutureTumor = expandingMicro;
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            Notification.FinalCompletionNotification("   ");
//        }
//        private void SetSynchronousUpdateList()
//        {
//            _synchronousCellsToUpdate = new Dictionary<TVertex, bool>();
//            List<int> tumorKeys = _dictTumor.Keys.ToList();
//            int tumorKeysCount = tumorKeys.Count;
//            for (int i = 0; i < tumorKeysCount; i++)
//            {
//                List<TVertex> tumorCells = _dictTumor[tumorKeys[i]];
//                int tumorCellsCount = tumorCells.Count;
//                for (int j = 0; j < tumorCellsCount; j++)
//                {
//                    TVertex cell = tumorCells[j];
//                    List<TVertex> neighbours = GetN(cell);
//                    List<TVertex> nearNeighbours = GetNN(cell, neighbours);
//                    List<TVertex> normalNearNeighbours = GetNNE(cell, nearNeighbours, new List<CellState> { CellState.Epith, CellState.Lumen, CellState.Strom });
//                    int normalNearNeighboursCount = normalNearNeighbours.Count;
//                    if (normalNearNeighboursCount > 0)
//                    {
//                        for (int k = 0; k < normalNearNeighboursCount; k++)
//                        {
//                            TVertex normalCellKey = normalNearNeighbours[k];
//                            if (!_synchronousCellsToUpdate.ContainsKey(normalCellKey))
//                                _synchronousCellsToUpdate.Add(normalCellKey, true);
//                        }
//                    }
//                }
//            }
//            List<int> microKeys = _dictMicro.Keys.ToList();
//            int microKeysCount = microKeys.Count;
//            for (int i = 0; i < microKeysCount; i++)
//            {
//                List<TVertex> microCells = _dictMicro[microKeys[i]];
//                int microCellsCount = microCells.Count;
//                for (int j = 0; j < microCellsCount; j++)
//                {
//                    TVertex cell = microCells[j];
//                    List<TVertex> neighbours = GetN(cell);
//                    List<TVertex> nearNeighbours = GetNN(cell, neighbours);
//                    List<TVertex> normalNearNeighbours = GetNNE(cell, nearNeighbours, new List<CellState> { CellState.Epith, CellState.Lumen, CellState.Strom });
//                    int normalNearNeighboursCount = normalNearNeighbours.Count;
//                    if (normalNearNeighboursCount > 0)
//                    {
//                        for (int k = 0; k < normalNearNeighboursCount; k++)
//                        {
//                            TVertex normalCellKey = normalNearNeighbours[k];
//                            if (!_synchronousCellsToUpdate.ContainsKey(normalCellKey))
//                                _synchronousCellsToUpdate.Add(normalCellKey, false);
//                        }
//                    }
//                }
//            }
//        }
//        private void UpdateSynchronousCellsOptimized()
//        {
//            List<KeyValuePair<TVertex, bool>> cellsToUpdatePairs = _synchronousCellsToUpdate.ToList();
//            int cellsCount = cellsToUpdatePairs.Count;
//            bool written = false;
//            for (int i = 0; i < cellsCount; i++)
//            {
//                TVertex cellKey = cellsToUpdatePairs[i].Key;
//                bool cellValue = cellsToUpdatePairs[i].Value;
//                Notification.CompletionNotification(i, cellsCount, ref written, "   ");
//                if (_dictStates[cellKey].Updated == true)
//                    throw new Exception("Updated cell made it to the synchronous update list.");
//                _dictStates[cellKey].Updated = true;
//                List<TVertex> neighbours = GetN(cellKey);
//                List<TVertex> nearNeighbours = GetNN(cellKey, neighbours);
//                switch (cellValue)
//                {
//                    // if cellValue is true => there is a tumor expanding towards cellKey
//                    case true:
//                        List<TVertex> nearNeighbours3 = GetNNE(cellKey, nearNeighbours, new List<CellState> { CellState.Tumor });
//                        if (nearNeighbours3.Count == 0)
//                            throw new Exception("No tumoral neighbours cell made it to the synchronous update list.");
//                        // tumor growth
//                        List<int> tumorCompitingIDs = GetCompitingTumorsID(nearNeighbours3);
//                        List<int[]> nutrientsVectorsRegion = GetRegionNutrients(cellKey);
//                        List<double> individualTumoralCellApparitionProbs = new List<double>();
//                        // all expanding tumors growth probabilities
//                        for (int j = 0; j < tumorCompitingIDs.Count; j++)
//                        {
//                            int[] tumorCentroid = GetTumorCentroid(tumorCompitingIDs[j]);
//                            int[] cellPos = MF.GetTVertexID(cellKey);
//                            int[] expansionVector = MF.BuildVector(cellPos, tumorCentroid);
//                            List<double> simValues = new List<double>();
//                            if (nutrientsVectorsRegion.Count != 0)
//                            {
//                                for (int k = 0; k < nutrientsVectorsRegion.Count; k++)
//                                {
//                                    double sim = MF.GetSim(expansionVector, nutrientsVectorsRegion[k]);
//                                    double simAlt = Math.Cos(Math.Acos(sim) / 3);
//                                    simValues.Add(simAlt);
//                                }
//                            }
//                            else simValues.Add(1);
//                            int maxSimIndex = MF.GetMaxValueIndex(simValues);
//                            double beta = simValues[maxSimIndex];
//                            List<TVertex> tumorCells = _dictTumor[tumorCompitingIDs[j]];
//                            bool mode = true;
//                            if (_heavisideMode == HeavisideMode.Population)
//                            {
//                                if (tumorCells.Count > _modelSettings.P0v)
//                                    mode = false;
//                            }
//                            else
//                            {
//                                if (_generation > _modelSettings.N_l)
//                                    mode = false;
//                            }
//                            if (mode)
//                            {
//                                CellState healthyCellState = _dictStates[cellKey].CurrentState;
//                                if (healthyCellState == CellState.Strom)
//                                    individualTumoralCellApparitionProbs.Add(0);
//                                else
//                                {
//                                    int relativetime = _dictTumorData[tumorCompitingIDs[j]];
//                                    double avascularProb = GetGrowthProbAvascular(_generation - relativetime);
//                                    double prob = avascularProb * beta;
//                                    individualTumoralCellApparitionProbs.Add(prob);
//                                }
//                            }
//                            else
//                            {
//                                int relativetime = _dictTumorData[tumorCompitingIDs[j]];
//                                double vascularProb = GetGrowthProbVascular(_generation - (relativetime + _modelSettings.N_l));
//                                double prob = vascularProb * beta;
//                                individualTumoralCellApparitionProbs.Add(prob);
//                            }
//                        }
//                        int maxProbIndexGrowth = MF.GetMaxValueIndex(individualTumoralCellApparitionProbs);
//                        int expandingTumor = tumorCompitingIDs[maxProbIndexGrowth];
//                        double rho3 = individualTumoralCellApparitionProbs[maxProbIndexGrowth];
//                        if (_dictStates[cellKey].CurrentState == CellState.Strom)
//                        {
//                            // checking migrant cell apparition probability
//                            List<double> migrantApparitionProbs = new List<double>();
//                            for (int j = 0; j < tumorCompitingIDs.Count; j++)
//                            {
//                                List<TVertex> tumorCurrentCells = _dictTumor[tumorCompitingIDs[j]];
//                                if (tumorCurrentCells.Count <= _modelSettings.P0v)
//                                    migrantApparitionProbs.Add(0);
//                                else
//                                {
//                                    double prob = GetMigrantCellApparitionProb(tumorCurrentCells.Count);
//                                    migrantApparitionProbs.Add(prob);
//                                }
//                            }
//                            int maxProbIndexApparition = MF.GetMaxValueIndex(migrantApparitionProbs);
//                            int tumorSpawningMigrantCell = tumorCompitingIDs[maxProbIndexApparition];
//                            double rho4 = migrantApparitionProbs[maxProbIndexApparition];
//                            if (rho3 + rho4 > 1) throw new Exception("Probabilities sums more than 1.");
//                            double Xboth = _random.NextDouble();
//                            // deciding if the tumor grows or throw a new migrant cell
//                            if (Xboth <= rho3)
//                            {
//                                _dictStates[cellKey].FutureState = CellState.Tumor;
//                                _dictStates[cellKey].FutureTumor = expandingTumor;
//                            }
//                            else if (rho3 < Xboth && Xboth <= rho3 + rho4)
//                            {
//                                _dictStates[cellKey].FutureState = CellState.Migra;
//                                _dictStates[cellKey].FutureTumor = tumorSpawningMigrantCell;
//                            }
//                        }
//                        else
//                        {
//                            // applying tumor growth rule, updating states
//                            double Xtumor = _random.NextDouble();
//                            if (Xtumor <= rho3)
//                            {
//                                _dictStates[cellKey].FutureState = CellState.Tumor;
//                                _dictStates[cellKey].FutureTumor = expandingTumor;
//                            }
//                        }
//                        break;
//                    // if cellValue is false => there is a micrometastasis expanding towards cellKey
//                    case false:
//                        List<TVertex> nearNeighbours5 = GetNNE(cellKey, nearNeighbours, new List<CellState> { CellState.Micro });
//                        if (nearNeighbours5.Count > 0)
//                            throw new Exception("No micrometastasis neighbours cell made it to the synchronous update list.");
//                        // micrometastasis growth
//                        List<int> microCompitingIDs = GetCompitingTumorsID(nearNeighbours5);
//                        List<int[]> nutrientsVectorsRegion5 = GetRegionNutrients(cellKey);
//                        List<double> individualMicroCellApparitionProbs = new List<double>();
//                        for (int j = 0; j < microCompitingIDs.Count; j++)
//                        {
//                            int[] microCentroid = GetTumorCentroid(microCompitingIDs[j]);
//                            int[] cellPos = MF.GetTVertexID(cellKey);
//                            int[] expansionVector = MF.BuildVector(cellPos, microCentroid);
//                            List<double> simValues = new List<double>();
//                            if (nutrientsVectorsRegion5.Count != 0)
//                            {
//                                for (int k = 0; k < nutrientsVectorsRegion5.Count; k++)
//                                {
//                                    double sim = MF.GetSim(expansionVector, nutrientsVectorsRegion5[k]);
//                                    double simAlt = Math.Cos(Math.Acos(sim) / 3);
//                                    simValues.Add(simAlt);
//                                }
//                            }
//                            else simValues.Add(1);
//                            int maxSimIndex = MF.GetMaxValueIndex(simValues);
//                            double beta = simValues[maxSimIndex];
//                            List<TVertex> tumorCells = _dictTumor[microCompitingIDs[j]];
//                            bool mode = true;
//                            if (_heavisideMode == HeavisideMode.Population)
//                            {
//                                if (tumorCells.Count > _modelSettings.P0v)
//                                    mode = false;
//                            }
//                            else
//                            {
//                                if (_generation > _modelSettings.N_l)
//                                    mode = false;
//                            }
//                            if (mode)
//                            {
//                                int relativetime = _dictTumorData[microCompitingIDs[j]];
//                                double avascularProb = GetGrowthProbAvascular(_generation - relativetime);
//                                double prob = avascularProb * beta;
//                                individualMicroCellApparitionProbs.Add(prob);
//                            }
//                            else individualMicroCellApparitionProbs.Add(0);
//                        }
//                        int maxProbIndexGrowth5 = MF.GetMaxValueIndex(individualMicroCellApparitionProbs);
//                        int expandingMicro = microCompitingIDs[maxProbIndexGrowth5];
//                        double rho5 = individualMicroCellApparitionProbs[maxProbIndexGrowth5];
//                        double X = _random.NextDouble();
//                        // applying rule, updating states
//                        if (X <= rho5)
//                        {
//                            _dictStates[cellKey].FutureState = CellState.Micro;
//                            _dictStates[cellKey].FutureTumor = expandingMicro;
//                        }
//                        break;
//                }
//            }
//            Notification.FinalCompletionNotification("   ");
//        }
//        private void CheckMicrometastasisSurvival()
//        {
//            List<int> micrometastasisToRemove = new List<int>();
//            List<int> micrometastasisIDs = _dictMicro.Keys.ToList();
//            bool written = false;
//            int count = micrometastasisIDs.Count;
//            for (int i = 0; i < count; i++)
//            {
//                Notification.CompletionNotification(i, count, ref written, "   ");
//                double prob = _random.NextDouble();
//                if (prob <= 1 - _parametersSettings.xi_mic)
//                {
//                    // micrometastasis death
//                    List<TVertex> cellsInMicrometastasis = _dictMicro[micrometastasisIDs[i]];
//                    _dictTumorData.Remove(micrometastasisIDs[i]);
//                    for (int j = 0; j < cellsInMicrometastasis.Count; j++)
//                    {
//                        _dictStates[cellsInMicrometastasis[j]].CurrentState = _dictStatesOriginal[cellsInMicrometastasis[j]];
//                        _dictStates[cellsInMicrometastasis[j]].CurrentTumor = -1;
//                    }
//                    micrometastasisToRemove.Add(micrometastasisIDs[i]);
//                }
//            }
//            for (int i = 0; i < micrometastasisToRemove.Count; i++)
//                _dictMicro.Remove(micrometastasisToRemove[i]);
//            Notification.FinalCompletionNotification("   ");
//        }
//        private void UpdateTumorMigratoryCells()
//        {
//            int count = 0;
//            bool written = false;
//            int total = _dictAsyncTum[0].Count;
//            while (_dictAsyncTum[0].Count != 0)
//            {
//                Notification.CompletionNotification(count, total, ref written, "   ");
//                count++;
//                int index = _random.Next(_dictAsyncTum[0].Count);
//                TVertex cell = _dictAsyncTum[0][index];
//                _dictAsyncTum[0].Remove(cell);
//                _dictAsyncTum[1].Add(cell);
//                _dictStates[cell].Updated = true;
//                int tumorID = _dictStates[cell].CurrentTumor;
//                List<TVertex> tumorCells = _dictTumor[tumorID];
//                double prob = 0.0;
//                if (tumorCells.Count > _modelSettings.P0v)
//                    prob = Math.Pow(tumorCells.Count / (double)(_modelSettings.Kv + _parametersSettings.K_mig), 1.0 / _parametersSettings.eta_mig);
//                double X = _random.NextDouble();
//                if (X <= prob)
//                {
//                    List<TVertex> neighbours = GetN(cell);
//                    List<TVertex> distantNeighbours = GetDN(cell, neighbours);
//                    List<TVertex> filteredDistantNeighbours = GetDNE(cell, distantNeighbours, new List<CellState> { CellState.Strom, CellState.Tumor, CellState.Micro });
//                    TVertex w = GetMetastasisDestiny(filteredDistantNeighbours);
//                    _dictAsyncCS.Add(_cellInBloodstreamIDGenerator, w);
//                    _cellInBloodstreamIDGenerator++;
//                }
//            }
//            Notification.FinalCompletionNotification("   ");
//        }
//        private void UpdateMigratoryCells()
//        {
//            int tentativeMovements = 0;
//            while (tentativeMovements < _parametersSettings.mu_mig)
//            {
//                Console.WriteLine("   cellularautomata library: tentative movement " + tentativeMovements);
//                int count = 0;
//                bool written = false;
//                int total = _dictAsyncMig[0].Count;
//                tentativeMovements++;
//                while (_dictAsyncMig[0].Count != 0)
//                {
//                    Notification.CompletionNotification(count, total, ref written, "   ");
//                    count++;
//                    int index = _random.Next(_dictAsyncMig[0].Keys.Count);
//                    TVertex cell = _dictAsyncMig[0].Keys.ToList()[index];
//                    int movements = _dictAsyncMig[0][cell];
//                    _dictAsyncMig[0].Remove(cell);
//                    _dictStates[cell].Updated = true;
//                    List<TVertex> neighbours = GetN(cell);
//                    List<TVertex> distantNeighbours = GetDN(cell, neighbours);
//                    List<TVertex> filteredDistantNeighbours = GetDNE(cell, distantNeighbours, new List<CellState> { CellState.Strom, CellState.Tumor, CellState.Micro });
//                    if (filteredDistantNeighbours.Count > 0)
//                    {
//                        // metastasis
//                        _dictStates[cell].CurrentState = CellState.Strom;
//                        TVertex w = GetMetastasisDestiny(filteredDistantNeighbours);
//                        _dictAsyncCS.Add(_cellInBloodstreamIDGenerator, w);
//                        _cellInBloodstreamIDGenerator++;
//                    }
//                    else
//                    {
//                        double moveOrNotMove = _random.NextDouble();
//                        if (moveOrNotMove <= _parametersSettings.psi_mig)
//                        {
//                            // cell choose to move
//                            List<TVertex> nearNeighbours = GetNN(cell, neighbours);
//                            List<TVertex> nearNeighbours2 = GetNNE(cell, nearNeighbours, new List<CellState> { CellState.Epith });
//                            if (nearNeighbours2.Count > 0)
//                            {
//                                // cell can move
//                                _dictStates[cell].CurrentState = CellState.Strom;
//                                int tumorID = _dictStates[cell].CurrentTumor;
//                                int[] tumorCentroid = GetTumorCentroid(tumorID);
//                                List<TVertex> availableDestinies = GetAvailableDestinies(cell, MF.BuildTVertexID(tumorCentroid), nearNeighbours2);
//                                TVertex w = cell;
//                                if (availableDestinies.Count != 0)
//                                {
//                                    // possible destinies
//                                    int[] cellPos = MF.GetTVertexID(cell);
//                                    List<double> probs = new List<double>();
//                                    for (int i = 0; i < availableDestinies.Count; i++)
//                                    {
//                                        List<int[]> nutrientsVectorsRegion = GetRegionNutrients(availableDestinies[i]);
//                                        int[] destinyPos = MF.GetTVertexID(availableDestinies[i]);
//                                        int[] migrationVector = MF.BuildVector(destinyPos, cellPos);
//                                        List<double> simValues = new List<double>();
//                                        if (nutrientsVectorsRegion.Count != 0)
//                                        {
//                                            for (int j = 0; j < nutrientsVectorsRegion.Count; j++)
//                                            {
//                                                double sim = MF.GetSim(migrationVector, nutrientsVectorsRegion[j]);
//                                                double simAlt = Math.Cos(Math.Acos(sim) / 3);
//                                                simValues.Add(simAlt);
//                                            }
//                                        }
//                                        else simValues.Add(1);
//                                        int maxSimIndex = MF.GetMaxValueIndex(simValues);
//                                        double beta = simValues[maxSimIndex];
//                                        double prob = 1 / (double)availableDestinies.Count * beta;
//                                        probs.Add(prob);
//                                    }
//                                    // choosing one possible destiny
//                                    double random = _random.NextDouble();
//                                    List<double> normalizedProbs = MF.GetNormalizedProbabilities(probs);
//                                    List<double> normalizedAddedProbs = MF.GetNormalizedAddedProbs(normalizedProbs);
//                                    int availableMovementIndex = 0;
//                                    for (int i = 1; i < normalizedAddedProbs.Count; i++)
//                                    {
//                                        if (normalizedAddedProbs[i - 1] <= random && random < normalizedAddedProbs[i])
//                                        {
//                                            availableMovementIndex = i - 1;
//                                            break;
//                                        }
//                                    }
//                                    w = availableDestinies[availableMovementIndex];
//                                }
//                                // applying movement rule, updating states
//                                double rho = Math.Pow(movements / (double)_parametersSettings.mu_max, 1.0 / _parametersSettings.eta_mig_prima);
//                                double X = _random.NextDouble();
//                                if (X <= 1 - rho)
//                                {
//                                    _dictStates[w].CurrentState = CellState.Migra;
//                                    _dictAsyncMig[1].Add(w, movements + 1);
//                                }
//                            }
//                            else
//                            {
//                                // cell cannot move
//                                double rho = Math.Pow(movements / _parametersSettings.mu_max, 1 / _parametersSettings.eta_mig_prima);
//                                double X = _random.NextDouble();
//                                if (X <= 1 - rho)
//                                {
//                                    _dictStates[cell].CurrentState = CellState.Migra;
//                                    _dictAsyncMig[1].Add(cell, movements);
//                                }
//                                else _dictStates[cell].CurrentState = CellState.Strom;
//                            }
//                        }
//                        else
//                        {
//                            // cell choose not to move
//                            _dictAsyncMig[1].Add(cell, movements);
//                        }
//                    }
//                }
//                Notification.FinalCompletionNotification("   ");
//            }
//        }
//        private void UpdateMigratoryCellsInBloodstream()
//        {
//            int count = 0;
//            bool written = false;
//            int total = _dictAsyncCS.Count;
//            while (_dictAsyncCS.Count != 0)
//            {
//                Notification.CompletionNotification(count, total, ref written, "   ");
//                count++;
//                int index = _random.Next(_dictAsyncCS.Keys.Count);
//                int cellID = _dictAsyncCS.Keys.ToList()[index];
//                TVertex destiny = _dictAsyncCS[cellID];
//                _dictAsyncCS.Remove(cellID);

//                int destinyOrgan = _dictStates[destiny].Organ;
//                double arrivalProbability = (destinyOrgan == 0) ? _parametersSettings.psi_met0 : _parametersSettings.psi_met1;
//                double random0 = _random.NextDouble();
//                double random1 = _random.NextDouble();
//                if (random0 <= _parametersSettings.xi_sc && random1 <= arrivalProbability && _dictStates[destiny].CurrentState == CellState.Strom)
//                {
//                    _dictStates[destiny].CurrentState = CellState.Micro;
//                    _dictStates[destiny].CurrentTumor = _tumorIDGenerator;
//                    _dictMicro.Add(_tumorIDGenerator, new List<TVertex> { destiny });
//                    _dictTumorData.Add(_tumorIDGenerator, _generation);
//                    _tumorIDGenerator++;
//                }
//            }
//            Notification.FinalCompletionNotification("   ");
//            _cellInBloodstreamIDGenerator = 0;
//        }
//    }
//}