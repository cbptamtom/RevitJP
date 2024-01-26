using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PresentationFilter.ViewModels;
using PresentationFilter.Views;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PresentationFilter.Command
{
    [Transaction(TransactionMode.Manual)]
    public class PrismRevitCmd : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            DIContainer.Instance.Register(doc);
            DocumentService.SetDocument(doc);

            TransactionGroup transGr = new TransactionGroup(doc);
            using (transGr)
            {
                transGr.Start("Do something");


                #region Main 
                DIContainer.Instance.Register(transGr);
                MainViewModel viewModel = new MainViewModel();
                MainWindow mainWindow = new MainWindow();
                mainWindow.DataContext = viewModel;
                if (mainWindow.ShowDialog() == false) return Result.Succeeded;
                transGr.Assimilate();
                #endregion
                return Result.Succeeded;

            }
        }


        private static List<FamilySymbol> GetAllFamilySymbol(Family family)
        {
            List<FamilySymbol> familySymbols = new List<FamilySymbol>();

            foreach (ElementId familySymbolId in family.GetFamilySymbolIds())
            {
                FamilySymbol familySymbol = family.Document.GetElement(familySymbolId) as FamilySymbol;
                familySymbols.Add(familySymbol);
            }

            return familySymbols;
        }

        public void DeleteAllParameterFilterElements(Autodesk.Revit.DB.Document doc)
        {
            // Lấy danh sách tất cả các ParameterFilterElements trong dự án
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> filterElements = collector.OfClass(typeof(ParameterFilterElement)).ToElements();
            using (Transaction transaction = new Transaction(doc, "Delete All Parameter Filter Elements"))
            {
                transaction.Start();

                foreach (Element filterElement in filterElements)
                {
                    doc.Delete(filterElement.Id);
                }

                transaction.Commit();
            }
        }

        CurveArray ConvertLoopToArray(CurveLoop loop)
        {
            CurveArray a = new CurveArray();
            foreach (Curve c in loop)
            {
                a.Append(c);
            }
            return a;
        }
        public PlanarFace GetTopFace(Solid solid)
        {
            PlanarFace topFace = null;
            FaceArray faces = solid.Faces;
            foreach (Face f in faces)
            {
                PlanarFace pf = f as PlanarFace;
                if (pf.FaceNormal.IsAlmostEqualTo(new XYZ(0, 0, 1)))
                {
                    topFace = pf;
                }
            }
            return topFace;
        }
        public PlanarFace GetBottom(Element element)
        {
            Solid a = GetSolidOneElement(element);
            FaceArray faceArray = a.Faces;
            List<PlanarFace> planarFaces = new List<PlanarFace>();
            foreach (var item in faceArray)
            {
                PlanarFace a1 = item as PlanarFace;
                if (a1 != null)
                {
                    if ((PointModel.AreEqual(a1.FaceNormal.AngleTo(XYZ.BasisZ), Math.PI)) || (PointModel.AreEqual(a1.FaceNormal.AngleTo(XYZ.BasisZ), 0)))
                    {
                        planarFaces.Add(a1);
                    }
                }
            }
            planarFaces = planarFaces.OrderBy(x => x.Origin.Z).ToList();
            PlanarFace bottom = planarFaces[0];
            return bottom;
        }
        public Solid GetSolidOneElement(Element element)
        {
            List<Solid> a = new List<Solid>();
            List<Solid> b = new List<Solid>();
            Solid c = null;
            Options options = new Options();
            options.ComputeReferences = true;
            GeometryElement geometryElement = element.get_Geometry(options) as GeometryElement;
            foreach (GeometryObject geometryObject in geometryElement)
            {
                Solid solid = geometryObject as Solid;
                if (solid != null)
                {
                    a.Add(solid);
                }
                else
                {
                    GeometryInstance geometryInstance = geometryObject as GeometryInstance;
                    GeometryElement geometryElement1 = geometryInstance.GetInstanceGeometry();
                    foreach (GeometryObject geometryObject1 in geometryElement1)
                    {
                        Solid solid1 = geometryObject1 as Solid;
                        if (solid1 != null)
                        {
                            a.Add(solid1);
                        }
                    }
                }
            }
            foreach (Solid item in a)
            {
                if (item.Volume != 0) { b.Add(item); }
            }
            if (b.Count == 1) { c = b[0]; } else { c = null; }
            return c;
        }
        public class PointModel
        {
            public static bool AreEqual(double firstValue, double secondValue, double tolerance = 1.0e-9)
            {
                return (secondValue - tolerance < firstValue && firstValue < secondValue + tolerance);
            }
            public static XYZ ProjectToPlane(XYZ po, PlanarFace p)
            {
                XYZ vecPoToPlaneOrigin = p.Origin - po;
                if (!(Math.Abs(vecPoToPlaneOrigin.DotProduct(p.FaceNormal)) < 1e-9))
                {

                    return po + p.FaceNormal * vecPoToPlaneOrigin.DotProduct(p.FaceNormal);
                }
                return po;
            }
            public static double DistanceTo2(PlanarFace plane, XYZ point, Autodesk.Revit.DB.Document document)
            {
                return Math.Abs(double.Parse(UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Length, (point - plane.Origin).DotProduct(plane.FaceNormal), false)));

            }

        }
    }
}
