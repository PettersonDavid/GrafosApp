using System;
using System.Collections.Generic;
using System.Linq;
using GrafosApp.Models;

namespace GrafosApp.Algorithms
{
    /// <summary>
    /// Implementa o algoritmo para encontrar o fecho transitivo de um vértice
    /// </summary>
    public class TransitiveClosure : IGraphAlgorithm
    {
        public string Name => "Fecho Transitivo";
        public string Description => "Encontra o fecho transitivo direto e inverso de um vértice";

        public class ClosureResult
        {
            public List<Vertex> DirectClosure { get; set; }
            public List<Vertex> InverseClosure { get; set; }

            public ClosureResult()
            {
                DirectClosure = new List<Vertex>();
                InverseClosure = new List<Vertex>();
            }
        }

        public ClosureResult Execute(Graph graph, Vertex vertex)
        {
            var result = new ClosureResult();

            if (graph.Vertices.Count == 0 || vertex == null)
                return result;

            // Fecho transitivo direto (vértices alcançáveis a partir de vertex)
            result.DirectClosure = GetReachableVertices(graph, vertex, true);

            // Fecho transitivo inverso (vértices que alcançam vertex)
            result.InverseClosure = GetReachableVertices(graph, vertex, false);

            return result;
        }

        private List<Vertex> GetReachableVertices(Graph graph, Vertex start, bool forward)
        {
            var visited = new HashSet<Vertex>();
            var stack = new Stack<Vertex>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (visited.Contains(current))
                    continue;

                visited.Add(current);

                if (forward)
                {
                    var neighbors = graph.GetEdgesFromVertex(current).Select(e => e.To);
                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                            stack.Push(neighbor);
                    }
                }
                else
                {
                    var neighbors = graph.GetEdgesToVertex(current).Select(e => e.From);
                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                            stack.Push(neighbor);
                    }
                }
            }

            visited.Remove(start); // Remove o vértice inicial da lista
            return visited.ToList();
        }
    }
}

