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
using System.Windows.Shapes;

namespace Map_Builder
{
    /// <summary>
    /// Interaction logic for ImportTemplateWindow.xaml
    /// </summary>
    public partial class ImportMapWindow : Window
    {
        ImportMapViewModel ViewModel { get; set; }
        public ImportMapWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            ViewModel = new ImportMapViewModel(this, mainWindowViewModel);
        }
    }
}
