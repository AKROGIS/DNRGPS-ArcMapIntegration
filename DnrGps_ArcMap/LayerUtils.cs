using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;

namespace DnrGps_ArcMap
{
    internal class LayerUtils
    {
        /// <summary>
        /// Gets all layers in a map document of a particular type 
        /// </summary>
        /// <param name="doc">The map document to search</param>
        /// <param name="type">A GUID type string for the layer type. see http://help.arcgis.com/en/sdk/10.0/arcobjects_net/componenthelp/index.html#/Loop_Through_Layers_of_Specific_UID_Snippet/00490000005w000000/ </param>
        /// <returns>a list (possibly empty) of layer that were found to match the type requested</returns>
        internal static IEnumerable<ILayer> GetAllLayers(IMxDocument doc, string type)
        {
            if (doc == null)
                throw new ArgumentNullException("doc");
            if (type == null)
                throw new ArgumentNullException("type");

            UID uid = new UIDClass();
            uid.Value = type;
            IMaps maps = doc.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                IMap map = maps.Item[i];
                IEnumLayer layersEnumerator = map.Layers[uid];
                ILayer layer;
                while ((layer = layersEnumerator.Next()) != null)
                {
                    yield return layer;
                }
            }
        }

        internal static IEnumerable<ILayer> GetAllLayers(IMap map, string type)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (type == null)
                throw new ArgumentNullException("type");

            UID uid = new UIDClass();
            uid.Value = type;
            IEnumLayer layersEnumerator = map.Layers[uid];
            ILayer layer;
            while ((layer = layersEnumerator.Next()) != null)
            {
                yield return layer;
            }
        }

        #region layer naming methods

        /// <summary>
        /// Gets the full path name of the layer (including ancestor group layers and data frame)
        /// </summary>
        /// <param name="doc">The map document that this layer is in</param>
        /// <param name="layer">The ILayer whose name we want</param>
        /// <param name="mapSeparator">A character string used to data frame name from the group/layer names</param>
        /// <param name="layerSeparator">A character string used to the group names from the layer name</param>
        /// <returns>null if the layer does not exist in the map document, full name otherwise</returns>
        internal static string GetFullName(IMxDocument doc, ILayer layer, string mapSeparator = ":",
                                           string layerSeparator = "/")
        {
            //ILayer does not know where it is in the maps/groups heirarchy, so a search is required.
            IMaps maps = doc.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                IMap map = maps.Item[i];
                string name = GetFullName(map, layer, layerSeparator);
                if (name != null)
                {
                    return map.Name + mapSeparator + name;
                }
            }
            return null;
        }

        //FIXME - These two methods are nearly identical, except the type of the parent.  Can I combine them??

        /// <summary>
        /// Gets the full path name of the layer (including ancestor group layers) relative to the IMap
        /// </summary>
        /// <param name="parent">The data frame that this layer is in</param>
        /// <param name="layer">The ILayer whose full path name we want</param>
        /// <param name="separator">A character string used to separate names in path</param>
        /// <returns>null if the layer does not exist in IMap, full name otherwise</returns>
        internal static string GetFullName(IMap parent, ILayer layer, string separator = "/")
        {
            for (int i = 0; i < parent.LayerCount; i++)
            {
                if (parent.Layer[i] == layer)
                    return layer.Name;

                if (!(parent.Layer[i] is ICompositeLayer))
                    continue;

                string name = GetFullName((ICompositeLayer)parent.Layer[i], layer, separator);
                if (name != null)
                {
                    return parent.Layer[i].Name + separator + name;
                }
            }
            return null;
        }

        private static string GetFullName(ICompositeLayer parent, ILayer layer, string separator)
        {
            for (int i = 0; i < parent.Count; i++)
            {
                if (parent.Layer[i] == layer)
                    return layer.Name;

                if (!(parent.Layer[i] is ICompositeLayer))
                    continue;

                string name = GetFullName((ICompositeLayer)parent.Layer[i], layer, separator);
                if (name != null)
                {
                    return parent.Layer[i].Name + separator + name;
                }
            }
            return null;
        }

        #endregion

        #region Layer Indexing Methods

        // each index but the last is assumed to reference a composite layer,
        // the first non-composite layer, or the layer at the last index is returned.
        // if any index is out of bounds for that level of grouping, an exception will be thrown
        internal static ILayer GetLayer(IMap map, string indexes)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (indexes == null)
                throw new ArgumentNullException("indexes");

            return GetLayer(map, indexes, "-");
        }

        internal static string GetIndexString(IMap map, ILayer layer)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (layer == null)
                throw new ArgumentNullException("layer");

            return GetIndexString(map, layer, "-");
        }

        private static ILayer GetLayer(IMap map, string indexes, string separator)
        {
            string[] separators = (separator == null) ? new string[0] : new[] {separator};

            string[] items = indexes.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //Convert.ToInt32() may throw System.FormatException or System.OverflowException
            int[] ints = items.Select(a => Convert.ToInt32(a)).ToArray();
            return GetLayer(map, ints);
        }

        private static ILayer GetLayer(IMap map, int[] indexes)
        {
            if (indexes == null || indexes.Length < 1)
                throw new ArgumentException("Must provide at least one integer", "indexes");

            int i = 0;
            int index = indexes[i];
            if (index < 0 || map.LayerCount <= index)
            {
                if (map.LayerCount == 0)
                    throw new InvalidOperationException("The focus map has no layers.");
                string message = string.Format("The layer index ({0}) for the focus map is not in the range (0,{1})",
                                               index, map.LayerCount - 1);
                throw new ArgumentException(message, "indexes");
            }
            var layer = map.Layer[index];
            while (layer is ICompositeLayer && i < indexes.Length - 1)
            {
                index = indexes[++i];
                if (index < 0 || ((ICompositeLayer)layer).Count <= index)
                {
                    if (((ICompositeLayer)layer).Count == 0)
                        throw new InvalidOperationException("Group layer'" + layer.Name + "' is empty.");
                    string message = string.Format("The index ({0}) for group layer '{1}' is not in the range (0,{2})",
                                                   index, layer.Name, ((ICompositeLayer)layer).Count - 1);
                    throw new ArgumentException(message, "indexes");                    
                }
                layer = ((ICompositeLayer)layer).Layer[index];
            }
            return layer;
        }

        private static string GetIndexString(IMap map, ILayer layer, string separator)
        {
            for (int i = 0; i < map.LayerCount; i++)
            {
                if (map.Layer[i] == layer)
                    return i.ToString(CultureInfo.InvariantCulture);
                if (map.Layer[i] is ICompositeLayer)
                {
                    string result = GetIndexString((ICompositeLayer)map.Layer[i], layer, separator);
                    if (result != null)
                        return i.ToString(CultureInfo.InvariantCulture) + separator + result;
                }
            }
            return null;
        }

        private static string GetIndexString(ICompositeLayer group, ILayer layer, string separator)
        {
            for (int i = 0; i < group.Count; i++)
            {
                if (group.Layer[i] == layer)
                    return i.ToString(CultureInfo.InvariantCulture);
                if (group.Layer[i] is ICompositeLayer)
                {
                    string result = GetIndexString((ICompositeLayer)group.Layer[i], layer, separator);
                    if (result != null)
                        return i.ToString(CultureInfo.InvariantCulture) + separator + result;
                }
            }
            return null;
        }


        public static IList<NamedLayer> SearchAllToc(IMxDocument doc, Type type)
        {
            var layers = new List<NamedLayer>();
            if (doc.Maps.Count > 1)
                for (int i = 0; i < doc.Maps.Count; i++)
                    SearchMap(doc.Maps.Item[i], type, layers, i + "-", doc.Maps.Item[i].Name + "/");
            else
                SearchMap(doc.FocusMap, type, layers);
            return layers;
        }

        public static IList<NamedLayer> SearchFocusToc(IMap map, Type type)
        {
            var layers = new List<NamedLayer>();
                SearchMap(map, type, layers);
            return layers;
        }

        private static void SearchMap(IMap map, Type type, ICollection<NamedLayer> layers, string indexPrefix = "", string namePrefix = "")
        {
            for (int i = 0; i < map.LayerCount; i++)
            {
                var layer = map.Layer[i];
                if (ComImplementsInterface(layer, type))
                {
                    layers.Add(new NamedLayer
                    {
                        Name = namePrefix + layer.Name,
                        Index = indexPrefix + i,
                        Layer = layer
                    });
                    continue;
                }
                if (layer is IGroupLayer)
                {
                    SearchLayer(layer as ICompositeLayer, type, layers, indexPrefix + i + "-", namePrefix + layer.Name + "/" );
                }
            }
        }

        public static void SearchLayer(ICompositeLayer groupLayer, Type type, ICollection<NamedLayer> layers, string indexPrefix = "", string namePrefix = "")
        {
            for (int i = 0; i < groupLayer.Count; i++)
            {
                var layer = groupLayer.Layer[i];
                if (ComImplementsInterface(layer, type))
                {
                    layers.Add(new NamedLayer
                    {
                        Name = namePrefix + layer.Name,
                        Index = indexPrefix + i,
                        Layer = layer
                    });
                    continue;
                }
                if (layer is IGroupLayer)
                {
                    SearchLayer(layer as ICompositeLayer, type, layers, indexPrefix + i + "-", namePrefix + layer.Name + "/");
                }
            }
        }

        private static bool ComImplementsInterface(object comObject, Type type)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.GetComInterfaceForObject(comObject, type);
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        #endregion
    }
}
