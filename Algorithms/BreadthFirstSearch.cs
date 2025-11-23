using System;
using System.Collections.Generic;
using System.Linq;
using GrafosApp.Models;

namespace GrafosApp.Algorithms
{
    /// <summary>
    /// Implementa busca em largura (BFS) a partir de um vértice
    /// </summary>
    public class BreadthFirstSearch : IGraphAlgorithm
    {
        public string Name => "Busca em Largura (BFS)";
        public string Description => "Imprime o caminho em largura a partir de um vértice";

        public class BFSResult
        {
            public List<Vertex> Path { get; set; }
            public Dictionary<Vertex, int> Distance { get; set; }
            public Dictionary<Vertex, Vertex> Parent { get; set; }

            public BFSResult()
            {
                Path = new List<Vertex>();
                Distance = new Dictionary<Vertex, int>();
                Parent = new Dictionary<Vertex, Vertex>();
            }
        }

        public BFSResult Execute(Graph graph, Vertex startVertex)
        {
            var result = new BFSResult();

            if (graph.Vertices.Count == 0 || startVertex == null)
                return result;

            var visited = new HashSet<Vertex>();
            var queue = new Queue<Vertex>();

            visited.Add(startVertex);
            queue.Enqueue(startVertex);
            result.Distance[startVertex] = 0;
            result.Parent[startVertex] = null;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Path.Add(current);

                var neighbors = graph.IsDirected
                    ? graph.GetEdgesFromVertex(current).Select(e => e.To)
                    : graph.GetNeighbors(current);

                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                        result.Distance[neighbor] = result.Distance[current] + 1;
                        result.Parent[neighbor] = current;
                    }
                }
            }

            return result;
        }
    }
}

