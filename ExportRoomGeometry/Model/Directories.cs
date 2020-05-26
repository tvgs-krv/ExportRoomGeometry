using System;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ExportRoomGeometry.Model
{
    class Directories
    {
        public string CreatePath(Document document, Room room, string buildingName)
        {
            if (room != null)
            {
                var getDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                var path = Directory.CreateDirectory($"{getDesktop}\\Revit_Export\\{document.Title}").FullName;

                return $"{path}\\{buildingName}";
            }

            return null;
        }

        public string CreatePath(Document document, string roomNumber)
        {
            if (roomNumber != null)
            {
                var getDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                var path = Directory.CreateDirectory($"{getDesktop}\\Revit_Export\\{document.Title}").FullName;

                return $"{path}\\{roomNumber}.xml";
            }

            return null;
        }


    }
}
