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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Color = Autodesk.Revit.DB.Color;

namespace PresentationFilter.ViewModels
{

    public class DeleteFilterViewModel : BindableBase
    {
        public Color[] ColorProvider { get; } =
        typeof(Colors).GetProperties()
                      .Where(p => p.PropertyType == typeof(Color))
                      .Select(p => (Color)p.GetValue(null))
                      .ToArray();


        private TransactionGroup _transactionGroup;
        private Document _document;
        private ObservableCollection<FilterterDel> _sampleItems = new ObservableCollection<FilterterDel>();
        public ObservableCollection<FilterterDel> SampleItems
        {
            get { return _sampleItems; }
            set { SetProperty(ref _sampleItems, value); }
        }
        private bool _isShiftKeyDown;
        public bool IsShiftKeyDown
        {
            get { return _isShiftKeyDown; }
            set { SetProperty(ref _isShiftKeyDown, value); }
        }

        private int _sampleItemsCount;

        public int SampleItemsCount
        {
            get { return _sampleItemsCount; }
            set { SetProperty(ref _sampleItemsCount, value); }
        }
        public ICommand DelFilterCommand { get; private set; }
        public ICommand CheckboxCommand { get; private set; }
        public ICommand SelectAllCommand { get; private set; }
        public ICommand UnSelectAllCommand { get; private set; }



        public DeleteFilterViewModel()
        {
            _isShiftKeyDown = false;
            SampleItems = GenerateFilter();
            SampleItemsCount = SampleItems.Count;
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
                        using (Transaction transaction = new Transaction(_document, "Delete Filters"))
                        {
                            transaction.Start();
                            _document.Delete(f.FilterElem.Id);
                            transaction.Commit();
                        }

                    }
                }
                MessageBox.Show("Delete success");

                _transactionGroup.Commit();
                MainWindow.Instance.Close();

            });
            SelectAllCommand = new DelegateCommand(() =>
            {
                foreach (FilterterDel filter in SampleItems)
                {
                    filter.Selected = true;
                }
            });
            UnSelectAllCommand = new DelegateCommand(() =>
            {
                foreach (FilterterDel filter in SampleItems)
                {
                    filter.Selected = false;
                }
            });
            CheckboxCommand = new DelegateCommand(() =>
            {

            });


        }

        private ObservableCollection<FilterterDel> GenerateFilter()
        {
            _document = DIContainer.Instance.Resolve<Document>();

            var filters = new ObservableCollection<FilterterDel>();

            // Lấy tất cả các phần tử kiểu ParameterFilterElement
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            ICollection<Element> filterElements = collector.OfClass(typeof(ParameterFilterElement)).ToElements();
            filterElements = filterElements.OrderBy(f => f.Name).ToList();
            // Chuyển các phần tử thành các mục FilterterDel và thêm vào danh sách
            foreach (ParameterFilterElement filterElement in filterElements)
            {
                filters.Add(new FilterterDel { Name = filterElement.Name, Selected = false, FilterElem = filterElement });
            }

            return filters;
        }
    }
    public class FilterterDel : BindableBase
    {
        private string _name;
        private bool _selected;
        private Element _filterElem;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public bool Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }

        public Element FilterElem
        {
            get { return _filterElem; }
            set { SetProperty(ref _filterElem, value); }
        }
    }
}
