using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace TSP
{
	public class Path
	{
		public TSPGraphNode pointA;
		public TSPGraphNode pointB;
		public double pheromoneLevel;

		public Path(TSPGraphNode a, TSPGraphNode b)
		{
			pointA = a;
			pointB = b;
			pheromoneLevel = 0;
		}

		public void Print()
		{
			Debug.WriteLine(pointA.id + " To " + pointB.id + ": " + pheromoneLevel);
		}

	}


	public class Ant
	{
		public AntColonyManager manager;

		public List<TSPGraphNode> currentTour;

		public Ant(AntColonyManager manager)
		{
			this.manager = manager;
			currentTour = new List<TSPGraphNode>();

		}

		public void Move(List<Path> paths)
		{
			TSPGraphNode nextLocation = ChooseNextLocation(paths);

			Path findPath = null;

			while(nextLocation.id != currentTour.First().id)
			{
				findPath = manager.paths.Find(path => path.pointA.id == nextLocation.id && path.pointB.id == currentTour.Last().id);
				if (findPath == null)
				{
					// Try to find path other direction
					findPath = manager.paths.Find(path => path.pointA.id == currentTour.Last().id && path.pointB.id == nextLocation.id);

					if (findPath == null)
					{
						// No path has been found so make one
						findPath = new Path(currentTour.Last(), nextLocation);
						manager.paths.Add(findPath);
					}
				}

				currentTour.Add(nextLocation);

				nextLocation = ChooseNextLocation(paths);
			}

			findPath = manager.paths.Find(path => path.pointA.id == nextLocation.id && path.pointB.id == currentTour.Last().id);
			if (findPath == null)
			{
				// Try to find path other direction
				findPath = manager.paths.Find(path => path.pointA.id == currentTour.Last().id && path.pointB.id == nextLocation.id);

				if (findPath == null)
				{
					// No path has been found so make one
					findPath = new Path(currentTour.Last(), nextLocation);
					manager.paths.Add(findPath);
				}
			}
		}

		public void AddPheromone()
		{
			double totalDistance = GetTotalDistance();

			Path pathToDecay = null;
			for (int i = 0; i < currentTour.Count - 1; i++)
			{

				pathToDecay = manager.GetPath(currentTour[i], currentTour[i + 1]);

				pathToDecay.pheromoneLevel += manager.pheromoneConstant / totalDistance;
			}

			pathToDecay = manager.GetPath(currentTour.Last(), currentTour.First());

			pathToDecay.pheromoneLevel += manager.pheromoneConstant / totalDistance;


		}

		public TSPGraphNode ChooseNextLocation(List<Path> paths)
		{
			double[] distanceToNode = new double[manager.originalMap.Count];
			List<double> pheromoneLevels = new List<double>(manager.originalMap.Count);

			for(int i = 0; i < manager.originalMap.Count; i++)
			{
				TSPGraphNode node = manager.originalMap[i];

				if (!currentTour.Contains(node))
				{
					distanceToNode[i] = CalculateDistance(currentTour.Last(), node);

					// Try to find path
					Path findPath = paths.Find(path => path.pointA.id == node.id && path.pointB.id == currentTour.Last().id);
					if (findPath == null)
					{
						// Try to find path other direction
						findPath = paths.Find(path => path.pointA.id == currentTour.Last().id && path.pointB.id == node.id);

						if (findPath == null)
						{
							pheromoneLevels.Add(0);
						}
						else
						{
							pheromoneLevels.Add(findPath.pheromoneLevel);
						}
					}
					else
					{
						pheromoneLevels.Add(findPath.pheromoneLevel);
					}
				}
				else
				{
					pheromoneLevels.Add(0);
					distanceToNode[i] = -1;
				}
			}

			if(pheromoneLevels.All(x=> x == 0))
			{
				for (int i = 0; i < pheromoneLevels.Count; i++)
				{
					pheromoneLevels.RemoveAt(i);
					pheromoneLevels.Insert(i, 1);
				}
			}

			double maxWheelSize = 0;
			for(int i = 0; i < distanceToNode.Length; i++)
			{
				if (distanceToNode[i] != -1)
				{
					maxWheelSize += Math.Pow(pheromoneLevels[i], manager.alpha) * Math.Pow(1 / distanceToNode[i], manager.beta);
				}
			}

			double selectedNum = manager.tspManager.random.NextDouble() * (maxWheelSize);
			double wheelTotal = 0;
			int selectedIndex = -1;
			for(int i = 0; i < distanceToNode.Length; i++)
			{
				if(distanceToNode[i] != -1)
				{
					wheelTotal += Math.Pow(pheromoneLevels[i], manager.alpha) * Math.Pow(1 / distanceToNode[i], manager.beta);
					if (wheelTotal > selectedNum)
					{
						selectedIndex = i;
						break;
					}
				}
			}

			// has no more nodes and needs to return to start
			if(selectedIndex == -1)
			{
				return currentTour.First();
			}

			//Debug.WriteLine("Node Selected: " + manager.originalMap[selectedIndex].id);

			return manager.originalMap[selectedIndex];
		}

		public void Reset()
		{
			TSPGraphNode startnode = currentTour.First();

			currentTour.Clear();
			currentTour.Add(startnode);
		}

		public double CalculateDistance(TSPGraphNode pointA, TSPGraphNode pointB)
		{
			int xDiff;
			int yDiff;
			double distance;

			xDiff = Math.Abs(pointA.position.x - pointB.position.x);
			yDiff = Math.Abs(pointA.position.y - pointB.position.y);

			distance = Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));

			return distance;
		}


		public double GetTotalDistance()
		{
			double totalDistance = 0;
			// Get the total distance
			for (int i = 0; i < currentTour.Count - 1; i++)
			{
				totalDistance += CalculateDistance(currentTour[i], currentTour[i + 1]);
			}
			totalDistance += CalculateDistance(currentTour.Last(), currentTour.First());

			return totalDistance;
		}

		public void Print()
		{
			foreach(TSPGraphNode node in currentTour)
			{
				node.Print();	
			}
		}
	}


	public class AntColonyManager
	{
		public TSPManager tspManager;

		int antPerNode = 1;
		public List<TSPGraphNode> originalMap;

		public List<Ant> antColony;
		public List<Path> paths;

		public List<Ant> bestAntPerIteration;

		public double alpha = 1;
		public double beta = 1.2;
		public int pheromoneConstant = 10;
		public double decayRate = 0.25;

		public int totalIterations;

		public List<TSPGraphNode> bestDistanceRoute;
		public double lastBestDistance;
		public int iterationsSinceBestChange;


		public AntColonyManager(TSPManager tsp)
		{
			this.tspManager = tsp;
			antColony = new List<Ant>();
			paths = new List<Path>();
			bestAntPerIteration = new List<Ant>();
		}

		public void Initialise(List<TSPGraphNode> nodes)
		{
			originalMap = new List<TSPGraphNode>(nodes);
			paths.Clear();
			antColony.Clear();
			bestAntPerIteration.Clear();
			lastBestDistance = double.MaxValue;
			iterationsSinceBestChange = 0;
			totalIterations = 0;

			for(int i = 0; i < nodes.Count; i++)
			{
				
				for(int j = 0; j < antPerNode; j++)
				{
					Ant ant = new Ant(this);
					ant.currentTour.Add(nodes[i]);
					antColony.Add(ant);
				}
			}
		}

		public void MainLoop()
		{
			while(GenerateNewIteration())
			{
				List<Path> pathsCopy = new List<Path>(paths);

				foreach(Ant ant in antColony)
				{
					ant.Reset();
					ant.Move(pathsCopy);
					ant.AddPheromone();
				}

				ApplyPheromoneDecay();
				CalculateBestAnt();

				totalIterations++;
			}
		}

		public bool GenerateNewIteration()
		{
			if (iterationsSinceBestChange >= originalMap.Count)
			{
				return false;
			}
			return true;
		}

		public void CalculateBestAnt()
		{
			double bestDistance = int.MaxValue;
			int bestAntIndex = -1;
			for(int i = 0; i < antColony.Count; i++)
			{
				double currentAntDist = antColony[i].GetTotalDistance();
				if (currentAntDist < bestDistance)
				{
					bestDistance = currentAntDist;
					bestAntIndex = i;
				}
			}

			Ant a = new Ant(this)
			{
				currentTour = new List<TSPGraphNode>(antColony[bestAntIndex].currentTour)
			};

			bestAntPerIteration.Add(a);

			if (bestDistance < lastBestDistance)
			{
				lastBestDistance = bestDistance;
				bestDistanceRoute = new List<TSPGraphNode>(antColony[bestAntIndex].currentTour);
				iterationsSinceBestChange = 0;
			}
			else
			{
				iterationsSinceBestChange++;
			}
		}

		public void ApplyPheromoneDecay()
		{
			foreach(Path p in paths)
			{
				double temp = decayRate * p.pheromoneLevel;
				p.pheromoneLevel = temp;
			}
		}

		public string GetResults()
		{
			string returnString = "======= Results =======";
			returnString += "\nTotal Ants: " + antColony.Count;
			returnString += "\nTotal Iterations: " + totalIterations;
			returnString += "\nRoute Length: " + (int)lastBestDistance;

			return returnString;
		}

		public List<TSPGraphNode> GetFinalPath()
		{
			return bestDistanceRoute;
		}


		public Path GetPath(TSPGraphNode a, TSPGraphNode b)
		{
			foreach(Path p in paths)
			{
				if (p.pointA.Equals(a) && p.pointB.Equals(b))
				{
					return p;
				}
				else if(p.pointB.Equals(a) && p.pointA.Equals(b))
				{
					return p;
				}
			}

			return null;
		}

		public void sortPaths(List<Path> ps)
		{
			bool swapped = true;
			Path temp;

			while (swapped)
			{
				swapped = false;
				for (int i = 0; i < ps.Count - 1; i++)
				{
					if (ps[i].pheromoneLevel < ps[i + 1].pheromoneLevel)
					{
						temp = ps[i];
						ps[i] = ps[i + 1];
						ps[i + 1] = temp;
						swapped = true;
					}
				}
			}
		}

		public void PrintColony()
		{
			Debug.WriteLine("==== Ants ====");
			for(int i = 0; i < antColony.Count; i++)
			{
				Debug.WriteLine("==== Ant " + i + " ====");
				antColony[i].Print();
			}
		}

		public void PrintPaths()
		{
			Debug.WriteLine("==== Paths ====");
			for (int i = 0; i < paths.Count; i++)
			{
				paths[i].Print();
			}
		}
	}
}
