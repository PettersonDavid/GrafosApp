using System;
using System.Collections.Generic;
using System.Linq;
using GrafosApp.Models;

namespace GrafosApp.Algorithms
{
    /// <summary>
    /// Calcula e imprime a sequência de graus do grafo
    /// </summary>
    public class DegreeSequence : IGraphAlgorithm
    {
        public string Name => "Sequência de Graus";
        public string Description => "Imprime a sequência de graus do grafo (ou do grafo subjacente se for dígrafo)";

        public class DegreeResult
        {
            public Dictionary<Vertex, int> VertexDegrees { get; set; }
            public List<int> DegreeSequence { get; set; }
            public Dictionary<Vertex, int> InDegrees { get; set; }
            public Dictionary<Vertex, int> OutDegrees { get; set; }

            public DegreeResult()
            {
                VertexDegrees = new Dictionary<Vertex, int>();
                DegreeSequence = new List<int>();
                InDegrees = new Dictionary<Vertex, int>();
                OutDegrees = new Dictionary<Vertex, int>();
            }
        }

        public DegreeResult Execute(Graph graph)
        {
            var result = new DegreeResult();

            if (graph.Vertices.Count == 0)
                return result;

            if (graph.IsDirected)
            {
                // Para dígrafos, calcula graus de entrada e saída
                foreach (var vertex in graph.Vertices)
                {
                    result.OutDegrees[vertex] = graph.GetEdgesFromVertex(vertex).Count;
                    result.InDegrees[vertex] = graph.GetEdgesToVertex(vertex).Count;
                }

                // Para a sequência de graus, usa o grafo subjacente
                var subjacentGraph = graph.GetSubjacentGraph();
                foreach (var vertex in subjacentGraph.Vertices)
                {
                    int degree = subjacentGraph.GetNeighbors(vertex).Count;
                    result.VertexDegrees[vertex] = degree;
                }
            }
            else
            {
                // Para grafos não dirigidos, calcula o grau normalmente
                foreach (var vertex in graph.Vertices)
                {
                    int degree = graph.GetNeighbors(vertex).Count;
                    result.VertexDegrees[vertex] = degree;
                }
            }

            // Cria a sequência de graus (ordenada decrescentemente)
            result.DegreeSequence = result.VertexDegrees.Values.OrderByDescending(d => d).ToList();

            return result;
        }
    }
}

