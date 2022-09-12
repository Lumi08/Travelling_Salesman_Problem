using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace TSP
{
	public class Population
	{
		public int generationCount = 0;

		public List<double> pastFitness;
		public List<Chromosome> chromosomes;
		public List<Chromosome> bestChromosomePerGeneration;
		private Random random;
		private int mutationChance;

		public Population(Random random, int mutationChance)
		{
			pastFitness = new List<double>();
			chromosomes = new List<Chromosome>();
			bestChromosomePerGeneration = new List<Chromosome>();
			this.random = random;
			this.mutationChance = mutationChance;
		}

		public void Fitness()
		{
			sortChromosomes(chromosomes, chromosomes.Count);

			for(int i = 0; i < chromosomes.Count; i++)
			{
				chromosomes[i].rank = i + 1;
			}

			bestChromosomePerGeneration.Add(chromosomes[chromosomes.Count - 1]);

			//Debug.WriteLine("===== Best Chromosome =====");
			//chromosomes[chromosomes.Count - 1].Print();
		}

		public bool GenerateNewPopulation()
		{
			if(pastFitness.Count == chromosomes[0].cities.Count)
			{
				pastFitness.RemoveAt(0);
				pastFitness.Add(chromosomes[chromosomes.Count - 1].Fitness());
				
				if(pastFitness.Distinct().Count() == 1)
				{
					return false;
				}
			}
			else
			{
				pastFitness.Add(chromosomes[chromosomes.Count - 1].Fitness());
			}


			// Elitism 
			Chromosome bestChromosome = chromosomes[chromosomes.Count - 1];
			Chromosome secondBestChromosome = chromosomes[chromosomes.Count - 1];

			chromosomes[0] = bestChromosome;
			chromosomes[1] = secondBestChromosome;
			
			for (int i = 2; i < chromosomes.Count; i += 2)
			{
				Chromosome parent1 = RankSelection();
				Chromosome parent2 = RankSelection();

				Chromosome offspring1 = new Chromosome()
				{
					cities = new List<TSPGraphNode>(parent1.cities.Count)
				};
				Chromosome offspring2 = new Chromosome()
				{
					cities = new List<TSPGraphNode>(parent2.cities.Count)
				};

				Reproduce(parent1, parent2, ref offspring1, ref offspring2);

				chromosomes[i] = offspring1;
				if(i != chromosomes.Count - 1)
				{
					chromosomes[i+1] = offspring2;
				}
			}

			generationCount++;
			return true;
		}

		public void Reproduce(Chromosome parent1, Chromosome parent2, ref Chromosome offspring1, ref Chromosome offspring2)
		{
			int startCuttingPoint = parent1.cities.Count / 3;
			int endCuttingPoint = parent2.cities.Count - startCuttingPoint;

			List<int> offspring1TravelledPoints = new List<int>();
			List<int> offspring2TravelledPoints = new List<int>();

			// False node to avoid duplicate genes
			TSPGraphNode falseNode = new TSPGraphNode()
			{
				id = -1,
				position = new Vector2D(-1, -1)
			};

			// Fills in middle part of offsprings
			for (int i = startCuttingPoint; i < endCuttingPoint; i++)
			{
				offspring1.cities.Add(parent2.cities[i]);
				offspring1TravelledPoints.Add(parent2.cities[i].id);
				offspring2.cities.Add(parent1.cities[i]); 
				offspring2TravelledPoints.Add(parent1.cities[i].id);
			}

			// Fill in 1st part
			for(int i = 0; i < startCuttingPoint; i++)
			{
				// Offspring 1
				if (offspring1TravelledPoints.Contains(parent1.cities[i].id))
				{
					offspring1.cities.Insert(i, falseNode);
				}
				else
				{
					offspring1.cities.Insert(i, parent1.cities[i]);
					offspring1TravelledPoints.Add(parent1.cities[i].id);
				}

				// Offspring 2
				if (offspring2TravelledPoints.Contains(parent2.cities[i].id))
				{
					offspring2.cities.Insert(i, falseNode);
				}
				else
				{
					offspring2.cities.Insert(i, parent2.cities[i]);
					offspring2TravelledPoints.Add(parent2.cities[i].id);
				}
			}

			// Fill in last part
			for (int i = endCuttingPoint; i < parent1.cities.Count; i++)
			{
				// Offspring 1
				if (offspring1TravelledPoints.Contains(parent1.cities[i].id))
				{
					offspring1.cities.Insert(i, falseNode);
				}
				else
				{
					offspring1.cities.Insert(i, parent1.cities[i]);
					offspring1TravelledPoints.Add(parent1.cities[i].id);
				}

				// Offspring 2
				// Offspring 1
				if (offspring2TravelledPoints.Contains(parent2.cities[i].id))
				{
					offspring2.cities.Insert(i, falseNode);
				}
				else
				{
					offspring2.cities.Insert(i, parent2.cities[i]);
					offspring2TravelledPoints.Add(parent2.cities[i].id);
				}
			}

			// Fix Offspring1 Genes
			for(int i = 0; i < offspring1.cities.Count; i++)
			{
				// Loop through and find -1s
				if (offspring1.cities[i].id == -1)
				{
					// loop through all node ids
					for (int j = 1; j < offspring1.cities.Count + 1; j++)
					{
						// If its missing a node
						if (!offspring1TravelledPoints.Contains(j))
						{
							// Add it to path
							int index = parent1.cities.FindIndex(city => city.id == j);
							offspring1.cities[i] = parent1.cities[index];
							offspring1TravelledPoints.Add(j);
							break;
						}
					}
				}
			}

			// Fix Offspring2 Genes
			for (int i = 0; i < offspring2.cities.Count; i++)
			{
				// Loop through and find -1s
				if (offspring2.cities[i].id == -1)
				{
					// loop through all node ids
					for (int j = 1; j < offspring2.cities.Count + 1; j++)
					{
						// If its missing a node
						if (!offspring2TravelledPoints.Contains(j))
						{
							// Add it to path
							int index = parent2.cities.FindIndex(city => city.id == j);
							offspring2.cities[i] = parent2.cities[index];
							offspring2TravelledPoints.Add(j);
							break;
						}
					}
				}
			}

			int mutationRoll = random.Next(1, 100);
			if(mutationRoll <= mutationChance)
			{
				offspring1.Mutate(random);
			}
			mutationRoll = random.Next(1, 100);
			if (mutationRoll <= mutationChance)
			{
				offspring2.Mutate(random);
			}
		}

		public Chromosome RankSelection()
		{
			Chromosome selectedChromosome = new Chromosome();
			int maxWheelSize = 0;
			for(int i = 0; i < chromosomes.Count; i++)
			{
				maxWheelSize += chromosomes[i].rank;
			}

			int selectedNum = random.Next(1, maxWheelSize);
			int wheelTotal = 0;
			for(int i = 0; i < chromosomes.Count; i++)
			{
				wheelTotal += chromosomes[i].rank;

				if(wheelTotal > selectedNum)
				{
					selectedChromosome = chromosomes[i];
					break;
				}
			}

			return selectedChromosome;
		}

		public void sortChromosomes(List<Chromosome> cs, int num)
		{
			bool swapped = true;
			Chromosome temp;

			while (swapped)
			{
				swapped = false;
				for (int i = 0; i < num - 1; i++)
				{
					if (cs[i].Fitness() > cs[i + 1].Fitness())
					{
						temp = cs[i];
						cs[i] = cs[i + 1];
						cs[i + 1] = temp;
						swapped = true;
					}
				}
			}
		}

		public void Finish()
		{
			pastFitness.Clear();
		}

		public Chromosome GetFittest()
		{
			return bestChromosomePerGeneration.Last();
		}

		public void Print()
		{
			Debug.WriteLine("====================================");
			foreach (Chromosome c in chromosomes)
			{
				Debug.WriteLine("===========");
				c.Print();
			}
		}
	}

	public class Chromosome
	{
		public List<TSPGraphNode> cities;
		public int rank;

		public Chromosome()
		{
			rank = 0;
			cities = new List<TSPGraphNode>();
		}

		public double Fitness()
		{
			double distanceTravelled = GetTotalDistance();

			return 1 / distanceTravelled;
		}

		public void Mutate(Random random)
		{
			int randomNum = random.Next(2);

			if(randomNum == 0)
			{
				SwapMutate(random);
			}
			else
			{
				ReverseMutate(random);
			}

		}

		public void SwapMutate(Random random)
		{
			int randomIndex1 = random.Next(cities.Count - 1);
			int randomIndex2 = random.Next(cities.Count - 1);

			// Make sure the numbers are different
			while (randomIndex1 == randomIndex2)
			{
				randomIndex2 = random.Next(cities.Count - 1);
			}

			// Perform swap
			TSPGraphNode temp = new TSPGraphNode();
			temp = cities[randomIndex1];
			cities[randomIndex1] = cities[randomIndex2];
			cities[randomIndex2] = temp;
		}

		public void ReverseMutate(Random random)
		{
			int randomIndex1 = random.Next(cities.Count - 1);
			int randomIndex2 = random.Next(cities.Count - 1);

			// Make sure the numbers are different
			while (randomIndex1 == randomIndex2)
			{
				randomIndex2 = random.Next(cities.Count - 1);
			}

			// makesure randomIndex1 is the smaller number
			if (randomIndex2 < randomIndex1)
			{
				int temp = randomIndex1;
				randomIndex1 = randomIndex2;
				randomIndex2 = temp;
			}

			// Create a temp list holding our segment to reverse
			List <TSPGraphNode> selectedNodes = new List<TSPGraphNode>();
			for(int i = randomIndex1; i <= randomIndex2; i++)
			{
				selectedNodes.Add(cities[i]);
			}

			// perform the reverse and add it to original chromosome
			int n = selectedNodes.Count -1;
			for(int i = randomIndex1; i <= randomIndex2; i++)
			{
				cities[i] = selectedNodes[n];
				n--;
			}
		}

		public double GetTotalDistance()
		{
			double distanceTravelled = 0;
			Vector2D point1;
			Vector2D point2;
			int xDiff;
			int yDiff;
			double distance;

			for (int i = 0; i < cities.Count - 1; i++)
			{
				point1 = cities[i].position;
				point2 = cities[i + 1].position;

				xDiff = Math.Abs(point1.x - point2.x);
				yDiff = Math.Abs(point1.y - point2.y);

				distance = Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));

				distanceTravelled += distance;
			}

			// End to Start
			point1 = cities[cities.Count - 1].position;
			point2 = cities[0].position;

			xDiff = Math.Abs(point1.x - point2.x);
			yDiff = Math.Abs(point1.y - point2.y);

			distanceTravelled += Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));
			return distanceTravelled;
		}

		public void Print()
		{
			foreach (TSPGraphNode node in cities)
			{
				node.Print();
			}

			Debug.WriteLine(rank + " " + Fitness());
		}
	}

	public class GeneticAlgorithmManager
	{
		public Population currentPopulation;
		public List<TSPGraphNode> originalGraph;
		private TSPManager tspManager;
		private int populationSize = 100;
		private int mutationChance = 20;

		public GeneticAlgorithmManager(TSPManager tsp)
		{
			tspManager = tsp;
			currentPopulation = new Population(tsp.random, mutationChance);
		}

		public void Initialise(List<TSPGraphNode> nodes)
		{
			Clear();

			originalGraph = nodes;
			for(int i = 0; i < populationSize; i++)
			{
				ShuffleCities(nodes);

				Chromosome c = new Chromosome()
				{
					cities = new List<TSPGraphNode>(nodes)
				};
				currentPopulation.chromosomes.Add(c);
			}
			currentPopulation.Fitness();
		}

		public void MainLoop()
		{
			while (currentPopulation.GenerateNewPopulation())
			{
				currentPopulation.Fitness();
			}

			currentPopulation.Finish();
		}

		public Chromosome GetBestChromsomeFromGeneration(int generation)
		{
			return currentPopulation.bestChromosomePerGeneration.ElementAt(generation);
		}

		public List<TSPGraphNode> GetFinalPath()
		{
			return currentPopulation.bestChromosomePerGeneration.Last().cities;
		}

		public string GetResults()
		{
			string returnString = "======= Results =======";
			returnString += "\nTotal Generations: " + currentPopulation.generationCount;
			returnString += "\nFitness: " + currentPopulation.GetFittest().Fitness() * 10000;
			returnString += "\nRoute Length: " + (int)currentPopulation.GetFittest().GetTotalDistance();

			return returnString;
		}

		public void Clear()
		{
			currentPopulation.chromosomes.Clear();
			currentPopulation.bestChromosomePerGeneration.Clear();
			currentPopulation.pastFitness.Clear();
			currentPopulation.generationCount = 0;
		}

		public void ShuffleCities(List<TSPGraphNode> cityList)
		{
			int numOfCities = cityList.Count;
			while(numOfCities > 0)
			{
				numOfCities--;
				int randomNum = tspManager.random.Next(numOfCities + 1);
				TSPGraphNode swappedNode = cityList[randomNum];
				cityList[randomNum] = cityList[numOfCities];
				cityList[numOfCities] = swappedNode;
			}
		}
	}
}
