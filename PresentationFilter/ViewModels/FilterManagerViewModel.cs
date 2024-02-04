
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PresentationFilter.Models;
using PresentationFilter.Utilities;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PresentationFilter.ViewModels
{
    public class FilterManagerViewModel : BindableBase
    {
        private TransactionGroup _transactionGroup;
        private Document _document;

        private List<Element> _structuralFramingElements;
        public List<Element> StructuralFramingElements
        {
            get { return _structuralFramingElements; }
            set { SetProperty(ref _structuralFramingElements, value); }
        }

        private List<Element> _floors;
        public List<Element> Floors
        {
            get { return _floors; }
            set { SetProperty(ref _floors, value); }
        }

        private List<Element> _foundation;
        public List<Element> Foundation
        {
            get { return _foundation; }
            set { SetProperty(ref _foundation, value); }
        }

        private ObservableCollection<FilterModel> _FilterModel = new ObservableCollection<FilterModel>();
        public ObservableCollection<FilterModel> FilterModel
        {
            get { return _FilterModel; }
            set { SetProperty(ref _FilterModel, value); }
        }
        private List<string> checker = new List<string>();


        public ICommand InitFilterCommand { get; private set; }

        public FilterManagerViewModel()
        {
            _transactionGroup = DIContainer.Instance.Resolve<TransactionGroup>();
            _document = DIContainer.Instance.Resolve<Document>();
            PopulateFilterFramingElevation();
            PopulateFilterFramingMark();
            PopulateFilterFloorWithElevation();
            PopulateFilterFloorWithMark();
            PopulateFilterFoundationWithElevation();
            PopulateFilterFoundationWithMark();
            InitFilterCommand = new DelegateCommand(ProcessFilterModels);

        }
        private void PopulateFilterFramingElevation()
        {
            StructuralFramingElements = DocumentService.GetAllStructuralFramingWithEqualOffsets();
            foreach (Element element in StructuralFramingElements)
            {
                string offsetValue = element.LookupParameter("Start Level Offset").AsValueString();
                double _tempValue = double.Parse(offsetValue);
                string offsetString = _tempValue == 0 ? $"±{offsetValue}" : (_tempValue > 0 ? $"+{offsetValue}" : offsetValue);
                string filterName = $"梁レベル/Structual Framing Level_FL {offsetString}";

                if (!checker.Contains(filterName))
                {
                    FilterModel.Add(new FilterModel
                    {
                        Element = element,
                        Category = element.Category,
                        CategoryName = element.Category.Name,
                        Name = element.Name,
                        FilterName = filterName,
                        OffsetValue = offsetValue,
                        FilterRule = "Start Level Offset",
                        Elevation = element.LookupParameter("Start Level Offset").AsDouble()

                    });
                    checker.Add(filterName);
                }
            }
        }
        private void PopulateFilterFramingMark()
        {
            StructuralFramingElements = DocumentService.GetAllStructuralFramingWithEqualOffsets();
            foreach (Element element in StructuralFramingElements)
            {
                string parameterValue = DocumentService.GetParameterValueFromSymbol(element, "符号");
                string offsetValue = element.LookupParameter("Start Level Offset").AsValueString();
                double _tempValue = double.Parse(offsetValue);
                string offsetString = _tempValue == 0 ? $"±{offsetValue}" : (_tempValue > 0 ? $"+{offsetValue}" : offsetValue);
                string filterName = $"梁符号/Structual Framing Mark_{parameterValue}";
                if (!checker.Contains(filterName))
                {
                    FilterModel.Add(new FilterModel
                    {
                        Element = element,
                        Category = element.Category,
                        CategoryName = element.Category.Name,
                        Name = parameterValue,
                        FilterName = filterName,
                        OffsetValue = offsetValue,
                        FilterRule = "符号",
                        Elevation = element.LookupParameter("Start Level Offset").AsDouble()
                    });
                    checker.Add(filterName);
                }

            }
        }
        private void PopulateFilterFloorWithElevation()
        {
            Floors = DocumentService.GetAllFloorsWithEqualOffsets();

            foreach (Element element in Floors)
            {
                string offsetValue = element.LookupParameter("Height Offset From Level").AsValueString();
                double _tempValue = double.Parse(offsetValue);
                string offsetString = _tempValue == 0 ? $"±{offsetValue}" : (_tempValue > 0 ? $"+{offsetValue}" : offsetValue);
                string filterName = $"スラブレベル/Slab Level_FL {offsetString}";

                if (!checker.Contains(filterName))
                {
                    FilterModel.Add(new FilterModel
                    {

                        Element = element,
                        Category = element.Category,
                        CategoryName = element.Category.Name,
                        Name = element.Name,
                        FilterName = filterName,
                        OffsetValue = offsetValue,
                        Elevation = element.LookupParameter("Height Offset From Level").AsDouble(),
                        FilterRule = "Height Offset From Level"

                    });
                    checker.Add(filterName);
                }
            }
        }
        private void PopulateFilterFloorWithMark()
        {
            Floors = DocumentService.GetAllFloorsWithEqualOffsets();

            foreach (Element element in Floors)
            {
                string parameterValue = DocumentService.GetParameterValueFromSymbol(element, "符号");
                if (!string.IsNullOrEmpty(parameterValue))
                {
                    string offsetValue = element.LookupParameter("Height Offset From Level").AsValueString();
                    double _tempValue = double.Parse(offsetValue);
                    string offsetString = _tempValue == 0 ? $"±{offsetValue}" : (_tempValue > 0 ? $"+{offsetValue}" : offsetValue);
                    string filterName = $"スラブ符号/Slab Mark_{parameterValue}";

                    if (!checker.Contains(filterName))
                    {
                        FilterModel.Add(new FilterModel
                        {
                            Element = element,
                            Category = element.Category,
                            CategoryName = element.Category.Name,
                            Name = parameterValue,
                            FilterName = filterName,
                            OffsetValue = offsetValue,
                            FilterRule = "符号",


                        });
                        checker.Add(filterName);
                    }
                }
            }
        }
        private void PopulateFilterFoundationWithElevation()
        {
            Foundation = DocumentService.GetAllStructuralFoundations();

            foreach (Element element in Foundation)
            {
                string offsetValue = element.LookupParameter("Height Offset From Level").AsValueString();
                double _tempValue = double.Parse(offsetValue);
                string offsetString = _tempValue == 0 ? $"±{offsetValue}" : (_tempValue > 0 ? $"+{offsetValue}" : offsetValue);
                string filterName = $"基礎レベル/Foundation Level_{offsetString}";

                if (!checker.Contains(filterName))
                {
                    FilterModel.Add(new FilterModel
                    {
                        Element = element,
                        Category = element.Category,
                        CategoryName = element.Category.Name,
                        Name = element.Name,
                        FilterName = filterName,
                        OffsetValue = offsetValue,
                        Elevation = element.LookupParameter("Height Offset From Level").AsDouble(),
                        FilterRule = "Height Offset From Level"


                    });
                    checker.Add(filterName);
                }
            }
        }
        private void PopulateFilterFoundationWithMark()
        {
            Foundation = DocumentService.GetAllStructuralFoundations();

            foreach (Element element in Foundation)
            {
                string parameterValue = DocumentService.GetParameterValueFromSymbol(element, "符号");
                string offsetValue = element.LookupParameter("Height Offset From Level").AsValueString();
                double _tempValue = double.Parse(offsetValue);
                string offsetString = _tempValue == 0 ? $"±{offsetValue}" : (_tempValue > 0 ? $"+{offsetValue}" : offsetValue);
                string filterName = $"基礎符号/Foundation Mark_{parameterValue}";

                if (!checker.Contains(filterName))
                {
                    FilterModel.Add(new FilterModel
                    {
                        Element = element,
                        Category = element.Category,
                        CategoryName = element.Category.Name,
                        Name = parameterValue,
                        FilterName = filterName,
                        OffsetValue = offsetValue,
                        FilterRule = "符号",

                    });
                    checker.Add(filterName);
                }
            }
        }
        private void ProcessFilterModels()
        {
            try
            {
                if (!_transactionGroup.HasStarted())
                {
                    return;
                }

                // Sử dụng danh sách để theo dõi FilterName đã được tạo và các FilterName đã tồn tại
                List<string> createdFilterNames = new List<string>();
                List<string> existingFilterNames = new List<string>();

                var filteredFilterModels = FilterModel
.Where(filterModel => filterModel.FilterRule == "符号")
                    .ToList();

                var filterModelsToCreateStartLevelOffset = FilterModel
.Where(filterModel => filterModel.FilterRule == "Start Level Offset")
                    .ToList();

                var filterModelsToCreateHeightOffsetFromLevel = FilterModel
.Where(filterModel => filterModel.FilterRule == "Height Offset From Level")
                    .ToList();


                CreateFiltersForFilterModels("符号", "Floors", "ee1f33e1-5506-4a64-b87b-7b98d30aea52", filteredFilterModels, createdFilterNames, existingFilterNames);
                CreateFiltersForFilterModels("符号", "Structural Framing", "ee1f33e1-5506-4a64-b87b-7b98d30aea52", filteredFilterModels, createdFilterNames, existingFilterNames);
                CreateFiltersForFilterModels("符号", "Structural Foundations", "ee1f33e1-5506-4a64-b87b-7b98d30aea52", filteredFilterModels, createdFilterNames, existingFilterNames);

                CreateFiltersForFilterModels("Start Level Offset", "Structural Framing", BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION, filterModelsToCreateStartLevelOffset, createdFilterNames, existingFilterNames);
                CreateFiltersForFilterModels("Height Offset From Level", "Floors", BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM, filterModelsToCreateHeightOffsetFromLevel, createdFilterNames, existingFilterNames);
                CreateFiltersForFilterModels("Height Offset From Level", "Structural Foundations", BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM, filterModelsToCreateHeightOffsetFromLevel, createdFilterNames, existingFilterNames);




                _transactionGroup.Commit();

                // Hiển thị thông báo sau khi hoàn thành vòng lặp
                TaskDialog taskDialog = new TaskDialog("Filter Creation Result");

                if (createdFilterNames.Count > 0)
                {
                    taskDialog.MainContent = "The following filters were created:";
                    foreach (string createdFilterName in createdFilterNames)
                    {
                        taskDialog.MainContent += $"\n- {createdFilterName}";
                    }
                    taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconInformation;
                }

                if (existingFilterNames.Count > 0)
                {
                    if (createdFilterNames.Count > 0)
                    {
                        taskDialog.ExpandedContent = "The following filters already exist and were skipped:";
                    }
                    else
                    {
                        taskDialog.ExpandedContent = "The following filters were skipped as they already exist:";
                    }

                    foreach (string existingFilterName in existingFilterNames)
                    {
                        taskDialog.ExpandedContent += $"\n- {existingFilterName}";
                    }
                    taskDialog.ExpandedContent += "\n\nNo filters were created.";
                    taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                }
                else
                {
                    if (createdFilterNames.Count == 0)
                    {
                        taskDialog.MainContent = "No filters were created.";
                        taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconInformation;
                    }
                }

                taskDialog.Show();

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "An error occurred: " + ex.Message);
                if (_transactionGroup.HasStarted())
                {
                    _transactionGroup.RollBack();
                }
            }
        }
        private void CreateFiltersForFilterModels(string filterRule, string categoryName, object sharedParameter, List<FilterModel> filterModels, List<string> createdFilterNames, List<string> existingFilterNames)
        {
            foreach (var filterModel in filterModels)
            {
                if (createdFilterNames.Contains(filterModel.FilterName))
                {
                    existingFilterNames.Add(filterModel.FilterName);
                }
                else
                {
                    try
                    {
                        if (sharedParameter is string sharedParameterGuid)
                        {
                            DocumentService.CreateViewFilterNameMarkSharedParameter(filterModel.FilterName, new List<string> { categoryName }, sharedParameterGuid, filterModel.Name);
                        }
                        else if (sharedParameter is BuiltInParameter sharedParameterEnum)
                        {
                            if (categoryName == "Structural Framing")
                            {
                                DocumentService.CreateViewFilterNameMarkSharedParameter(filterModel.FilterName, new List<string> { categoryName },
                                    filterModel.Element.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION), filterModel.Elevation,
                                    filterModel.Element.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION), filterModel.Elevation);
                            }
                            else if (categoryName == "Floors")
                            {
                                DocumentService.CreateViewFilterNameMarkSharedParameter(filterModel.FilterName, new List<string> { categoryName },
                                    filterModel.Element.get_Parameter(sharedParameterEnum), filterModel.Elevation,
                                    filterModel.Element.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP));
                            }
                            else
                            {
                                // For other categories, only add the specified shared parameter
                                DocumentService.CreateViewFilterNameMarkSharedParameter(filterModel.FilterName, new List<string> { categoryName }, filterModel.Element.get_Parameter(sharedParameterEnum), filterModel.Elevation);
                            }
                        }
                        createdFilterNames.Add(filterModel.FilterName);
                    }
                    catch (Exception)
                    {
                        existingFilterNames.Add(filterModel.FilterName);
                    }
                }
            }
        }





    }
}
