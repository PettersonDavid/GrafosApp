using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using GrafosApp.Models;

namespace GrafosApp.Algorithms
{
    /// <summary>
    /// Gera grafos aleatórios (dirigidos ou não dirigidos) valorados
    /// </summary>
    public class GraphGenerator
    {
        private static Random random = new Random();

        public static Graph GenerateRandomGraph(int numVertices, int numEdges, bool isDirected, bool isWeighted, double minWeight = 1.0, double maxWeight = 10.0)
        {
            var graph = new Graph(isDirected, isWeighted);
            var vertices = new List<Vertex>();

            // Cria os vértices
            for (int i = 0; i < numVertices; i++)
            {
                var vertex = new Vertex(i, $"V{i}");
                vertices.Add(vertex);
                graph.AddVertex(vertex);
            }

            // Calcula o número máximo de arestas possível
            int maxEdges = isDirected 
                ? numVertices * (numVertices - 1) 
                : numVertices * (numVertices - 1) / 2;

            numEdges = Math.Min(numEdges, maxEdges);

            // Gera as arestas
            var addedEdges = new HashSet<string>();
            int edgesAdded = 0;

            while (edgesAdded < numEdges)
            {
                int fromIndex = random.Next(numVertices);
                int toIndex = random.Next(numVertices);

                if (fromIndex == toIndex)
                    continue;

                var from = vertices[fromIndex];
                var to = vertices[toIndex];

                string edgeKey = isDirected 
                    ? $"{from.Id}-{to.Id}" 
                    : from.Id < to.Id ? $"{from.Id}-{to.Id}" : $"{to.Id}-{from.Id}";

                if (!addedEdges.Contains(edgeKey))
                {
                    double weight = isWeighted 
                        ? Math.Round(random.NextDouble() * (maxWeight - minWeight) + minWeight, 2) 
                        : 1.0;

                    graph.AddEdge(from, to, weight);
                    addedEdges.Add(edgeKey);
                    edgesAdded++;
                }
            }

            return graph;
        }
    }
}

