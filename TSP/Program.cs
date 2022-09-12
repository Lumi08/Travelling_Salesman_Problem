using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Debug
using System.Diagnostics;

namespace TSP
{
	internal class Program
	{
		[STAThread]
		static void Main()
		{
			// Debug to show this is ran before showing the window
			Debug.WriteLine("this ran!");
			// Initialise the TSPManager
			TSPManager tsp = new TSPManager();
		}
	}
}

