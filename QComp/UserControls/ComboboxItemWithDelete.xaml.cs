using QComp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace QComp.UserControls
{
    public partial class ComboboxItemWithDelete : UserControl
    {
        public SaveItem Save { get; }

        private Func<SaveItem, Task> _onDelete;
        public ComboboxItemWithDelete(SaveItem save, Func<SaveItem,Task> onDelete)
        {
            InitializeComponent();
            ItemText.Content = save.ToString();
            _onDelete = onDelete;
            Save = save;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            await _onDelete.Invoke(Save);
        }
    }
}
