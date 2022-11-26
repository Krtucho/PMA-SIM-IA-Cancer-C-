using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using AutomataLibrary2D;
using QuickGraph;
using TEdge = QuickGraph.UndirectedEdge<string>;
using TVertex = System.String;
using TEdgeString = System.String;

namespace ExperimentSnapper
{
    public partial class ExperimentSnapper : Form
    {
        private bool _loaded = false;
        private Dictionary<TVertex, CellState> _dictStates = null;
        private Dictionary<int, List<TVertex>> _dictTumor = null;
        private Dictionary<int, List<TVertex>> _dictMicro = null;
        private Dictionary<int, List<TVertex>> _dictMigra = null;
        private string _experimentID = string.Empty;
        private float _paintScale = 4;
        private int _division = -1;
        private int _maxX = -1;
        private int _maxY = -1;
        private string _openedOriginalStatesFiles = null;
        private List<KeyValuePair<int, string>> _openedGenerationsFiles = null;

        private SelectedOrgan SelectedOrgan
        {
            get
            {
                if (radioButton4.Checked == true)
                    return SelectedOrgan.Primary;
                else if (radioButton5.Checked == true)
                    return SelectedOrgan.Secondary;
                else throw new Exception("Radio buttons are not working correctly.");
            }
        }
        private SelectedZoom SelectedZoom
        {
            get
            {
                if (radioButton1.Checked == true)
                    return SelectedZoom.Normal;
                else if (radioButton2.Checked == true)
                    return SelectedZoom.Increased;
                else throw new Exception("Radio buttons are not working correctly.");
            }
        }
        private PaintMode PaintMode
        {
            get
            {
                if (radioButton3.Checked == true)
                    return PaintMode.Rectangle;
                else if (radioButton6.Checked == true)
                    return PaintMode.Ellipse;
                else throw new Exception("Radio buttons are not working correctly.");
            }
        }
        private KeyValuePair<int, string> SelectedGeneration { get; set; }

        public ExperimentSnapper()
        {
            InitializeComponent();
            SetGroupVisibility(false);
            ResetForm();
            UpdateLabels();
        }
        private void UpdateLabels()
        {
            label1.Text = "Network Size X -> " + _maxX;
            label2.Text = "Network Size Y -> " + _maxY;
            label3.Text = "Network Division -> " + _division;
            string localization = (SelectedOrgan == SelectedOrgan.Primary) ? "Primaria" : "Secundaria";
            label6.Text = "Localización " + localization;
            int generation = SelectedGeneration.Key;
            string realtime = (generation * 3).ToString();
            label7.Text = "Generación " + generation.ToString() + " (" + realtime + " días)";
        }
        private void ResetForm()
        {
            radioButton4.Checked = true;
            radioButton5.Checked = false;
            radioButton1.Checked = true;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton6.Checked = true;
            textBox1.Text = "0,0";
            textBox2.Text = "3";
        }
        private void SetGroupVisibility(bool visibility)
        {
            groupBox1.Visible = visibility;
            groupBox2.Visible = visibility;
            groupBox3.Visible = visibility;
            groupBox7.Visible = visibility;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = "Author: Darien Viera Barredo\nContact Emails: \n\td.viera@nauta.cu\n\tdarienviera7@gmail.com";
            MessageBox.Show(text, "About the author", MessageBoxButtons.OK);
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _loaded = false;
            _dictStates = null;
            _dictTumor = null;
            _dictMicro = null;
            _dictMigra = null;
            _experimentID = string.Empty;
            _division = -1;
            _maxX = -1;
            _maxY = -1;
            _openedOriginalStatesFiles = null;
            _openedGenerationsFiles = null;
            listBox1.Items.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _openedOriginalStatesFiles = string.Empty;
                _openedGenerationsFiles = new List<KeyValuePair<int, string>>();
                bool assignedOriginalStatesFile = false;
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    string extension = Path.GetExtension(openFileDialog.FileNames[i]);                    
                    if (assignedOriginalStatesFile == false)
                    {
                        if (extension == ".originals")
                        {
                            _openedOriginalStatesFiles = openFileDialog.FileNames[i];
                            assignedOriginalStatesFile = true;
                        }
                    }
                    if (extension == ".generation")
                    {
                        string filename = Path.GetFileNameWithoutExtension(openFileDialog.FileNames[i]);
                        _openedGenerationsFiles.Add(new KeyValuePair<int, string>(int.Parse(filename), openFileDialog.FileNames[i]));
                    }
                }                
                for (int i = 0; i < _openedGenerationsFiles.Count; i++)
                {
                    if (_experimentID == string.Empty)
                    {
                        string[] textbody = File.ReadAllLines(_openedGenerationsFiles[i].Value);
                        string experimentID = textbody[1].Split(':')[0];
                        _experimentID = experimentID;
                    }
                    if (_experimentID != string.Empty)
                        listBox1.Items.Add(_experimentID + " => " + _openedGenerationsFiles[i].Key);
                }
                listBox1.SelectedIndex = 0;
                SelectedGeneration = _openedGenerationsFiles[0];
                bool parseOriginalStatesFile = ParseOriginalStatesFile();
                bool parseGenerationFile = ParseGenerationFile();
                if (parseOriginalStatesFile && parseGenerationFile)
                {
                    _loaded = true;
                    SetGroupVisibility(true);
                    ResetForm();
                    UpdateLabels();
                    pictureBox1.Invalidate();
                }
            }
        }
        private bool ParseOriginalStatesFile()
        {
            if (_openedOriginalStatesFiles != null)
            {
                string[] textbody = File.ReadAllLines(_openedOriginalStatesFiles);
                int linenumber = 0;
                if (textbody[linenumber] == "[Original States Data]")
                {
                    linenumber += 3;
                    string[] splitted = null;
                    while (linenumber < textbody.Length && textbody[linenumber] != "[EOF]")
                    {
                        switch (textbody[linenumber])
                        {
                            case "[Grid]":
                                linenumber++;
                                splitted = textbody[linenumber].Split(':');
                                _maxX = int.Parse(splitted[0]);
                                _maxY = int.Parse(splitted[1]);
                                _division = int.Parse(splitted[2]);
                                linenumber += 2;
                                break;
                            case "[States]":
                                linenumber++;
                                _dictStates = new Dictionary<string, CellState>();
                                while (textbody[linenumber] != "")
                                {
                                    splitted = textbody[linenumber].Split(':');
                                    int[] pos = new int[2]
                                    {
                                    int.Parse(splitted[0].Split(',')[0]),
                                    int.Parse(splitted[0].Split(',')[1])
                                    };
                                    TVertex cell = MF.BuildTVertexID(pos);
                                    CellState state = MF.ConvertIntToCellState(int.Parse(splitted[1]));
                                    _dictStates.Add(cell, state);
                                    linenumber++;
                                }
                                linenumber++;
                                break;
                            default:
                                MessageBox.Show("Unexpected error parsing the original states file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                        }
                    }
                    if (_dictStates == null)
                    {
                        MessageBox.Show("Missing cells data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    else return true;
                }
                else
                {
                    MessageBox.Show("Error loading the original states data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("No original states file found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private bool ParseGenerationFile()
        {
            string filename = SelectedGeneration.Value;
            string[] textbody = File.ReadAllLines(filename);
            int linenumber = 0;
            if (textbody[linenumber] == "[Generation Data]")
            {
                linenumber += 3;
                while (linenumber < textbody.Length && textbody[linenumber] != "[EOF]")
                {
                    switch (textbody[linenumber])
                    {
                        case "[Grid]":
                            linenumber++;
                            string[] splitted = textbody[linenumber].Split(':');
                            int maxX = int.Parse(splitted[0]);
                            int maxY = int.Parse(splitted[1]);
                            int division = int.Parse(splitted[2]);
                            if (_maxX != maxX || _maxY != maxY || _division != division)
                            {
                                MessageBox.Show("Error loading the data.\nGeneration with wrong grid dimensions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                            linenumber += 2;
                            break;
                        case "[Tumors]":
                            ParseGenerationFileAuxiliar(textbody, ref linenumber, ref _dictTumor);
                            break;
                        case "[Micros]":
                            ParseGenerationFileAuxiliar(textbody, ref linenumber, ref  _dictMicro);
                            break;
                        case "[Migra]":
                            ParseGenerationFileAuxiliar(textbody, ref linenumber, ref _dictMigra);
                            break;
                        case "[BloodstreamTumor]":
                            linenumber++;
                            while (textbody[linenumber] != "") linenumber++;
                            linenumber++;
                            break;
                        case "[BloodstreamMigra]":
                            linenumber++;
                            while (textbody[linenumber] != "") linenumber++;
                            linenumber++;
                            break;
                        case "[MigraGenerated]":
                            linenumber++;
                            while (textbody[linenumber] != "") linenumber++;
                            linenumber++;
                            break;
                        default:
                            MessageBox.Show("Unexpected error parsing the generation file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                    }
                }
                if (_dictTumor == null)
                {
                    MessageBox.Show("Missing tumor data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (_dictMicro == null)
                {
                    MessageBox.Show("Missing micrometastasis data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (_dictMigra == null)
                {
                    MessageBox.Show("Missing migratory cells data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else return true;
            }
            else
            {
                MessageBox.Show("Error loading the data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void ParseGenerationFileAuxiliar(string[] textbody, ref int linenumber, ref Dictionary<int, List<TVertex>> dataToFill)
        {
            linenumber++;
            dataToFill = new Dictionary<int, List<TVertex>>();
            while (textbody[linenumber] != "")
            {
                string[] splitted = textbody[linenumber].Split(':');
                int ID = int.Parse(splitted[0]);
                dataToFill.Add(ID, new List<TVertex>());
                for (int i = 3; i < splitted.Length; i++)
                {
                    if (splitted[i] != "")
                    {
                        int[] pos = new int[2] { int.Parse(splitted[i].Split(',')[0]), int.Parse(splitted[i].Split(',')[1]) };
                        TVertex cell = MF.BuildTVertexID(pos);
                        dataToFill[ID].Add(cell);
                    }
                }
                linenumber++;
            }
            linenumber++;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked == true)
            {
                pictureBox1.Invalidate();
            }
        }
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked == true)
            {
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (_loaded)
            {
                _paintScale = float.Parse(textBox2.Text);
                UpdateLabels();
                switch (SelectedZoom)
                {
                    case SelectedZoom.Normal:
                        PaintNormalZoom(e);
                        break;
                    case SelectedZoom.Increased:
                        PaintIncreasedZoom(e);
                        break;
                }
            }
        }
        private void PaintIncreasedZoom(PaintEventArgs e)
        {
            float height = (float)pictureBox1.Height / 100;
            float width = (float)pictureBox1.Width / 100;
            string[] splitted = textBox1.Text.Split(',');
            if (splitted.Length == 2)
            {
                int startx = 0, starty = 0, topx = 0, topy = 0;
                bool parse1 = int.TryParse(splitted[0], out startx);
                bool parse2 = int.TryParse(splitted[1], out starty);
                if (parse1 && parse2)
                {
                    topx = startx + 100;
                    topy = starty + 100;
                    if (starty + 100 >= _maxY)
                        starty = _maxY - 100;
                    if (SelectedOrgan == SelectedOrgan.Primary)
                    {
                        if (startx + 100 >= _division)
                        {
                            startx = _division - 100;
                            topx = _division;
                        }
                    }
                    else if (startx + 100 >= _maxX)
                    {
                        startx = _maxX - 100;
                        topx = _maxX;
                    }
                    SolidBrush solidBrush = new SolidBrush(Color.LightBlue);
                    List<TVertex> epith = new List<TVertex>();
                    for (int i = startx; i < topx; i++)
                    {
                        for (int j = starty; j < topy; j++)
                        {
                            TVertex cell = MF.BuildTVertexID(i, j);
                            if (_dictStates[cell] == CellState.Epith)
                                epith.Add(cell);
                            else
                            {
                                solidBrush.Color = GetBrushColorFromCellState(cell);
                                switch (PaintMode)
                                {
                                    case PaintMode.Rectangle:
                                        e.Graphics.FillRectangle(solidBrush, (i - startx) * width, (j - starty) * height, width * _paintScale, height * _paintScale);
                                        break;
                                    case PaintMode.Ellipse:
                                        e.Graphics.FillEllipse(solidBrush, (i - startx) * width, (j - starty) * height, (float)(width * _paintScale), (float)(height * _paintScale));
                                        break;
                                }
                            }
                        }
                    }
                    solidBrush.Color = Color.SandyBrown;
                    for (int i = 0; i < epith.Count; i++)
                    {
                        int[] pos = MF.GetTVertexID(epith[i]);
                        switch (PaintMode)
                        {
                            case PaintMode.Rectangle:
                                e.Graphics.FillRectangle(solidBrush, (pos[0] - startx) * width, (pos[1] - starty) * height, width * _paintScale, height * _paintScale);
                                break;
                            case PaintMode.Ellipse:
                                e.Graphics.FillEllipse(solidBrush, (pos[0] - startx) * width, (pos[1] - starty) * height, (float)(width * _paintScale), (float)(height * _paintScale));
                                break;
                        }
                    }

                    PaintMicrometastasisIncreasedZoom(e, height, width, startx, starty, topx, topy, solidBrush);
                    PaintMigratoryCellsIncreasedZoom(e, height, width, startx, starty, topx, topy, solidBrush);
                    PaintTumorsIncreasedZoom(e, height, width, startx, starty, topx, topy, solidBrush);
                }
                else MessageBox.Show("Incorrect input.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else MessageBox.Show("Incorrect input.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void PaintMigratoryCellsIncreasedZoom(PaintEventArgs e, float height, float width, int startx, int starty, int topx, int topy, SolidBrush solidBrush)
        {
            List<TVertex> migra = GetVertexsFromDictionaries(startx, topx, starty, topy, _dictMigra);
            for (int i = 0; i < migra.Count; i++)
            {
                solidBrush.Color = Color.Red;
                int[] pos = MF.GetTVertexID(migra[i]);
                switch (PaintMode)
                {
                    case PaintMode.Rectangle:
                        e.Graphics.FillRectangle(solidBrush, (pos[0] - startx) * width, (pos[1] - starty) * height, width * _paintScale, height * _paintScale);
                        break;
                    case PaintMode.Ellipse:
                        e.Graphics.FillEllipse(solidBrush, (pos[0] - startx) * width, (pos[1] - starty) * height, (float)(width * _paintScale), (float)(height * _paintScale));
                        break;
                }
            }
        }
        private void PaintMicrometastasisIncreasedZoom(PaintEventArgs e, float height, float width, int startx, int starty, int topx, int topy, SolidBrush solidBrush)
        {
            List<TVertex> micro = GetVertexsFromDictionaries(startx, topx, starty, topy, _dictMicro);
            for (int i = 0; i < micro.Count; i++)
            {
                solidBrush.Color = Color.Yellow;
                int[] pos = MF.GetTVertexID(micro[i]);
                switch (PaintMode)
                {
                    case PaintMode.Rectangle:
                        e.Graphics.FillRectangle(solidBrush, (pos[0] - startx) * width, (pos[1] - starty) * height, width * _paintScale, height * _paintScale);
                        break;
                    case PaintMode.Ellipse:
                        e.Graphics.FillEllipse(solidBrush, (pos[0] - startx) * width, (pos[1] - starty) * height, (float)(width * _paintScale), (float)(height * _paintScale));
                        break;
                }
            }
        }
        private void PaintTumorsIncreasedZoom(PaintEventArgs e, float height, float width, int startx, int starty, int topx, int topy, SolidBrush solidBrush)
        {
            List<TVertex> tumors = GetVertexsFromDictionaries(startx, topx, starty, topy, _dictTumor);
            for (int i = 0; i < tumors.Count; i++)
            {
                solidBrush.Color = Color.Black;
                int[] pos = MF.GetTVertexID(tumors[i]);
                switch (PaintMode)
                {
                    case PaintMode.Rectangle:
                        e.Graphics.FillRectangle(solidBrush, (pos[0] - startx) * width, (pos[1] - starty) * height, width * _paintScale, height * _paintScale);
                        break;
                    case PaintMode.Ellipse:
                        e.Graphics.FillEllipse(solidBrush, (pos[0] - startx) * width, (pos[1] - starty) * height, (float)(width * _paintScale), (float)(height * _paintScale));
                        break;
                    default:
                        break;
                }
            }
        }

        private void PaintNormalZoom(PaintEventArgs e)
        {
            float height = (float)pictureBox1.Height / (_maxX / 2);
            float width = (float)pictureBox1.Width / _maxY;
            int startx = 0;
            int topx = _division;
            SolidBrush solidBrush = new SolidBrush(Color.LightBlue);
            if (SelectedOrgan == SelectedOrgan.Secondary)
            {
                startx = _division;
                topx = _maxX;
            }
            List<TVertex> epith = new List<TVertex>();
            for (int i = startx; i < topx; i++)
            {
                for (int j = 0; j < _maxY; j++)
                {
                    TVertex cell = MF.BuildTVertexID(i, j);
                    if (_dictStates[cell] == CellState.Epith)
                        epith.Add(cell);
                    else
                    {
                        solidBrush.Color = GetBrushColorFromCellState(cell);
                        switch (PaintMode)
                        {
                            case PaintMode.Rectangle:
                                e.Graphics.FillRectangle(solidBrush, (i - startx) * width, j * height, width * _paintScale, height * _paintScale);
                                break;
                            case PaintMode.Ellipse:
                                e.Graphics.FillEllipse(solidBrush, (i - startx) * width, j * height, (float)(width * _paintScale), (float)(height * _paintScale));
                                break;
                        }
                    }
                }
            }
            solidBrush.Color = Color.SandyBrown;
            for (int i = 0; i < epith.Count; i++)
            {
                int[] pos = MF.GetTVertexID(epith[i]);
                switch (PaintMode)
                {
                    case PaintMode.Rectangle:
                        e.Graphics.FillRectangle(solidBrush, (pos[0] - startx) * width, pos[1] * height, width * _paintScale, height * _paintScale);
                        break;
                    case PaintMode.Ellipse:
                        e.Graphics.FillEllipse(solidBrush, (pos[0] - startx) * width, pos[1] * height, (float)(width * _paintScale), (float)(height * _paintScale));
                        break;
                }
            }
            PaintMicroNormalZoom(e, height, width, startx, topx, solidBrush);
            PaintMigraNormalZoom(e, height, width, startx, topx, solidBrush);
            PaintTumorsNormalZoom(e, height, width, startx, topx, solidBrush);
        }
        private void PaintMigraNormalZoom(PaintEventArgs e, float height, float width, int startx, int topx, SolidBrush solidBrush)
        {
            float oldpaintscale = _paintScale;
            _paintScale = float.Parse("3");
            List<TVertex> migra = GetVertexsFromDictionaries(startx, topx, 0, _maxY, _dictMigra);
            solidBrush.Color = Color.Red;
            for (int i = 0; i < migra.Count; i++)
            {
                int[] pos = MF.GetTVertexID(migra[i]);
                switch (PaintMode)
                {
                    case PaintMode.Rectangle:
                        e.Graphics.FillRectangle(solidBrush, (pos[0] - startx) * width, pos[1] * height, width * _paintScale, height * _paintScale);
                        break;
                    case PaintMode.Ellipse:
                        e.Graphics.FillEllipse(solidBrush, (pos[0] - startx) * width, pos[1] * height, (float)(width * _paintScale), (float)(height * _paintScale));
                        break;
                }                
            }
            _paintScale = oldpaintscale;
        }
        private void PaintMicroNormalZoom(PaintEventArgs e, float height, float width, int startx, int topx, SolidBrush solidBrush)
        {
            float oldpaintscale = _paintScale;
            _paintScale = float.Parse("3");
            List<TVertex> micro = GetVertexsFromDictionaries(startx, topx, 0, _maxY, _dictMicro);
            solidBrush.Color = Color.Yellow;
            for (int i = 0; i < micro.Count; i++)
            {
                int[] pos = MF.GetTVertexID(micro[i]);
                switch (PaintMode)
                {
                    case PaintMode.Rectangle:
                        e.Graphics.FillRectangle(solidBrush, (pos[0] - startx) * width, pos[1] * height, width * _paintScale, height * _paintScale);
                        break;
                    case PaintMode.Ellipse:
                        e.Graphics.FillEllipse(solidBrush, (pos[0] - startx) * width, pos[1] * height, (float)(width * _paintScale), (float)(height * _paintScale));
                        break;
                }                
            }
            _paintScale = oldpaintscale;
        }
        private void PaintTumorsNormalZoom(PaintEventArgs e, float height, float width, int startx, int topx, SolidBrush solidBrush)
        {
            List<TVertex> tumors = GetVertexsFromDictionaries(startx, topx, 0, _maxY, _dictTumor);
            solidBrush.Color = Color.Black;
            for (int i = 0; i < tumors.Count; i++)
            {
                int[] pos = MF.GetTVertexID(tumors[i]);
                switch (PaintMode)
                {
                    case PaintMode.Rectangle:
                        e.Graphics.FillRectangle(solidBrush, (pos[0] - startx) * width, pos[1] * height, width * _paintScale, height * _paintScale);
                        break;
                    case PaintMode.Ellipse:
                        e.Graphics.FillEllipse(solidBrush, (pos[0] - startx) * width, pos[1] * height, (float)(width * _paintScale), (float)(height * _paintScale));
                        break;
                }
            }
        }

        private Color GetBrushColorFromCellState(TVertex cell)
        {
            switch (_dictStates[cell])
            {
                case CellState.Lumen:
                    return Color.White;
                case CellState.Epith:
                    return Color.SandyBrown;
                case CellState.Strom:
                    return Color.LightGray;
                case CellState.Tumor:
                    return Color.Black;
                case CellState.Migra:
                    return Color.Red;
                case CellState.Micro:
                    return Color.Yellow;
                default:
                    MessageBox.Show("Unexpected error, a cell cannot have NULL state.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return Color.LightBlue;
            }
        }        
        private List<TVertex> GetVertexsFromDictionaries(int startx, int topx, int starty, int topy, Dictionary<int, List<TVertex>> dictToRead)
        {
            List<TVertex> result = new List<TVertex>();
            List<List<TVertex>> values = dictToRead.Values.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                List<TVertex> cells = values[i];
                for (int j = 0; j < cells.Count; j++)
                {
                    int[] pos = MF.GetTVertexID(cells[j]);
                    if ((startx <= pos[0] && pos[0] < topx) &&
                        (starty <= pos[1] && pos[1] < topy))
                    {
                        result.Add(cells[j]);
                    }
                }
            }
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectedGeneration = _openedGenerationsFiles[listBox1.SelectedIndex];
            ParseGenerationFile();
            pictureBox1.Invalidate();
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                pictureBox1.Invalidate();
            }
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                pictureBox1.Invalidate();
            }
        }
        private void radioButton2_Click(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                pictureBox1.Invalidate();
            }
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                pictureBox1.Invalidate();
            }
        }
        private void radioButton3_Click(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                pictureBox1.Invalidate();
            }
        }
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked == true)
            {
                pictureBox1.Invalidate();
            }
        }
        private void radioButton6_Click(object sender, EventArgs e)
        {
            if (radioButton6.Checked == true)
            {
                pictureBox1.Invalidate();
            }
        }
        
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = (int)(e.X / (pictureBox1.Height / ((double)_maxX / 2)));
                int y = (int)(e.Y / (pictureBox1.Width / ((double)_maxY)));
                if (x - 50 < 0)
                    x = 50;
                if (y - 50 < 0)
                    y = 50;
                textBox1.Text = (x - 50) + "," + (y - 50);
            }
        }        

        private void button2_Click(object sender, EventArgs e)
        {
            string imagesFolder = Directory.GetCurrentDirectory() + "\\SavedSnaps";
            Directory.CreateDirectory(imagesFolder);
            Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.DrawToBitmap(bitmap, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            string interior = SelectedOrgan.ToString();
            string fileName = imagesFolder + "\\" + SelectedGeneration.Key.ToString() + "-" + _experimentID.ToString() + "-" + interior + ".png";
            bitmap.Save(fileName, ImageFormat.Png);
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            string imagesFolder = Directory.GetCurrentDirectory() + "\\SavedSnaps";
            Directory.CreateDirectory(imagesFolder);
            string experimentFolder = imagesFolder + "\\" + _experimentID.ToString();
            Directory.CreateDirectory(experimentFolder);
            int listboxCount = listBox1.Items.Count;
            for (int i = 0; i < listboxCount; i++)
            {
                Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                listBox1.SelectedIndex = i;
                SelectedGeneration = _openedGenerationsFiles[listBox1.SelectedIndex];
                ParseGenerationFile();

                // primary organ
                radioButton4.Checked = true;
                UpdateLabels();
                pictureBox1.Invalidate();
                pictureBox1.DrawToBitmap(bitmap, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
                label6.DrawToBitmap(bitmap, new Rectangle(1, 1, 121, 13));
                label7.DrawToBitmap(bitmap, new Rectangle(1, 14, 140, 13));

                // secondary organ
                //radioButton5.Checked = true;
                //UpdateLabels();
                //pictureBox1.Invalidate();
                //pictureBox1.DrawToBitmap(bitmap, new Rectangle(pictureBox1.Width, 0, pictureBox1.Width * 2, pictureBox1.Height));
                //label6.DrawToBitmap(bitmap, new Rectangle(pictureBox1.Width + 1, 1, 121, 13));
                //label7.DrawToBitmap(bitmap, new Rectangle(pictureBox1.Width + 1, 14, 140, 13));

                string fileName = experimentFolder + "\\" + SelectedGeneration.Key.ToString() + "-" + _experimentID.ToString() + ".png";
                bitmap.Save(fileName, ImageFormat.Png);
                await Wait();
            }
        }

        private async Task Wait()
        {
            await Task.Delay(1000);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
