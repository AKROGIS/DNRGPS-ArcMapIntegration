using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DnrGps_ArcMap;
using System.Linq;

namespace TestDataTable
{
    public partial class TestForm : Form
    {
        private readonly ArcMapController _arcMap;
        private IDnrGpsController _controller;

        public TestForm()
        {
            InitializeComponent();
            _arcMap = new ArcMapController();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DisableButtons();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            AddMessage("Connect button clicked.");

            if (!_arcMap.HasOpenDocuments)
            {
                AddMessage("ArcMap has no documents open, trying to open a new document");
                _arcMap.StartNewDocument();
            }
            else
            {
                AddMessage("ArcMap has some documents open");
            }
            if (!_arcMap.HasOpenDocuments)
            {
                AddMessage("Unable to open an ArcMap document");
                return;
            }
            string title = _arcMap.GetTitleFromTopDocument();
            AddMessage("Title of top document = " + title);
            _controller = _arcMap.GetExtensionFromTopDocument();
            if (_controller == null)
            {
                AddMessage("Failed to get DNR GPS/ArcMap Extension");
                return;
            }

            AddMessage("Connected To ArcMap.  Proceed.");
            EnableButtons();
        }

        private void layersButton_Click(object sender, EventArgs e)
        {
            AddMessage("Get Layers button clicked.");
            layerListBox.Items.Clear();
            //string[] layerNames;
            Dictionary<string,string> layers;
            try
            {
                //layerNames = _controller.GetFeatureLayerNames();
                layers = (Dictionary<string,string>)_controller.GetFeatureLayers();
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller threw an exception. " + ex.Message);
                return;
            }
            //if (layerNames == null || layerNames.Length == 0)
            if (layers == null || layers.Count == 0)
                    AddMessage("ArcMap returned no layer names.");
            else
            {
                AddMessage("ArcMap returned the layer names now in the list View.");
                _layers = layers.Select(layer => new Layer {Index = layer.Key, Name = layer.Value}).ToArray();
                //string[] names = new string[layerNames.Count];
                //layerNames.Values.CopyTo(names,0);
                //layerListBox.Items.AddRange(names);
                layerListBox.Items.AddRange(_layers);
            }
        }

        private class Layer
        {
            public string Index { get; set; }
            public string Name { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        private Layer[] _layers;

        private void dataButton_Click(object sender, EventArgs e)
        {
            AddMessage("Get Data button clicked.");
            dataGridView.DataSource = null;
            //string layerName = layerListBox.Text;
            var layer = (Layer)layerListBox.SelectedItem;
            if (layer == null || String.IsNullOrEmpty(layer.Name))
            {
                AddMessage("Failed to Get data.  There is no layer name selected.");
                return;
            }
            AddMessage("Getting Data for layer: "+layer.Name+ " at Index: "+ layer.Index);
            DataTable newtable;
            try
            {
                //newtable = _controller.GetFeatureLayerData(layer.Index);
                _controller.TenItemsAdded += UpdateProgressor;
                newtable = GetDataTable(layer.Index);
                _controller.TenItemsAdded -= UpdateProgressor;
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller threw an exception. " + ex.Message);
                return;
            }
            if (newtable == null)
                AddMessage("Failed to Get data.  ArcMap returned null.");
            else
            {
                AddMessage("ArcMap returned the data table now in the Data Grid View.");
                dataGridView.DataSource = newtable;
            }
        }

        void UpdateProgressor(object sender, TenItemsAddedEventArgs e)
        {
            //do something with e.ItemCount and e.TotalCount
            string msg = string.Format("Added {0:0.0}% {1}/{2}", 100.0*e.ItemCount/e.TotalCount, e.ItemCount, e.TotalCount);
            AddMessage(msg);
        }

        private DataTable GetDataTable(string id)
        {
            //MessageBox.Show(string.Format("Current Process: {0}", System.Diagnostics.Process.GetCurrentProcess().Id));

            //DateTime start = DateTime.Now;

            //Option 1, get DataTable from DnrGpsController
            // 3m50.1s for 2137 points with 8 columns
            var table = _controller.GetFeatureLayerData(id);

            //Option 2, serialize DataTable to/from temp file.
            // 2m17.2s, 1m4.4s, 1m3.2s for 2137 points with 8 columns
            // 3m43.6s serialize; 0.032s deserial; 3m42.1s serialize, 0.02s deserial
            //string tempFile = System.IO.Path.GetTempFileName();
            //_controller.WriteFeatureLayerToFile(layerName, tempFile);
            //var table = new DataTable();
            //table.ReadXml(tempFile);

            //MessageBox.Show(string.Format("Getting DataTable took {0}",DateTime.Now-start));
            
            return table;
        }

        private void loadFeatureClassButton_Click(object sender, EventArgs e)
        {
            try
            {
                _controller.LoadDataSet(GisDataType.Shapefile, @"C:\tmp\Lake Depth", "lakeshorevertices.shp");
                //_controller.LoadDataSet(GisDataType.Shapefile, @"C:\tmp", "lines");
                AddMessage("Added a feature class to the focus map");
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller.LoadDataSet() threw an exception. " + ex.Message);
            }
        }


        private void sendButton_Click(object sender, EventArgs e)
        {
            AddMessage("Send GPS button clicked.");
            //randomize the location and direction
            _x += .25 - _random.NextDouble() * .50;
            _y += .15 - _random.NextDouble() * .3;
            _a += 30 - _random.NextDouble() * 60;
            _a = (_a + 360) % 360;

            //draw the new graphic
            System.Diagnostics.Debug.Print("Draw GPS @ x = {0}, y = {1}, a = {2}", _x, _y, _a);
            int id;
            try
            {
                id = _controller.DrawGpsPoint(_y, _x, _a, BreadCrumbs.Lines);
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller.DrawGpsPoint threw an exception. " + ex.Message);
                return;
            }
            if (id == -1)
                AddMessage("ArcMap was unable to add a point.");
            else
            {
                AddMessage("ArcMap added a graphic point with id = " + id);
            }
            try
            {
                //keep only the last five points, older ones are cleared
                // (assumes this method is the only one creating graphic ids)
                if (id > 5)
                    _controller.ClearGpsGraphics(id - 5);
                //refresh the display so the changes are visible
                _controller.RefreshDisplay(_y, _x, 0.5);
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller.ClearGpsGraphics/RefreshDisplay threw an exception. " + ex.Message);
            }
        }
        private double _x = -136.53;
        private double _y = 58.76;
        private double _a;
        private readonly Random _random = new Random();

        private void sendCepButton_Click(object sender, EventArgs e)
        {
            AddMessage("Send CEP button clicked.");
            //randomize the location and direction
            _x += .25 - _random.NextDouble() * .50;
            _y += .15 - _random.NextDouble() * .3;

            //draw the new graphic
            System.Diagnostics.Debug.Print("Draw CEP at x = {0}, y = {1}, ", _x, _y);
            int id;
            try
            {
                //draw the new graphic
                id = _controller.DrawGpsCep(_y, _x, new double[] { 30, 45, 60, 75 });
                //refresh the display so the changes are visible
                _controller.RefreshDisplay(_y, _x, 0.5);
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller.DrawGpsCep threw an exception. " + ex.Message);
                return;
            }
            if (id == -1)
                AddMessage("ArcMap was unable to add a point.");
            else
            {
                AddMessage("ArcMap added a graphic point with id = " + id);
            }
            try
            {
                //clear the previous graphic
                // (assumes this method is the only one creating graphic ids)
                if (id > 1)
                    _controller.ClearGpsGraphics(id - 1);
                //refresh the display so the changes are visible
                _controller.RefreshDisplay(_y, _x, 0.5);
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller.ClearGpsGraphics/RefreshDisplay threw an exception. " + ex.Message);
            }

        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            AddMessage("Clear button clicked.");
            try
            {
                _controller.ClearGpsGraphics();
                AddMessage("Cleared the graphics Layer.");
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller threw an exception. " + ex.Message);
            }
        }

        private void quitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void EnableButtons()
        {
            layersButton.Enabled = true;
            dataButton.Enabled = true;
            sendButton.Enabled = true;
            sendCepButton.Enabled = true;
            //connectButton.Enabled = false;
        }

        private void DisableButtons()
        {
            layersButton.Enabled = false;
            dataButton.Enabled = false;
            sendButton.Enabled = false;
            sendCepButton.Enabled = false;
            connectButton.Enabled = true;
        }

        private void AddMessage(string msg)
        {
            messages.Text = messages.Text + Environment.NewLine + msg;
        }


        private void getGraphicsButton_Click(object sender, EventArgs e)
        {
            AddMessage("Get Graphics button clicked.");
            dataGridView.DataSource = null;
            DataTable newtable;
            try
            {
                _controller.TenItemsAdded += UpdateProgressor;
                newtable = _controller.GetGraphics();
                _controller.TenItemsAdded -= UpdateProgressor;
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller threw an exception. " + ex.Message);
                return;
            }
            if (newtable == null)
                AddMessage("Failed to Get data.  ArcMap returned null.");
            else
            {
                AddMessage("ArcMap returned the data table now in the Data Grid View.");
                dataGridView.DataSource = newtable;
            }
        }

        private void sendGraphicsButton_Click(object sender, EventArgs e)
        {
            AddMessage("Send Graphics button clicked.");
            dataGridView.DataSource = null;
            DataTable newtable = BuildTestTable();
            dataGridView.DataSource = newtable;
            try
            {
                _controller.Defaults.LineColor = Color.Purple;
                _controller.Defaults.LineWidth = 3.5;
                _controller.Defaults.MarkerColor = Color.Fuchsia;
                _controller.Defaults.MarkerOutlineWidth = 0.0;
                _controller.Defaults.MarkerSize = 13;

                _controller.TenItemsAdded += UpdateProgressor;
                _controller.AddGraphics(newtable);
                _controller.TenItemsAdded -= UpdateProgressor;
                _controller.RefreshDisplay();
            }
            catch (Exception ex)
            {
                AddMessage("ArcMap Controller threw an exception. " + ex.Message);
            }
        }

        private DataTable BuildTestTable()
        {
            var table = new DataTable();
            var column = new DataColumn
            {
                ColumnName = "Shape",
                DataType = typeof(string)
            };
            table.Columns.Add(column);
            foreach (var s in _wkt)
            {
                DataRow newRow = table.NewRow();
                newRow["Shape"] = s;
                table.Rows.Add(newRow);                
            }
            return table;
        }

        private readonly string[] _wkt = new[]
        {
            "POINT (-158.20 66.20)",
            "POINT z (-158.22 66.20 10.0)",
            "POINT m (-158.21 66.21 100.0)",
            "POINT Zm (-158.20 66.22 10. 100.0)",
            "MultiPoint ((-158.22 65.98), (-158.23 65.98))",
            "linestring (-157.95 65.99, -157.95 65.98, -157.97 65.99)",
            "multilinestring ((-157.97 65.98, -157.94 65.97), (-157.95 65.97, -157.96 65.96))",
            "polygon ((-158.09 66.00, -158.08 66.00, -158.08 66.05, -158.09 66.05, -158.09 66.00))",
            //returns POLYGON ((-158.08 66.00, -158.09 66.00, -158.09 66.05, -158.08 66.05, -158.08 66.00))
            "polygon ((-158.07 66.00, -158.04 66.00, -158.04 66.05, -158.07 66.05, -158.07 66.00), (-158.06 66.01, -158.06 66.02, -158.05 66.02, -158.05 66.01, -158.06 66.01) )",
            //return POLYGON ((-158.04 66.05, -158.04 66.00, -158.07 66.00, -158.07 66.05, -158.04 66.05), (-158.06 66.02, -158.06 66.01, -158.05 66.01, -158.05 66.02, -158.06 66.02))
            "polygon ((-158.03 66.00, -158.00 66.00, -158.00 66.05, -158.03 66.05, -158.03 66.00), (-158.02 66.01, -158.02 66.02, -158.01 66.02, -158.01 66.01, -158.02 66.01), (-158.02 66.03, -158.02 66.04, -158.01 66.04, -158.01 66.03, -158.02 66.03))",
            "multipolygon (((-158.09 66.06, -158.08 66.06, -158.08 66.11, -158.09 66.11, -158.09 66.06))" +
                        ", ((-158.07 66.06, -158.04 66.06, -158.04 66.11, -158.07 66.11, -158.07 66.06), (-158.06 66.07, -158.06 66.08, -158.05 66.08, -158.05 66.07, -158.06 66.07))" +
                        ", ((-158.03 66.06, -158.00 66.06, -158.00 66.11, -158.03 66.11, -158.03 66.06), (-158.02 66.07, -158.02 66.08, -158.01 66.08, -158.01 66.07, -158.02 66.07), (-158.02 66.09, -158.02 66.10, -158.01 66.10, -158.01 66.09, -158.02 66.09)))",
            //returns POLYGON ((-158.08 66.06,-158.09 66.06,-158.09 66.11,-158.08 66.11,-158.08 66.06),
                             //(-158.04 66.11,-158.04 66.06,-158.07 66.06,-158.07 66.11,-158.04 66.11),
                             //(-158.06 66.08,-158.06 66.07,-158.05 66.07,-158.05 66.08,-158.06 66.08),
                             //(-158.00 66.11,-158.00 66.06,-158.03 66.06,-158.03 66.11,-158.00 66.11),
                             //(-158.02 66.10,-158.02 66.09,-158.01 66.09,-158.01 66.10,-158.02 66.10),
                             //(-158.02 66.08,-158.02 66.07,-158.01 66.07,-158.01 66.08,-158.02 66.08))

        };

    }
}
