using System;
using System.Collections.Generic;
using System.Linq;
using GrafosApp.Models;

namespace GrafosApp.Algorithms
{
    /// <summary>
    /// Implementa o algoritmo de Kruskal para encontrar a Árvore Geradora Mínima (AGM)
    /// </summary>
    public class MinimumSpanningTree : IGraphAlgorithm
    {
        public string Name => "Árvore Geradora Mínima (Kruskal)";
        public string Description => "Encontra a árvore geradora mínima usando o algoritmo de Kruskal";

        public List<Edge> Execute(Graph graph)
        {
            if (graph.Vertices.Count == 0)
                return new List<Edge>();

            // Se for dígrafo, trabalha com o grafo subjacente
            Graph workingGraph = graph.IsDirected ? graph.GetSubjacentGraph() : graph;

            var mst = new List<Edge>();
            var sortedEdges = workingGraph.Edges.OrderBy(e => e.Weight).ToList();
            var parent = new Dictionary<Vertex, Vertex>();

            // Inicializa cada vértice como seu próprio pai
            foreach (var vertex in workingGraph.Vertices)
            {
                parent[vertex] = vertex;
            }

            foreach (var edge in sortedEdges)
            {
                var rootFrom = Find(parent, edge.From);
                var rootTo = Find(parent, edge.To);

                if (rootFrom != rootTo)
                {
                    mst.Add(edge);
                    Union(parent, rootFrom, rootTo);
                }
            }

            return mst;
        }

        private Vertex Find(Dictionary<Vertex, Vertex> parent, Vertex vertex)
        {
            if (parent[vertex] != vertex)
            {
                parent[vertex] = Find(parent, parent[vertex]);
            }
            return parent[vertex];
        }

        private void Union(Dictionary<Vertex, Vertex> parent, Vertex x, Vertex y)
        {
            parent[x] = y;
        }
    }
}

