﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using BinCleanerExtension22.Windows;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace BinCleanerExtension22.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class BinCleaner
    {
        #region Fields

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a6ab2fb9-8dcd-4442-b446-fe92aed67978");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        #endregion

        #region Constructor


        /// <summary>
        /// Initializes a new instance of the <see cref="BinCleaner"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private BinCleaner(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        #endregion

        #region Properties


        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static BinCleaner Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }


        #endregion

        #region Methods

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in BinCleaner's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new BinCleaner(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsSolution solution = (IVsSolution)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(IVsSolution));

            var projects = GetProjects(solution);

            if (!projects.Any())
            {
                return;
            }

            System.Windows.Window window = new System.Windows.Window()
            {
                SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
                ResizeMode = System.Windows.ResizeMode.CanResize,
                MinHeight = 250,
                MinWidth = 450,
                MaxHeight = 1000,
                Title = "Clean Bin and obj?",
                Content = new ProjectsView(projects.Select(p =>
                {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();
                    return p.FullName;
                }).ToList())
            };

            WindowHelper.ShowModal(window);
        }

        #endregion

        #region Project listy methods

        /// <summary>
        /// Get all project from the solution (Non-null management)
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public static IEnumerable<EnvDTE.Project> GetProjects(IVsSolution solution)
        {
            foreach (IVsHierarchy hier in GetProjectsInSolution(solution))
            {
                EnvDTE.Project project = GetDTEProject(hier);
                if (project != null)
                    yield return project;
            }
        }

        /// <summary>
        /// Get all project form solution
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public static IEnumerable<IVsHierarchy> GetProjectsInSolution(IVsSolution solution)
        {
            return GetProjectsInSolution(solution, __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION);
        }

        /// <summary>
        /// Load all project in solution from flags
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<IVsHierarchy> GetProjectsInSolution(IVsSolution solution, __VSENUMPROJFLAGS flags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (solution == null)
                yield break;

            IEnumHierarchies enumHierarchies;
            Guid guid = Guid.Empty;
            solution.GetProjectEnum((uint)flags, ref guid, out enumHierarchies);
            if (enumHierarchies == null)
                yield break;

            IVsHierarchy[] hierarchy = new IVsHierarchy[1];
            uint fetched;
            while (enumHierarchies.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1)
            {
                if (hierarchy.Length > 0 && hierarchy[0] != null)
                    yield return hierarchy[0];
            }
        }

        /// <summary>
        /// Get DTE projects
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static EnvDTE.Project GetDTEProject(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (hierarchy == null)
                throw new ArgumentNullException("hierarchy");

            object obj;
            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out obj);
            return obj as EnvDTE.Project;
        }

        #endregion
    }
}
