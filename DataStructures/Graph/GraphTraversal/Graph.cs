using System;
using System.Collections.Generic;

namespace GraphTraversal
{
    public class Graph
    {
        private readonly Dictionary<int, List<int>> _adjacencyList;

        public Graph()
        {
            _adjacencyList = [];
        }

        public void AddVertex(int vertex)
        {
            if (!_adjacencyList.ContainsKey(vertex))
            {
                _adjacencyList[vertex] = new List<int>();
            }
        }

        public void AddEdge(int source, int destination)
        {
            AddVertex(source);
            AddVertex(destination);

            _adjacencyList[source].Add(destination);

            // Uncomment for an undirected graph
            // _adjacencyList[destination].Add(source);
        }

        public void DFS(int startVertex)
        {
            var visited = new HashSet<int>();

            Console.Write("DFS Traversal: ");
            DFSRecursive(startVertex, visited);
            Console.WriteLine();
        }

        private void DFSRecursive(int vertex, HashSet<int> visited)
        {
            visited.Add(vertex);
            Console.Write(vertex + " ");

            foreach (var neighbor in _adjacencyList[vertex])
            {
                if (!visited.Contains(neighbor))
                {
                    DFSRecursive(neighbor, visited);
                }
            }
        }

        public void BFS(int startVertex)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<int>();

            visited.Add(startVertex);
            queue.Enqueue(startVertex);

            Console.Write("BFS Traversal: ");

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                Console.Write(current + " ");

                foreach (var neighbor in _adjacencyList[current])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            Console.WriteLine();
        }
    }
}
