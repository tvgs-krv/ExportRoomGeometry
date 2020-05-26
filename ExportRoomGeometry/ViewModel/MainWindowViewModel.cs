using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExportRoomGeometry.Model;

namespace ExportRoomGeometry.ViewModel
{
    enum Radio
    {
        ByBuilding,
        ByLevel
    }

    class MainWindowViewModel : ModelBase
    {
        internal RevitData RevitModel { get; set; }
        private string _buildingName;
        public string BuildingName
        {
            get => _buildingName;
            set
            {
                _buildingName = value;
                OnPropertyChanged(nameof(BuildingName));
            }
        }

        #region ExportButton

        public ICommand ExportCommand => new RelayCommandWithoutParameter(Export);

        private void Export()
        {

            var coord = new BuildingCoordinates { X = CoordinateXForBuilding, Y = CoordinateYForBuilding, Z = CoordinateZForBuilding };
            if (IsByBuilding)
            {
                RevitModel.ExportByBuilding(coord, IsEnableExport);
                MessageBox.Show("Экспорт помещений по зданию завершен!");
            }

            if (IsByLevel)
            {
                RevitModel.ExportByLevel(coord);
                MessageBox.Show("Экспорт помещений по уровням завершен!");
            }
        }


        #endregion

        #region CheckButton

        public ICommand CheckCommand => new RelayCommandWithoutParameter(Check);

        private void Check()
        {
            IsEnableExport = true;
            CheckDescription = RevitModel.CheckRooms();
        }

        private string _checkDescription;

        public string CheckDescription
        {
            get => _checkDescription;
            set
            {
                _checkDescription = value;
                OnPropertyChanged(nameof(CheckDescription));
            }
        }

        #endregion



        private bool _isEnabledExport;
        public bool IsEnableExport
        {
            get => _isEnabledExport;
            set
            {
                _isEnabledExport = value;
                OnPropertyChanged(nameof(IsEnableExport));
            }
        }

        private bool _isReportExport;
        public bool IsReportExport
        {
            get => _isReportExport;
            set
            {
                _isReportExport = value;
                OnPropertyChanged(nameof(IsReportExport));
            }
        }


        #region RadioButtons
        private Radio _defaultRadio = Radio.ByBuilding;
        public Radio Radio
        {
            get { return _defaultRadio; }
            set
            {
                if (_defaultRadio == value)
                    return;

                _defaultRadio = value;
                OnPropertyChanged("ByBuilding");
                OnPropertyChanged("ByLevel");
            }
        }
        public bool IsByBuilding
        {
            get => Radio == Radio.ByBuilding;
            set => Radio = value ? Radio.ByBuilding : Radio;
        }
        public bool IsByLevel
        {
            get => Radio == Radio.ByLevel;
            set => Radio = value ? Radio.ByLevel : Radio;
        }
        #endregion

        #region Coordinates

        private double _coordinateXForBuilding;

        public double CoordinateXForBuilding
        {
            get { return _coordinateXForBuilding; }
            set
            {
                _coordinateXForBuilding = value;
                OnPropertyChanged(nameof(CoordinateXForBuilding));

            }
        }

        private double _coordinateYForBuilding;

        public double CoordinateYForBuilding
        {
            get { return _coordinateYForBuilding; }
            set
            {
                _coordinateYForBuilding = value;
                OnPropertyChanged(nameof(CoordinateYForBuilding));

            }
        }

        private double _coordinateZForBuilding;

        public double CoordinateZForBuilding
        {
            get { return _coordinateZForBuilding; }
            set
            {
                _coordinateZForBuilding = value;
                OnPropertyChanged(nameof(CoordinateZForBuilding));

            }
        }

        #endregion

        public void ShowCoordinates(string buildingName)
        {
            if (!string.IsNullOrEmpty(buildingName) && RevitModel.GetBuildingName(buildingName)!=null)
            {
                var name = RevitModel.GetBuildingName(buildingName);
                MessageBox.Show(name);
                var coordinates = new Coordinates(name);
                var buildCoordinates = coordinates.GetBuildingCoordinate;
                CoordinateXForBuilding = buildCoordinates.X;
                CoordinateYForBuilding = buildCoordinates.Y;
                CoordinateZForBuilding = buildCoordinates.Z;
            }
            else
            {
                CoordinateXForBuilding = 0;
                CoordinateYForBuilding = 0;
                CoordinateZForBuilding = 0;

            }
        }



    }
}
