using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using ExportRoomGeometry.Abstractions;

namespace ExportRoomGeometry.Model
{
    class RevitData : CompareNames
    {
        private static ExternalCommandData _commandData;
        private static UIApplication _app;
        private static UIDocument _uidoc;
        private static Document _document;

        public RevitData(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _app = _commandData.Application;
            _uidoc = _app.ActiveUIDocument;
            _document = _uidoc.Document;
        }
        public string CheckRooms()
        {
            string checkedRooms = string.Empty;
            checkedRooms += $"{DateTime.Now}\t Проверка...\n";
            var rooms = new FilteredElementCollector(_document).OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Select(r => r as Room)
                .Where(r => r != null)
                .Where(r => r.Location != null)
                .Where(r => !r.Area.Equals(0));
            foreach (var room in rooms)
            {
                if (string.IsNullOrEmpty(room.Number))
                {
                    checkedRooms += $"ID {room.Id}\tПараметр \"НОМЕР\" не заполнен\n";
                }
                else
                {
                    if (GetBuildingName(room.Number) == null || GetBuildingLevel(room.Number) == null)
                    {
                        checkedRooms += $"ID {room.Id}\t{room.Number}\tДАННОЕ ПОМЕЩЕНИЕ БУДЕТ ИГНОРИРОВАНО ПРИ ЭКСПОРТЕ, " +
                                        $"ТАК КАК ОНО НЕ СООТВЕТСТВУЕТ СТРУКТУРЕ ВЫГРУЗКИ. ПРОВЕРЬТЕ ПРАВИЛЬНОСТЬ НАИМЕНОВАНИЯ НОМЕРА ПОМЕЩЕНИЯ(должно быть например 51UKT00R242)\n";
                    }
                    else
                    {
                        checkedRooms += $"ID {room.Id}\t{room.Number}\tOK\n";
                    }
                }
            }

            return checkedRooms;
        }

        public void ExportByBuilding(BuildingCoordinates buildingCoordinates, bool isReport)
        {
            var xml = new XmlStructure();
            xml.XmlForBuilding(_document,RoomInfoList(), buildingCoordinates, isReport);
        }

        public void ExportByLevel(BuildingCoordinates buildingCoordinates)
        {
            var xml = new XmlStructure();
            xml.XmlForLevels(_document,RoomInfoList(), buildingCoordinates);
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
                                info.RoomBoundarySegment.Add(new RoomInfo
                                {
                                    RoomStartPoint = boundarySegment.GetCurve().GetEndPoint(0),
                                    RoomAlongPoint = boundarySegment.GetCurve().Evaluate(0.5, true),
                                    RoomEndPoint = boundarySegment.GetCurve().GetEndPoint(1),
                                    IsArc = true
                                });
                            }
                        }
                    }
                }

                return info;

            }

            return null;
        }

        private List<RoomInfo> RoomInfoList()
        {
            var rooms = new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .OfType<Room>()
                .Where(r => true)
                .Where(r => r.Area > 0);
            var roomInfoList = new List<RoomInfo>();
            foreach (var room in rooms)
            {
                roomInfoList.Add(Getinfo_Room(_document, room));
            }

            return roomInfoList;
        }

    }
}
