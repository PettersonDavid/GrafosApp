using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GrafosApp.Models;

namespace GrafosApp.UI
{
    /// <summary>
    /// Painel customizado para desenhar o grafo
    /// </summary>
    public class GraphPanel : Panel
    {
        private Graph graph;
        private Vertex selectedVertex;
        private Vertex hoveredVertex;
        private const int BaseVertexRadius = 20;
        private const int VertexLabelOffset = 5;
        private float zoomFactor = 1.0f;

        public float ZoomFactor
        {
            get => zoomFactor;
            set
            {
                zoomFactor = value;
                Invalidate();
            }
        }

        private int VertexRadius => (int)(BaseVertexRadius * zoomFactor);

        public Graph Graph
        {
            get => graph;
            set
            {
                graph = value;
                Invalidate();
            }
        }

        public Vertex SelectedVertex
        {
            get => selectedVertex;
            set
            {
                selectedVertex = value;
                Invalidate();
            }
        }

        public Vertex HoveredVertex
        {
            get => hoveredVertex;
            set
            {
                hoveredVertex = value;
                Invalidate();
            }
        }

        public event EventHandler<Vertex> VertexClicked;
        public event EventHandler<Vertex> VertexDoubleClicked;

        public GraphPanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint | 
                         ControlStyles.DoubleBuffer | 
                         ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            if (graph == null || graph.Vertices.Count == 0)
                return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Distribui os vértices em um círculo se não tiverem posição definida
            DistributeVertices();

            // Desenha as arestas
            DrawEdges(g);

            // Desenha os vértices
            DrawVertices(g);
        }

        private void DistributeVertices()
        {
            if (graph.Vertices.Count == 0)
                return;

            int centerX = Width / 2;
            int centerY = Height / 2;
            int radius = Math.Min(Width, Height) / 3;
            
            double angleStep = 2 * Math.PI / graph.Vertices.Count;

            for (int i = 0; i < graph.Vertices.Count; i++)
            {
                var vertex = graph.Vertices[i];
                if (vertex.Position.X == 0 && vertex.Position.Y == 0)
                {
                    double angle = i * angleStep;
                    int x = centerX + (int)(radius * Math.Cos(angle));
                    int y = centerY + (int)(radius * Math.Sin(angle));
                    vertex.Position = new Point(x, y);
                }
            }
        }

        private void DrawEdges(Graphics g)
        {
            float fontSize = 8 * zoomFactor;
            var font = new Font("Arial", fontSize);
            var brush = new SolidBrush(Color.Black);

            foreach (var edge in graph.Edges)
            {
                float penWidth = (edge.IsHighlighted ? 3 : 1) * zoomFactor;
                var pen = new Pen(edge.Color, penWidth);
                if (edge.IsHighlighted)
                    pen.Color = Color.Red;

                Point from = edge.From.Position;
                Point to = edge.To.Position;

                if (graph.IsDirected)
                {
                    DrawDirectedEdge(g, pen, from, to);
                }
                else
                {
                    g.DrawLine(pen, from, to);
                }

                // Desenha o peso da aresta
                if (graph.IsWeighted)
                {
                    int midX = (from.X + to.X) / 2;
                    int midY = (from.Y + to.Y) / 2;
                    string weightText = edge.Weight.ToString("F1");
                    var textSize = g.MeasureString(weightText, font);
                    g.FillEllipse(Brushes.White, midX - textSize.Width / 2 - 2, 
                        midY - textSize.Height / 2 - 2, textSize.Width + 4, textSize.Height + 4);
                    g.DrawString(weightText, font, brush, midX - textSize.Width / 2, 
                        midY - textSize.Height / 2);
                }

                pen.Dispose();
            }

            font.Dispose();
            brush.Dispose();
        }

        private void DrawDirectedEdge(Graphics g, Pen pen, Point from, Point to)
        {
            // Calcula o ângulo da linha
            double angle = Math.Atan2(to.Y - from.Y, to.X - from.X);
            
            // Ajusta os pontos para que a seta comece e termine na borda do círculo
            int offset = VertexRadius;
            Point adjustedFrom = new Point(
                from.X + (int)(offset * Math.Cos(angle)),
                from.Y + (int)(offset * Math.Sin(angle))
            );
            Point adjustedTo = new Point(
                to.X - (int)(offset * Math.Cos(angle)),
                to.Y - (int)(offset * Math.Sin(angle))
            );

            g.DrawLine(pen, adjustedFrom, adjustedTo);

            // Desenha a seta
            double arrowAngle = Math.PI / 6;
            int arrowLength = (int)(10 * zoomFactor);
            Point arrowPoint1 = new Point(
                adjustedTo.X - (int)(arrowLength * Math.Cos(angle - arrowAngle)),
                adjustedTo.Y - (int)(arrowLength * Math.Sin(angle - arrowAngle))
            );
            Point arrowPoint2 = new Point(
                adjustedTo.X - (int)(arrowLength * Math.Cos(angle + arrowAngle)),
                adjustedTo.Y - (int)(arrowLength * Math.Sin(angle + arrowAngle))
            );

            g.DrawLine(pen, adjustedTo, arrowPoint1);
            g.DrawLine(pen, adjustedTo, arrowPoint2);
        }

        private void DrawVertices(Graphics g)
        {
            float fontSize = 10 * zoomFactor;
            var font = new Font("Arial", fontSize, FontStyle.Bold);
            var textBrush = new SolidBrush(Color.Black);

            foreach (var vertex in graph.Vertices)
            {
                // Usa a cor do vértice diretamente (já definida no MainForm)
                // Apenas aplica um leve destaque no hover sem mudar a cor
                Color vertexColor = vertex.Color;
                if (vertex == hoveredVertex && vertex != selectedVertex)
                {
                    // Apenas clareia um pouco a cor no hover, mantendo a cor original
                    vertexColor = Color.FromArgb(
                        Math.Min(255, vertex.Color.R + 20),
                        Math.Min(255, vertex.Color.G + 20),
                        Math.Min(255, vertex.Color.B + 20)
                    );
                }

                var brush = new SolidBrush(vertexColor);
                float penWidth = 2 * zoomFactor;
                var pen = new Pen(Color.Black, penWidth);

                Rectangle rect = new Rectangle(
                    vertex.Position.X - VertexRadius,
                    vertex.Position.Y - VertexRadius,
                    VertexRadius * 2,
                    VertexRadius * 2
                );

                g.FillEllipse(brush, rect);
                g.DrawEllipse(pen, rect);

                // Desenha o label
                var textSize = g.MeasureString(vertex.Label, font);
                g.DrawString(vertex.Label, font, textBrush,
                    vertex.Position.X - textSize.Width / 2,
                    vertex.Position.Y - textSize.Height / 2);

                brush.Dispose();
                pen.Dispose();
            }

            font.Dispose();
            textBrush.Dispose();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var vertex = GetVertexAtPoint(e.Location);
            HoveredVertex = vertex;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var vertex = GetVertexAtPoint(e.Location);
            if (vertex != null)
            {
                SelectedVertex = vertex;
                VertexClicked?.Invoke(this, vertex);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            var vertex = GetVertexAtPoint(e.Location);
            if (vertex != null)
            {
                VertexDoubleClicked?.Invoke(this, vertex);
            }
        }

        private Vertex GetVertexAtPoint(Point point)
        {
            if (graph == null)
                return null;

            foreach (var vertex in graph.Vertices)
            {
                int dx = point.X - vertex.Position.X;
                int dy = point.Y - vertex.Position.Y;
                if (dx * dx + dy * dy <= VertexRadius * VertexRadius)
                {
                    return vertex;
                }
            }
            return null;
        }
    }
}

