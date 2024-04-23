using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Text.Json;
using System.Windows;

namespace Map_Builder
{
    public class TemplateFormViewModel
    {
        MainWindowViewModel Parent { get; set; }

        TemplateForm Window { get; set; }

        TemplateFormPage1 Form { get; set; }

        TemplateFormPage2 ContourPlacement { get; set; }

        private Page _currentPage;

        List<Point> ContourPoints { get; set; }

        Template TemplateToEdit { get; set; }


        Page CurrentPage 
        { 
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                Window.contentFrame.Navigate(_currentPage);
            }
        }

        public TemplateFormViewModel(TemplateForm templateFormWindow, MainWindowViewModel mainWindowViewModel, Template templateToEdit = null)
        {
            Parent = mainWindowViewModel;
            Window = templateFormWindow;
            Form = new TemplateFormPage1();
            Form.NewAssetPathFieldButton.Click += AddAssetField;
            CurrentPage = Form;
            ContourPoints = new List<Point>();
            if (templateToEdit != null)
            {
                TemplateToEdit = templateToEdit;
                Form.templateNameTextBox.Text = templateToEdit.Name;
                Form.assetPath0TextBox.Text = templateToEdit.AssetPaths[0];
                
                for (var i = 1; i < templateToEdit.AssetPaths.Length; i++)
                {
                    AddAssetField(null, null);
                    Form.assetPathGrid.Children.OfType<TextBox>().ToArray()[i].Text = templateToEdit.AssetPaths[i];
                }

                Form.materialTextBox.Text = templateToEdit.Material;
                Form.hpTextBox.Text = templateToEdit.HP.ToString();
                Form.collidableTextBox.Text = templateToEdit.Colidable.ToString();
                Form.eventTextBox.Text = templateToEdit.Event.ToString();
                Form.sizeRatioTextBox.Text = templateToEdit.SizeRatio.ToString();
                ContourPoints = Parent.GetAbsoluteContourPoints(new Point { X = 150, Y = 150 }, templateToEdit.Contour, 1.0).ToList();
            }
            else
            {
                Form.hpTextBox.Text = "0";
                Form.sizeRatioTextBox.Text = "1.0";
            }
        }

        public void NextPage()
        {
            if (CurrentPage is TemplateFormPage1)
            {
                if (File.Exists(Form.assetPath0TextBox.Text))
                {
                    ContourPlacement = new TemplateFormPage2();

                    ImageBrush brush = new ImageBrush();
                    brush.ImageSource = new BitmapImage(new Uri(@Form.assetPath0TextBox.Text, UriKind.Relative));
                    var item = new Rectangle()
                    {
                        Width = brush.ImageSource.Width,
                        Height = brush.ImageSource.Height,
                    };
                    item.Fill = brush;
                    Canvas.SetLeft(item, 150);
                    Canvas.SetTop(item, 150);

                    ContourPlacement.canvas.Children.Add(item);
                    foreach(var point in ContourPoints)
                    {
                        CreateCircle(point);
                    }
                    UpdateContourLines();
                    CurrentPage = ContourPlacement;
                }
            }
            else if (CurrentPage is TemplateFormPage2)
            {

                var relativeContourPoints = ConvertToRelativePoints(new Point { X = 150, Y = 150}, ContourPoints.ToArray());

                var assetPaths = Form.assetPathGrid.Children.OfType<TextBox>().Select(t => t.Text).ToArray();

                if (TemplateToEdit != null)
                {
                    TemplateToEdit.Name = Form.templateNameTextBox.Text;
                    TemplateToEdit.AssetPaths = assetPaths;
                    TemplateToEdit.Material = Form.materialTextBox.Text;
                    TemplateToEdit.HP = Convert.ToInt16(Form.hpTextBox.Text);
                    TemplateToEdit.Colidable = Form.collidableTextBox.Text.ToLower() == "true" ? true : false;
                    TemplateToEdit.Event = Form.eventTextBox.Text;
                    TemplateToEdit.Contour = relativeContourPoints;
                    TemplateToEdit.SizeRatio = Convert.ToDouble(Form.sizeRatioTextBox.Text);
                }
                else
                {
                    var newTemplate = new Template
                    {
                        Name = Form.templateNameTextBox.Text,
                        AssetPaths = assetPaths,
                        Material = Form.materialTextBox.Text,
                        HP = Convert.ToInt16(Form.hpTextBox.Text),
                        Colidable = Form.collidableTextBox.Text.ToLower() == "true" ? true : false,
                        Event = Form.eventTextBox.Text,
                        Contour = relativeContourPoints,
                        SizeRatio = Convert.ToDouble(Form.sizeRatioTextBox.Text)
                    };

                    Parent.AddTemplate(newTemplate);
                }

                Window.Close();
            }
        }

        public void AddPoint()
        {
            if (CurrentPage is not TemplateFormPage2)
                return;

            var mousePosition = GetMousePosition();

            if (ContourPoints.Count > 0 && ContourPoints.Any(p => CalculateDistance(p, mousePosition) <= 20))
                return;

            ContourPoints.Add(mousePosition);

            CreateCircle(mousePosition);

            UpdateContourLines();
        }

        public void RemovePoint()
        {

            if (CurrentPage is not TemplateFormPage2)
                return;

            var mousePosition = GetMousePosition();

            var pointToRemove = ContourPoints.FirstOrDefault(p => CalculateDistance(p, mousePosition) <= 20);

            if (pointToRemove is null)
                return;

            ContourPoints.Remove(pointToRemove);

            RemoveCircle(pointToRemove);

            UpdateContourLines();
        }

        public Point GetMousePosition()
        {
            return new Point
            {

                X = (int)Mouse.GetPosition(Window.contentFrame).X,
                Y = (int)Mouse.GetPosition(Window.contentFrame).Y - 20
            };
        }

        public void CreateCircle(Point point)
        {

            var circle = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red
            };
            Canvas.SetTop(circle, point.Y - 10);
            Canvas.SetLeft(circle, point.X - 10);
            ContourPlacement.canvas.Children.Add(circle);
        }

        public void UpdateContourLines()
        {
            if (ContourPoints.Count <= 2)
                return;

            RemoveAllLinesFromCanvas();

            for (int i = 0; i < ContourPoints.Count - 1; i++)
            {
                Line line = new Line
                {
                    X1 = ContourPoints[i].X, // Start X coordinate
                    Y1 = ContourPoints[i].Y, // Start Y coordinate
                    X2 = ContourPoints[i + 1].X, // End X coordinate
                    Y2 = ContourPoints[i + 1].Y, // End Y coordinate
                    Stroke = Brushes.Green, // Set line color
                    StrokeThickness = 2 // Set line thickness
                };
                ContourPlacement.canvas.Children.Add(line);
            }

            Line _line = new Line
            {
                X1 = ContourPoints[ContourPoints.Count - 1].X,
                X2 = ContourPoints[0].X,
                Y1 = ContourPoints[ContourPoints.Count - 1].Y,
                Y2 = ContourPoints[0].Y,
                Stroke = Brushes.Green,
                StrokeThickness = 2
            };
            ContourPlacement.canvas.Children.Add(_line);

        }

        private void RemoveAllLinesFromCanvas()
        {
            // Create a list to store the line objects that need to be removed
            var linesToRemove = new List<Line>();

            // Iterate through the children of the canvas
            foreach (var child in ContourPlacement.canvas.Children)
            {
                // Check if the child is a Line object
                if (child is Line)
                {
                    // If it is, add it to the list of lines to be removed
                    linesToRemove.Add((Line)child);
                }
            }

            // Remove all lines from the canvas
            foreach (var line in linesToRemove)
            {
                ContourPlacement.canvas.Children.Remove(line);
            }
        }

        public void RemoveCircle(Point point)
        {
            foreach(var child in ContourPlacement.canvas.Children)
            {
                if (child is Ellipse && Canvas.GetTop((Ellipse)child) == point.Y - 10 && Canvas.GetLeft((Ellipse)child) == point.X - 10)
                {
                    ContourPlacement.canvas.Children.Remove((Ellipse)child);
                    break;
                }
            }
        }

        public static double CalculateDistance(Point point1, Point point2)
        {
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;

            // Calculate the square of the differences
            double deltaXSquared = Math.Pow(deltaX, 2);
            double deltaYSquared = Math.Pow(deltaY, 2);

            // Calculate the sum of the squares
            double sumOfSquares = deltaXSquared + deltaYSquared;

            // Calculate the square root of the sum of the squares
            double distance = Math.Sqrt(sumOfSquares);

            return distance;
        }

        public static Point[] ConvertToRelativePoints(Point origin, Point[] contourPoints)
        {
            var relativePoints = new Point[contourPoints.Length];
            for (int i = 0; i < contourPoints.Length; i++)
            {
                relativePoints[i] = new Point
                {
                    X = contourPoints[i].X - (int)origin.X,
                    Y = contourPoints[i].Y - (int)origin.Y
                };
            }
            return relativePoints;
        }


        public void ExportTemplate()
        {
            var localAssetNames = new List<string>();
            foreach (var assetPath in Form.assetPathGrid.Children.OfType<TextBox>().ToArray())
            {
                CopyImageToLocalDir(assetPath.Text);
                assetPath.Text = "templates/assets/" + System.IO.Path.GetFileName(assetPath.Text);
                localAssetNames.Add(assetPath.Text);
            }

            var relativeContourPoints = ConvertToRelativePoints(new Point { X = 150, Y = 150}, ContourPoints.ToArray());

            var templateToExport = new Template
            {
                Name = Form.templateNameTextBox.Text,
                AssetPaths = localAssetNames.ToArray(),
                Material = Form.materialTextBox.Text,
                HP = Convert.ToInt16(Form.hpTextBox.Text),
                Colidable = Form.collidableTextBox.Text.ToLower() == "true" ? true : false,
                Event = Form.eventTextBox.Text,
                Contour = relativeContourPoints,
                SizeRatio = Convert.ToDouble(Form.sizeRatioTextBox.Text)
            };

            var templateAsJsonString = JsonSerializer.Serialize(templateToExport);
            var directoryString = "templates";
            
            if (!Directory.Exists(directoryString))
                Directory.CreateDirectory(directoryString);

            using (StreamWriter writer = new StreamWriter(directoryString + "/" + templateToExport.Name + ".json"))
            {
                writer.Write(templateAsJsonString);
            }

        }

        public void CopyImageToLocalDir(string sourcePath)
        {
            if (!Directory.Exists("templates"))
                Directory.CreateDirectory("templates");

            if (!Directory.Exists("templates/assets"))
                Directory.CreateDirectory("templates/assets");

            try
            {
                File.Copy(sourcePath, "templates/assets/" + System.IO.Path.GetFileName(sourcePath), true);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }
        }
        public void ImportTemplate(string templatePath)
        {
            if (!File.Exists(templatePath))
                return;

            using (StreamReader r = new StreamReader(templatePath))
            {
                var templateString = r.ReadToEnd();
                var importedTemplate = JsonSerializer.Deserialize<Template>(templateString);
                Form.templateNameTextBox.Text = importedTemplate.Name;
                Form.assetPath0TextBox.Text = importedTemplate.AssetPaths[0];

                for (var i = 1; i < importedTemplate.AssetPaths.Length; i++)
                {
                    AddAssetField(null, null);
                    Form.assetPathGrid.Children.OfType<TextBox>().ToArray()[i].Text = importedTemplate.AssetPaths[i];
                }

                Form.materialTextBox.Text = importedTemplate.Material;
                Form.hpTextBox.Text = importedTemplate.HP.ToString();
                Form.collidableTextBox.Text = importedTemplate.Colidable.ToString();
                Form.eventTextBox.Text = importedTemplate.Event.ToString();
                Form.sizeRatioTextBox.Text = importedTemplate.SizeRatio.ToString();
                ContourPoints = Parent.GetAbsoluteContourPoints(new Point { X = 150, Y = 150 }, importedTemplate.Contour, 1.0).ToList();

            }
        }

        public void AddAssetField(object sender, RoutedEventArgs e)
        {
            var newAssetTextBox = new TextBox
            {
                Width = 200,
                Height = 30,
                Margin = new Thickness(10)
            };

            var newAssetLabel = new Label
            {
                Content = "Asset Path " + Form.assetLabelGrid.RowDefinitions.Count,
                Margin = new Thickness(10)
            };

            var numberOfRows = Form.assetPathGrid.RowDefinitions.Count;
            if (numberOfRows <= Form.assetPathGrid.Children.Count)
            {
                Form.assetPathGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Form.assetLabelGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            Grid.SetRow(newAssetLabel, Form.assetLabelGrid.RowDefinitions.Count - 1);
            Grid.SetRow(newAssetTextBox, Form.assetPathGrid.RowDefinitions.Count - 1);
            Form.assetLabelGrid.Children.Add(newAssetLabel);
            Form.assetPathGrid.Children.Add(newAssetTextBox);
        }
    }
}
