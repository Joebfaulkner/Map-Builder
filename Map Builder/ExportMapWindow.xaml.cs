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
    /// Interaction logic for ExportTemplateWindow.xaml
    /// </summary>
    public partial class ExportMapWindow : Window
    {
        public ExportMapViewModel ViewModel {get; set;}
        public ExportMapWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            ViewModel = new ExportMapViewModel(mainWindowViewModel, this);
        }
    }
}
