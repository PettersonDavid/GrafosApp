using System;
using System.Collections.Generic;
using System.Linq;
using GrafosApp.Models;

namespace GrafosApp.Algorithms
{
    /// <summary>
    /// Implementa o algoritmo de Dijkstra para encontrar o caminho mínimo
    /// </summary>
    public class ShortestPath : IGraphAlgorithm
    {
        public string Name => "Caminho Mínimo (Dijkstra)";
        public string Description => "Encontra o caminho mínimo entre dois vértices usando Dijkstra";

        public class PathResult
        {
            public List<Vertex> Path { get; set; }
            public double TotalWeight { get; set; }
            public bool Exists { get; set; }

            public PathResult()
            {
                Path = new List<Vertex>();
                TotalWeight = double.MaxValue;
                Exists = false;
            }
        }

        public PathResult Execute(Graph graph, Vertex source, Vertex target)
        {
            var result = new PathResult();

            if (graph.Vertices.Count == 0 || source == null || target == null)
                return result;

            var distances = new Dictionary<Vertex, double>();
            var previous = new Dictionary<Vertex, Vertex>();
            var unvisited = new HashSet<Vertex>(graph.Vertices);

            foreach (var vertex in graph.Vertices)
            {
                distances[vertex] = double.MaxValue;
            }
            distances[source] = 0;

            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(v => distances[v]).FirstOrDefault();
                if (current == null || distances[current] == double.MaxValue)
                    break;

                unvisited.Remove(current);

                if (current == target)
                    break;

                var neighbors = graph.IsDirected 
                    ? graph.GetEdgesFromVertex(current).Select(e => e.To)
                    : graph.GetNeighbors(current);

                foreach (var neighbor in neighbors)
                {
                    if (!unvisited.Contains(neighbor))
                        continue;

                    var edge = graph.Edges.FirstOrDefault(e =>
                        (e.From == current && e.To == neighbor) ||
                        (!graph.IsDirected && e.From == neighbor && e.To == current));

                    if (edge == null)
                        continue;

                    var alt = distances[current] + edge.Weight;
                    if (alt < distances[neighbor])
                    {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                    }
                }
            }

            if (distances[target] < double.MaxValue)
            {
                result.Exists = true;
                result.TotalWeight = distances[target];

                var path = new List<Vertex>();
                var current = target;
                while (current != null)
                {
                    path.Insert(0, current);
                    previous.TryGetValue(current, out current);
                    if (current == source)
                    {
                        path.Insert(0, source);
                        break;
                    }
                }

                result.Path = path;
            }

            return result;
        }
    }
}

