using Autodesk.Revit.DB;
using PresentationFilter.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PresentationFilter.ViewModels
{
    public class ApplyFilterViewModel : BindableBase
    {

        private Document _document;

        private ObservableCollection<View> _viewTemplate;
        public ObservableCollection<View> ViewTemplate
        {
            get { return _viewTemplate; }
            set { SetProperty(ref _viewTemplate, value); }
        }
        private ObservableCollection<ParameterFilterElement> _ParameterFilterElement;
        public ObservableCollection<ParameterFilterElement> ParameterFilterElement
        {
            get { return _ParameterFilterElement; }
            set { SetProperty(ref _ParameterFilterElement, value); }
        }
        public ApplyFilterViewModel()
        {
            _document = DIContainer.Instance.Resolve<Document>();
            ViewTemplate = GetViewTemplates(_document);
            ParameterFilterElement = GetParameterFilterElements(_document);
        }

        private ObservableCollection<View> GetViewTemplates(Document doc)
        {
            var viewTemplates = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().Where(v => v.IsTemplate).ToList();
            return new ObservableCollection<View>(viewTemplates);
        }

        private ObservableCollection<ParameterFilterElement> GetParameterFilterElements(Document doc)
        {
            var viewTemplates = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).Cast<ParameterFilterElement>().Where(v => v.Name.StartsWith("梁レベル/Structual Framing Level")).ToList();
            return new ObservableCollection<ParameterFilterElement>(viewTemplates);
        }
        //ParameterFilterElement


    }
}
