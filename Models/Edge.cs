using System.Drawing;

namespace GrafosApp.Models
{
    /// <summary>
    /// Representa uma aresta de um grafo
    /// </summary>
    public class Edge
    {
        public Vertex From { get; set; }
        public Vertex To { get; set; }
        public double Weight { get; set; }
        public bool IsDirected { get; set; }
        public Color Color { get; set; }
        public bool IsHighlighted { get; set; }

        public Edge(Vertex from, Vertex to, double weight = 1.0, bool isDirected = false)
        {
            From = from;
            To = to;
            Weight = weight;
            IsDirected = isDirected;
            Color = Color.Black;
            IsHighlighted = false;
        }

        public override string ToString()
        {
            return $"{From.Label} -> {To.Label} ({Weight})";
        }
    }
}

