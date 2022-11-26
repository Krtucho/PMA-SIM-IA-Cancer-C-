using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVertex = System.String;

namespace AutomataLibrary2D
{
    public enum CellState { NULL, Lumen, Epith, Strom, Tumor, Migra, Micro }
    public enum SelectedOrgan { Primary, Secondary }
    public enum SelectedScheme { Scheme1, Scheme2, Scheme3 }
    public enum SelectedZoom { Normal, Increased }
    public enum HeavisideMode { Population, Generation }
    public enum NeighboursAmountInfluence { No, Yes }
    public enum ExecutionMode { Normal, Optimized }
    public enum PaintMode { Rectangle, Ellipse }

    public class CellData
    {
        public bool Updated { get; set; }
        public CellState CurrentState { get; set; }
        public CellState FutureState { get; set; }
        public int CurrentTumor { get; set; }
        public int FutureTumor { get; set; }
        public int Organ { get; set; }
        public int CantCells { get; set; }

        public CellData(bool upd, CellState current, CellState future, int currentt, int futuret, int organ, int cantCells)
        {
            Updated = upd;
            CurrentState = current;
            FutureState = future;
            CurrentTumor = currentt;
            FutureTumor = futuret;
            Organ = organ;
            CantCells = cantCells;
        }
    }

    public class NetworkPropertiesData
    {
        public double AveragePathLength { get; set; }
        public double ClusteringCoefficient { get; set; }

        public NetworkPropertiesData()
        {
            AveragePathLength = 0;
            ClusteringCoefficient = 0;
        }
    }

    public class ModelSettings
    {
        public double ra { get; private set; }
        public double rv { get; private set; }
        public double Ka { get; private set; }
        public double Kv { get; private set; }
        public double P0a { get; private set; }
        public double P0v { get; private set; }
        public double deltata { get; private set; }
        public double deltatv { get; private set; }
        public double n_a { get; set; }

        public ModelSettings(double ka, double kv, double p0a, double p0v, double rain, double rvin, 
            double delta, double deltav, double n_a_in)
        {
            ra = rain;
            rv = rvin;
            Ka = ka;
            Kv = kv;
            P0a = p0a;
            P0v = p0v;
            deltata = delta;
            deltatv = deltav;
            n_a = n_a_in;
        }
    }

    public class NetworkSettings
    {
        public int NetworkSizeX { get; private set; }
        public int NetworkSizeY { get; private set; }
        public int NetworkDivision { get; set; }
        public double ReconnectionProbability { get; private set; }
        public double NeighbourhoodRadius { get; private set; }
        public bool IsNetworkTest { get; private set; }
        public bool HasPeriodicEdges { get; private set; }

        public NetworkSettings(int sizex, int sizey, int div, double p, double r, bool test, bool periodic)
        {
            NetworkSizeX = sizex;
            NetworkSizeY = sizey;
            NetworkDivision = div;
            ReconnectionProbability = p;
            NeighbourhoodRadius = r;
            IsNetworkTest = test;
            HasPeriodicEdges = periodic;
        }
    }

    public abstract class OrganScheme
    {
        public SelectedScheme SelectedScheme { get; set; }
        public int TumorPosX { get; set; }
        public int TumorPosY { get; set; }
        public double TumorRadius { get; set; }
    }

    public class OrganScheme1 : OrganScheme
    {
        public int Lumen { get; set; } // Scheme 1
        public int Epithelium { get; set; }  // Scheme 1
        public int Stroma { get; set; } // Scheme 1

        public OrganScheme1(int lumen, int epith, int stroma, int tumorx, int tumory, double tumorradius)
        {
            SelectedScheme = SelectedScheme.Scheme1;
            Lumen = lumen;
            Epithelium = epith;
            Stroma = stroma;
            TumorPosX = tumorx;
            TumorPosY = tumory;
            TumorRadius = tumorradius;
        }
    }

    public class OrganScheme2 : OrganScheme
    {
        public int Epithelium { get; private set; }  // Scheme 2
        public int CentralDuct { get; set; } // Scheme 2
        public int CentralDuctRadius { get; set; } // Scheme 2

        public OrganScheme2(int epith, int centralDuct, int centralDuctRadius, int tumorx, int tumory, double tumorradius)
        {
            SelectedScheme = SelectedScheme.Scheme2;
            Epithelium = epith;
            CentralDuct = centralDuct;
            CentralDuctRadius = centralDuctRadius;
            TumorPosX = tumorx;
            TumorPosY = tumory;
            TumorRadius = tumorradius;
        }
    }

    public class OrganScheme3 : OrganScheme
    {
        public OrganScheme3()
        {
            SelectedScheme = SelectedScheme.Scheme3;
        }      
    }

    public class NutrientsSettings
    {
        public List<int[]> Regions { get; private set; }

        public List<List<int[]>> Vectors { get; private set; }

        public NutrientsSettings(List<int[]> regions, List<List<int[]>> vectors)
        {
            Regions = regions;
            Vectors = vectors;
        }
    }

    public class ParametersSettings
    {
        public double mu_mig { get; private set; }
        public double eta_mig { get; private set; }
        public double eta_mig_prima { get; private set; }
        public double mu_max { get; private set; }
        public double xi_sc { get; private set; }
        public double xi_mic0 { get; private set; }
        public double psi_mic0 { get; private set; }
        public double xi_mic1 { get; private set; }
        public double psi_mic1 { get; private set; }
        public double K_mig { get; private set; }
        public int SimScale { get; set; }

        public ParametersSettings(double mu_mig_in, double eta_mig_in,
            double eta_mig_prima_in, double mu_max_in, double xi_sc_in, 
            double xi_mic_in0, double psi_mic_in0, double xi_mic_in1, 
            double psi_mic_in1, double K_mig_in, int simScale)
        {
            mu_mig = mu_mig_in;
            eta_mig = eta_mig_in;
            eta_mig_prima = eta_mig_prima_in;
            mu_max = mu_max_in;
            xi_sc = xi_sc_in;
            xi_mic0 = xi_mic_in0;
            psi_mic0 = psi_mic_in0;
            xi_mic1 = xi_mic_in1;
            psi_mic1 = psi_mic_in1;
            K_mig = K_mig_in;
            SimScale = simScale;
        }
    }
}