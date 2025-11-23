using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace GrafosApp.Models
{
    /// <summary>
    /// Representa um grafo (dirigido ou não dirigido) valorado
    /// </summary>
    public class Graph
    {
        public List<Vertex> Vertices { get; set; }
        public List<Edge> Edges { get; set; }
        public bool IsDirected { get; set; }
        public bool IsWeighted { get; set; }

        public Graph(bool isDirected = false, bool isWeighted = true)
        {
            Vertices = new List<Vertex>();
            Edges = new List<Edge>();
            IsDirected = isDirected;
            IsWeighted = isWeighted;
        }

        public void AddVertex(Vertex vertex)
        {
            if (!Vertices.Contains(vertex))
            {
                Vertices.Add(vertex);
            }
        }

        public void AddEdge(Vertex from, Vertex to, double weight = 1.0)
        {
            if (!Vertices.Contains(from) || !Vertices.Contains(to))
                return;

            var edge = new Edge(from, to, weight, IsDirected);
            Edges.Add(edge);
            from.Edges.Add(edge);

            if (!IsDirected)
            {
                var reverseEdge = new Edge(to, from, weight, false);
                to.Edges.Add(reverseEdge);
            }
        }

        public void RemoveVertex(Vertex vertex)
        {
            if (Vertices.Remove(vertex))
            {
                Edges.RemoveAll(e => e.From == vertex || e.To == vertex);
                foreach (var v in Vertices)
                {
                    v.Edges.RemoveAll(e => e.To == vertex || e.From == vertex);
                }
            }
        }

        public void RemoveEdge(Edge edge)
        {
            Edges.Remove(edge);
            edge.From.Edges.Remove(edge);
            if (!IsDirected)
            {
                edge.To.Edges.RemoveAll(e => e.To == edge.From && e.From == edge.To);
            }
        }

        public Vertex GetVertexById(int id)
        {
            return Vertices.FirstOrDefault(v => v.Id == id);
        }

        public List<Edge> GetEdgesFromVertex(Vertex vertex)
        {
            return Edges.Where(e => e.From == vertex).ToList();
        }

        public List<Edge> GetEdgesToVertex(Vertex vertex)
        {
            return Edges.Where(e => e.To == vertex).ToList();
        }

        public List<Vertex> GetNeighbors(Vertex vertex)
        {
            var neighbors = new List<Vertex>();
            foreach (var edge in Edges)
            {
                if (edge.From == vertex)
                    neighbors.Add(edge.To);
                else if (!IsDirected && edge.To == vertex)
                    neighbors.Add(edge.From);
            }
            return neighbors.Distinct().ToList();
        }

        public Graph GetSubjacentGraph()
        {
            var subjacent = new Graph(false, IsWeighted);
            foreach (var vertex in Vertices)
            {
                subjacent.AddVertex(new Vertex(vertex.Id, vertex.Label));
            }
            foreach (var edge in Edges)
            {
                if (!subjacent.Edges.Any(e => 
                    (e.From.Id == edge.From.Id && e.To.Id == edge.To.Id) ||
                    (e.From.Id == edge.To.Id && e.To.Id == edge.From.Id)))
                {
                    subjacent.AddEdge(
                        subjacent.GetVertexById(edge.From.Id),
                        subjacent.GetVertexById(edge.To.Id),
                        edge.Weight
                    );
                }
            }
            return subjacent;
        }

        public void Clear()
        {
            Vertices.Clear();
            Edges.Clear();
        }
    }
}

