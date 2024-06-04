using QComp.Helpers;
using QComp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            _index = new SavesIndex(new Dictionary<string, List<SaveItem>>());
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
                    _index = new SavesIndex(new Dictionary<string, List<SaveItem>>());
                    File.WriteAllText(_indexFileName, JsonSerializer.Serialize(_index));
                }
            }
        }

        public void Save(string sourceContent, string project, string name)
        {
            if (!_index.Items.ContainsKey(project))
                _index.Items.Add(project, new List<SaveItem>());
            if (_index.Items[project].Any(x => x.Name == name))
                return;
            var newSaveDir = Path.Combine(_qCompSaveDir, project, name);
            FileHelper.Copy(sourceContent, newSaveDir);
            _index.Items[project].Add(new SaveItem(name, $"{project}.exe", DateTime.Now));
            File.WriteAllText(_indexFileName, JsonSerializer.Serialize(_index));
        }

        public void Delete(string project, string name)
        {
            if (!_index.Items.ContainsKey(project))
                _index.Items.Add(project, new List<SaveItem>());
            if (!_index.Items[project].Any(x => x.Name == name))
                return;
            var newSaveDir = Path.Combine(_qCompSaveDir, name);
            Directory.Delete(newSaveDir, true);
            _index.Items[project].RemoveAll(x => x.Name == name);
            File.WriteAllText(_indexFileName, JsonSerializer.Serialize(_index));
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
    }
}
