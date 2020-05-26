using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ExportRoomGeometry.Model
{
    class Coordinates
    {
        public BuildingCoordinates GetBuildingCoordinate { get; set; }

        public Coordinates(string buildingName)
        {
            //["50UKT"] = new BuildingCoordinates { X = 1063.4884, Y = 25.404, Z = 0.000 } //Coordinates without wallShift
            var coordinate = new Dictionary<string, BuildingCoordinates>
            {
                ["50UKT"] = new BuildingCoordinates{X = 1062.8884, Y = 24.804, Z = 0.000},
                ["51UKT"] = new BuildingCoordinates{X = 1062.8884+86.212, Y = 24.804-65.304, Z = 0.000}
            };
            if (coordinate.ContainsKey(buildingName))
            {
                GetBuildingCoordinate = coordinate[buildingName];
            }
            else
            {
                GetBuildingCoordinate = new BuildingCoordinates { X = 0, Y = 0, Z = 0.000 };
            }

        }
    }

    class BuildingCoordinates
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
