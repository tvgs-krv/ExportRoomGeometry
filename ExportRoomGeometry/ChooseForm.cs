using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;

namespace ExportRoomGeometry
{
    public partial class ChooseForm : Form
    {
        private ExternalCommandData RevitCommandData { get; set; }
        public ChooseForm(ExternalCommandData commandData)
        {
            InitializeComponent();
            RevitCommandData = commandData;

        }

        private void button1_Click(object sender, EventArgs e)
        {

            var app = RevitCommandData.Application;
            var uidoc = app.ActiveUIDocument;
            Document document = uidoc.Document;
            if (radioButton1.Checked)
            {
                var rooms = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType();
                XDocument xdoc = new XDocument();
                XNamespace ns = "urn:S3DIntegration";
                var objects = new XElement(ns + "Objects");
                foreach (var element in rooms)
                {
                    if (element == null) continue;
                    var r = (Room)element;
                    if (r.Area > 0)
                    {
                        objects.Add(CAreaStructure(ns, document, r));

                    }
                }

                xdoc.Add(objects);
                var getBuidName = (Room)rooms.FirstElement();

                string path = CreatePath(document, getBuidName);
                xdoc.Save(path);
            }

            if (radioButton2.Checked)
            {
                var rooms = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .Select(r => r as Room)
                    .Where(r => r != null)
                    .Where(r => r.Area > 0)
                    .GroupBy(r => r.Level.Name);

                string mes = String.Empty;
                foreach (var element in rooms)
                {
                    XDocument xdoc = new XDocument();
                    XNamespace ns = "urn:S3DIntegration";
                    var objects = new XElement(ns + "Objects");
                    var levelName = element.Key;
                    mes += levelName + "\n";
                    foreach (Room room in element)
                    {
                        mes += "\t" + room.Number + "\n";
                        objects.Add(CAreaStructure(ns, document, room));
                    }
                    xdoc.Add(objects);
                    var getBuidName = GetBuildingName(element.First().Number);
                    string path = CreatePath(document, $"{getBuidName}_{levelName}");
                    xdoc.Save(path);
                }

            }


            Close();
        }


        private RoomInfo Getinfo_Room(Document document, Room room)
        {
            if (room != null)

            {
                var info = new RoomInfo();
                info.Room = room;
                info.RoomName = room.Name;
                info.RoomLevel = room.Level.Name;
                info.RoomNumber = room.Number;

                if (room.Location is LocationPoint location)
                {
                    XYZ point = location.Point;
                    info.RoomLocation = point;
                }

                #region GetHeight
                SpatialElementBoundaryOptions sebOptions = new SpatialElementBoundaryOptions
                {
                    SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish
                };
                SpatialElementGeometryCalculator calc = new SpatialElementGeometryCalculator(document, sebOptions);
                SpatialElementGeometryResults results = calc.CalculateSpatialElementGeometry(room);
                Solid roomSolid = results.GetGeometry();
                var getbb = roomSolid.GetBoundingBox();
                var maxZ = getbb.Max.Z;
                var minZ = getbb.Min.Z;
                info.RoomHeight = maxZ - minZ;
                #endregion

                IList<IList<BoundarySegment>> segments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());

                if (null != segments)
                {
                    foreach (IList<BoundarySegment> segmentList in segments)
                    {
                        foreach (BoundarySegment boundarySegment in segmentList)
                        {
                            if (boundarySegment.GetCurve() is Line)
                            {
                                info.RoomBoundarySegment.Add(new RoomInfo
                                {
                                    RoomStartPoint = boundarySegment.GetCurve().GetEndPoint(0),
                                    RoomEndPoint = boundarySegment.GetCurve().GetEndPoint(1)
                                });

                            }
                            if (boundarySegment.GetCurve() is Arc)
                            {

                                TaskDialog.Show("s", boundarySegment.GetCurve().GetType().ToString());
                            }
                        }
                    }
                }

                return info;

            }

            return null;
        }

        public XElement CAreaStructure(XNamespace ns, Document document, Room room)
        {
            var roomInfo = Getinfo_Room(document, room);
            Random rnd = new Random();
            int value = rnd.Next(10000, 99999);

            var carea = new XElement(ns + "CArea", new XAttribute("PartNumber", "SPACE_DEF_SA01"));
            //EIDS
            var eids = new XElement(ns + "EIDs");
            var eid = new XElement(ns + "EID", Guid.NewGuid().ToString().ToUpper() + "_" + value);
            eids.Add(eid);
            carea.Add(eids);
            //Name
            XElement name;
            //name = roomInfo.RoomNumber != null ? new XElement(ns + "Name", roomInfo.RoomNumber) : new XElement(ns + "Name", "NotFoundNumber");
            if (!string.IsNullOrEmpty(roomInfo.RoomNumber))
                name = new XElement(ns + "Name", roomInfo.RoomNumber);
            else
                name = new XElement(ns + "Name", "NotFoundNumber");


            carea.Add(name);
            //SpaceParents
            var spaceParents = new XElement(ns + "SpaceParents");
            spaceParents.Add(SpaceParentTag(ns, "RoomManagement"));
            spaceParents.Add(SpaceParentTag(ns, GetBuildingName(roomInfo.RoomNumber)));
            spaceParents.Add(SpaceParentTag(ns, GetBuildingLevel(roomInfo.RoomNumber)));
            carea.Add(spaceParents);
            //Path
            var pathElement = new XElement(ns + "Path");
            //var getHeight = roomInfo.Room.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsDouble();
            var getHeight = roomInfo.RoomHeight;
            pathElement.Add(Line3dTag(ns, roomInfo.RoomLocation, new XYZ(roomInfo.RoomLocation.X, roomInfo.RoomLocation.Y, roomInfo.RoomLocation.Z + getHeight)));
            carea.Add(pathElement);
            //Contour
            var contour = new XElement(ns + "Contour");
            foreach (var segments in roomInfo.RoomBoundarySegment)
            {
                var line3dTag = Line3dTag(ns, segments.RoomStartPoint, segments.RoomEndPoint);
                contour.Add(line3dTag);
            }
            carea.Add(contour);
            return carea;
        }

        private XElement Line3dTag(XNamespace ns, XYZ startPointXyz, XYZ endPointXyz)
        {
            var wallXShift = 0.6;
            var wallYShift = 0.6;

            #region 50UKT
            //var pX = 1052.10008;
            //var pX = 1049.10000+12.1+2.2564-0.050+0.882-0.8; //‭1063,4884
            //var pX = 1063.4884; //‭1063,4884
            //var pY = 25.404; //25.404
            //var pY = 27.400-1.848-0.148; //25.404
            #endregion

            #region 51UKT
            var pX = 1063.4884;
            var pY = 25.404;
            #endregion
            if (startPointXyz != null && endPointXyz != null)
            {
                var startX = MathRound2(ConvertToExport(startPointXyz.X) + pX - wallXShift);
                var startY = MathRound2(ConvertToExport(startPointXyz.Y) + pY - wallYShift);
                var startZ = MathRound(startPointXyz.Z);
                var endX = MathRound2(ConvertToExport(endPointXyz.X) + pX - wallXShift);
                var endY = MathRound2(ConvertToExport(endPointXyz.Y) + pY - wallYShift);
                var endZ = MathRound(endPointXyz.Z);

                XElement line3d = new XElement(ns + "Line3d");
                var startPoint = new XElement(ns + "StartPoint", new XAttribute("X", startX), new XAttribute("Y", startY), new XAttribute("Z", startZ));
                var endPoint = new XElement(ns + "EndPoint", new XAttribute("X", endX), new XAttribute("Y", endY), new XAttribute("Z", endZ));
                line3d.Add(startPoint);
                line3d.Add(endPoint);
                return line3d;
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

        private string MathRound(double digit)
        {
            var convertToMm = digit * 304.8 / 1000;
            var round = Math.Round(convertToMm, 3);
            return $"{round:0.000}".Replace(",", ".");
        }

        private double ConvertToExport(double digit)
        {
            var round = Math.Round(digit, 3);
            var convertToMm = round * 304.8 / 1000;
            return convertToMm;
        }

        private string MathRound2(double digit)
        {
            return $"{digit:0.000}".Replace(",", ".");
        }



        private string GetBuildingName(string activeName)
        {
            if (!string.IsNullOrEmpty(activeName))
            {
                Regex regex = new Regex(@"[0-9][0-9][A-Z][A-Z][A-Z]");
                string mc = regex.Matches(activeName).OfType<Match>().ToList().Select(a => a.Value).FirstOrDefault();
                if (mc != null)
                    return mc;
            }
            return "NotFoundName";

        }

        private string GetBuildingLevel(string activeName)
        {
            if (!string.IsNullOrEmpty(activeName))
            {
                Regex regex = new Regex(@"[0-9][0-9][A-Z][A-Z][A-Z][0-9][0-9]");
                string mc = regex.Matches(activeName).OfType<Match>().ToList().Select(a => a.Value).FirstOrDefault();
                if (mc != null)
                    return mc;
            }
            return "NotFoundLevel";

        }

        private string CreatePath(Document document, Room room)
        {
            if (room != null)
            {
                var getDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                var path = Directory.CreateDirectory($"{getDesktop}\\Revit_Export\\{document.Title}").FullName;

                return $"{path}\\{GetBuildingName(room.Number)}.xml";
            }

            return null;
        }

        private string CreatePath(Document document, string room)
        {
            if (room != null)
            {
                var getDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                var path = Directory.CreateDirectory($"{getDesktop}\\Revit_Export\\{document.Title}").FullName;

                return $"{path}\\{room}.xml";
            }

            return null;
        }

        private void Checked_Click(object sender, EventArgs e)
        {
            var app = RevitCommandData.Application;
            var uidoc = app.ActiveUIDocument;
            Document document = uidoc.Document;
            richTextBox1.AppendText($"{DateTime.Now}\t Проверка...\nNumber (Номер помещения) отсутствует в след. помещениях\n");
            var rooms = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Select(r => r as Room)
                .Where(r => r != null)
                .Where(r => r.Location != null)
                .Where(r => !r.Area.Equals(0));

            foreach (var room in rooms)
            {
                if (string.IsNullOrEmpty(room.Number))
                {
                    var res = $"ID {room.Id}\n";
                    richTextBox1.AppendText(res);

                }
            }

        }
    }
}
