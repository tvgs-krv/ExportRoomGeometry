using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ExportRoomGeometry
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
        public List<RoomInfo> RoomBoundarySegment { get; set; } = new List<RoomInfo>();
        

    }
}
