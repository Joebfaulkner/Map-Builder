using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Map_Builder
{
    public class ExportMapViewModel
    {
        public MainWindowViewModel Parent { get; set; }
        public ExportMapWindow Window { get; set; }

        public ExportMapViewModel(MainWindowViewModel parent, ExportMapWindow window)
        {
            Parent = parent;
            Window = window;

            Window.ExportButton.Click += Export;
        }

        public void Export(object sender, RoutedEventArgs e)
        {
            Parent.ExportMap(Window.ExportName.Text);
            Window.Close();
        }
    }
}
