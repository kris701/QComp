using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace QComp.Helpers
{
    public static class DTE2Helper
    {
        public static EnvDTE80.DTE2 GetDTE2()
        {
            return Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
        }

        public static async Task<DirectoryInfo> GetSolutionPathAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            return new FileInfo(_applicationObject.Solution.FullName).Directory;
        }

        private static async Task<EnvDTE.Project> GetProjectAsync(string name)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _dte = GetDTE2();

            var _projects = _dte.ActiveSolutionProjects as Array;
            if (_projects != null && _projects.Length != 0)
            {
                for (int i = 0; i < _projects.Length; i++)
                    if (_projects.GetValue(i) is EnvDTE.Project proj && proj.FullName.EndsWith(name))
                        return proj;
            }
            return null;
        }

        public static async Task<DirectoryInfo> GetActiveProjectAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _dte = GetDTE2();

            var _startupProjects = _dte.Solution.SolutionBuild.StartupProjects as Array;
            if (_startupProjects != null && _startupProjects.Length != 0)
            {
                var targetProject = _startupProjects.GetValue(0) as string;
                var _selectedProject = await GetProjectAsync(targetProject);
                return new FileInfo(_selectedProject.FullName).Directory;
            }
            return null;
        }

        public static async Task<string> GetOutputDirAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE80.DTE2 _dte = GetDTE2();

            var _startupProjects = _dte.Solution.SolutionBuild.StartupProjects as Array;
            if (_startupProjects != null && _startupProjects.Length != 0)
            {
                var targetProject = _startupProjects.GetValue(0) as string;
                var _selectedProject = await GetProjectAsync(targetProject);
                var configuration = _selectedProject.ConfigurationManager.ActiveConfiguration.ConfigurationName;
                foreach (Property prop in _selectedProject.Properties)
                    if (prop.Name == "FriendlyTargetFramework")
                        return Path.Combine("bin", configuration, $"{prop.Value}");
            }
            return "";
        }
    }
}
