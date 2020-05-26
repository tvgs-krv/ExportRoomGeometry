using System;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ExportRoomGeometry.View;
using ExportRoomGeometry.ViewModel;

namespace ExportRoomGeometry.Model
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class Command : IExternalCommand
    {
        public static MainWindow MainWindow { get; private set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (MainWindow == null)
                {
                    string title = commandData.Application.ActiveUIDocument.Document.Title;
                    MainWindowViewModel mvvm = new MainWindowViewModel();
                    mvvm.RevitModel = new RevitData(commandData);
                    mvvm.BuildingName = title;
                    mvvm.ShowCoordinates(title);
                    MainWindow = new MainWindow { DataContext = mvvm };
                    MainWindow.Closed += (sender, args) => MainWindow = null;
                    MainWindow.ShowDialog();
                }
                else
                {
                    MainWindow.Activate();
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Revit error", e.ToString());
                return Result.Failed;
            }

            return Result.Succeeded;

        }



    }
}
