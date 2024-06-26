﻿using QComp.Helpers;
using QComp.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace QComp
{
    public class SavesManager
    {
        public static string SaveLocation = "QComp";
        public static string IndexName = "index.json";

        private SavesIndex _index;
        private string _qCompSaveDir;
        private string _indexFileName;

        public SavesManager()
        {
            _index = new SavesIndex(new Dictionary<string, List<SaveItem>>(), new Dictionary<string, List<string>>());
        }

        public async Task InitializeAsync()
        {
            var solution = await DTE2Helper.GetSolutionPathAsync();
            if (solution != null)
            {
                _qCompSaveDir = Path.Combine(solution.FullName, ".vs", SaveLocation);
                if (!Directory.Exists(_qCompSaveDir))
                    Directory.CreateDirectory(_qCompSaveDir);
                _indexFileName = Path.Combine(_qCompSaveDir, IndexName);
                if (File.Exists(_indexFileName))
                    _index = JsonSerializer.Deserialize<SavesIndex>(File.ReadAllText(_indexFileName));
                else
                {
                    _index = new SavesIndex(new Dictionary<string, List<SaveItem>>(), new Dictionary<string, List<string>>());
                    File.WriteAllText(_indexFileName, JsonSerializer.Serialize(_index));
                }
            }
        }

        public void SaveBinary(string sourceContent, string project, string name)
        {
            if (!_index.Items.ContainsKey(project))
                _index.Items.Add(project, new List<SaveItem>());
            if (_index.Items[project].Any(x => x.Name == name))
                return;
            var newSaveDir = Path.Combine(_qCompSaveDir, project, name);
            if (!File.Exists(Path.Combine(sourceContent, $"{project}.exe")))
                return;
            FileHelper.Copy(sourceContent, newSaveDir);
            _index.Items[project].Add(new SaveItem(name, $"{project}.exe", DateTime.Now));
            File.WriteAllText(_indexFileName, JsonSerializer.Serialize(_index));
        }

        public void DeleteBinary(string project, string name)
        {
            if (!_index.Items.ContainsKey(project))
                _index.Items.Add(project, new List<SaveItem>());
            if (!_index.Items[project].Any(x => x.Name == name))
                return;
            var newSaveDir = Path.Combine(_qCompSaveDir, project, name);
            Directory.Delete(newSaveDir, true);
            _index.Items[project].RemoveAll(x => x.Name == name);
            File.WriteAllText(_indexFileName, JsonSerializer.Serialize(_index));
        }

        public void DeleteAllBinaries(string project)
        {
            if (!_index.Items.ContainsKey(project))
                _index.Items.Add(project, new List<SaveItem>());
            foreach (var save in _index.Items[project])
                DeleteBinary(project, save.Name);
        }

        public List<SaveItem> GetSavesForProject(string project)
        {
            if (!_index.Items.ContainsKey(project))
                _index.Items.Add(project, new List<SaveItem>());
            return _index.Items[project];
        }

        public DirectoryInfo GetBinaryPath(string project, string name)
        {
            if (!_index.Items.ContainsKey(project))
                _index.Items.Add(project, new List<SaveItem>());
            if (!_index.Items[project].Any(x => x.Name == name))
                return null;
            var item = _index.Items[project].First(x => x.Name == name);
            return new DirectoryInfo(Path.Combine(_qCompSaveDir, project, item.Name, item.Binary));
        }

        public void SaveArguments(string project, string arguments)
        {
            if (!_index.CachedArguments.ContainsKey(project))
                _index.CachedArguments.Add(project, new List<string>());
            _index.CachedArguments[project].Add(arguments);
            File.WriteAllText(_indexFileName, JsonSerializer.Serialize(_index));
        }

        public void DeleteArguments(string project, string arguments)
        {
            if (!_index.CachedArguments.ContainsKey(project))
                _index.CachedArguments.Add(project, new List<string>());
            _index.CachedArguments[project].Remove(arguments);
            File.WriteAllText(_indexFileName, JsonSerializer.Serialize(_index));
        }

        public List<string> GetArgumentsForProject(string project)
        {
            if (!_index.CachedArguments.ContainsKey(project))
                _index.CachedArguments.Add(project, new List<string>());
            return _index.CachedArguments[project];
        }
    }
}
