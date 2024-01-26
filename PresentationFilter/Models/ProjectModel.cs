using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresentationFilter.Models
{
    public class ProjectModel : BindableBase
    {
        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set { SetProperty(ref _projectName, value); }
        }
        private string _projectID;
        public string ProjectID
        {
            get { return _projectID; }
            set { SetProperty(ref _projectID, value); }
        }
        private string _projectAddress;
        public string ProjectAddress
        {
            get { return _projectAddress; }
            set { SetProperty(ref _projectAddress, value); }
        }

        public ProjectModel()
        {
            ProjectName = DocumentService.GetProjectName();
            ProjectID = DocumentService.GetProjectNumber();
            ProjectAddress = DocumentService.GetProjectAddress();

        }

    }
}
