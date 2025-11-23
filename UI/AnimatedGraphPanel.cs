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
    /// Painel customizado com animações e efeitos visuais avançados
    /// </summary>
    public class AnimatedGraphPanel : Panel
    {
        private Graph graph;
        private Vertex selectedVertex;
        private Vertex hoveredVertex;
        private const int BaseVertexRadius = 20;
        private float zoomFactor = 1.0f;
        private System.Windows.Forms.Timer animationTimer;
        private float animationTime = 0f;
        private Dictionary<Vertex, float> vertexAnimationProgress = new Dictionary<Vertex, float>();
        private Dictionary<Edge, float> edgeAnimationProgress = new Dictionary<Edge, float>();
        private float pulseAnimation = 0f;

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
                vertexAnimationProgress.Clear();
                edgeAnimationProgress.Clear();
                if (graph != null)
                {
                    foreach (var vertex in graph.Vertices)
                    {
                        vertexAnimationProgress[vertex] = 0f;
                    }
                    foreach (var edge in graph.Edges)
                    {
                        edgeAnimationProgress[edge] = 0f;
                    }
                }
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

        public AnimatedGraphPanel()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint | 
                         ControlStyles.DoubleBuffer | 
                         ControlStyles.ResizeRedraw, true);

            // Timer para animações
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16; // ~60 FPS
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            animationTime += 0.016f; // Incrementa baseado em ~60 FPS
            pulseAnimation = (float)(Math.Sin(animationTime * 3) * 0.3 + 0.7); // Pulso entre 0.4 e 1.0

            // Anima progresso dos vértices
            bool needsRedraw = false;
            if (graph != null)
            {
                foreach (var vertex in graph.Vertices)
                {
                    if (vertexAnimationProgress.ContainsKey(vertex))
                    {
                        if (vertexAnimationProgress[vertex] < 1f)
                        {
                            vertexAnimationProgress[vertex] = Math.Min(1f, vertexAnimationProgress[vertex] + 0.05f);
                            needsRedraw = true;
                        }
                    }
                }

                // Anima progresso das arestas destacadas
                foreach (var edge in graph.Edges)
                {
                    if (edge.IsHighlighted && edgeAnimationProgress.ContainsKey(edge))
                    {
                        if (edgeAnimationProgress[edge] < 1f)
                        {
                            edgeAnimationProgress[edge] = Math.Min(1f, edgeAnimationProgress[edge] + 0.08f);
                            needsRedraw = true;
                        }
                    }
                }
            }

            if (needsRedraw || selectedVertex != null)
            {
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Desenha background gradiente
            DrawGradientBackground(g);

            if (graph == null || graph.Vertices.Count == 0)
                return;

            // Distribui os vértices em um círculo se não tiverem posição definida
            DistributeVertices();

            // Desenha as arestas
            DrawEdges(g);

            // Desenha os vértices
            DrawVertices(g);
        }

        private void DrawGradientBackground(Graphics g)
        {
            using (var brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(250, 252, 255),
                Color.FromArgb(240, 243, 248),
                45f))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }
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
            var font = new Font("Segoe UI", fontSize, FontStyle.Bold);
            var brush = new SolidBrush(Color.FromArgb(60, 60, 60));

            foreach (var edge in graph.Edges)
            {
                float progress = edgeAnimationProgress.ContainsKey(edge) ? edgeAnimationProgress[edge] : 1f;
                
                if (edge.IsHighlighted)
                {
                    // Animação de brilho para arestas destacadas com efeito pulsante
                    float glowIntensity = (float)(Math.Sin(animationTime * 5) * 0.4 + 0.6);
                    float penWidth = (3.5f + glowIntensity * 2.5f) * zoomFactor;
                    
                    // Cores dinâmicas baseadas na cor da aresta
                    Color baseColor = edge.Color;
                    int alpha = (int)(200 + 55 * glowIntensity);
                    var pen = new Pen(Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B), penWidth);
                    pen.EndCap = LineCap.Round;
                    pen.StartCap = LineCap.Round;
                    pen.LineJoin = LineJoin.Round;

                    Point from = edge.From.Position;
                    Point to = edge.To.Position;

                    if (graph.IsDirected)
                    {
                        DrawDirectedEdge(g, pen, from, to, true);
                    }
                    else
                    {
                        g.DrawLine(pen, from, to);
                    }

                    // Desenha o peso com fundo destacado
                    if (graph.IsWeighted)
                    {
                        int midX = (from.X + to.X) / 2;
                        int midY = (from.Y + to.Y) / 2;
                        string weightText = edge.Weight.ToString("F1");
                        var textSize = g.MeasureString(weightText, font);
                        
                        // Fundo com gradiente
                        using (var bgBrush = new LinearGradientBrush(
                            new RectangleF(midX - textSize.Width / 2 - 4, midY - textSize.Height / 2 - 4,
                                textSize.Width + 8, textSize.Height + 8),
                            Color.FromArgb(255, 240, 240),
                            Color.FromArgb(255, 200, 200),
                            45f))
                        {
                            g.FillEllipse(bgBrush, midX - textSize.Width / 2 - 4, 
                                midY - textSize.Height / 2 - 4, textSize.Width + 8, textSize.Height + 8);
                        }
                        g.DrawEllipse(new Pen(Color.Red, 2), midX - textSize.Width / 2 - 4, 
                            midY - textSize.Height / 2 - 4, textSize.Width + 8, textSize.Height + 8);
                        g.DrawString(weightText, font, new SolidBrush(Color.Red), 
                            midX - textSize.Width / 2, midY - textSize.Height / 2);
                    }

                    pen.Dispose();
                }
                else
                {
                    // Arestas normais com sombra
                    float penWidth = 1.5f * zoomFactor;
                    var pen = new Pen(Color.FromArgb(120, 120, 120), penWidth);
                    pen.EndCap = LineCap.Round;
                    pen.StartCap = LineCap.Round;

                    Point from = edge.From.Position;
                    Point to = edge.To.Position;

                    // Sombra suave
                    using (var shadowPen = new Pen(Color.FromArgb(25, 0, 0, 0), penWidth + 3))
                    {
                        shadowPen.EndCap = LineCap.Round;
                        shadowPen.StartCap = LineCap.Round;
                        if (graph.IsDirected)
                        {
                            DrawDirectedEdge(g, shadowPen, 
                                new Point(from.X + 2, from.Y + 2), 
                                new Point(to.X + 2, to.Y + 2), false);
                        }
                        else
                        {
                            g.DrawLine(shadowPen, from.X + 2, from.Y + 2, to.X + 2, to.Y + 2);
                        }
                    }

                    if (graph.IsDirected)
                    {
                        DrawDirectedEdge(g, pen, from, to, false);
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
                        
                        // Fundo branco com sombra
                        g.FillEllipse(new SolidBrush(Color.FromArgb(250, 250, 250)), 
                            midX - textSize.Width / 2 - 3, midY - textSize.Height / 2 - 3, 
                            textSize.Width + 6, textSize.Height + 6);
                        g.DrawEllipse(new Pen(Color.FromArgb(200, 200, 200), 1), 
                            midX - textSize.Width / 2 - 3, midY - textSize.Height / 2 - 3, 
                            textSize.Width + 6, textSize.Height + 6);
                        g.DrawString(weightText, font, brush, 
                            midX - textSize.Width / 2, midY - textSize.Height / 2);
                    }

                    pen.Dispose();
                }
            }

            font.Dispose();
            brush.Dispose();
        }

        private void DrawDirectedEdge(Graphics g, Pen pen, Point from, Point to, bool isHighlighted)
        {
            double angle = Math.Atan2(to.Y - from.Y, to.X - from.X);
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

            // Desenha a seta com preenchimento
            double arrowAngle = Math.PI / 6;
            int arrowLength = (int)(12 * zoomFactor);
            Point arrowPoint1 = new Point(
                adjustedTo.X - (int)(arrowLength * Math.Cos(angle - arrowAngle)),
                adjustedTo.Y - (int)(arrowLength * Math.Sin(angle - arrowAngle))
            );
            Point arrowPoint2 = new Point(
                adjustedTo.X - (int)(arrowLength * Math.Cos(angle + arrowAngle)),
                adjustedTo.Y - (int)(arrowLength * Math.Sin(angle + arrowAngle))
            );

            Point[] arrowPoints = { adjustedTo, arrowPoint1, arrowPoint2 };
            if (isHighlighted)
            {
                g.FillPolygon(new SolidBrush(pen.Color), arrowPoints);
            }
            else
            {
                g.DrawPolygon(pen, arrowPoints);
            }
        }

        private void DrawVertices(Graphics g)
        {
            float fontSize = 10 * zoomFactor;
            var font = new Font("Segoe UI", fontSize, FontStyle.Bold);
            var textBrush = new SolidBrush(Color.FromArgb(40, 40, 40));

            foreach (var vertex in graph.Vertices)
            {
                float progress = vertexAnimationProgress.ContainsKey(vertex) ? vertexAnimationProgress[vertex] : 1f;
                if (progress < 1f) continue; // Não desenha vértices ainda não animados

                Color vertexColor = vertex.Color;
                bool isSelected = (vertex == selectedVertex);
                bool isHovered = (vertex == hoveredVertex && !isSelected);

                // Efeito de hover
                if (isHovered)
                {
                    vertexColor = Color.FromArgb(
                        Math.Min(255, vertex.Color.R + 30),
                        Math.Min(255, vertex.Color.G + 30),
                        Math.Min(255, vertex.Color.B + 30)
                    );
                }

                int radius = VertexRadius;
                if (isSelected)
                {
                    // Animação de pulso para vértices selecionados
                    radius = (int)(VertexRadius * (1 + pulseAnimation * 0.15f));
                }

                Rectangle rect = new Rectangle(
                    vertex.Position.X - radius,
                    vertex.Position.Y - radius,
                    radius * 2,
                    radius * 2
                );

                // Sombra do vértice
                Rectangle shadowRect = new Rectangle(
                    rect.X + 2,
                    rect.Y + 2,
                    rect.Width,
                    rect.Height
                );
                using (var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                {
                    g.FillEllipse(shadowBrush, shadowRect);
                }

                // Gradiente no vértice
                using (var brush = new LinearGradientBrush(
                    rect,
                    Color.FromArgb(Math.Min(255, vertexColor.R + 30), Math.Min(255, vertexColor.G + 30), Math.Min(255, vertexColor.B + 30)),
                    vertexColor,
                    135f))
                {
                    g.FillEllipse(brush, rect);
                }

                // Borda com gradiente
                float penWidth = (isSelected ? 3 : 2) * zoomFactor;
                using (var pen = new Pen(Color.FromArgb(80, 80, 80), penWidth))
                {
                    g.DrawEllipse(pen, rect);
                }

                // Efeito de brilho para vértices selecionados com múltiplas camadas
                if (isSelected)
                {
                    // Camada externa de brilho
                    float outerGlowRadius = radius * (1 + pulseAnimation * 0.4f);
                    Rectangle outerGlowRect = new Rectangle(
                        vertex.Position.X - (int)outerGlowRadius,
                        vertex.Position.Y - (int)outerGlowRadius,
                        (int)(outerGlowRadius * 2),
                        (int)(outerGlowRadius * 2)
                    );
                    using (var outerGlowBrush = new SolidBrush(Color.FromArgb((int)(30 * pulseAnimation), vertexColor)))
                    {
                        g.FillEllipse(outerGlowBrush, outerGlowRect);
                    }
                    
                    // Camada interna de brilho
                    float innerGlowRadius = radius * (1 + pulseAnimation * 0.2f);
                    Rectangle innerGlowRect = new Rectangle(
                        vertex.Position.X - (int)innerGlowRadius,
                        vertex.Position.Y - (int)innerGlowRadius,
                        (int)(innerGlowRadius * 2),
                        (int)(innerGlowRadius * 2)
                    );
                    using (var innerGlowBrush = new SolidBrush(Color.FromArgb((int)(80 * pulseAnimation), vertexColor)))
                    {
                        g.FillEllipse(innerGlowBrush, innerGlowRect);
                    }
                }

                // Desenha o label
                var textSize = g.MeasureString(vertex.Label, font);
                // Sombra do texto
                g.DrawString(vertex.Label, font, new SolidBrush(Color.FromArgb(100, 255, 255, 255)),
                    vertex.Position.X - textSize.Width / 2 + 1,
                    vertex.Position.Y - textSize.Height / 2 + 1);
                // Texto principal
                g.DrawString(vertex.Label, font, textBrush,
                    vertex.Position.X - textSize.Width / 2,
                    vertex.Position.Y - textSize.Height / 2);
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
                int radius = VertexRadius;
                if (selectedVertex == vertex)
                {
                    radius = (int)(VertexRadius * 1.15f);
                }
                if (dx * dx + dy * dy <= radius * radius)
                {
                    return vertex;
                }
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer?.Stop();
                animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

