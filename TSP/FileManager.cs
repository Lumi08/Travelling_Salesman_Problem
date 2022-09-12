using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
	public class FileManager
	{
		public FileManager()
		{

		}
		
		public List<TSPGraphNode> ImportFromFile(string path)
		{
			if(path.Length > 0)
			{
				List<TSPGraphNode> list = new List<TSPGraphNode>();

				using (StreamReader sr = File.OpenText(path))
				{
					string s;
					int lineNum = 0;
					while ((s = sr.ReadLine()) != null)
					{
						lineNum++;
						string[] coords = s.Split(' ');

						if (Int32.TryParse(coords[0], out int x) && Int32.TryParse(coords[1], out int y))
						{
							TSPGraphNode node = new TSPGraphNode();
							node.id = lineNum;
							node.position.x = x;
							node.position.y = y;
							list.Add(node);
						}
						else
						{
							list.Clear();
							return list;
						}
					}
				}

				return list;
			}
			return null;
		}

		public void ExportToFile(string path, string data)
		{
			if (path.Length > 0)
			{
				using (StreamWriter sw = new StreamWriter(path, false))
				{
					sw.Write(data);
				}
			}
		}
	}
}
