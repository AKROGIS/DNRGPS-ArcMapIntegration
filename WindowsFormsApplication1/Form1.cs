using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;

using DebuggingAddin;
//using DebuggingAddin.Internal;
using DnrGps_ArcMap;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //b.Click();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var a = new ArcMapController();
            //if (a.HasOpenDocuments)
            //    _aa = ArcMapController.GetArcMap(a.GetTitleFromTopDocument());
            //else
            //{
            //    a.StartNewDocument();
            //    _aa = ArcMapController.GetArcMap(a.GetTitleFromTopDocument());
            //}
        }

        private IApplication _aa;
    }
}
