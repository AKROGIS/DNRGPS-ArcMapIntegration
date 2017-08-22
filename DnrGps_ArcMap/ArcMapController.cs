using System;
using System.Diagnostics;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Framework;

// Read Register Legacy Components with ArcGIS 10
// http://support.esri.com/en/knowledgebase/techarticles/detail/37639
// for deploying a dll compiled for a earlier version on ArcGIS 10.

namespace DnrGps_ArcMap
{
    [System.Runtime.InteropServices.ComVisible(false)]
    public class ArcMapController
    {

        public bool HasOpenDocuments
        {
            get
            {
                var process = GetTopProcessByName("arcmap");
                return (process != null);
            }
        }

        public bool StartNewDocument()
        {
            try
            {
                // Only required on 10; requires reference to ESRI.ArcGIS.Version (also only on 10)
                if (!ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop))
                    return false;

                IDocument doc = new MxDocumentClass();
                doc.Parent.Visible = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool KillTopDocument()
        {
            var process = GetTopProcessByName("arcmap");
            if (process == null)
                return false;
            var arcMap = GetArcMap(process.MainWindowTitle);
            if (arcMap == null)
                return false;
            arcMap.Shutdown();
            return true;
        }

        public IDnrGpsController GetExtensionFromTopDocument()
        {
            var process = GetTopProcessByName("arcmap");
            if (process == null)
                return null;
            var arcMap = GetArcMap(process.MainWindowTitle);
            if (arcMap == null)
                return null;
            return GetCustomClass(arcMap);
        }

        public string GetTitleFromTopDocument()
        {
            var process = GetTopProcessByName("arcmap");
            if (process == null)
                return null;
            return process.MainWindowTitle;
        }





        private static IApplication GetArcMap(string caption)
        {
            try
            {
                // Only on 10; requires reference to ESRI.ArcGIS.Version (also only on 10)
                if (!ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop))
                    return null;
            }
            catch (Exception)
            {
                return null;
            }

            //FIXME - AppROT is only returning instances of ArcMap that were created by this process
            //or previous versions of the same process.  Not ArcMap launched by windows.
            //If there are no ArcMap sessions started by this process (or a prior one),
            //but there are other ArcMap sessions running, then this call will take a long time,
            //and then throw an exception.
            IAppROT appTable = new AppROT();

            for (int i = 0; i < appTable.Count; i++)
            {
                if (appTable.Item[i] is IMxApplication &&
                    appTable.Item[i].Visible &&
                    appTable.Item[i].Caption == caption)
                    return appTable.Item[i];
            }
            return null;
        }

        private static IDnrGpsController GetCustomClass(IApplication arcMap)
        {
            var objectFactory = arcMap as IObjectFactory;
            if (objectFactory == null)
                return null;
            //var controller = (IDnrGpsController)objectFactory.Create("DnrGps_ArcMap.DnrGpsController");
            var controller = new DnrGpsController
                                 {ObjectFactory = objectFactory, 
                                     MxDocument = (IMxDocument)arcMap.Document};
            return controller;
        }

        //Returns the top (by window order) process with name
        //returns null if no process with name is found
        private static Process GetTopProcessByName(string name)
        {
            Process topProcess = null;
            //Z value of a window is the number of windows above it
            int topZ = int.MaxValue;
            foreach (var p in Process.GetProcessesByName(name))
            {
                int z = ZOrder(p.MainWindowHandle);
                if (z >= topZ) continue; //Ignore - This process is below another similar process 
                topProcess = p;
                topZ = z;
            }
            return topProcess;
        }

        //ZOrder is the number of windows above the specified window
        private static int ZOrder(IntPtr windowHandle)
        {
            int z = 0;
            while ((windowHandle = GetWindow(windowHandle, 3)) != IntPtr.Zero)
            {
                z++;
            }
            return z;
        }

        //Retrieves a handle to a window that has the specified relationship (Z-Order or owner) to the specified window.
        //hWnd: A handle to a window. The window handle retrieved is relative to this window, based on the value of the uCmd parameter.
        //uCmd: The relationship between the specified window and the window whose handle is to be retrieved.
        //If uCmd = 3 (GW_HWNDPREV), Then retrieved handle identifies the window above the specified window in the Z order.
        //The window at the top of the z-order overlaps all other windows. 
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
    }
}



//static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

//enum GetWindow_Cmd : uint
    //{
    //    GW_HWNDFIRST = 0,
    //    GW_HWNDLAST = 1,
    //    GW_HWNDNEXT = 2,
    //    GW_HWNDPREV = 3,
    //    GW_OWNER = 4,
    //    GW_CHILD = 5,
    //    GW_ENABLEDPOPUP = 6
    //}



//private static IApplication StartArcMap2()
//{
//    // Only on 10; requires reference to ESRI.ArcGIS.Version (also only on 10)
//    ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);

//    //ESRI.ArcGIS.Carto.IMap map = new ESRI.ArcGIS.Carto.MapClass();
//    IDocument doc = new ESRI.ArcGIS.ArcMapUI.MxDocumentClass();
//    doc.Parent.Visible= true;
//    return doc.Parent;
//}

//        Process p = TopProcessByName("arcmap");

//if (p == null)
//{
//    AddMessage("ArcMap not Started");
//    AddMessage("Starting ArcMap");
//    _arcMap.StartArcMap();
//}
//else
//{
//    AddMessage("ArcMap is Running");
//    AddMessage(p.MainWindowTitle);
//    _arcMap = GetArcMap(p.MainWindowTitle);
//}
//if (_arcMap == null)

//private IApplication GetArcMap1()
//{
//    // Only on 10; requires reference to ESRI.ArcGIS.Version (also only on 10)
//    ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);

//    // Get the actual underlying COM type
//    Type t = Type.GetTypeFromCLSID(typeof(AppRefClass).GUID);

//    // FAILS.  Only works when run in an ArcGIS process 
//    System.Object obj = Activator.CreateInstance(t);
//    return obj as IApplication;
//}

//private IDNRGarminController GetCustomClass2(IApplication arcMap)
//{
//    var objectFactory = arcMap as ESRI.ArcGIS.Framework.IObjectFactory;
//    if (objectFactory == null)
//        return null;
//    return (IDNRGarminController)objectFactory.Create("DNRGarmin_ArcMap.DNRGarminController");
//}

//private void AddDataToMap(IApplication arcMap)
//{
//    var objectFactory = arcMap as ESRI.ArcGIS.Framework.IObjectFactory;
//    if (objectFactory == null)
//        return;

//    Type shpWkspFactType = typeof(ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass);
//    string typeClsID = shpWkspFactType.GUID.ToString("B");
//    IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)objectFactory.Create(typeClsID);
//    IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)
//        workspaceFactory.OpenFromFile(@"C:\tmp", 0);

//    //Create the layer.
//    IFeatureLayer featureLayer = (IFeatureLayer)objectFactory.Create(
//        "esriCarto.FeatureLayer");
//    featureLayer.FeatureClass = featureWorkspace.OpenFeatureClass("pt99");
//    featureLayer.Name = featureLayer.FeatureClass.AliasName;

//    //Add the layer to the document.
//    IBasicDocument document = (IBasicDocument)_arcMap.Document;
//    document.AddLayer(featureLayer);
//    document.UpdateContents();
//}
