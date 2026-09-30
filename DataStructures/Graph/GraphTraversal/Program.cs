using System;
using GraphTraversal;

class Program
{
    static void Main()
    {
        Graph graph = new Graph();

        graph.AddEdge(1, 2);
        graph.AddEdge(1, 3);
        graph.AddEdge(2, 4);
        graph.AddEdge(2, 5);
        graph.AddEdge(3, 6);
        graph.AddEdge(3, 7);

        graph.DFS(1);
        graph.BFS(1);

        Console.ReadLine();
    }
}