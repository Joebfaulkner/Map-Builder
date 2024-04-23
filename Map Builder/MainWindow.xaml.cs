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
using System.Windows.Threading;

namespace Map_Builder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel;


        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel(this);
            

        }

        private void LeftClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Select();
        }

        private void RightClick(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenOptions();
        }


        private void KeyPressed(object sender, KeyEventArgs e)
        {
            ViewModel.AddKey(e.Key);
        }

        private void KeyReleased(object sender, KeyEventArgs e)
        {
            ViewModel.RemoveKey(e.Key);
        }
        private void HideGridButton_Click(object sender, RoutedEventArgs e)
        {
            TemplateGrid.Visibility = Visibility.Collapsed;
        }

    }


}
