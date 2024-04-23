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
using System.ComponentModel;

namespace Map_Builder
{
    /// <summary>
    /// Interaction logic for TemplateForm.xaml
    /// </summary>
    public partial class TemplateForm : Window
    {
        public TemplateFormViewModel ViewModel { get; set; }

        private void Next(object sender, RoutedEventArgs e)
        {
            ViewModel.NextPage();
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            ViewModel.ExportTemplate();
        }

        private void RightClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.RemovePoint();
        }

        private void LeftClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.AddPoint();
        }


        private void Import(object sender, RoutedEventArgs e)
        {
            ViewModel.ImportTemplate(ImportPath.Text);
        }

        public TemplateForm(MainWindowViewModel mainWindowViewModel, Template templateToEdit = null)
        {
            InitializeComponent();
            ViewModel = new TemplateFormViewModel(this, mainWindowViewModel, templateToEdit);
        }
    }
}
