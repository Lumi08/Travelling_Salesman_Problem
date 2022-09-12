using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace TSP
{
	public struct Vector2D
	{
		public int x;
		public int y;

		public Vector2D(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public struct TSPGraphNode
	{
		public int id;
		public Vector2D position;

		public void Print()
		{
			Debug.WriteLine(id + " " + position.x + " " + position.y);
		}
	}

	public class TSPManager
	{
		// WPF windows
		public MainWindow mainWindow;

		// Managers
		private FileManager fileManager;
		private GeneticAlgorithmManager gaManager;
		private AntColonyManager acManager;

		// Other vars
		public Random random;
		public int currentStepPosition = 0;
		public Stopwatch timer;
		
		public TSPManager()
		{
			// Initialise the window
			mainWindow = new MainWindow(this);
			// initialise random for best pseduo random
			random = new Random();
			// initialise timer
			timer = new Stopwatch();
			//initialise the managers
			fileManager = new FileManager();
			gaManager = new GeneticAlgorithmManager(this);
			acManager = new AntColonyManager(this);
			// Show the window to the screen
			mainWindow.ShowDialog();
		}

		public void Start()
		{
			
			// Original data from the text box
			string textBoxData = mainWindow.PointsTextBox.Text;
			// Each set of coordinates
			string[] coordinateSets = textBoxData.Split('\n');

			List<TSPGraphNode> nodePositions = new List<TSPGraphNode>();

			// For each set of coordinates we want to extract the individual coordinates
			for (int i = 0; i < coordinateSets.Length; i++)
			{
				// Seperate the x and y from the coordinate set
				string[] coords = coordinateSets[i].Split(' ');

				// Makes sure we have 2 coordinates
				if (coords.Length != 2)
				{
					//MessageBox.Show("Error: PointTextBox Only Accepts 2 Coordinates Per Line", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
					mainWindow.ShowErrorMessage("Error: PointTextBox Only Accepts 2 Coordinates Per Line");
					return;
				}

				// makes sure the X and Y coordinates given are valid integers
				if (Int32.TryParse(coords[0], out int x) && Int32.TryParse(coords[1], out int y))
				{
					TSPGraphNode node = new TSPGraphNode();
					node.id = i + 1;
					node.position.x = x;
					node.position.y = y;
					nodePositions.Add(node);
				}
				else
				{
					mainWindow.ShowErrorMessage("Error: Not a valid Int in PointsTextBox");
					//MessageBox.Show("Error: Not a valid Int in PointsTextBox", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			int algorithmSelection = mainWindow.AlgorithmsComboBox.SelectedIndex;
			if(algorithmSelection == -1)
			{
				mainWindow.ShowErrorMessage("Error: Not algorithm selected");
				return;
			}
			else if (algorithmSelection == 0)
			{
				currentStepPosition = 0;
				
				// Make sure our timer is at 0
				timer.Reset();
				
				// Start timer
				timer.Start();
				// Run algorithm
				gaManager.Initialise(nodePositions);
				gaManager.MainLoop();
				// Stop timer
				timer.Stop();

				// Add time to results
				string resultsString = gaManager.GetResults();
				resultsString += "\nTime Elapsed: " + timer.ElapsedMilliseconds;

				mainWindow.UpdateUI(gaManager.GetFinalPath());
				mainWindow.UpdateResultsTextBox(resultsString);

				mainWindow.StepBackwardsButton.IsEnabled = true;
				mainWindow.StepForwardsButton.IsEnabled = true;
			}
			else if (algorithmSelection == 1)
			{
				currentStepPosition = 0;
				timer.Reset();

				timer.Start();
				acManager.Initialise(nodePositions);
				acManager.MainLoop();
				timer.Stop();

				string resultsString = acManager.GetResults();
				resultsString += "\nTime Elapsed: " + timer.ElapsedMilliseconds;

				mainWindow.UpdateUI(acManager.GetFinalPath());
				mainWindow.UpdateResultsTextBox(resultsString);

				mainWindow.StepBackwardsButton.IsEnabled = true;
				mainWindow.StepForwardsButton.IsEnabled = true;
			}
		}

		public void StepForward()
		{
			if(mainWindow.AlgorithmsComboBox.SelectedIndex == 0)
			{
				if (currentStepPosition < gaManager.currentPopulation.bestChromosomePerGeneration.Count - 1)
				{
					currentStepPosition++;
				}

				DisplayGAStepData();
			}
			else if(mainWindow.AlgorithmsComboBox.SelectedIndex == 1)
			{
				if(currentStepPosition < acManager.bestAntPerIteration.Count - 1)
				{
					currentStepPosition++;
				}

				DisplayACStepData();
			}
			
		}

		public void StepBackwards()
		{
			if(currentStepPosition > 0)
			{
				currentStepPosition--;
			}

			if (mainWindow.AlgorithmsComboBox.SelectedIndex == 0)
			{
				DisplayGAStepData();
			}
			else if (mainWindow.AlgorithmsComboBox.SelectedIndex == 1) 
			{
				DisplayACStepData();
			}
		}

		public void DisplayGAStepData()
		{
			Chromosome bestForGeneration = gaManager.GetBestChromsomeFromGeneration(currentStepPosition);

			mainWindow.UpdateUI(bestForGeneration.cities);

			string resultsData = "======= Results =======";
			resultsData += "\nCurrent Generation: " + currentStepPosition;
			resultsData += "\nFitness: " + bestForGeneration.Fitness() * 10000;
			resultsData += "\nDistance: " + bestForGeneration.GetTotalDistance();

			mainWindow.UpdateResultsTextBox(resultsData);
			mainWindow.UpdateGraph(bestForGeneration.cities);
		}

		public void DisplayACStepData()
		{
			Ant bestAntForIteration = acManager.bestAntPerIteration[currentStepPosition];

			string resultsData = "======= Results =======";
			resultsData += "\nCurrent Iteration: " + currentStepPosition;
			resultsData += "\nDistance: " + bestAntForIteration.GetTotalDistance();

			mainWindow.UpdateResultsTextBox(resultsData);
			mainWindow.UpdateGraph(bestAntForIteration.currentTour);
		}

		public void ImportData(string path)
		{
			List<TSPGraphNode> vectors = fileManager.ImportFromFile(path);

			if(vectors != null)
			{
				if (vectors.Count == 0)
				{
					mainWindow.ShowErrorMessage("Error Importing from File");
				}
				else
				{
					mainWindow.UpdateUI(vectors);
				}
			}
		}

		public void ExportData(string path, string dataToSave)
		{
			fileManager.ExportToFile(path, dataToSave);
		}

		public void GenerateRandomGraph()
		{
			List<TSPGraphNode> newNodes = new List<TSPGraphNode>();
			int numOfNodes = random.Next(5, 50);

			for(int i = 0; i < numOfNodes; i++)
			{
				TSPGraphNode temp = new TSPGraphNode();
				temp.id = i + 1;
				temp.position.x = random.Next(20, 740);
				temp.position.y = random.Next(20, 350);

				newNodes.Add(temp);
			}

			mainWindow.UpdateUI(newNodes);
		}
	}
}
