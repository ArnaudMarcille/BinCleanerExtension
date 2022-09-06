using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using BinCleanerExtension22.Models;

namespace BinCleanerExtension22.Windows
{
    /// <summary>
    /// Interaction logic for ProjectsView.xaml
    /// </summary>
    public partial class ProjectsView : UserControl, INotifyPropertyChanged
    {
        #region Constants

        private const string BinFolder = "bin";
        private const string ObjFolder = "obj";

        #endregion

        #region Fields

        /// <summary>
        /// Select all boolean
        /// </summary>
        private bool selectAll;

        #endregion

        #region Properties

        /// <summary>
        /// Property change event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public List<ProjectModel> ProjectList { get; set; } = new List<ProjectModel>();

        /// <summary>
        /// Select all get set
        /// </summary>
        public bool SelectAll
        {
            get { return selectAll; }
            set
            {
                selectAll = value;

                foreach (var project in ProjectList)
                {
                    project.SetSelected(selectAll);
                }
            }
        }

        public bool ActiveProgress { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projects">Project list</param>
        public ProjectsView(IList<string> projects)
        {
            InitializeComponent();
            DataContext = this;

            SelectAll = true;

            foreach (var project in projects.Select(p => new ProjectModel(p)).Where(p => !string.IsNullOrWhiteSpace(p.Key)))
            {
                ProjectList.Add(project);
                project.OnSelected += OnItemSelected;
            }
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Method used for update properties in view
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// On item selected event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, EventArgs e)
        {
            ProjectModel project = sender as ProjectModel;

            if (project == null)
            {
                return;
            }

            selectAll = ProjectList.All(p => p.Selected);
            NotifyPropertyChanged(nameof(SelectAll));
        }

        /// <summary>
        /// On button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Launch async task for folder deletion
            new Task(() => { DeleteFolders(); }).Start();
        }

        /// <summary>
        /// Delete all the folders
        /// </summary>
        private void DeleteFolders()
        {
            ActiveProgress = true;
            NotifyPropertyChanged(nameof(ActiveProgress));
            foreach (var project in ProjectList.Where(p => p.Selected))
            {
                var path = Path.GetDirectoryName(project.FullPath);
                if (!Directory.Exists(path))
                {
                    continue;
                }

                try
                {
                    if (Directory.Exists(Path.Combine(path, BinFolder)))
                    {
                        Directory.Delete(Path.Combine(path, BinFolder), true);
                    }

                    if (Directory.Exists(Path.Combine(path, ObjFolder)))
                    {
                        Directory.Delete(Path.Combine(path, ObjFolder), true);
                    }

                    project.SetDone(true);
                }
                catch (Exception)
                {
                    project.SetDone(false);
                }
            }
            ActiveProgress = false;
            NotifyPropertyChanged(nameof(ActiveProgress));
        }

        #endregion
    }
}
