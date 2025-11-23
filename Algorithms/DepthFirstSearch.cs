using System;
using System.Collections.Generic;
using System.Linq;
using GrafosApp.Models;

namespace GrafosApp.Algorithms
{
    /// <summary>
    /// Implementa busca em profundidade (DFS) a partir de um vértice
    /// </summary>
    public class DepthFirstSearch : IGraphAlgorithm
    {
        public string Name => "Busca em Profundidade (DFS)";
        public string Description => "Imprime o caminho em profundidade a partir de um vértice";

        public class DFSResult
        {
            public List<Vertex> Path { get; set; }
            public Dictionary<Vertex, int> DiscoveryTime { get; set; }
            public Dictionary<Vertex, int> FinishTime { get; set; }

            public DFSResult()
            {
                Path = new List<Vertex>();
                DiscoveryTime = new Dictionary<Vertex, int>();
                FinishTime = new Dictionary<Vertex, int>();
            }
        }

        public DFSResult Execute(Graph graph, Vertex startVertex)
        {
            var result = new DFSResult();

            if (graph.Vertices.Count == 0 || startVertex == null)
                return result;

            var visited = new HashSet<Vertex>();
            int time = 0;

            DFSVisit(graph, startVertex, visited, result, ref time);

            // Visita vértices não conectados (para grafos não conectados)
            foreach (var vertex in graph.Vertices)
            {
                if (!visited.Contains(vertex))
                {
                    DFSVisit(graph, vertex, visited, result, ref time);
                }
            }

            return result;
        }

        private void DFSVisit(Graph graph, Vertex vertex, HashSet<Vertex> visited, 
            DFSResult result, ref int time)
        {
            visited.Add(vertex);
            result.Path.Add(vertex);
            result.DiscoveryTime[vertex] = time++;

            var neighbors = graph.IsDirected
                ? graph.GetEdgesFromVertex(vertex).Select(e => e.To)
                : graph.GetNeighbors(vertex);

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    DFSVisit(graph, neighbor, visited, result, ref time);
                }
            }

            result.FinishTime[vertex] = time++;
        }
    }
}

