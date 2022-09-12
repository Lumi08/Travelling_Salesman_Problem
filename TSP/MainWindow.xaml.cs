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

// Debug
using System.Diagnostics;

namespace TSP
{ 
	public partial class MainWindow : Window
	{
		private TSPManager tspManager;

		// TSPGraphNodes Config
		private int nodeRadius = 10;

		public MainWindow(TSPManager tsp)
		{
			InitializeComponent();
			// We may need this in the future
			this.tspManager = tsp;

			AlgorithmsComboBox.Items.Add("Genetic Algorithm");
			AlgorithmsComboBox.Items.Add("Ant Colony Optimisation");
			AlgorithmsComboBox.SelectedIndex = 0;

			StepBackwardsButton.IsEnabled = false;
			StepForwardsButton.IsEnabled = false;
		}

		public void UpdateUI(List<TSPGraphNode> nodePositions)
		{
			UpdateGraph(nodePositions);
			UpdatePointsTextBox(nodePositions);
			UpdateLinesTextBox(nodePositions);
		}

		public void UpdateGraph(List<TSPGraphNode> nodePositions)
		{
			// Clears the Nodes Labels
			NodeNumbersGrid.Children.Clear();

			GeometryGroup geometryGroup = new GeometryGroup();
			PathSegmentCollection pathSegments = new PathSegmentCollection();

			EllipseGeometry ellipseGeometry;
			PathFigure pathFigure = new PathFigure();
			for (int i = 0; i < nodePositions.Count; i++)
			{
				if (i == 0)
				{
					pathFigure.StartPoint = new Point(nodePositions[i].position.x, nodePositions[i].position.y);
					pathFigure.IsClosed = true;
				}
				else
				{
					LineSegment lineSegment = new LineSegment();
					lineSegment.Point = new Point(nodePositions[i].position.x, nodePositions[i].position.y);
					pathSegments.Add(lineSegment);
				}

				ellipseGeometry = new EllipseGeometry();
				ellipseGeometry.Center = new Point(nodePositions[i].position.x, nodePositions[i].position.y);
				ellipseGeometry.RadiusX = nodeRadius;
				ellipseGeometry.RadiusY = nodeRadius;

				TextBlock textBlock = new TextBlock();
				textBlock.Text = nodePositions[i].id.ToString();
				textBlock.FontSize = 13;
				textBlock.FontWeight = FontWeights.Bold;
				textBlock.Margin = new Thickness(ellipseGeometry.Bounds.TopLeft.X + 5, ellipseGeometry.Bounds.TopLeft.Y, 0, 0);
				if(nodePositions[i].id < 10)
				{
					textBlock.Margin = new Thickness(ellipseGeometry.Bounds.TopLeft.X + 5, ellipseGeometry.Bounds.TopLeft.Y+1, 0, 0);
				}
				else
				{
					textBlock.Margin = new Thickness(ellipseGeometry.Bounds.TopLeft.X + 2, ellipseGeometry.Bounds.TopLeft.Y+1, 0, 0);
				}

				NodeNumbersGrid.Children.Add(textBlock);

				geometryGroup.Children.Add(ellipseGeometry);
			}
			pathFigure.Segments = pathSegments;

			PathFigureCollection pathFigures = new PathFigureCollection();
			pathFigures.Add(pathFigure);

			PathGeometry pathGeometry = new PathGeometry();
			pathGeometry.Figures = pathFigures;

			TSPGraphPath.Data = pathGeometry;
			TSPGraphNodes.Data = geometryGroup;
		}

		public void UpdatePointsTextBox(List<TSPGraphNode> nodePositions)
		{
			string finalText = "";

			foreach (TSPGraphNode node in nodePositions)
			{
				finalText += node.position.x + " " + node.position.y + '\n';
			}
			
			// Removes final \n which causes error with pointtextbox
			finalText = finalText.Substring(0, finalText.Length-1);
			PointsTextBox.Text = finalText;
		}

		public void UpdateLinesTextBox(List<TSPGraphNode> nodePositions)
		{
			string finalText = "";
			foreach(TSPGraphNode node in nodePositions)
			{
				finalText += node.id + " ";
			}

			LinesTextBox.Text = finalText;
		}

		public void UpdateResultsTextBox(string results)
		{
			ResultsTextBox.Text = results;
		}

		public void ShowErrorMessage(string message)
		{
			MessageBox.Show(message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		#region Button Events
		private void ImportDataButton_OnClick(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Import Button Click Event Run!");

			// Configure open file dialog box
			var dialogWindow = new Microsoft.Win32.OpenFileDialog();
			dialogWindow.FileName = "Document"; // Default file name
			dialogWindow.DefaultExt = ".txt"; // Default file extension
			dialogWindow.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

			// Show open file dialog box
			bool? result = dialogWindow.ShowDialog();

			string filename = "";
			// Process open file dialog box results
			if (result == true)
			{
				// Document Path
				filename = dialogWindow.FileName;
			}

			tspManager.ImportData(filename);
		}

		private void ExportDataButton_OnClick(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Export Button Click Event Run!");

			// Configure save file dialog box
			var dialogWindow = new Microsoft.Win32.SaveFileDialog();
			dialogWindow.FileName = "Document"; // Default file name
			dialogWindow.DefaultExt = ".txt"; // Default file extension
			dialogWindow.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

			// Show save file dialog box
			bool? result = dialogWindow.ShowDialog();

			string filename = "";
			// Process save file dialog box results
			if (result == true)
			{
				// Save document
				filename = dialogWindow.FileName;
			}

			tspManager.ExportData(filename, PointsTextBox.Text);
		}

		private void StepBackwardsButton_OnClick(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Step Backwards Button Click Event Run!");
			tspManager.StepBackwards();
		}

		private void StepForwardsButton_OnClick(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Step Forwards Button Click Event Run!");
			tspManager.StepForward();
		}

		private void CreateRandomGraphButton_OnClick(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Create Random Graph Button Click Event Run!");

			tspManager.GenerateRandomGraph();
		}

		private void ClearGraphButton_OnClick(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Clear Graph Button Click Event Run!");

			PointsTextBox.Text = "";
			TSPGraphPath.Data = null;
			TSPGraphNodes.Data = null;
			LinesTextBox.Text = "";
			ResultsTextBox.Text = "";
			NodeNumbersGrid.Children.Clear();
			StepBackwardsButton.IsEnabled = false;
			StepForwardsButton.IsEnabled = false;
		}

		private void StartButton_OnClick(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Start Button Click Event Run!");
			tspManager.Start();
		}
		#endregion
	}
}
