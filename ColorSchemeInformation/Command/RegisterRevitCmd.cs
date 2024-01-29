using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ColorSchemeInformation.ViewModels;
using ColorSchemeInformation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ColorSchemeInformation.Command
{
    [Transaction(TransactionMode.Manual)]
    public class PrismRevitCmd : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            global::System.Windows.Forms.MessageBox.Show("Cái con cặt ");




            return Result.Succeeded;
        }
    }
}
