using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Map_Builder
{
    public class ImportMapViewModel
    {
        public MainWindowViewModel Parent { get; set; }
        public ImportMapWindow Window { get; set; }

        public ImportMapViewModel(ImportMapWindow importTemplateWindow, MainWindowViewModel parent)
        {
            Window = importTemplateWindow;
            Parent = parent;

            Window.ImportButton.Click += Import;
        }

        public void Import(object sender, RoutedEventArgs e)
        {
            Parent.ImportMap(Window.ImportPath.Text);
            Window.Close();
        }
    }
}
