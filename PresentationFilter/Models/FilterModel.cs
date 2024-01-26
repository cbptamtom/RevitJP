using Autodesk.Revit.DB;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PresentationFilter.Models
{
    public class FilterModel : BindableBase
    {
        private Element _element;
        public Element Element
        {
            get { return _element; }
            set { SetProperty(ref _element, value); }
        }
        private Category _category;
        public Category Category
        {
            get { return _category; }
            set { SetProperty(ref _category, value); }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _filterName;
        public string FilterName
        {
            get { return _filterName; }
            set { SetProperty(ref _filterName, value); }
        }
        private string _offsetValue;
        public string OffsetValue
        {
            get { return _offsetValue; }
            set { SetProperty(ref _offsetValue, value); }
        }
        private string _filterRule;
        public string FilterRule
        {
            get { return _filterRule; }
            set { SetProperty(ref _filterRule, value); }
        }
        private string _categoryName;
        public string CategoryName
        {
            get { return _categoryName; }
            set { SetProperty(ref _categoryName, value); }
        }

        private double _elevation;
        public double Elevation
        {
            get { return _elevation; }
            set { SetProperty(ref _elevation, value); }
        }
        public FilterModel()
        {
        }

    }
}
