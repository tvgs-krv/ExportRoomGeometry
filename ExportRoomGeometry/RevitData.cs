using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace ExportRoomGeometry
{
    class RevitData
    {
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

    }
}
