using Autodesk.Revit.DB;
using PresentationFilter.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PresentationFilter.ViewModels
{

    public class DeleteFilterViewModel : BindableBase
    {
        private TransactionGroup _transactionGroup;
        private Document _document;
        private ObservableCollection<FilterterDel> _sampleItems = new ObservableCollection<FilterterDel>();
        public ObservableCollection<FilterterDel> SampleItems
        {
            get { return _sampleItems; }
            set { SetProperty(ref _sampleItems, value); }
        }
        public ICommand DelFilterCommand { get; private set; }

        public DeleteFilterViewModel()
        {
            SampleItems = GenerateFilter();
            _document = DIContainer.Instance.Resolve<Document>();
            _transactionGroup = DIContainer.Instance.Resolve<TransactionGroup>();
            DelFilterCommand = new DelegateCommand(() =>
            {
                if (!_transactionGroup.HasStarted())
                {
                    return;
                }
                foreach (FilterterDel f in SampleItems)
                {
                    if (f.Selected)
                    {
                        _document.Delete(f.filterElem.Id);

                    }
                }
                _transactionGroup.Commit();

            });
        }



        private ObservableCollection<FilterterDel> GenerateFilter()
        {
            _document = DIContainer.Instance.Resolve<Document>();

            var filters = new ObservableCollection<FilterterDel>();

            // Lấy tất cả các phần tử kiểu ParameterFilterElement
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            ICollection<Element> filterElements = collector.OfClass(typeof(ParameterFilterElement)).ToElements();

            // Chuyển các phần tử thành các mục FilterterDel và thêm vào danh sách
            foreach (ParameterFilterElement filterElement in filterElements)
            {
                filters.Add(new FilterterDel { Name = filterElement.Name, Selected = false, filterElem = filterElement });
            }

            return filters;
        }
    }
    public class FilterterDel
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
        public Element filterElem { get; set; }
    }
}
