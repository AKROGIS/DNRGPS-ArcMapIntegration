using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DnrGps_ArcMap
{
    [Guid("6ba6fe4f-abc5-423f-9fed-cccb82f854a6")]
    public interface ILayerName
    {
        string Name { get; }
        string[] Groups { get; }        //Could be empty, but never null
        string Dataframe { get; set; }  //Could be null - if there is only one default dataframe
        void AddGroup(string group);
        string ToString();
    }
}
