using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace ExportRoomGeometry
{
    [Transaction(TransactionMode.Manual)]

    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                ChooseForm form = new ChooseForm(commandData);
                form.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Revit error", e.ToString());
                return Result.Failed;
            }
        }



    }
}
