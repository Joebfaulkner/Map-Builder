using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.Json;
using Path = System.IO.Path;

namespace Map_Builder
{
    public class MainWindowViewModel
    {
        public List<Key> Keys { get; set; }

        public MainWindow Window { get; set; }

        DispatcherTimer DispatcherTimer { get; set; }

        Button AddTemplateButton { get; set; }

        Button ChangeVisibilityTemplateGridButton { get; set; }

        Template SelectedTemplate { get; set; }

        ComponentIcon ComponentPreview { get; set; }

        CheckBox ShowContoursCheckBox { get; set; }

        Rectangle Boarder { get; set; }

        double ZoomPercentage { get; set; }

        Point RelativeViewCoordinates { get; set; }

        double CurrentZoom { get; set; }

        Button ExportMapButton { get; set; }

        Button ImportMapButton { get; set; }

        public MainWindowViewModel(MainWindow window)
        {
            Window = window;
            Keys = new List<Key>();
            DispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Tick += Update;
            // this timer will run every 20 milliseconds
            DispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);
            // start the timer
            DispatcherTimer.Start();

            ImportMapButton = new Button
            {
                Content = "Import Map"
            };
            ImportMapButton.Click += OpenImportMapWindow;
            AddItemToTemplateGrid(ImportMapButton);

            ExportMapButton = new Button
            {
                Content = "Export Map"
            };

            ExportMapButton.Click += OpenExportMapWindow;
            AddItemToTemplateGrid(ExportMapButton);

            AddTemplateButton = new Button();
            AddTemplateButton.Content = "Add Template"; // Set the text displayed on the button

            // Add event handler for the Click event
            AddTemplateButton.Click += OpenNewTemplateWindow;
            // Add the button to a container (e.g., a Grid)
            // Replace "grid" with the name of the container you want to add the button to
            AddItemToTemplateGrid(AddTemplateButton);

            ShowContoursCheckBox = new CheckBox
            {
                Content = "Show Contours",
                LayoutTransform = new RotateTransform(90)
            };
            ShowContoursCheckBox.Checked += ChangeContourVisibility;
            ShowContoursCheckBox.Unchecked += ChangeContourVisibility;
            Window.TemplateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto});
            Window.TemplateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(ShowContoursCheckBox, 1);
            Window.TemplateGrid.Children.Add(ShowContoursCheckBox);

            ChangeVisibilityTemplateGridButton = new Button
            {
                Content = "Hide",
                Height = 15,
                LayoutTransform = new RotateTransform(90),
                FontSize = 8
            };

            ChangeVisibilityTemplateGridButton.Click += ChangeTemplateGridVisibility;
            Window.stackpannel.Children.Add(ChangeVisibilityTemplateGridButton);


            Boarder = new Rectangle
            {
                Width = 4000,
                Height = 4000,
                Stroke = Brushes.Black,
                StrokeThickness = 100
            };
            Canvas.SetTop(Boarder, -100);
            Canvas.SetLeft(Boarder, -100);
            Window.canvas.Children.Add(Boarder);

            ZoomPercentage = 1.0;
            CurrentZoom = 1.0;

            RelativeViewCoordinates = new Point
            {
                X = (int)(Window.Left + (Window.Width / 2)),
                Y = (int)(Window.Top + (Window.Height / 2))
            };

        }

        public void Select()
        {
            if(SelectedTemplate is not null && ComponentPreview is not null)
            {
                var lineOpacity = (bool)ShowContoursCheckBox.IsChecked ? 1 : 0;
                SelectedTemplate = null;
                ComponentPreview.Opacity = 1.0;
                foreach(var line in ComponentPreview.ContourLines)
                {
                    line.Opacity = lineOpacity;
                }
                ComponentPreview = null;
            }
        }

        public void OpenOptions()
        {
        }

        public Point GetMousePosition()
        {
            var mousePosition = Mouse.GetPosition(Window.canvas);
            return new Point
            {
                X = (int)mousePosition.X,
                Y = (int)mousePosition.Y
            };
        }

        public void AddKey(Key key)
        {
            if (!Keys.Contains(key))
                Keys.Add(key);
        }

        public void RemoveKey(Key key)
        {
            if (Keys.Contains(key))
                Keys.Remove(key);
        }

        public void Update(object sender, EventArgs e)
        {
            if (Keys.Count() > 0)
                MoveView();
            if (SelectedTemplate is not null)
            {

                RenderSelectedTemplatePreview(SelectedTemplate, GetMousePosition());
            }
        }

        public static bool IsPointInsideRectangle(Point point, Rect rectangle)
        {
            // Check if point's X coordinate is within rectangle's X and X + Width
            bool isInsideX = (point.X >= rectangle.X) && (point.X <= (rectangle.X + rectangle.Width));

            // Check if point's Y coordinate is within rectangle's Y and Y + Height
            bool isInsideY = (point.Y >= rectangle.Y) && (point.Y <= (rectangle.Y + rectangle.Height));

            // Return true if both X and Y coordinates are inside the rectangle
            return isInsideX && isInsideY;
        }

        public void AddTemplate(Template newTemplate)
        {
            var templateIcon = new TemplateIcon(newTemplate);
            templateIcon.MouseLeftButtonDown += SelectTemplate;
            AddItemToTemplateGrid(templateIcon);
            
        }


        private void OpenNewTemplateWindow(object sender, RoutedEventArgs e)
        {
            TemplateForm templateFrom = new TemplateForm(this);

            templateFrom.Show();
        }

        public void AddItemToTemplateGrid(UIElement element)
        {
            var numberOfRows = Window.TemplateGrid.RowDefinitions.Count;
            if (numberOfRows <= Window.TemplateGrid.Children.Count)
            {
                Window.TemplateGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            Grid.SetRow(element, Window.TemplateGrid.RowDefinitions.Count - 1);
            Window.TemplateGrid.Children.Add(element);
        }

        public void RemoveTemplate(Template template)
        {
            var templateIcon = Window.TemplateGrid.Children.OfType<TemplateIcon>().FirstOrDefault(t => t.Template == template);
            Window.TemplateGrid.Children.Remove(templateIcon);
        }

        public void ChangeTemplateGridVisibility(object sender, RoutedEventArgs e)
        {
            if (Window.TemplateGrid.Visibility == Visibility.Visible)
            {
                Window.TemplateGrid.Visibility = Visibility.Collapsed;
                ChangeVisibilityTemplateGridButton.Content = "Show";
            }
            else
            {
                Window.TemplateGrid.Visibility = Visibility.Visible;
                ChangeVisibilityTemplateGridButton.Content = "Hide";
            }
        }

        public void SelectTemplate(object sender, MouseEventArgs e)
        {
            if (sender is TemplateIcon templateIcon)
                SelectedTemplate = templateIcon.Template;
        }

        public void RenderSelectedTemplatePreview(Template template, Point mousePosition)
        {
            if (ComponentPreview is null)
            {
                var component = new Component(template);
                ComponentPreview = new ComponentIcon(component);
                
                ComponentPreview.Opacity = 0.7;

                Window.canvas.Children.Add(ComponentPreview);
            }
            ComponentPreview.Component.Location = new Point
            {
                X = (int)(mousePosition.X - (ComponentPreview.Size.Width / 2)),
                Y = (int)(mousePosition.Y - (ComponentPreview.Size.Height / 2))
            };
            Canvas.SetLeft(ComponentPreview, ComponentPreview.Component.Location.X);
            Canvas.SetTop(ComponentPreview, ComponentPreview.Component.Location.Y);

            var absoluteContourPoints = GetAbsoluteContourPoints(ComponentPreview.Component.Location, template.Contour, (double)ComponentPreview.Component.Template.SizeRatio);
            LoadPreviewContourLines(absoluteContourPoints);
        }


        public Point[] GetAbsoluteContourPoints(Point componentLoaction, Point[] relativeContourPoints, double sizeRatio)
        {
            var absoluteContourPoints = new Point[relativeContourPoints.Length];

            for (int i = 0; i < relativeContourPoints.Length; i++)
            {
                absoluteContourPoints[i] = new Point
                {
                    X = (int)(relativeContourPoints[i].X * sizeRatio + componentLoaction.X),
                    Y = (int)(relativeContourPoints[i].Y * sizeRatio + componentLoaction.Y)
                };
            }

            return absoluteContourPoints;
        }

        public void LoadPreviewContourLines(Point[] points)
        {
            RemoveOldLines();
            RenderNewLines(points);
        }

        public void RemoveOldLines()
        {
            if (ComponentPreview is null || ComponentPreview.ContourLines is null)
                return;

            foreach (var line in ComponentPreview.ContourLines)
            {
                Window.canvas.Children.Remove(line);
            }
        }

        public void RenderNewLines(Point[] points)
        {
            var opacity = (bool)ShowContoursCheckBox.IsChecked ? 1.0 : 0.0;
            ComponentPreview.ContourLines = DrawLines(points, opacity).ToList();
        }

        public Line[] DrawLines(Point[] points, double opacity)
        {
            List<Line> lines = new List<Line>();

            if (points.Length < 1)
                return lines.ToArray();

            for (int i = 0; i < points.Length - 1; i++)
            {
                Line line = new Line
                {
                    X1 = points[i].X, // Start X coordinate
                    Y1 = points[i].Y, // Start Y coordinate
                    X2 = points[i + 1].X, // End X coordinate
                    Y2 = points[i + 1].Y, // End Y coordinate
                    Stroke = Brushes.Green, // Set line color
                    StrokeThickness = 2, // Set line thickness
                    Opacity = opacity
                };
                Window.canvas.Children.Add(line);
                lines.Add(line);
            }

            Line _line = new Line
            {
                X1 = points[points.Length - 1].X,
                X2 = points[0].X,
                Y1 = points[points.Length - 1].Y,
                Y2 = points[0].Y,
                Stroke = Brushes.Green,
                StrokeThickness = 2,
                Opacity = opacity
            };
            Window.canvas.Children.Add(_line);
            lines.Add(_line);

            return lines.ToArray();
        }

        public void ChangeContourVisibility(object sender, EventArgs e)
        {
            var newOpacity = (bool)ShowContoursCheckBox.IsChecked ? 1 : 0;
            foreach (var component in Window.canvas.Children.OfType<ComponentIcon>())
            {
                foreach (var line in Window.canvas.Children.OfType<Line>())
                {
                    if (component.ContourLines.Contains(line))
                    {
                        line.Opacity = newOpacity;
                    }
                }
            }
        }

        public void MoveView()
        {
            var changeX = 0;
            var changeY = 0;
            var movement = (int)(10 / ZoomPercentage);
            if (Keys.Contains(Key.D))
                changeX -= movement;

            if (Keys.Contains(Key.A))
                changeX += movement;


            if (Keys.Contains(Key.W))
                changeY += movement;


            if (Keys.Contains(Key.S))
                changeY -= movement;

            MoveAllElements(changeX, changeY);

            UpdateComponentLocations();

            RelativeViewCoordinates.X += changeX;
            RelativeViewCoordinates.Y += changeY;

            if (Keys.Contains(Key.OemPlus))
            {
                if (ZoomPercentage < 10)
                    ZoomPercentage += .01;
            }
            else if (Keys.Contains(Key.OemMinus))
            {
                if (ZoomPercentage > 0.1)
                    ZoomPercentage -= .01;
            }

            if (CurrentZoom != ZoomPercentage)
                ApplyZoom();
            CurrentZoom = ZoomPercentage;
        }

        public void MoveAllElements(int changeX, int changeY)
        {
            foreach(var element in Window.canvas.Children.OfType<UIElement>())
            {
                Canvas.SetTop(element, Canvas.GetTop(element) + changeY);
                Canvas.SetLeft(element, Canvas.GetLeft(element) + changeX);
            }

            foreach(var element in Window.canvas.Children.OfType<Line>())
            {
                element.X1 += changeX;
                element.X2 += changeX;
                element.Y1 += changeY;
                element.Y2 += changeY;
            }
        }
        
        public void UpdateComponentLocations()
        {
            foreach (var element in Window.canvas.Children.OfType<ComponentIcon>())
            {
                element.Component.Location = new Point { X = (int)Canvas.GetLeft(element), Y = (int)Canvas.GetTop(element) };
            }

        }

        public void SelectComponent(ComponentIcon componentIcon)
        {
            ComponentPreview = componentIcon;
            SelectedTemplate = componentIcon.Component.Template;
            componentIcon.Opacity = .7;
        }

        public void RemoveComponent(ComponentIcon componentIcon)
        {
            Window.canvas.Children.Remove(componentIcon);
            var lines = Window.canvas.Children.OfType<Line>().Where(l => componentIcon.ContourLines.Contains(l)).ToArray();
            foreach (var line in lines)
            {
                Window.canvas.Children.Remove(line);
            }
        }

        public void ApplyZoom()
        {
            var scaleTransform = new ScaleTransform(ZoomPercentage, ZoomPercentage);
            Window.canvas.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            Window.canvas.RenderTransform = scaleTransform;
        }

        public void ExportMap(string exportName)
        {
            SetZoom(1.0);
            UpdateComponentLocations();

            var templates = Window.TemplateGrid.Children.OfType<TemplateIcon>().Select(t => t.Template).ToArray();
            var components = Window.canvas.Children.OfType<ComponentIcon>().Select(c => c.Component).ToArray();
            var boarderLeft = (int)Canvas.GetLeft(Boarder);
            var boarderTop = (int)Canvas.GetTop(Boarder);

            foreach (var component in components)
            {
                component.Location = new Point { X = component.Location.X - boarderLeft, Y = component.Location.Y - boarderTop };
            }

            var exportComponents = components.Select(c =>
            new ExportComponent
            {
                Location = c.Location,
                TemplateName = c.Template.Name
            }).ToArray();

            var map = new Map {Templates = templates, Components = exportComponents };

            var mapJson = JsonSerializer.Serialize(map);

            if (!Directory.Exists("maps"))
                Directory.CreateDirectory("maps");

            using (var write = new StreamWriter($"maps/{exportName}.json"))
            {
                write.Write(mapJson);
            }
        }

        public void ImportMap(string mapDir)
        {
            if (!File.Exists(mapDir))
                return;

            SetZoom(1.0);
            using (StreamReader r = new StreamReader(mapDir))
            {
                var mapString = r.ReadToEnd();
                var map = JsonSerializer.Deserialize<Map>(mapString);

                if (map is null)
                    return;

                foreach (var template in map.Templates)
                {
                    AddTemplate(template);
                }

                foreach (var exportComponent in map.Components)
                {
                    var component = new Component(map.Templates.FirstOrDefault(t => t.Name == exportComponent.TemplateName));
                    var boarderLeft = (int)Canvas.GetLeft(Boarder);
                    var boarderTop = (int)Canvas.GetTop(Boarder);
                    var loadedX = (boarderLeft + exportComponent.Location.X);
                    var loadedY = (boarderTop + exportComponent.Location.Y);
                    component.Location = new Point { X = exportComponent.Location.X + boarderLeft, Y = exportComponent.Location.Y + boarderTop };

                    AddComponent(component);

                    AddContourLines(component);
                }

            }
        }

        public void AddComponent(Component component)
        {
            var template = Window.TemplateGrid.Children.OfType<TemplateIcon>().FirstOrDefault(t => t.Template.Name == component.Template.Name).Template;
            component.Template = template;
            var componentIcon = new ComponentIcon(component);
            
            Canvas.SetLeft(componentIcon, component.Location.X);
            Canvas.SetTop(componentIcon, component.Location.Y);

            Window.canvas.Children.Add(componentIcon);

            UpdateComponentLocations();
        }

        public void AddContourLines(Component component)
        {
            var opacity = (bool)ShowContoursCheckBox.IsChecked ? 1.0 : 0;
            var absoluteContourPoints = GetAbsoluteContourPoints(component.Location, component.Template.Contour, (double)component.Template.SizeRatio);
            Window.canvas.Children.OfType<ComponentIcon>().FirstOrDefault(c => c.Component == component).ContourLines = DrawLines(absoluteContourPoints, opacity).ToList();
        }

        public void OpenImportMapWindow(object sender, RoutedEventArgs e)
        {
            var importMapWindow = new ImportMapWindow(this);
            importMapWindow.Show();
        }

        public void OpenExportMapWindow(object sender, RoutedEventArgs e)
        {
            var exportMapWindow = new ExportMapWindow(this);
            exportMapWindow.Show();
        }

        public void SetZoom(double zoom)
        {
            CurrentZoom = zoom;
            ZoomPercentage = zoom;
            ApplyZoom();
        }
    }
}
