using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using ExportRoomGeometry.Abstractions;

namespace ExportRoomGeometry.Model
{
    class XmlStructure : CompareNames
    {

        public void XmlForBuilding(Document document, List<RoomInfo> roomInfoList, BuildingCoordinates buildingCoordinates, bool isReport)
        {
            var now = DateTime.Now.ToString("yyyy.MM.dd HH:mm");
            var dirs = new Directories();
            RoomInfo room = roomInfoList.First();
            string number = GetBuildingName(room.RoomNumber);
            string path = dirs.CreatePath(document, room.Room, number);
            //if (isReport)
            //{
            //    Debug.Listeners.Add(new TextWriterTraceListener($"{path}.log"));
            //}

            System.Diagnostics.Debug.WriteLine($"{now}\t[INFO]\tНачало выгрузки в XML по зданию для SmartPlant 3D");
            System.Diagnostics.Debug.WriteLine($"{now}\t[INFO]\tНаименование файла {path}.xml");
            XDocument xdoc = new XDocument();
            XNamespace ns = "urn:S3DIntegration";
            var objects = new XElement(ns + "Objects");
            System.Diagnostics.Debug.WriteLine($"{now}\t[INFO]\tСоздание структуры файла");
            var roomForExport = roomInfoList
                .Where(x => GetBuildingName(x.RoomNumber) != null)
                .Where(x => GetBuildingLevel(x.RoomNumber) != null);
            System.Diagnostics.Debug.WriteLine($"{now}\t[INFO]\tВсего помещений найдено {roomInfoList.Count} шт. Из них будет выгружено {roomForExport.Count()} шт.");
            foreach (var roomInfo in roomInfoList)
            {
                if (GetBuildingName(roomInfo.RoomNumber) != null && GetBuildingLevel(roomInfo.RoomNumber) != null)
                {
                    objects.Add(CAreaStructure(ns, roomInfo, buildingCoordinates));
                }
            }

            xdoc.Add(objects);
            System.Diagnostics.Debug.WriteLine($"{now}\t[INFO]\tСохранение файла {path}.xml");
            xdoc.Save($"{path}.xml");

            //Debug.Flush();

        }

        public void XmlForLevels(Document document, List<RoomInfo> roomInfoList, BuildingCoordinates buildingCoordinates)
        {
            var groupByLevel = roomInfoList.GroupBy(r => r.RoomLevel);

            foreach (var element in groupByLevel)
            {
                var dirs = new Directories();

                XDocument xdoc = new XDocument();
                XNamespace ns = "urn:S3DIntegration";
                var objects = new XElement(ns + "Objects");
                var levelName = element.Key;
                foreach (RoomInfo roomInfo in element)
                {
                    if (GetBuildingName(roomInfo.RoomNumber) != null && GetBuildingLevel(roomInfo.RoomNumber) != null)
                    {
                        var cAreaStruct = CAreaStructure(ns, roomInfo, buildingCoordinates);
                        objects.Add(cAreaStruct);
                    }
                }
                xdoc.Add(objects);
                RoomInfo room = roomInfoList.First();
                string number = GetBuildingName(room.RoomNumber);
                string path = dirs.CreatePath(document, $"{number}_{levelName}");
                xdoc.Save(path);
            }
        }

        private XElement CAreaStructure(XNamespace ns, RoomInfo roomInfo, BuildingCoordinates buildingCoordinates)
        {
            var carea = new XElement(ns + "CArea", new XAttribute("PartNumber", "SPACE_DEF_SA01"));
            #region EIDS
            var eids = new XElement(ns + "EIDs");
            var eid = new XElement(ns + "EID", Guid.NewGuid().ToString().ToUpper() + "_" + roomInfo.Room.Id);
            eids.Add(eid);
            carea.Add(eids);
            #endregion

            #region Name
            carea.Add(new XElement(ns + "Name", roomInfo.RoomNumber));
            #endregion

            #region SpaceParents
            var spaceParents = new XElement(ns + "SpaceParents");
            spaceParents.Add(SpaceParentTag(ns, "RoomManagement"));
            spaceParents.Add(SpaceParentTag(ns, GetBuildingName(roomInfo.RoomNumber)));
            spaceParents.Add(SpaceParentTag(ns, GetBuildingLevel(roomInfo.RoomNumber)));
            carea.Add(spaceParents);
            #endregion

            #region Path
            var pathElement = new XElement(ns + "Path");
            var getHeight = roomInfo.RoomHeight;
            pathElement.Add(Line3dTag(ns,
                roomInfo.RoomLocation,
                new XYZ(roomInfo.RoomLocation.X, roomInfo.RoomLocation.Y, roomInfo.RoomLocation.Z + getHeight), buildingCoordinates));
            carea.Add(pathElement);
            #endregion

            #region Contour
            var contour = new XElement(ns + "Contour");
            foreach (RoomInfo segments in roomInfo.RoomBoundarySegment)
            {
                if (segments.IsArc)
                {
                    var arc3dTag = Arc3dTag(ns, segments.RoomStartPoint, segments.RoomEndPoint, segments.RoomAlongPoint, buildingCoordinates);
                    contour.Add(arc3dTag);
                }
                else
                {
                    var line3dTag = Line3dTag(ns, segments.RoomStartPoint, segments.RoomEndPoint, buildingCoordinates);
                    contour.Add(line3dTag);

                }
            }
            carea.Add(contour);
            #endregion
            return carea;
        }

        private XElement Line3dTag(XNamespace ns, XYZ startPointXyz, XYZ endPointXyz, BuildingCoordinates buildingCoordinates)
        {
            //var wallXShift = 0.6;
            //var wallYShift = 0.6;

            #region 50UKT
            //var pX = 1052.10008;
            //var pX = 1049.10000+12.1+2.2564-0.050+0.882-0.8; //‭1063,4884
            //var pX = 1063.4884; //‭1063,4884
            //var pY = 25.404; //25.404
            //var pY = 27.400-1.848-0.148; //25.404
            #endregion

            #region 50UKT
            //var pX = 1063.4884;
            //var pY = 25.404;
            var pX = buildingCoordinates.X;
            var pY = buildingCoordinates.Y;
            var pZ = buildingCoordinates.Z;
            #endregion
            if (startPointXyz != null && endPointXyz != null)
            {
                var startX = TextFormat(ConvertForExport(startPointXyz.X) + pX);
                var startY = TextFormat(ConvertForExport(startPointXyz.Y) + pY);
                var startZ = TextFormat(ConvertForExport(startPointXyz.Z) + pZ);
                var endX = TextFormat(ConvertForExport(endPointXyz.X) + pX);
                var endY = TextFormat(ConvertForExport(endPointXyz.Y) + pY);
                var endZ = TextFormat(ConvertForExport(endPointXyz.Z) + pZ);

                XElement line3d = new XElement(ns + "Line3d");
                var startPoint = new XElement(ns + "StartPoint", new XAttribute("X", startX), new XAttribute("Y", startY), new XAttribute("Z", startZ));
                var endPoint = new XElement(ns + "EndPoint", new XAttribute("X", endX), new XAttribute("Y", endY), new XAttribute("Z", endZ));
                line3d.Add(startPoint);
                line3d.Add(endPoint);
                return line3d;
            }
            return null;
        }

        private XElement Arc3dTag(XNamespace ns, XYZ startPointXyz, XYZ endPointXyz, XYZ alongPointXyz,
            BuildingCoordinates buildingCoordinates)
        {
            var pX = buildingCoordinates.X;
            var pY = buildingCoordinates.Y;
            var pZ = buildingCoordinates.Z;
            if (startPointXyz != null && endPointXyz != null && alongPointXyz != null)
            {
                var startX = TextFormat(ConvertForExport(startPointXyz.X) + pX);
                var startY = TextFormat(ConvertForExport(startPointXyz.Y) + pY);
                var startZ = TextFormat(ConvertForExport(startPointXyz.Z) + pZ);
                var endX = TextFormat(ConvertForExport(endPointXyz.X) + pX);
                var endY = TextFormat(ConvertForExport(endPointXyz.Y) + pY);
                var endZ = TextFormat(ConvertForExport(endPointXyz.Z) + pZ);

                var alongX = TextFormat(ConvertForExport(alongPointXyz.X) + pX);
                var alongY = TextFormat(ConvertForExport(alongPointXyz.Y) + pY);
                var alongZ = TextFormat(ConvertForExport(alongPointXyz.Z) + pZ);


                XElement arc3dTag = new XElement(ns + "Arc3d");
                var startPoint = new XElement(ns + "StartPoint", new XAttribute("X", startX), new XAttribute("Y", startY), new XAttribute("Z", startZ));
                var alongPoint = new XElement(ns + "AlongPoint", new XAttribute("X", alongX), new XAttribute("Y", alongY), new XAttribute("Z", alongZ));
                var endPoint = new XElement(ns + "EndPoint", new XAttribute("X", endX), new XAttribute("Y", endY), new XAttribute("Z", endZ));
                arc3dTag.Add(startPoint);
                arc3dTag.Add(alongPoint);
                arc3dTag.Add(endPoint);
                return arc3dTag;
            }
            return null;

        }

        private XElement SpaceParentTag(XNamespace ns, string value)
        {
            var spaceParent = new XElement(ns + "SpaceParent");
            var spaceParentName = new XElement(ns + "Name", value);
            spaceParent.Add(spaceParentName);
            return spaceParent;
        }

        private double ConvertForExport(double digit)
        {
            var round = Math.Round(digit, 3);
            var convertToMm = round * 304.8 / 1000;
            return convertToMm;
        }

        private string TextFormat(double digit)
        {
            return $"{digit:0.000}".Replace(",", ".");
        }

    }
}
