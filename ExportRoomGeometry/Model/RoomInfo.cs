using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ExportRoomGeometry.Model
{
    public class RoomInfo
    {
        public Room Room { get; set; }
        public string RoomName { get; set; }
        public string RoomLevel { get; set; }
        public string RoomNumber { get; set; }
        public double RoomHeight { get; set; }
        public XYZ RoomLocation { get; set; }
        public XYZ RoomStartPoint { get; set; }
        public XYZ RoomEndPoint { get; set; }
        public XYZ RoomAlongPoint { get; set; }
        public bool IsArc { get; set; }
        public List<RoomInfo> RoomBoundarySegment { get; set; } = new List<RoomInfo>();
    }
}
