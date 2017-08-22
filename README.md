# DNRGPS-ArcMapIntegration

This is a component of the DNR-GPS application (formerly DNR-Garmin).
http://www.dnr.state.mn.us/mis/gis/DNRGPS/DNRGPS.html.
It supports integration with ArcMap.

DNR-GPS uses interprocess communication to get and send data to a running
ArcMap session.
This was standard behavior in DNR-Garmin and ArcMap 9.x.  This tool was designed
to replicate that functionality in 10.x.  However, starting with v10.0, interprocess communication became more difficult and less stable.  It seems that this was
not a supported operation with Esri.  By version 10.4 it stopped working all
together.

This is a copy of the original source code hosted on assembla (http://www.assembla.com/spaces/dnrgps)
