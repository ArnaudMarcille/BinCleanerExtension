using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BinCleanerExtension22.Models
{
    /// <summary>
    /// Project model
    /// </summary>
    public class ProjectModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Selected
        /// </summary>
        private bool selected;

        #endregion

        #region Properties

        /// <summary>
        /// Property change event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Full path
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Selected getter setter
        /// </summary>
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                OnSelected.Invoke(this, EventArgs.Empty);
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Done
        /// </summary>
        public bool Done { get; set; }

        /// <summary>
        /// Event raised when a property is changed
        /// </summary>
        public event EventHandler OnSelected;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project"></param>
        public ProjectModel(string project)
        {
            Key = Path.GetFileName(project);
            FullPath = project;
            selected = true;
            Done = false;
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Set selected
        /// </summary>
        /// <param name="value"></param>
        public void SetSelected(bool value)
        {
            selected = value;
            NotifyPropertyChanged(nameof(Selected));
        }

        /// <summary>
        /// Set to Done
        /// </summary>
        /// <param name="value"></param>
        public void SetDone(bool value)
        {
            Done = value;
            NotifyPropertyChanged(nameof(Done));
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

        #endregion
    }
}
