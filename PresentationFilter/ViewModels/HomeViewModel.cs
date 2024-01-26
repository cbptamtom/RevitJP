using PresentationFilter.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresentationFilter.ViewModels
{
    public class HomeViewModel : BindableBase
    {

        private ProjectModel _projectModel;
        public ProjectModel ProjectModel
        {
            get { return _projectModel; }
            set { SetProperty(ref _projectModel, value); }
        }

        public HomeViewModel()
        {
            ProjectModel = new ProjectModel();
        }
    }
}
