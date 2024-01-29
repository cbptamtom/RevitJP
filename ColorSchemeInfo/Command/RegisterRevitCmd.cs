using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ColorSchemeInfo.Command
{
    [Transaction(TransactionMode.Manual)]
    public class RegisterRevitCmd : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            TransactionGroup transGr = new TransactionGroup(doc);
            using (transGr)
            {
                transGr.Start("Do something");


                #region Main 
                XYZ pickObject = uidoc.Selection.PickPoint();
                CreateFilledRegionWithText(doc, pickObject);
                //Tạo ra 1 nhóm bao gồm filled + text
                ////////// Đổi màu và tên của nó 
                ///di chuyển nó xuống 




                transGr.Assimilate();
                #endregion
                return Result.Succeeded;

            }

        }


        public void CreateFilledRegionWithText(Document document, XYZ pickPoint)
        {
            // Lấy loại filled region từ tên
            FilteredElementCollector collector = new FilteredElementCollector(document);
            Element filledRegionType = collector.OfClass(typeof(FilledRegionType))
                .Cast<FilledRegionType>()
                .FirstOrDefault(type => type.Name == "Solid_Black");

            if (filledRegionType == null)
            {
                return;
            }

            double widthInMM = 600; // Kích thước 600mm
            double heightInMM = 400; // Chiều cao 300mm
            double spacingBetweenFilledRegions = 300; // Khoảng cách giữa các FilledRegion
            double textNoteOffset = 200; // Khoảng cách giữa FilledRegion và TextNote

            double widthInInch = UnitUtils.ConvertToInternalUnits(widthInMM, document.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId());
            double heightInInch = UnitUtils.ConvertToInternalUnits(heightInMM, document.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId());
            double spacingInInch = UnitUtils.ConvertToInternalUnits(spacingBetweenFilledRegions, document.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId());
            double textNoteOffsetInInch = UnitUtils.ConvertToInternalUnits(textNoteOffset, document.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId());
            // Lấy ActiveView
            View activeView = document.ActiveView;

            // Lấy danh sách View Filters và màu sắc tương ứng
            List<ElementId> filterIds = activeView.GetFilters().ToList();
            List<Color> listColors = filterIds
                .Select(filterId => activeView.GetFilterOverrides(filterId)?.SurfaceForegroundPatternColor ?? new Color(0, 0, 0))
                .ToList();

            // Lấy danh sách tên filters tương ứng
            List<string> filterNames = filterIds
                .Select(filterId =>
                    (document.GetElement(filterId) as FilterElement)?.Name)
                .ToList();
            // Gắn filled region và áp dụng Element Overrides cho mỗi Filter
            using (Transaction transaction = new Transaction(document, "Create Filled Region with Text"))
            {
                transaction.Start();

                XYZ startPoint = new XYZ(pickPoint.X, pickPoint.Y, 0);

                for (int i = 0; i < filterIds.Count; i++)
                {
                    CurveLoop boundary = new CurveLoop();

                    boundary.Append(Line.CreateBound(startPoint, new XYZ(startPoint.X + widthInInch, startPoint.Y, 0)));
                    boundary.Append(Line.CreateBound(new XYZ(startPoint.X + widthInInch, startPoint.Y, 0), new XYZ(startPoint.X + widthInInch, startPoint.Y + heightInInch, 0)));
                    boundary.Append(Line.CreateBound(new XYZ(startPoint.X + widthInInch, startPoint.Y + heightInInch, 0), new XYZ(startPoint.X, startPoint.Y + heightInInch, 0)));
                    boundary.Append(Line.CreateBound(new XYZ(startPoint.X, startPoint.Y + heightInInch, 0), startPoint));

                    FilledRegion fillResult = FilledRegion.Create(document, filledRegionType.Id, activeView.Id, new List<CurveLoop> { boundary });

                    OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();
                    FillPatternElement filled = GetFillPatternElements(document);

                    // Lấy màu sắc từ danh sách
                    Color color = listColors[i];

                    // Áp dụng Element Overrides cho FilledRegion tương ứng
                    SetElementOverride(activeView, fillResult, filled, color);

                    // Lấy tên của Filter
                    string filterName = filterNames[i];

                    // Tạo TextNote với các thông số cần thiết
                    double noteWidth = .3;

                    // make sure note width works for the text type
                    ElementId defaultTextTypeId = document.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);

                    double minWidth = TextNote.GetMinimumAllowedWidth(document, defaultTextTypeId);
                    double maxWidth = TextNote.GetMaximumAllowedWidth(document, defaultTextTypeId);
                    if (noteWidth < minWidth)
                    {
                        noteWidth = minWidth;
                    }
                    else if (noteWidth > maxWidth)
                    {
                        noteWidth = maxWidth;
                    }

                    string desiredTextTypeName = "3.0mm"; // Thay thế bằng tên mong muốn
                    Element textNoteType = new FilteredElementCollector(document)
                        .OfClass(typeof(TextNoteType))
                        .Cast<TextNoteType>()
                        .FirstOrDefault(t => t.Name == desiredTextTypeName);

                    if (textNoteType != null)
                    {
                        // Lấy giá trị Y từ startPoint và sử dụng nó khi tạo textNotePosition
                        double textNoteY = startPoint.Y + heightInInch;
                        XYZ textNotePosition = new XYZ(startPoint.X + widthInInch + textNoteOffsetInInch, textNoteY, 0);

                        TextNoteOptions opts = new TextNoteOptions(defaultTextTypeId);
                        opts.TypeId = textNoteType.Id;
                        opts.HorizontalAlignment = HorizontalTextAlignment.Left;

                        TextNote textNote = TextNote.Create(document, activeView.Id, textNotePosition, noteWidth, filterName, opts);

                        // Cập nhật startPoint cho lần lặp tiếp theo
                        startPoint = new XYZ(startPoint.X, startPoint.Y - (heightInInch + spacingInInch), 0);
                    }
                }

                transaction.Commit();
            }

        }

        private FillPatternElement GetFillPatternElements(Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().FirstOrDefault(v => v.Name.Equals("<Solid fill>"));
        }
        private void SetElementOverride(View view, FilledRegion paramFilter, FillPatternElement fillPattern, Color color)
        {

            OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();
            overrideGraphicSettings.SetSurfaceForegroundPatternId(fillPattern.Id);

            overrideGraphicSettings.SetSurfaceForegroundPatternColor(color);


            view.SetElementOverrides(paramFilter.Id, overrideGraphicSettings);
        }


    }
}
