using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
//using System.Runtime.InteropServices;

namespace DnrGps_ArcMap
{
    //This object is used to describe a layer in a map document.
    //CAUTION: The user may alter the TOC in the map after this object is created.
    //So this object may be bogus when it comes time to retrieve a layer.
    //This is similar to never assuming that the file system is static.
    
    //[Guid("6df1f7fc-e6e9-48cb-ad55-4b7b1ebc6d25")]
    public class LayerName : ILayerName//, IEquatable<LayerName>
    {
        private List<string> _groups;  //Could be empty, but never null
        public string Name { get; private set; }
        public string Dataframe { get; set; }            //Could be null - if there is only one default dataframe
        //Might want to add the geometry type for an icon - should be part of constructor

        public LayerName(string name)
        {
            Name = name;
            _groups = new List<string>();
        }


        public string[] Groups
        {
            get
            {
                return _groups.ToArray();
            }
        }

        public void AddGroup(string group)
        {
            _groups.Add(group);
        }

        public override bool Equals(object other)
        {
            if (this.GetType() != other.GetType())
                return false;

            return this.Equals(other as LayerName);
        }

        #region IEquatable<LayerName>
        
        public bool Equals(LayerName other)
        {
            if (object.ReferenceEquals(other, null))
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return Name == other.Name &&
                   Groups.SequenceEqual(other.Groups) && 
                   Dataframe == other.Dataframe;
        }

        #endregion

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(LayerName a, LayerName b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;

            //Now that we know a is not null, call the instance method
            return a.Equals(b);
        }

        public static bool operator !=(LayerName a, LayerName b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(Dataframe))
                sb.Append(Dataframe + ":");
            foreach (string item in Groups)
            {
                sb.Append(item + "/");
            }
            sb.Append(Name);
            return sb.ToString();
        }
    }


}
