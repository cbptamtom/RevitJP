using Autodesk.Revit.DB;
using PresentationFilter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

public static class DocumentService
{
    private static Document _document;
    private static string _projectName;
    private static string _projectNumber;
    private static string _projectAddress;

    public static Document GetDocument()
    {
        return _document;
    }

    public static void SetDocument(Document document)
    {
        _document = document;
        // Sau khi thiết lập _document, bạn có thể lấy thông tin dự án và gán cho các biến static
        if (_document != null)
        {
            Autodesk.Revit.DB.ProjectInfo projectInfo = _document.ProjectInformation;
            _projectName = projectInfo.Name;
            _projectNumber = projectInfo.Number;
            _projectAddress = projectInfo.Address;
        }
    }

    public static string GetProjectName()
    {
        return _projectName ?? "No document or project information available.";
    }

    public static string GetProjectNumber()
    {
        return _projectNumber ?? "No document or project information available.";
    }

    public static string GetProjectAddress()
    {
        return _projectAddress ?? "No document or project information available.";
    }
    public static List<Element> GetAllStructuralFraming()
    {
        List<Element> Elements = new List<Element>();

        if (_document != null)
        {
            // Sử dụng FilteredElementCollector để truy vấn các dầm trong tài liệu
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                if (element is FamilyInstance familyInstance && familyInstance.Category.Name == "Structural Framing")
                {
                    Elements.Add(element);
                }
            }
        }

        return Elements;
    }
    public static List<Element> GetAllStructuralFramingWithEqualOffsets()
    {
        List<Element> Elements = new List<Element>();

        if (_document != null)
        {
            // Sử dụng FilteredElementCollector để truy vấn các dầm trong tài liệu
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                if (element is FamilyInstance familyInstance && familyInstance.Category.Name == "Structural Framing")
                {
                    // Kiểm tra Start Level Offset và End Level Offset
                    Parameter startOffsetParam = familyInstance.LookupParameter("Start Level Offset");
                    Parameter endOffsetParam = familyInstance.LookupParameter("End Level Offset");

                    if (startOffsetParam != null && endOffsetParam != null)
                    {
                        double startOffset = startOffsetParam.AsDouble();
                        double endOffset = endOffsetParam.AsDouble();

                        if (startOffset == endOffset)
                        {
                            Elements.Add(element);
                        }
                    }
                }
            }
        }
        return Elements;
    }

    public static List<Element> GetAllFloorsWithEqualOffsets()
    {
        List<Element> Elements = new List<Element>();

        if (_document != null)
        {
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            collector.OfClass(typeof(Floor)).WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                if (element is Floor floor)
                {
                    Elements.Add(element);
                }
            }
        }

        return Elements;
    }

    public static List<Element> GetAllStructuralFoundations()
    {
        List<Element> foundationElements = new List<Element>();

        if (_document != null)
        {
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                if (element is FamilyInstance familyInstance && familyInstance.Category.Name == "Structural Foundations")
                {
                    foundationElements.Add(element);
                }
            }
        }

        return foundationElements;
    }


    public static List<Element> GetAllStructuralFramings()
    {
        List<Element> framingElements = new List<Element>();

        if (_document != null)
        {
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            collector.OfCategory(BuiltInCategory.OST_StructuralFraming).WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                framingElements.Add(element);
            }
        }

        return framingElements;
    }


    public static string GetParameterValueFromSymbol(Element element, string parameterName)
    {
        ElementId symbolId = element.GetTypeId();
        if (symbolId != ElementId.InvalidElementId)
        {
            Element symbol = _document.GetElement(symbolId);
            if (symbol != null)
            {
                Parameter parameter = symbol.LookupParameter(parameterName);
                if (parameter != null)
                {
                    return parameter.AsValueString();
                }
            }
        }
        return string.Empty;
    }

    private static ElementFilter CreateElementFilterFromFilterRules(IList<FilterRule> filterRules)
    {
        // We use a LogicalAndFilter containing one ElementParameterFilter
        // for each FilterRule. We could alternatively create a single
        // ElementParameterFilter containing the entire list of FilterRules.
        IList<ElementFilter> elemFilters = new List<ElementFilter>();
        foreach (FilterRule filterRule in filterRules)
        {
            ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);
            elemFilters.Add(elemParamFilter);
        }
        LogicalAndFilter elemFilter = new LogicalAndFilter(elemFilters);

        return elemFilter;
    }

    private static List<ElementId> FindCategoryIdsByNames(List<string> categoryNames)
    {
        List<ElementId> categoryIds = new List<ElementId>();
        Categories globalCategories = _document.Settings.Categories;

        foreach (string categoryName in categoryNames)
        {
            Category category = globalCategories.Cast<Category>().FirstOrDefault(cat => cat.Name == categoryName);
            if (category != null)
            {
                categoryIds.Add(category.Id);
            }
        }

        return categoryIds;
    }


    private static Parameter FindSharedParameterByGuid(string parameterGuid)
    {
        FilteredElementCollector collector = new FilteredElementCollector(_document);
        collector.OfClass(typeof(Floor));

        Element sharedParamElement = collector.FirstElement();
        return sharedParamElement?.get_Parameter(new Guid(parameterGuid));
    }

    private static void CreateFilterWithRule(string filterName, List<ElementId> categoryIds, Parameter sharedParameter, string sharedParameterValue)
    {
        ParameterValueProvider valueProvider = new ParameterValueProvider(sharedParameter.Id);
        FilterStringRuleEvaluator evaluator = new FilterStringEquals();
        string ruleString = sharedParameterValue;
        FilterRule rule = new FilterStringRule(valueProvider, evaluator, ruleString);

        // Tạo một ElementParameterFilter sử dụng quy tắc lọc
        ElementParameterFilter filter = new ElementParameterFilter(rule);

        // Tạo ParameterFilterElement và đặt ElementFilter
        ParameterFilterElement parameterFilter = ParameterFilterElement.Create(_document, filterName, categoryIds, filter);
    }

    private static void CreateFilterWithRule(string filterName, List<ElementId> categoryIds, Parameter sharedParameter, double sharedParameterValue)
    {
        ParameterValueProvider valueProvider = new ParameterValueProvider(sharedParameter.Id);
        FilterNumericEquals evaluator = new FilterNumericEquals();
        FilterRule rule = new FilterDoubleRule(valueProvider, evaluator, sharedParameterValue, 0.001);

        // Tạo một ElementParameterFilter sử dụng quy tắc lọc
        ElementParameterFilter filter = new ElementParameterFilter(rule);

        // Tạo ParameterFilterElement và đặt ElementFilter
        ParameterFilterElement parameterFilter = ParameterFilterElement.Create(_document, filterName, categoryIds, filter);
    }


    /// <summary>
    ///  Equel rule with 2 parameter
    /// </summary>
    /// <param name="filterName"></param>
    /// <param name="categoryIds"></param>
    /// <param name="sharedParameter1"></param>
    /// <param name="sharedParameterValue1"></param>
    /// <param name="sharedParameter2"></param>
    /// <param name="sharedParameterValue2"></param>
    private static void CreateFilterWithRule(string filterName, List<ElementId> categoryIds, Parameter sharedParameter1, double sharedParameterValue1, Parameter sharedParameter2, double sharedParameterValue2)
    {
        // Quy tắc cho sharedParameter1
        ParameterValueProvider valueProvider1 = new ParameterValueProvider(sharedParameter1.Id);
        FilterNumericEquals evaluator1 = new FilterNumericEquals();
        FilterRule rule1 = new FilterDoubleRule(valueProvider1, evaluator1, sharedParameterValue1, 0.001);

        // Quy tắc cho sharedParameter2
        ParameterValueProvider valueProvider2 = new ParameterValueProvider(sharedParameter2.Id);
        FilterNumericEquals evaluator2 = new FilterNumericEquals();
        FilterRule rule2 = new FilterDoubleRule(valueProvider2, evaluator2, sharedParameterValue2, 0.001);


        // Tạo một ElementParameterFilter từ ListRule
        ElementParameterFilter filter = new ElementParameterFilter(new List<FilterRule> { rule1, rule2 }, false);

        // Tạo ParameterFilterElement và đặt ElementFilter
        ParameterFilterElement parameterFilter = ParameterFilterElement.Create(_document, filterName, categoryIds, filter);
    }



    /// <summary>
    ///  Equel rule with 2 parameter (equal and no has value)
    /// </summary>
    /// <param name="filterName"></param>
    /// <param name="categoryIds"></param>
    /// <param name="sharedParameter1"></param>
    /// <param name="sharedParameterValue1"></param>
    /// <param name="sharedParameter2"></param>
    /// <param name="sharedParameterValue2"></param>
    private static void CreateFilterWithRule1(string filterName, List<ElementId> categoryIds, Parameter sharedParameter1, double sharedParameterValue1, Parameter sharedParameter2)
    {
        // Quy tắc cho sharedParameter1
        ParameterValueProvider valueProvider1 = new ParameterValueProvider(sharedParameter1.Id);
        FilterNumericEquals evaluator1 = new FilterNumericEquals();
        FilterRule rule1 = new FilterDoubleRule(valueProvider1, evaluator1, sharedParameterValue1, 0.001);

        // Quy tắc cho sharedParameter2
        ParameterValueProvider valueProvider2 = new ParameterValueProvider(sharedParameter2.Id);
        HasNoValueFilterRule evaluator2 = new HasNoValueFilterRule(sharedParameter2.Id);
        //FilterRule rule2 = new FilterRule().create

        // Tạo một ElementParameterFilter từ ListRule
        ElementParameterFilter filter = new ElementParameterFilter(new List<FilterRule> { rule1, evaluator2 }, false);

        // Tạo ParameterFilterElement và đặt ElementFilter
        ParameterFilterElement parameterFilter = ParameterFilterElement.Create(_document, filterName, categoryIds, filter);
    }




    public static void CreateViewFilterNameMarkSharedParameter(string filterName, List<string> categoryNames, string guidofParameter, string sharedParameterValue)
    {
        using (Transaction transaction = new Transaction(_document, "Create View Filter"))
        {
            transaction.Start();

            List<ElementId> categoryIds = FindCategoryIdsByNames(categoryNames);
            if (categoryIds.Count > 0)
            {
                Parameter sharedParameter = FindSharedParameterByGuid(guidofParameter);
                if (sharedParameter != null)
                {
                    CreateFilterWithRule(filterName, categoryIds, sharedParameter, sharedParameterValue);
                }
                else
                {
                    MessageBox.Show("符号 parameter is not exist");
                }
            }

            transaction.Commit();
        }
    }

    public static void CreateViewFilterNameMarkSharedParameter(string filterName, List<string> categoryNames, Parameter parameter, double sharedParameterValue)
    {
        using (Transaction transaction = new Transaction(_document, "Create View Filter"))
        {
            transaction.Start();

            List<ElementId> categoryIds = FindCategoryIdsByNames(categoryNames);
            if (categoryIds.Count > 0)
            {
                CreateFilterWithRule(filterName, categoryIds, parameter, sharedParameterValue);
            }

            transaction.Commit();
        }
    }
    public static void CreateViewFilterNameMarkSharedParameter(string filterName, List<string> categoryNames, Parameter sharedParameter1, double sharedParameterValue1, Parameter sharedParameter2, double sharedParameterValue2)
    {
        using (Transaction transaction = new Transaction(_document, "Create View Filter"))
        {
            transaction.Start();

            List<ElementId> categoryIds = FindCategoryIdsByNames(categoryNames);
            if (categoryIds.Count > 0)
            {
                CreateFilterWithRule(filterName, categoryIds, sharedParameter1, sharedParameterValue1, sharedParameter2, sharedParameterValue2);
            }

            transaction.Commit();
        }
    }

    public static void CreateViewFilterNameMarkSharedParameter(string filterName, List<string> categoryNames, Parameter sharedParameter1, double sharedParameterValue1, Parameter sharedParameter2)
    {
        using (Transaction transaction = new Transaction(_document, "Create View Filter"))
        {
            transaction.Start();

            List<ElementId> categoryIds = FindCategoryIdsByNames(categoryNames);
            if (categoryIds.Count > 0)
            {
                CreateFilterWithRule1(filterName, categoryIds, sharedParameter1, sharedParameterValue1, sharedParameter2);
            }

            transaction.Commit();
        }
    }

}
