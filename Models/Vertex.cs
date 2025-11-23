using System;
using System.Collections.Generic;
using System.Drawing;

namespace GrafosApp.Models
{
    /// <summary>
    /// Representa um vértice de um grafo
    /// </summary>
    public class Vertex
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public Point Position { get; set; }
        public Color Color { get; set; }
        public List<Edge> Edges { get; set; }

        public Vertex(int id, string label = null)
        {
            Id = id;
            Label = label ?? id.ToString();
            Position = new Point(0, 0);
            Color = Color.LightYellow;
            Edges = new List<Edge>();
        }

        public override string ToString()
        {
            return Label;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex vertex)
                return Id == vertex.Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

