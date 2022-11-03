using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using TEdge = QuickGraph.UndirectedEdge<string>;
using TVertex = System.String;

namespace AutomataLibrary2D
{
    public class CellularAutomata1_5
    {
        private SmallWorldNetwork _smallWorldNetwork = null;
        private Dictionary<TVertex, CellState> _dictStatesOriginal = null;
        private Dictionary<TVertex, CellData> _dictStates = null;
        private Dictionary<TVertex, int>[] _dictAsyncMig = null;
        private Dictionary<int, TVertex> _dictAsyncCS = null;
        private List<TVertex>[] _dictAsyncTum = null;
        private Dictionary<int, List<TVertex>> _dictTumor = null;
        private Dictionary<int, List<TVertex>> _dictMicro = null;
        private Dictionary<int, int> _dictRelativeTimesData = null;

        private Dictionary<TVertex, int> _dictWanderedDistance = null;
        private Dictionary<int, int> _dictWanderedDistanceMeasurementsMetas = null;
        private Dictionary<int, int> _dictWanderedDistanceMeasurementsDeath = null;

        private Dictionary<int, int>[] _dictSuccessfulMicrometastasis = null;
        private Dictionary<int, int>[] _dictFailedMicrometastasis = null;


        private Random _random = null;
        private ModelSettings _modelSettings = null;
        private OrganScheme _organ1Scheme = null;
        private OrganScheme _organ2Scheme = null;
        private NutrientsSettings _nutrientsSettings = null;
        private ParametersSettings _parametersSettings = null;
        private int _generation = 0;
        private int _tumorIDGenerator = 0;
        private int[] _scaleMax = null;

        private Dictionary<TVertex, bool> _synchronousCellsToUpdate = null;
        private Dictionary<TVertex, int> _tumorFrontiers = null;
        private Dictionary<TVertex, int> _microFrontiers = null;

        private int _cellInBloodstreamIDGenerator = 0;
        private int _amountCellsInBloodStreamFromTumors = 0;
        private int _amountCellsInBloodStreamFromMigra = 0;
        private int _amountCellsGeneratedMigration = 0;

        public List<KeyValuePair<TVertex, CellState>> DictOriginalStates
        {
            get
            {
                return _dictStatesOriginal.ToList();
            }
        }
        public List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>> DictTumors
        {
            get
            {
                List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>> result = new List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>>();
                List<int> tumorIDs = _dictTumor.Keys.ToList();
                for (int i = 0; i < tumorIDs.Count; i++)
                {
                    List<TVertex> tumorCells = _dictTumor[tumorIDs[i]];
                    List<KeyValuePair<TVertex, int>> ls = new List<KeyValuePair<TVertex, int>>();
                    for (int j = 0; j < tumorCells.Count; j++)
                    {
                        TVertex cell = tumorCells[j];
                        int amount = _dictStates[cell].CantCells;
                        KeyValuePair<TVertex, int> pair = new KeyValuePair<TVertex, int>(cell, amount);
                        ls.Add(pair);
                    }
                    result.Add(new KeyValuePair<int, List<KeyValuePair<TVertex, int>>>(tumorIDs[i], ls));
                }
                return result;
            }
        }
        public List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>> DictMicro
        {
            get
            {
                List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>> result = new List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>>();
                List<int> microIDs = _dictMicro.Keys.ToList();
                for (int i = 0; i < microIDs.Count; i++)
                {
                    List<TVertex> microCells = _dictMicro[microIDs[i]];
                    List<KeyValuePair<TVertex, int>> ls = new List<KeyValuePair<TVertex, int>>();
                    for (int j = 0; j < microCells.Count; j++)
                    {
                        TVertex cell = microCells[j];
                        int amount = _dictStates[cell].CantCells;
                        KeyValuePair<TVertex, int> pair = new KeyValuePair<TVertex, int>(cell, amount);
                        ls.Add(pair);
                    }
                    result.Add(new KeyValuePair<int, List<KeyValuePair<TVertex, int>>>(microIDs[i], ls));
                }
                return result;
            }
        }
        public List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>> DictMigra
        {
            get
            {
                Dictionary<int, List<KeyValuePair<TVertex, int>>> migratoryDictionay = new Dictionary<int, List<KeyValuePair<TVertex, int>>>();
                List<TVertex> migratoryCells = _dictAsyncMig[0].Keys.ToList();
                if (migratoryCells.Count != 0)
                {
                    for (int i = 0; i < migratoryCells.Count; i++)
                    {
                        int migratoryCellOriginalTumor = _dictStates[migratoryCells[i]].CurrentTumor;
                        int amount = _dictStates[migratoryCells[i]].CantCells;
                        KeyValuePair<TVertex, int> pair = new KeyValuePair<TVertex, int>(migratoryCells[i], amount);
                        if (migratoryDictionay.ContainsKey(migratoryCellOriginalTumor))
                            migratoryDictionay[migratoryCellOriginalTumor].Add(pair);
                        else
                        {
                            migratoryDictionay.Add(migratoryCellOriginalTumor, new List<KeyValuePair<TVertex, int>>());
                            migratoryDictionay[migratoryCellOriginalTumor].Add(pair);
                        }
                    }
                }
                else
                {
                    List<int> tumorsID = _dictTumor.Keys.ToList();
                    for (int i = 0; i < tumorsID.Count; i++)
                    {
                        migratoryDictionay.Add(tumorsID[i], new List<KeyValuePair<TVertex, int>>());
                    }
                }
                return migratoryDictionay.ToList();
            }
        }
        public Dictionary<int, int> DictExteriorCells(string type)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            switch (type)
            {
                case "[Migra]":
                    List<KeyValuePair<int, List<KeyValuePair<TVertex, int>>>> migra = DictMigra;
                    for (int i = 0; i < migra.Count; i++)
                        dict.Add(migra[i].Key, migra[i].Value.Count);
                    break;
                case "[Tumors]":
                    List<KeyValuePair<TVertex, int>> tumors = _tumorFrontiers.ToList();
                    for (int i = 0; i < tumors.Count; i++)
                    {
                        int tumorID = tumors[i].Value;
                        if (!dict.ContainsKey(tumorID))
                            dict.Add(tumorID, 1);
                        else dict[tumorID]++;
                    }
                    break;
                case "[Micros]":
                    List<KeyValuePair<TVertex, int>> micro = _microFrontiers.ToList();
                    for (int i = 0; i < micro.Count; i++)
                    {
                        int microID = micro[i].Value;
                        if (!dict.ContainsKey(microID))
                            dict.Add(microID, 1);
                        else dict[microID]++;
                    }
                    break;
                default:
                    throw new Exception("Unexpected string parameter");
            }
            return dict;
        }
        public int CellsInBloodstreamFromTumors { get { return _amountCellsInBloodStreamFromTumors; } }
        public int CellsInBloodstreamFromMigration { get { return _amountCellsInBloodStreamFromMigra; } }
        public int MigrationCellsGenerated { get { return _amountCellsGeneratedMigration; } }
        public List<KeyValuePair<int, int>> DictWanderedDistanceMeasurementsMetas { get { return _dictWanderedDistanceMeasurementsMetas.ToList(); } }
        public List<KeyValuePair<int, int>> DictWanderedDistanceMeasurementsDeath { get { return _dictWanderedDistanceMeasurementsDeath.ToList(); } }
        public List<KeyValuePair<int, int>>[] FailedMicrometastasis { get { return new[] { _dictFailedMicrometastasis[0].ToList(), _dictFailedMicrometastasis[1].ToList() }; } }
        public List<KeyValuePair<int, int>>[] SuccessfulMicrometastasis { get { return new[] { _dictSuccessfulMicrometastasis[0].ToList(), _dictSuccessfulMicrometastasis[1].ToList() }; } }

        public CellularAutomata1_5(SmallWorldNetwork network, ModelSettings model,
            OrganScheme organ1, OrganScheme organ2, NutrientsSettings nutrients,
            ParametersSettings parameters)
        {
            _smallWorldNetwork = network;
            _generation = 0;
            _cellInBloodstreamIDGenerator = 0;
            _amountCellsInBloodStreamFromTumors = 0;
            _amountCellsInBloodStreamFromMigra = 0;
            _amountCellsGeneratedMigration = 0;
            _tumorIDGenerator = 0;
            _random = new Random();
            _modelSettings = model;
            _parametersSettings = parameters;
            _nutrientsSettings = nutrients;
            _organ1Scheme = organ1;
            _organ2Scheme = organ2;
            InitializeDictionaries();
            InitializeGrid();
            InitializeTumor();
        }
        private void InitializeDictionaries()
        {
            _dictStates = new Dictionary<TVertex, CellData>();
            _dictAsyncMig = new Dictionary<TVertex, int>[2];
            _dictAsyncMig[0] = new Dictionary<TVertex, int>();
            _dictAsyncMig[1] = new Dictionary<TVertex, int>();
            _dictTumor = new Dictionary<int, List<TVertex>>();
            _dictRelativeTimesData = new Dictionary<int, int>();
            _dictAsyncCS = new Dictionary<int, TVertex>();
            _dictAsyncTum = new List<TVertex>[2];
            _dictAsyncTum[0] = new List<TVertex>();
            _dictAsyncTum[1] = new List<TVertex>();
            _dictStatesOriginal = new Dictionary<TVertex, CellState>();
            _dictMicro = new Dictionary<int, List<TVertex>>();
            _dictWanderedDistance = new Dictionary<string, int>();
            _dictWanderedDistanceMeasurementsMetas = new Dictionary<int, int>();
            _dictWanderedDistanceMeasurementsDeath = new Dictionary<int, int>();
            for (int i = 0; i <= _parametersSettings.mu_max; i++)
            {
                _dictWanderedDistanceMeasurementsMetas.Add(i, 0);
                _dictWanderedDistanceMeasurementsDeath.Add(i, 0);
            }
            _dictFailedMicrometastasis = new Dictionary<int, int>[2];
            _dictSuccessfulMicrometastasis = new Dictionary<int, int>[2];
            for (int i = 0; i < 2; i++)
            {
                _dictFailedMicrometastasis[i] = new Dictionary<int, int>();
                _dictFailedMicrometastasis[i].Add(-1, 0);
                _dictSuccessfulMicrometastasis[i] = new Dictionary<int, int>();
                _dictSuccessfulMicrometastasis[i].Add(-1, 0);
            }
            List<TVertex> vertices = _smallWorldNetwork.Vertices;
            int verticesCount = vertices.Count;
            for (int i = 0; i < verticesCount; i++)
            {
                _dictStates.Add(vertices[i], new CellData(false, CellState.NULL, CellState.NULL, -1, -1, -1, -1));
                _dictStatesOriginal.Add(vertices[i], CellState.NULL);
            }
            _tumorFrontiers = new Dictionary<TVertex, int>();
            _microFrontiers = new Dictionary<TVertex, int>();
            _scaleMax = new int[_parametersSettings.SimScale];
            for (int i = 0; i < _parametersSettings.SimScale; i++)
            {
                _scaleMax[i] = i + 1;
            }
        }
        private void InitializeGrid()
        {
            switch (_organ1Scheme.SelectedScheme)
            {
                case SelectedScheme.Scheme1:
                    InitializeScheme1(0, 0, _smallWorldNetwork.NetworkDivision, _smallWorldNetwork.NetworkSizeY, 0);
                    break;
                case SelectedScheme.Scheme2:
                    InitializeScheme2(0, 0, _smallWorldNetwork.NetworkDivision, _smallWorldNetwork.NetworkSizeY, 0);
                    break;
                case SelectedScheme.Scheme3:
                    InitializeScheme3(0, 0, _smallWorldNetwork.NetworkDivision, _smallWorldNetwork.NetworkSizeY, 0);
                    break;
            }
            switch (_organ2Scheme.SelectedScheme)
            {
                case SelectedScheme.Scheme1:
                    InitializeScheme1(_smallWorldNetwork.NetworkDivision, 0, _smallWorldNetwork.NetworkSizeX, _smallWorldNetwork.NetworkSizeY, 1);
                    break;
                case SelectedScheme.Scheme2:
                    InitializeScheme2(_smallWorldNetwork.NetworkDivision, 0, _smallWorldNetwork.NetworkSizeX, _smallWorldNetwork.NetworkSizeY, 1);
                    break;
                case SelectedScheme.Scheme3:
                    InitializeScheme3(_smallWorldNetwork.NetworkDivision, 0, _smallWorldNetwork.NetworkSizeX, _smallWorldNetwork.NetworkSizeY, 1);
                    break;
            }
        }
        private void InitializeScheme1(int x0, int y0, int xf, int yf, int organ)
        {
            OrganScheme1 current = null;
            if (organ == 0)
                current = (OrganScheme1)_organ1Scheme;
            else if (organ == 1)
                current = (OrganScheme1)_organ2Scheme;
            else throw new Exception("Invalid organ identifier.");
            CellState state = CellState.NULL;
            for (int i = y0; i < yf; i++)
            {
                if (0 <= i && i < current.Lumen)
                    state = CellState.Lumen;
                else if (current.Lumen <= i && i < current.Lumen + current.Epithelium)
                    state = CellState.Epith;
                else state = CellState.Strom;
                for (int j = x0; j < xf; j++)
                {
                    TVertex vertex = MF.BuildTVertexID(j, i);
                    _dictStates[vertex].CurrentState = state;
                    _dictStates[vertex].Organ = organ;
                    _dictStatesOriginal[vertex] = state;
                }
            }
        }
        private void InitializeScheme2(int x0, int y0, int xf, int yf, int organ)
        {
            OrganScheme2 current = null;
            if (organ == 0)
                current = (OrganScheme2)_organ1Scheme;
            else if (organ == 1)
                current = (OrganScheme2)_organ2Scheme;
            else throw new Exception("Invalid organ identifier.");
            CellState state = CellState.Lumen;
            for (int i = 0; i < yf / 2 + 1; i++)
            {
                if (current.CentralDuct + i < current.CentralDuct + current.CentralDuctRadius)
                    state = CellState.Lumen;
                else if (current.CentralDuct + i >= current.CentralDuct + current.CentralDuctRadius &&
                    current.CentralDuct + i < current.CentralDuct + current.CentralDuctRadius + current.Epithelium)
                    state = CellState.Epith;
                else state = CellState.Strom;
                for (int j = x0; j < xf; j++)
                {
                    if (i == 249)
                    {

                    }
                    if (current.CentralDuct + i < xf)
                    {
                        TVertex vertex = MF.BuildTVertexID(j, current.CentralDuct + i);
                        _dictStates[vertex].CurrentState = state;
                        _dictStates[vertex].Organ = organ;
                        _dictStatesOriginal[vertex] = state;
                    }
                    if (current.CentralDuct - i >= 0)
                    {
                        TVertex vertex = MF.BuildTVertexID(j, current.CentralDuct - i);
                        _dictStates[vertex].CurrentState = state;
                        _dictStates[vertex].Organ = organ;
                        _dictStatesOriginal[vertex] = state;
                    }
                }
            }
        }
        private void InitializeScheme3(int x0, int y0, int xf, int yf, int organ)
        {
            CellState state = CellState.Strom;
            for (int i = y0; i < yf; i++)
            {
                for (int j = x0; j < xf; j++)
                {
                    TVertex vertex = MF.BuildTVertexID(j, i);
                    _dictStates[vertex].CurrentState = state;
                    _dictStates[vertex].Organ = organ;
                    _dictStatesOriginal[vertex] = state;
                }
            }
        }
        private void InitializeTumor()
        {
            List<TVertex> tumorTemplate = MF.GetRegularNeighboursTemplate(_organ1Scheme.TumorRadius);
            List<TVertex> clusterTumorCells = new List<TVertex>();
            for (int i = 0; i < tumorTemplate.Count; i++)
            {
                int[] templatePos = MF.GetTVertexID(tumorTemplate[i]);
                TVertex tumorCell = MF.BuildTVertexID(templatePos[0] + _organ1Scheme.TumorPosX, templatePos[1] + _organ1Scheme.TumorPosY);
                clusterTumorCells.Add(tumorCell);
            }
            _dictTumor.Add(_tumorIDGenerator, new List<TVertex>());
            _dictRelativeTimesData.Add(_tumorIDGenerator, 0);
            for (int i = 0; i < clusterTumorCells.Count; i++)
            {
                _dictStates[clusterTumorCells[i]].CurrentState = CellState.Tumor;
                _dictStates[clusterTumorCells[i]].CurrentTumor = _tumorIDGenerator;
                _dictStates[clusterTumorCells[i]].CantCells = GetRandomAmountCells();
                _dictTumor[_tumorIDGenerator].Add(clusterTumorCells[i]);
                List<TVertex> neigh = GetN(clusterTumorCells[i]);
                List<TVertex> dneigh = GetDN(clusterTumorCells[i], neigh);
                if (dneigh.Count > 0 && !_dictAsyncTum[0].Contains(clusterTumorCells[i]))
                    _dictAsyncTum[0].Add(clusterTumorCells[i]);
                _tumorFrontiers.Add(clusterTumorCells[i], _tumorIDGenerator);
            }
            FilterFrontierCells();
            _tumorIDGenerator++;
        }

        private void FilterFrontierCells()
        {
            List<TVertex> toremove = new List<TVertex>();
            List<TVertex> frontiersCells = _tumorFrontiers.Keys.ToList();
            for (int i = 0; i < frontiersCells.Count; i++)
            {
                List<TVertex> n = GetN(frontiersCells[i]);
                List<TVertex> nn = GetNN(frontiersCells[i], n);
                List<TVertex> nnfree = GetNNE(frontiersCells[i], nn, new List<CellState> { CellState.Lumen, CellState.Epith, CellState.Strom, CellState.Migra });
                if (nnfree.Count == 0)
                    toremove.Add(frontiersCells[i]);
            }
            for (int i = 0; i < toremove.Count; i++)
                _tumorFrontiers.Remove(toremove[i]);
            for (int i = 0; i < toremove.Count; i++)
                _dictStates[toremove[i]].CantCells = _scaleMax[_scaleMax.Length - 1];
            toremove = new List<TVertex>();
            frontiersCells = _microFrontiers.Keys.ToList();
            for (int i = 0; i < frontiersCells.Count; i++)
            {
                List<TVertex> n = GetN(frontiersCells[i]);
                List<TVertex> nn = GetNN(frontiersCells[i], n);
                List<TVertex> nnfree = GetNNE(frontiersCells[i], nn, new List<CellState> { CellState.Lumen, CellState.Epith, CellState.Strom, CellState.Migra });
                if (nnfree.Count == 0)
                    toremove.Add(frontiersCells[i]);
            }
            for (int i = 0; i < toremove.Count; i++)
                _microFrontiers.Remove(toremove[i]);
            for (int i = 0; i < toremove.Count; i++)
                _dictStates[toremove[i]].CantCells = _scaleMax[_scaleMax.Length - 1];
        }
        private List<TVertex> GetN(TVertex focalVertex)
        {
            List<TVertex> neighbours = new List<TVertex>();
            List<TEdge> adjacentedges = _smallWorldNetwork.AdjacentEdges(focalVertex);
            for (int i = 0; i < adjacentedges.Count; i++)
                neighbours.Add((adjacentedges[i].Target == focalVertex) ? adjacentedges[i].Source : adjacentedges[i].Target);
            return neighbours;
        }
        private List<TVertex> GetDN(TVertex focalVertex, List<TVertex> neighbours)
        {
            List<TVertex> distantNeighbours = new List<TVertex>();
            for (int i = 0; i < neighbours.Count; i++)
                if (MF.EuclideanDistance(focalVertex, neighbours[i]) > _smallWorldNetwork.NeighbourhoodRadius)
                    distantNeighbours.Add(neighbours[i]);
            return distantNeighbours;
        }
        private List<TVertex> GetNN(TVertex focalVertex, List<TVertex> neighbours)
        {
            List<TVertex> nearNeighbours = new List<TVertex>();
            for (int i = 0; i < neighbours.Count; i++)
                if (MF.EuclideanDistance(focalVertex, neighbours[i]) <= _smallWorldNetwork.NeighbourhoodRadius)
                    nearNeighbours.Add(neighbours[i]);
            return nearNeighbours;
        }
        private List<TVertex> GetNNE(TVertex focalVertex, List<TVertex> nearNeighbours, List<CellState> E)
        {
            int[] vertexpos = MF.GetTVertexID(focalVertex);
            List<TVertex> filteredNeighbours = new List<TVertex>();
            int focalVertexOrgan = _dictStates[focalVertex].Organ;
            for (int i = 0; i < nearNeighbours.Count; i++)
            {
                int neighbourOrgan = _dictStates[nearNeighbours[i]].Organ;
                if (focalVertexOrgan == neighbourOrgan && E.Contains(_dictStates[nearNeighbours[i]].CurrentState))
                    filteredNeighbours.Add(nearNeighbours[i]);
            }
            return filteredNeighbours;
        }
        private List<TVertex> GetDNE(TVertex focalVertex, List<TVertex> distantNeighbours, List<CellState> E)
        {
            int[] vertexpos = MF.GetTVertexID(focalVertex);
            List<TVertex> filteredNeighbours = new List<TVertex>();
            for (int i = 0; i < distantNeighbours.Count; i++)
                if (E.Contains(_dictStates[distantNeighbours[i]].CurrentState))
                    filteredNeighbours.Add(distantNeighbours[i]);
            return filteredNeighbours;
        }
        private Dictionary<int, List<TVertex>> GetCompitingTumorsIDAndCells(List<TVertex> nearNeighbours)
        {
            Dictionary<int, List<TVertex>> tumorIDsAndCells = new Dictionary<int, List<TVertex>>();
            for (int i = 0; i < nearNeighbours.Count; i++)
            {
                if (!tumorIDsAndCells.ContainsKey(_dictStates[nearNeighbours[i]].CurrentTumor))
                {
                    if (_dictStates[nearNeighbours[i]].CurrentTumor != -1)
                        tumorIDsAndCells.Add(_dictStates[nearNeighbours[i]].CurrentTumor, new List<TVertex>() { nearNeighbours[i] });
                }
                else tumorIDsAndCells[_dictStates[nearNeighbours[i]].CurrentTumor].Add(nearNeighbours[i]);
            }
            return tumorIDsAndCells;
        }

        private double GetGrowthProbAvascular(double n)
        {
            double num = _modelSettings.P0a * _modelSettings.Ka * _modelSettings.ra * Math.Pow(Math.E, _modelSettings.ra * n * _modelSettings.deltata) * (_modelSettings.Ka - _modelSettings.P0a);
            double den = Math.Pow(((_modelSettings.P0a * Math.Pow(Math.E, _modelSettings.ra * n * _modelSettings.deltata)) - _modelSettings.P0a + _modelSettings.Ka), 2);
            double result = num / den;
            return result;
        }
        private double GetGrowthProbVascular(double n)
        {
            double num = (_modelSettings.P0v * _modelSettings.Kv * _modelSettings.rv) * (Math.Pow(Math.E, _modelSettings.rv * n * _modelSettings.deltatv)) * (_modelSettings.Kv - _modelSettings.P0v);
            double den = Math.Pow(((_modelSettings.P0v * Math.Pow(Math.E, _modelSettings.rv * n * _modelSettings.deltatv)) - _modelSettings.P0v + _modelSettings.Kv), 2);
            double result = num / den;
            return result;
        }
        private double GetMigrantCellApparitionProb(int relativetime)
        {
            double estimatedPob = (_modelSettings.P0v * _modelSettings.Kv) / (_modelSettings.P0v + (_modelSettings.Kv - _modelSettings.P0v)* Math.Pow(Math.E, -1 * _modelSettings.rv * relativetime * _modelSettings.deltatv));
            double inner = estimatedPob / (_modelSettings.Kv + _parametersSettings.K_mig);
            double power = 1.0 / _parametersSettings.eta_mig;
            return Math.Pow(inner, power);
        }
        private int GetRandomAmountCells()
        {
            int random = _random.Next(_scaleMax.Length);
            return _scaleMax[random];
        }
        private double GetMigrantDeathProbability(int movements)
        {
            return Math.Pow(movements / (_parametersSettings.mu_max), 1.0 / (_parametersSettings.eta_mig_prima));
        }

        private TVertex GetMetastasisDestiny(List<TVertex> distantFilteredNeighbours)
        {
            int random = _random.Next(distantFilteredNeighbours.Count);
            return distantFilteredNeighbours[random];
        }
        private List<TVertex> GetAvailableDestinies(TVertex migrantCell, TVertex tumorCentroid, List<TVertex> nearNeighbours2)
        {
            List<TVertex> possibleMoves = new List<TVertex>();
            for (int i = 0; i < nearNeighbours2.Count; i++)
            {
                double distanceCellTumorCentroid = MF.EuclideanDistance(migrantCell, tumorCentroid);
                double distanceNeighbourCentroid = MF.EuclideanDistance(nearNeighbours2[i], tumorCentroid);
                if (distanceNeighbourCentroid > distanceCellTumorCentroid)
                    possibleMoves.Add(nearNeighbours2[i]);
            }
            return possibleMoves;
        }
        private int[] GetTumorCentroid(int ID, bool tumor)
        {
            int[] centroid = new int[2];
            List<TVertex> cells = null;
            if (tumor) cells = _dictTumor[ID];
            else cells = _dictMicro[ID];
            int count = cells.Count;
            for (int i = 0; i < count; i++)
            {
                int[] pos = MF.GetTVertexID(cells[i]);
                centroid[0] += pos[0];
                centroid[1] += pos[1];
            }
            centroid[0] = (int)Math.Round(centroid[0] / (double)count);
            centroid[1] = (int)Math.Round(centroid[1] / (double)count);
            return centroid;
        }
        private List<int[]> GetRegionNutrients(TVertex cell)
        {
            int[] cellpos = MF.GetTVertexID(cell);
            for (int i = 0; i < _nutrientsSettings.Regions.Count; i++)
            {
                int[] regionLimits = _nutrientsSettings.Regions[i];
                if ((regionLimits[0] <= cellpos[0] && cellpos[0] < regionLimits[1]) &&
                    (regionLimits[2] <= cellpos[1] && cellpos[1] < regionLimits[3]))
                    return _nutrientsSettings.Vectors[i];
            }
            throw new Exception("Vertex not contained in any region.");
        }
        private Dictionary<int, int[]> GetTumorCentroidsDictionaries(bool tumor)
        {
            Dictionary<int, int[]> result = new Dictionary<int, int[]>();
            Dictionary<int, List<TVertex>> collection = _dictTumor;
            if (tumor == false) collection = _dictMicro;
            List<KeyValuePair<int, List<TVertex>>> pairs = collection.ToList();
            for (int i = 0; i < pairs.Count; i++)
            {
                result.Add(pairs[i].Key, null);
                int[] centroid = GetTumorCentroid(pairs[i].Key, tumor);
                result[pairs[i].Key] = centroid;
            }
            return result;
        }
        private double GetVelocity(double euclideanDistance)
        {
            if (euclideanDistance == Math.Sqrt(2))
                return 0.5;
            else return 1.5;
        }
        private double GetSimAlt(double sim)
        {
            // i = 0.5, s = 1.5
            return (1 / 2) * sim + (1);
        }

        public void UpdateProcedure()
        {
            //int timeStart = Environment.TickCount;
            Console.WriteLine("   cellularautomata library: updating migratory cells in bloodstream...");
            UpdateMigratoryCellsInBloodstream();
            //int elapsedMiliseconds = Environment.TickCount - timeStart;
            //string formattedTime = Notification.TimeStamp(elapsedMiliseconds);
            //Console.WriteLine("   cellularautomata library: finished updating migratory cells in bloodstream" + formattedTime);

            //timeStart = Environment.TickCount;
            Console.WriteLine("   cellularautomata library: updating migratory cells...");
            UpdateMigratoryCells();
            //elapsedMiliseconds = Environment.TickCount - timeStart;
            //formattedTime = Notification.TimeStamp(elapsedMiliseconds);
            //Console.WriteLine("   cellularautomata library: finished migratory cells" + formattedTime);

            //timeStart = Environment.TickCount;
            Console.WriteLine("   cellularautomata library: updating tumor migratory cells...");
            UpdateTumorMigratoryCells();
            //elapsedMiliseconds = Environment.TickCount - timeStart;
            //formattedTime = Notification.TimeStamp(elapsedMiliseconds);
            //Console.WriteLine("   cellularautomata library: finished tumor migratory cells" + formattedTime);

            //timeStart = Environment.TickCount;
            //Console.WriteLine("   cellularautomata library: checking micrometastasis survival...");
            //CheckMicrometastasisSurvival();
            //elapsedMiliseconds = Environment.TickCount - timeStart;
            //formattedTime = Notification.TimeStamp(elapsedMiliseconds);
            //Console.WriteLine("   cellularautomata library: finished checking micrometastasis survival" + formattedTime);

            //timeStart = Environment.TickCount;
            //Console.WriteLine("   cellularautomata library: checking micrometastasis colonization...");
            //CheckMicrometastasisColonization();
            //elapsedMiliseconds = Environment.TickCount - timeStart;
            //formattedTime = Notification.TimeStamp(elapsedMiliseconds);
            //Console.WriteLine("   cellularautomata library: finished checking micrometastasis colonization" + formattedTime);

            //timeStart = Environment.TickCount;
            Console.WriteLine("   cellularautomata library: updating synchronous cells...");
            SetSynchronousUpdateList();
            UpdateSynchronousCellsOptimized();
            //elapsedMiliseconds = Environment.TickCount - timeStart;
            //formattedTime = Notification.TimeStamp(elapsedMiliseconds);
            //Console.WriteLine("   cellularautomata library: finished updating synchronous cells" + formattedTime);            

            //timeStart = Environment.TickCount;
            Console.WriteLine("   cellularautomata library: setting up next iteration...");
            SetupIteration();
            //elapsedMiliseconds = Environment.TickCount - timeStart;
            //formattedTime = Notification.TimeStamp(elapsedMiliseconds);
            //Console.WriteLine("   cellularautomata library: finished setting up next iteration" + formattedTime);
        }
        private void SetupIteration()
        {
            _generation++;
            List<TVertex> statesKeys = _dictStates.Keys.ToList();
            int keysCount = statesKeys.Count;
            //bool written = false;
            for (int i = 0; i < keysCount; i++)
            {
                //Notification.CompletionNotification(i, keysCount, ref written, "   ");
                _dictStates[statesKeys[i]].Updated = false;
                if (_dictStates[statesKeys[i]].FutureState == CellState.Tumor)
                {
                    // update future and current info
                    _dictStates[statesKeys[i]].CurrentState = CellState.Tumor;
                    _dictStates[statesKeys[i]].FutureState = CellState.NULL;
                    int tumorID = _dictStates[statesKeys[i]].FutureTumor;
                    _dictStates[statesKeys[i]].FutureTumor = -1;
                    _dictStates[statesKeys[i]].CurrentTumor = tumorID;
                    _dictStates[statesKeys[i]].CantCells = GetRandomAmountCells();
                    _dictTumor[tumorID].Add(statesKeys[i]);
                    // update tumor async set
                    List<TVertex> neighbours = GetN(statesKeys[i]);
                    List<TVertex> distantNeighbours = GetDN(statesKeys[i], neighbours);
                    if (distantNeighbours.Count > 0 && !_dictAsyncTum[0].Contains(statesKeys[i]))
                        _dictAsyncTum[0].Add(statesKeys[i]);
                    // update frontiers
                    _tumorFrontiers.Add(statesKeys[i], tumorID);
                }
                //else if (_dictStates[statesKeys[i]].FutureState == CellState.Micro)
                //{
                //    _dictStates[statesKeys[i]].CurrentState = CellState.Micro;
                //    _dictStates[statesKeys[i]].FutureState = CellState.NULL;
                //    int microID = _dictStates[statesKeys[i]].FutureTumor;
                //    _dictStates[statesKeys[i]].FutureTumor = -1;
                //    _dictStates[statesKeys[i]].CurrentTumor = microID;
                //    _dictStates[statesKeys[i]].CantCells = GetRandomAmountCells();
                //    _dictMicro[microID].Add(statesKeys[i]);
                //    // update frontiers
                //    _microFrontiers.Add(statesKeys[i], microID);
                //}
                else if (_dictStates[statesKeys[i]].FutureState == CellState.Migra)
                {
                    _dictStates[statesKeys[i]].CurrentState = CellState.Migra;
                    _dictStates[statesKeys[i]].FutureState = CellState.NULL;
                    var tumorID = _dictStates[statesKeys[i]].FutureTumor;
                    _dictStates[statesKeys[i]].FutureTumor = -1;
                    _dictStates[statesKeys[i]].CurrentTumor = tumorID;
                    _dictStates[statesKeys[i]].CantCells = GetRandomAmountCells();
                    _dictAsyncMig[0].Add(statesKeys[i], 0);
                }
            }
            //Notification.FinalCompletionNotification("   ");
            FilterFrontierCells();
            List<TVertex> keys = _dictAsyncMig[1].Keys.ToList();
            int count = keys.Count;
            for (int i = 0; i < count; i++)
                _dictAsyncMig[0].Add(keys[i], _dictAsyncMig[1][keys[i]]);
            _dictAsyncMig[1].Clear();
            for (int i = 0; i < _dictAsyncTum[1].Count; i++)
                _dictAsyncTum[0].Add(_dictAsyncTum[1][i]);
            _dictAsyncTum[1].Clear();
        }
        private void CheckMicrometastasisColonization()
        {
            List<int> micrometastasisToUpdate = new List<int>();
            List<int> micrometastasisIDs = _dictMicro.Keys.ToList();
            _dictSuccessfulMicrometastasis[0].Add(_generation, _dictSuccessfulMicrometastasis[0][_generation - 1]);
            _dictSuccessfulMicrometastasis[1].Add(_generation, _dictSuccessfulMicrometastasis[1][_generation - 1]);
            //bool written = false;
            int count = micrometastasisIDs.Count;
            for (int i = 0; i < count; i++)
            {
                //Notification.CompletionNotification(i, count, ref written, "   ");
                double prob = _random.NextDouble();
                int spawntime = _dictRelativeTimesData[micrometastasisIDs[i]];
                TVertex cell = _dictMicro[micrometastasisIDs[i]][0];
                int destinyOrgan = _dictStates[cell].Organ;
                double psi = (destinyOrgan == 0) ? _parametersSettings.psi_mic0 : _parametersSettings.psi_mic1;
                if (_generation - spawntime >= _modelSettings.n_a && prob <= psi)
                {
                    // satisfactory colonization
                    _dictTumor.Add(micrometastasisIDs[i], new List<TVertex>());
                    List<TVertex> cellsInMicrometastasis = _dictMicro[micrometastasisIDs[i]];
                    for (int j = 0; j < cellsInMicrometastasis.Count; j++)
                    {
                        _dictStates[cellsInMicrometastasis[j]].CurrentState = CellState.Tumor;
                        _dictTumor[micrometastasisIDs[i]].Add(cellsInMicrometastasis[j]);
                        _tumorFrontiers.Add(cellsInMicrometastasis[j], micrometastasisIDs[i]);
                        _microFrontiers.Remove(cellsInMicrometastasis[j]);
                    }
                    micrometastasisToUpdate.Add(micrometastasisIDs[i]);
                    _dictSuccessfulMicrometastasis[destinyOrgan][_generation]++;
                }
            }
            for (int i = 0; i < micrometastasisToUpdate.Count; i++)
                _dictMicro.Remove(micrometastasisToUpdate[i]);
            //Notification.FinalCompletionNotification("   ");
        }
        private void SetSynchronousUpdateList()
        {
            _synchronousCellsToUpdate = new Dictionary<TVertex, bool>();
            List<TVertex> frontiersCells = _tumorFrontiers.Keys.ToList();
            for (int j = 0; j < frontiersCells.Count; j++)
            {
                TVertex cell = frontiersCells[j];
                List<TVertex> neighbours = GetN(cell);
                List<TVertex> nearNeighbours = GetNN(cell, neighbours);
                List<TVertex> normalNearNeighbours = GetNNE(cell, nearNeighbours, new List<CellState> { CellState.Epith, CellState.Lumen, CellState.Strom });
                if (normalNearNeighbours.Count > 0)
                {
                    for (int k = 0; k < normalNearNeighbours.Count; k++)
                    {
                        TVertex normalCellKey = normalNearNeighbours[k];
                        if (!_synchronousCellsToUpdate.ContainsKey(normalCellKey))
                            _synchronousCellsToUpdate.Add(normalCellKey, true);
                    }
                }
            }
            frontiersCells = _microFrontiers.Keys.ToList();
            for (int j = 0; j < frontiersCells.Count; j++)
            {
                TVertex cell = frontiersCells[j];
                List<TVertex> neighbours = GetN(cell);
                List<TVertex> nearNeighbours = GetNN(cell, neighbours);
                List<TVertex> normalNearNeighbours = GetNNE(cell, nearNeighbours, new List<CellState> { CellState.Epith, CellState.Lumen, CellState.Strom });
                if (normalNearNeighbours.Count > 0)
                {
                    for (int k = 0; k < normalNearNeighbours.Count; k++)
                    {
                        TVertex normalCellKey = normalNearNeighbours[k];
                        if (!_synchronousCellsToUpdate.ContainsKey(normalCellKey))
                            _synchronousCellsToUpdate.Add(normalCellKey, false);
                    }
                }
            }
        }
        private void UpdateSynchronousCellsOptimized()
        {
            List<KeyValuePair<TVertex, bool>> cellsToUpdatePairs = _synchronousCellsToUpdate.ToList();
            Dictionary<int, int[]> tumorCentroidsDict = GetTumorCentroidsDictionaries(true);
            Dictionary<int, int[]> microCentroidsDict = GetTumorCentroidsDictionaries(false);
            int cellsCount = cellsToUpdatePairs.Count;
            _amountCellsGeneratedMigration = 0;
            //bool written = false;
            for (int i = 0; i < cellsCount; i++)
            {
                KeyValuePair<TVertex, bool> pair = cellsToUpdatePairs[i];
                TVertex cellKey = pair.Key;
                bool cellValue = pair.Value;
                //Notification.CompletionNotification(i, cellsCount, ref written, "   ");
                CellData data = _dictStates[cellKey];
                if (_dictStates[cellKey].Updated == true)
                    throw new Exception("Updated cell made it to the synchronous update list.");
                _dictStates[cellKey].Updated = true;
                List<TVertex> neighbours = GetN(cellKey);
                List<TVertex> nearNeighbours = GetNN(cellKey, neighbours);
                List<TVertex> nearNeighbours3 = GetNNE(cellKey, nearNeighbours, new List<CellState> { CellState.Tumor });
                if (nearNeighbours3.Count > 0)
                {
                    // tumor growth
                    Dictionary<int, List<TVertex>> tumorCompitingIDsAndCells = GetCompitingTumorsIDAndCells(nearNeighbours3);
                    List<int> tumorCompitingIDs = tumorCompitingIDsAndCells.Keys.ToList();
                    List<int[]> nutrientsVectorsRegion = GetRegionNutrients(cellKey);
                    List<double> individualTumoralCellApparitionProbs = new List<double>();
                    // all expanding tumors growth probabilities
                    for (int j = 0; j < tumorCompitingIDs.Count; j++)
                    {
                        // get beta
                        int[] tumorCentroid = tumorCentroidsDict[tumorCompitingIDs[j]];
                        int[] cellPos = MF.GetTVertexID(cellKey);
                        int[] expansionVector = MF.BuildVector(cellPos, tumorCentroid);
                        List<double> simValues = new List<double>();
                        if (nutrientsVectorsRegion.Count != 0)
                        {
                            for (int k = 0; k < nutrientsVectorsRegion.Count; k++)
                            {
                                double sim = MF.GetSim(expansionVector, nutrientsVectorsRegion[k]);
                                double simAlt = GetSimAlt(sim);// Math.Cos(Math.Acos(sim) / 3);
                                simValues.Add(simAlt);
                            }
                        }
                        else simValues.Add(1);
                        int maxSimIndex = MF.GetMaxValueIndex(simValues);
                        double beta = simValues[maxSimIndex];
                        // get velocity
                        List<TVertex> neighbouringCells = tumorCompitingIDsAndCells[tumorCompitingIDs[j]];
                        double velocity = GetVelocity(MF.EuclideanDistance(neighbouringCells[0], cellKey));
                        for (int k = 1; k < neighbouringCells.Count; k++)
                        {
                            double newVelocity = GetVelocity(MF.EuclideanDistance(neighbouringCells[k], cellKey));
                            if (newVelocity > velocity)
                            {
                                velocity = newVelocity;
                            }
                        }
                        // get full probability
                        int spawntime = _dictRelativeTimesData[tumorCompitingIDs[j]];
                        bool mode = true;
                        if (_generation - spawntime > _modelSettings.n_a)
                            mode = false;
                        if (mode)
                        {
                            CellState healthyCellState = _dictStates[cellKey].CurrentState;
                            if (healthyCellState == CellState.Strom)
                                individualTumoralCellApparitionProbs.Add(0);
                            else
                            {
                                double avascularProb = GetGrowthProbAvascular(_generation - spawntime);
                                double prob = avascularProb * beta * velocity;
                                individualTumoralCellApparitionProbs.Add(prob);
                            }
                        }
                        else
                        {
                            double vascularProb = GetGrowthProbVascular(_generation - (spawntime + _modelSettings.n_a));
                            double prob = vascularProb * beta * velocity;
                            individualTumoralCellApparitionProbs.Add(prob);
                        }
                    }
                    int maxProbIndexGrowth = MF.GetMaxValueIndex(individualTumoralCellApparitionProbs);
                    int expandingTumor = tumorCompitingIDs[maxProbIndexGrowth];
                    double rho3 = individualTumoralCellApparitionProbs[maxProbIndexGrowth];
                    if (_dictStates[cellKey].CurrentState == CellState.Strom)
                    {
                        // checking migrant cell apparition probability
                        List<double> migrantApparitionProbs = new List<double>();
                        for (int j = 0; j < tumorCompitingIDs.Count; j++)
                        {
                            List<TVertex> tumorCurrentCells = _dictTumor[tumorCompitingIDs[j]];
                            int spawntime = _dictRelativeTimesData[tumorCompitingIDs[j]];
                            bool mode = true;
                            if (_generation - spawntime > _modelSettings.n_a)
                                mode = false;
                            if (mode)
                                migrantApparitionProbs.Add(0);
                            else
                            {
                                double poblation = _modelSettings.Kv;
                                if (tumorCurrentCells.Count < poblation)
                                    poblation = tumorCurrentCells.Count;
                                double prob = GetMigrantCellApparitionProb(_generation - spawntime);
                                migrantApparitionProbs.Add(prob);
                            }
                        }
                        int maxProbIndexApparition = MF.GetMaxValueIndex(migrantApparitionProbs);
                        int tumorSpawningMigrantCell = tumorCompitingIDs[maxProbIndexApparition];
                        double rho4 = migrantApparitionProbs[maxProbIndexApparition];
                        double x3 = _random.NextDouble();
                        // deciding if the tumor grows or throw a new migrant cell
                        if (x3 <= rho3)
                        {
                            _dictStates[cellKey].FutureState = CellState.Tumor;
                            _dictStates[cellKey].FutureTumor = expandingTumor;
                        }
                        else
                        {
                            double x4 = _random.NextDouble();
                            if (x4 <= rho4)
                            {
                                _dictStates[cellKey].FutureState = CellState.Migra;
                                _dictStates[cellKey].FutureTumor = tumorSpawningMigrantCell;
                                _amountCellsGeneratedMigration++;
                                _dictWanderedDistance.Add(cellKey, 0);
                            }
                        }
                    }
                    else
                    {
                        // applying tumor growth rule, updating states
                        double Xtumor = _random.NextDouble();
                        if (Xtumor <= rho3)
                        {
                            _dictStates[cellKey].FutureState = CellState.Tumor;
                            _dictStates[cellKey].FutureTumor = expandingTumor;
                        }
                    }
                }
                else
                {
                    List<TVertex> nearNeighbours5 = GetNNE(cellKey, nearNeighbours, new List<CellState> { CellState.Micro });
                    if (nearNeighbours5.Count > 0)
                    {
                        // micrometastasis growth
                        Dictionary<int, List<TVertex>> microCompitingIDsAndCells = GetCompitingTumorsIDAndCells(nearNeighbours5);
                        List<int> microCompitingIDs = microCompitingIDsAndCells.Keys.ToList();
                        List<int[]> nutrientsVectorsRegion5 = GetRegionNutrients(cellKey);
                        List<double> individualMicroCellApparitionProbs = new List<double>();
                        for (int j = 0; j < microCompitingIDs.Count; j++)
                        {
                            // get beta
                            int[] microCentroid = microCentroidsDict[microCompitingIDs[j]];
                            int[] cellPos = MF.GetTVertexID(cellKey);
                            int[] expansionVector = MF.BuildVector(cellPos, microCentroid);
                            List<double> simValues = new List<double>();
                            if (nutrientsVectorsRegion5.Count != 0)
                            {
                                for (int k = 0; k < nutrientsVectorsRegion5.Count; k++)
                                {
                                    double sim = MF.GetSim(expansionVector, nutrientsVectorsRegion5[k]);
                                    double simAlt = GetSimAlt(sim); // Math.Cos(Math.Acos(sim) / 3);
                                    simValues.Add(simAlt);
                                }
                            }
                            else simValues.Add(1);
                            int maxSimIndex = MF.GetMaxValueIndex(simValues);
                            double beta = simValues[maxSimIndex];
                            //get velocity
                            List<TVertex> neighbouringCells = microCompitingIDsAndCells[microCompitingIDs[j]];
                            double velocity = GetVelocity(MF.EuclideanDistance(neighbouringCells[0], cellKey));
                            for (int k = 1; k < neighbouringCells.Count; k++)
                            {
                                double newVelocity = GetVelocity(MF.EuclideanDistance(neighbouringCells[k], cellKey));
                                if (newVelocity > velocity)
                                {
                                    velocity = newVelocity;
                                }
                            }
                            // get full probability
                            int relativetime = _dictRelativeTimesData[microCompitingIDs[j]];
                            bool mode = true;
                            if (_generation - relativetime > _modelSettings.n_a)
                                mode = false;
                            if (mode)
                            {
                                double avascularProb = GetGrowthProbAvascular(_generation - relativetime);
                                double prob = avascularProb * beta * velocity;
                                individualMicroCellApparitionProbs.Add(prob);
                            }
                            else individualMicroCellApparitionProbs.Add(0);
                        }
                        int maxProbIndexGrowth5 = MF.GetMaxValueIndex(individualMicroCellApparitionProbs);
                        int expandingMicro = microCompitingIDs[maxProbIndexGrowth5];
                        double rho5 = individualMicroCellApparitionProbs[maxProbIndexGrowth5];
                        double X = _random.NextDouble();
                        // applying rule, updating states
                        //if (X <= rho5)
                        //{
                        //    _dictStates[cellKey].FutureState = CellState.Micro;
                        //    _dictStates[cellKey].FutureTumor = expandingMicro;
                        //}
                    }
                }
            }
            //Notification.FinalCompletionNotification("   ");
        }
        private void CheckMicrometastasisSurvival()
        {
            List<int> micrometastasisToRemove = new List<int>();
            List<int> micrometastasisIDs = _dictMicro.Keys.ToList();
            _dictFailedMicrometastasis[0].Add(_generation, _dictFailedMicrometastasis[0][_generation - 1]);
            _dictFailedMicrometastasis[1].Add(_generation, _dictFailedMicrometastasis[1][_generation - 1]);
            //bool written = false;
            int count = micrometastasisIDs.Count;
            for (int i = 0; i < count; i++)
            {
                //Notification.CompletionNotification(i, count, ref written, "   ");
                double prob = _random.NextDouble();
                //int spawntime = _dictRelativeTimesData[micrometastasisIDs[i]];
                TVertex cell = _dictMicro[micrometastasisIDs[i]][0];
                int destinyOrgan = _dictStates[cell].Organ;
                double xi = (destinyOrgan == 0) ? _parametersSettings.xi_mic0 : _parametersSettings.xi_mic1;
                if (prob <= 1 - xi) //_generation - spawntime < _modelSettings.n_a && 
                {
                    // micrometastasis death
                    List<TVertex> cellsInMicrometastasis = _dictMicro[micrometastasisIDs[i]];
                    _dictRelativeTimesData.Remove(micrometastasisIDs[i]);
                    for (int j = 0; j < cellsInMicrometastasis.Count; j++)
                    {
                        _dictStates[cellsInMicrometastasis[j]].CurrentState = _dictStatesOriginal[cellsInMicrometastasis[j]];
                        _dictStates[cellsInMicrometastasis[j]].CurrentTumor = -1;
                        _microFrontiers.Remove(cellsInMicrometastasis[j]);
                    }
                    micrometastasisToRemove.Add(micrometastasisIDs[i]);
                    _dictFailedMicrometastasis[destinyOrgan][_generation]++;
                }
            }
            for (int i = 0; i < micrometastasisToRemove.Count; i++)
                _dictMicro.Remove(micrometastasisToRemove[i]);
            //Notification.FinalCompletionNotification("   ");
        }
        private void UpdateTumorMigratoryCells()
        {
            int count = 0;
            //bool written = false;
            int total = _dictAsyncTum[0].Count;
            while (_dictAsyncTum[0].Count != 0)
            {
                //Notification.CompletionNotification(count, total, ref written, "   ");
                count++;
                int index = _random.Next(_dictAsyncTum[0].Count);
                TVertex cell = _dictAsyncTum[0][index];
                _dictAsyncTum[0].Remove(cell);
                _dictAsyncTum[1].Add(cell);
                int tumorID = _dictStates[cell].CurrentTumor;
                List<TVertex> tumorCells = _dictTumor[tumorID];
                double prob = 0.0;
                int spawntime = _dictRelativeTimesData[tumorID];
                bool mode = false;
                if (_generation - spawntime > _modelSettings.n_a)
                    mode = true;
                if (mode)
                    prob = GetMigrantCellApparitionProb(_generation - spawntime);
                double X = _random.NextDouble();
                if (X <= prob)
                {
                    List<TVertex> neighbours = GetN(cell);
                    List<TVertex> distantNeighbours = GetDN(cell, neighbours);
                    List<TVertex> filteredDistantNeighbours = GetDNE(cell, distantNeighbours, new List<CellState> { CellState.Strom, CellState.Tumor, CellState.Micro });
                    if (filteredDistantNeighbours.Count > 0)
                    {
                        TVertex w = GetMetastasisDestiny(filteredDistantNeighbours);
                        int cantCellsInCell = GetRandomAmountCells();
                        for (int i = 0; i < cantCellsInCell; i++)
                        {
                            _dictAsyncCS.Add(_cellInBloodstreamIDGenerator, w);
                            _cellInBloodstreamIDGenerator++;
                            _amountCellsInBloodStreamFromTumors++;
                        }
                    }
                }
            }
            //Notification.FinalCompletionNotification("   ");
        }
        private void UpdateMigratoryCells()
        {
            int tentativeMovements = 0;
            Dictionary<int, int[]> tumorCentroidsDict = GetTumorCentroidsDictionaries(true);
            while (tentativeMovements < _parametersSettings.mu_mig)
            {
                Console.WriteLine("   cellularautomata library: tentative movement " + tentativeMovements);
                int count = 0;
                //bool written = false;
                tentativeMovements++;
                while (_dictAsyncMig[0].Count != 0)
                {
                    //Notification.CompletionNotification(count, total, ref written, "   ");
                    count++;
                    int index = _random.Next(_dictAsyncMig[0].Keys.Count);
                    TVertex cell = _dictAsyncMig[0].Keys.ToList()[index];
                    int movements = _dictAsyncMig[0][cell];
                    _dictAsyncMig[0].Remove(cell);
                    int wanderedDistance = _dictWanderedDistance[cell];
                    _dictWanderedDistance.Remove(cell);
                    int cantCellsInCell = _dictStates[cell].CantCells;
                    List<TVertex> neighbours = GetN(cell);
                    List<TVertex> distantNeighbours = GetDN(cell, neighbours);
                    List<TVertex> filteredDistantNeighbours = GetDNE(cell, distantNeighbours, new List<CellState> { CellState.Strom, CellState.Tumor, CellState.Micro });
                    if (filteredDistantNeighbours.Count > 0)
                    {
                        // metastasis
                        _dictStates[cell].CurrentState = CellState.Strom;
                        _dictStates[cell].CurrentTumor = -1;
                        TVertex w = GetMetastasisDestiny(filteredDistantNeighbours);
                        _dictWanderedDistanceMeasurementsMetas[wanderedDistance]++;
                        for (int i = 0; i < cantCellsInCell; i++)
                        {
                            _dictAsyncCS.Add(_cellInBloodstreamIDGenerator, w);
                            _cellInBloodstreamIDGenerator++;
                            _amountCellsInBloodStreamFromMigra++;
                        }
                    }
                    else
                    {
                        // cell choose to move
                        List<TVertex> nearNeighbours = GetNN(cell, neighbours);
                        List<TVertex> nearNeighbours2 = GetNNE(cell, nearNeighbours, new List<CellState> { CellState.Strom });
                        int tumorID = _dictStates[cell].CurrentTumor;
                        if (nearNeighbours2.Count > 0)
                        {
                            // cell can move
                            _dictStates[cell].CurrentState = CellState.Strom;
                            _dictStates[cell].CurrentTumor = -1;
                            int[] tumorCentroid = tumorCentroidsDict[tumorID];
                            List<TVertex> availableDestinies = GetAvailableDestinies(cell, MF.BuildTVertexID(tumorCentroid), nearNeighbours2);
                            TVertex w = cell;
                            if (availableDestinies.Count != 0)
                            {
                                // possible destinies
                                int[] cellPos = MF.GetTVertexID(cell);
                                List<double> probs = new List<double>();
                                for (int i = 0; i < availableDestinies.Count; i++)
                                {
                                    List<int[]> nutrientsVectorsRegion = GetRegionNutrients(availableDestinies[i]);
                                    int[] destinyPos = MF.GetTVertexID(availableDestinies[i]);
                                    int[] migrationVector = MF.BuildVector(destinyPos, cellPos);
                                    List<double> simValues = new List<double>();
                                    if (nutrientsVectorsRegion.Count != 0)
                                    {
                                        for (int j = 0; j < nutrientsVectorsRegion.Count; j++)
                                        {
                                            double sim = MF.GetSim(migrationVector, nutrientsVectorsRegion[j]);
                                            double simAlt = GetSimAlt(sim); //Math.Cos(Math.Acos(sim) / 3);
                                            simValues.Add(simAlt);
                                        }
                                    }
                                    else simValues.Add(1);
                                    int maxSimIndex = MF.GetMaxValueIndex(simValues);
                                    double beta = simValues[maxSimIndex];
                                    double prob = 1 / (double)availableDestinies.Count * beta;
                                    probs.Add(prob);
                                }
                                // choosing one possible destiny
                                double random = _random.NextDouble();
                                List<double> normalizedProbs = MF.GetNormalizedProbabilities(probs);
                                List<double> normalizedAddedProbs = MF.GetNormalizedAddedProbs(normalizedProbs);
                                int availableMovementIndex = 0;
                                for (int i = 1; i < normalizedAddedProbs.Count; i++)
                                {
                                    if (normalizedAddedProbs[i - 1] <= random && random < normalizedAddedProbs[i])
                                    {
                                        availableMovementIndex = i - 1;
                                        break;
                                    }
                                }
                                w = availableDestinies[availableMovementIndex];
                            }
                            // applying movement rule, updating states
                            double rho = GetMigrantDeathProbability(movements);
                            double X = _random.NextDouble();
                            if (X <= 1 - rho)
                            {
                                // cell survived 
                                _dictStates[w].CurrentState = CellState.Migra;
                                _dictStates[w].CantCells = cantCellsInCell;
                                _dictStates[w].CurrentTumor = tumorID;
                                _dictAsyncMig[1].Add(w, movements + 1);
                                _dictWanderedDistance.Add(w, wanderedDistance + 1);
                            }
                            else
                            {
                                _dictStates[cell].CurrentState = CellState.Strom;
                                _dictWanderedDistanceMeasurementsDeath[wanderedDistance]++;
                            }
                        }
                        else
                        {
                            // cell cannot move
                            double rho = GetMigrantDeathProbability(movements);
                            double X = _random.NextDouble();
                            if (X <= 1 - rho)
                            {
                                // cell survived 
                                _dictStates[cell].CurrentState = CellState.Migra;
                                _dictStates[cell].CantCells = cantCellsInCell;
                                _dictStates[cell].CurrentTumor = tumorID;
                                //_dictAsyncMig[1].Add(cell, movements);
                                _dictAsyncMig[1].Add(cell, movements + 1);
                                _dictWanderedDistance.Add(cell, wanderedDistance);
                            }
                            else
                            {
                                _dictStates[cell].CurrentState = CellState.Strom;
                                _dictWanderedDistanceMeasurementsDeath[wanderedDistance]++;
                            }
                        }
                    }
                }
                //Notification.FinalCompletionNotification("   ");
            }
        }
        private void UpdateMigratoryCellsInBloodstream()
        {
            int count = 0;
            //bool written = false;
            int total = _dictAsyncCS.Count;
            while (_dictAsyncCS.Count != 0)
            {
                //Notification.CompletionNotification(count, total, ref written, "   ");
                count++;
                int index = _random.Next(_dictAsyncCS.Keys.Count);
                int cellID = _dictAsyncCS.Keys.ToList()[index];
                TVertex destiny = _dictAsyncCS[cellID];
                _dictAsyncCS.Remove(cellID);
                int destinyOrgan = _dictStates[destiny].Organ;
                double random0 = _random.NextDouble();
                double random1 = _random.NextDouble();
                //if (random0 <= _parametersSettings.xi_sc && _dictStates[destiny].CurrentState == CellState.Strom)
                //{
                //    _dictStates[destiny].CurrentState = CellState.Micro;
                //    _dictStates[destiny].CurrentTumor = _tumorIDGenerator;
                //    _dictStates[destiny].CantCells = GetRandomAmountCells();
                //    _dictMicro.Add(_tumorIDGenerator, new List<TVertex> { destiny });
                //    _dictRelativeTimesData.Add(_tumorIDGenerator, _generation);
                //    _microFrontiers.Add(destiny, _tumorIDGenerator);
                //    _tumorIDGenerator++;
                //}
            }
            //Notification.FinalCompletionNotification("   ");
            _cellInBloodstreamIDGenerator = 0;
            _amountCellsInBloodStreamFromMigra = 0;
            _amountCellsInBloodStreamFromTumors = 0;
        }
    }
}
