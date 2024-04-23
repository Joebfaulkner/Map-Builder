using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Map_Builder
{
    public class Template
    {
        public string? Name { get; set; }
        public string[]? AssetPaths { get; set; }
        public string? Material { get; set; }
        public int HP { get; set; }
        public bool Colidable { get; set; }
        public string? Event { get; set; }
        public Point[]? Contour { get; set; }
        public double? SizeRatio { get; set; }

    }

    public class Point
    {
        public int X { get; set; }

        public int Y { get; set; }
    }

    public class TemplateIcon : UIElement
    {
        public Template Template { get; set; }

        public Size Size { get; set; }

        public ContextMenu ContextMenu { get; set; }

        public TemplateIcon(Template template)
        {
            Template = template;
            InitializeContextMenu();
            MouseRightButtonDown += TemplateIcon_MouseRightButtonDown;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {

            base.OnRender(drawingContext);

            if (!string.IsNullOrEmpty(Template.AssetPaths[0]))
            {
                try
                {
                    BitmapImage assetImage = new BitmapImage(new Uri(Template.AssetPaths[0], uriKind:UriKind.Relative));
                    var widthToHeightRatio = assetImage.Width / assetImage.Height;
                    var templateContainer = new Rect
                    {
                        Width = 50 * widthToHeightRatio,
                        Height = 50
                    };
                    drawingContext.DrawImage(assetImage, templateContainer);
                    Size = new Size(templateContainer.Width, templateContainer.Height);
                    Measure(Size);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image: {ex.Message}");
                }
            }

        }

        protected override Size MeasureCore(Size availableSize)
        {
            return Size;
        }
        private void InitializeContextMenu()
        {
            ContextMenu = new ContextMenu();

            MenuItem edit = new MenuItem();
            edit.Header = "Edit";
            edit.Click += Edit;
            ContextMenu.Items.Add(edit);

            MenuItem delete = new MenuItem();
            delete.Header = "Delete";
            delete.Click += Delete;
            ContextMenu.Items.Add(delete);
        }

        private void TemplateIcon_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            var parentWindow = Helper.FindParentWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
                TemplateForm templateFrom = new TemplateForm(mainWindow.ViewModel, Template);

                templateFrom.Show();
            }

        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            var parentWindow = Helper.FindParentWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ViewModel.RemoveTemplate(Template);
            }
        }
    }

    public class Component
    {
        public Component(Template template)
        {
            Template = template;
        }
        public Template? Template { get; set; }
        public Point? Location { get; set; }
    }

    public class ComponentIcon : UIElement
    {
        public Component Component { get; set; }

        public Size Size { get; set; }

        BitmapImage AssetImage { get; set; }

        public List<Line> ContourLines { get; set; }

        public bool ShowContours { get; set; }
        public ContextMenu ContextMenu { get; set; }

        public ComponentIcon(Component component)
        {
            Component = component;
            AssetImage = new BitmapImage(new Uri(Component.Template.AssetPaths[0], UriKind.Relative));
            Size = new Size((double)(AssetImage.Width * component.Template.SizeRatio), (double)(AssetImage.Height * component.Template.SizeRatio));
            ContourLines = new List<Line>();
            InitializeContextMenu();
            MouseRightButtonDown += ComponentIcon_MouseRightButtonDown;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (!string.IsNullOrEmpty(Component.Template.AssetPaths[0]))
            {
                try
                {
                    var templateContainer = new Rect
                    {
                        Width = Size.Width,
                        Height = Size.Height
                    };

                    Measure(Size);

                    drawingContext.DrawImage(AssetImage, templateContainer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image: {ex.Message}");
                }
            }
        }

        private void InitializeContextMenu()
        {
            ContextMenu = new ContextMenu();

            MenuItem move = new MenuItem();
            move.Header = "Move";
            move.Click += Move;
            ContextMenu.Items.Add(move);

            MenuItem delete = new MenuItem();
            delete.Header = "Delete";
            delete.Click += Delete;
            ContextMenu.Items.Add(delete);
        }

        private void ComponentIcon_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        private void Move(object sender, RoutedEventArgs e)
        {
            var parentWindow = Helper.FindParentWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ViewModel.SelectComponent(this);
            }

        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            var parentWindow = Helper.FindParentWindow(this);
            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ViewModel.RemoveComponent(this);
            }
        }

        protected override Size MeasureCore(Size availableSize)
        {
            return Size;
        }
    }

    public class ExportComponent
    {
        public string? TemplateName { get; set; }
        public Point? Location { get; set; }
    }

    public class Map
    {
        public Template[]? Templates { get; set; }

        public ExportComponent[]? Components { get; set; }
    }

    public static class Helper
    {
        public static Window FindParentWindow(UIElement element)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null)
            {
                if (parent is Window window)
                {
                    return window;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
}
