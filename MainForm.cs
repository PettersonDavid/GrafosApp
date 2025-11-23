using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GrafosApp.Models;
using GrafosApp.Algorithms;
using GrafosApp.UI;

namespace GrafosApp
{
    public partial class MainForm : Form
    {
        private Graph currentGraph;
        private AnimatedGraphPanel graphPanel;
        private Panel controlPanel;
        private Panel resizePanel; // Painel para redimensionar
        private bool isControlPanelCollapsed = false;
        private int savedControlPanelWidth = 320;
        private const int collapsedWidth = 50;
        private const int minControlPanelWidth = 250;
        private const int maxControlPanelWidth = 500;
        private Button btnGenerateGraph;
        private Button btnMST;
        private Button btnShortestPath;
        private Button btnTransitiveClosure;
        private Button btnCycleDetection;
        private Button btnDFS;
        private Button btnBFS;
        private Button btnDegreeSequence;
        private Button btnClear;
        private Button btnReset;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private ComboBox cmbGraphType;
        private NumericUpDown numVertices;
        private NumericUpDown numEdges;
        private Label lblStatus;
        private Label lblZoom;
        private TextBox txtOutput;
        private Vertex selectedVertex1;
        private Vertex selectedVertex2;
        private float zoomFactor = 1.0f;
        private List<List<Vertex>> detectedCycles = new List<List<Vertex>>();
        private int currentCycleIndex = -1; // -1 significa que nenhum ciclo está sendo exibido
        private const float zoomStep = 0.1f;
        private const float minZoom = 0.8f;
        private const float maxZoom = 2.0f;
        private Size baseFormSize = new Size(1400, 900);
        private int baseControlPanelWidth = 320;
        private Button btnTogglePanel;

        public MainForm()
        {
            InitializeComponent();
            InitializeGraph();
        }

        private void InitializeComponent()
        {
            this.Text = "Aplicação de Algoritmos em Grafos";
            this.Size = baseFormSize;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.MinimumSize = new Size(1000, 700);

            // Painel de controle (lateral esquerdo) com gradiente
            controlPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = baseControlPanelWidth,
                BackColor = Color.FromArgb(45, 45, 50),
                Padding = new Padding(10)
            };
            controlPanel.Paint += ControlPanel_Paint;
            savedControlPanelWidth = baseControlPanelWidth;

            // Painel de redimensionamento (borda direita do painel de controle)
            // Será adicionado depois do scrollPanel para ficar por cima
            resizePanel = new Panel
            {
                Width = 5,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(60, 60, 65),
                Cursor = Cursors.SizeWE
            };
            resizePanel.MouseDown += ResizePanel_MouseDown;
            resizePanel.MouseMove += ResizePanel_MouseMove;
            resizePanel.MouseUp += ResizePanel_MouseUp;
            resizePanel.MouseLeave += ResizePanel_MouseLeave;
            resizePanel.Paint += ResizePanel_Paint;

            // Botão para colapsar/expandir o painel
            // Será adicionado depois do scrollPanel para ficar por cima
            btnTogglePanel = new Button
            {
                Text = "◀",
                Width = 30,
                Height = 30,
                Location = new Point(baseControlPanelWidth - 40, 10),
                BackColor = Color.FromArgb(70, 70, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnTogglePanel.FlatAppearance.BorderSize = 0;
            btnTogglePanel.Click += BtnTogglePanel_Click;
            btnTogglePanel.MouseEnter += (s, e) => btnTogglePanel.BackColor = Color.FromArgb(90, 90, 95);
            btnTogglePanel.MouseLeave += (s, e) => btnTogglePanel.BackColor = Color.FromArgb(70, 70, 75);

            // Painel do grafo (área principal) com animações
            graphPanel = new AnimatedGraphPanel
            {
                Dock = DockStyle.Fill
            };
            graphPanel.VertexClicked += GraphPanel_VertexClicked;

            // Controles
            var lblTitle = new Label
            {
                Text = "Algoritmos em Grafos",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 200, 100),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            // Efeito de sombra no título
            lblTitle.Paint += (s, e) => {
                e.Graphics.DrawString(lblTitle.Text, lblTitle.Font, 
                    new SolidBrush(Color.FromArgb(50, 0, 0, 0)), 
                    lblTitle.Location.X + 2, lblTitle.Location.Y + 2);
            };

            var lblGraphType = new Label
            {
                Text = "Tipo de Grafo:",
                ForeColor = Color.White,
                Location = new Point(10, 60),
                AutoSize = true
            };

            cmbGraphType = new ComboBox
            {
                Location = new Point(10, 80),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbGraphType.Items.AddRange(new[] { "Não Dirigido", "Dirigido" });
            cmbGraphType.SelectedIndex = 0;

            var lblVertices = new Label
            {
                Text = "Número de Vértices:",
                ForeColor = Color.White,
                Location = new Point(10, 120),
                AutoSize = true
            };

            numVertices = new NumericUpDown
            {
                Location = new Point(10, 140),
                Width = 300,
                Minimum = 2,
                Maximum = 50,
                Value = 5
            };

            var lblEdges = new Label
            {
                Text = "Número de Arestas:",
                ForeColor = Color.White,
                Location = new Point(10, 180),
                AutoSize = true
            };

            numEdges = new NumericUpDown
            {
                Location = new Point(10, 200),
                Width = 300,
                Minimum = 1,
                Maximum = 100,
                Value = 8
            };

            btnGenerateGraph = CreateButton("Gerar Grafo Aleatório", 240, Color.FromArgb(0, 120, 215),
                "Gera um grafo aleatório com o número especificado de vértices e arestas.\n" +
                "O grafo pode ser dirigido ou não dirigido, e as arestas são valoradas aleatoriamente.");
            
            btnMST = CreateButton("Árvore Geradora Mínima", 280, Color.FromArgb(16, 124, 16),
                "Encontra a Árvore Geradora Mínima (AGM) usando o algoritmo de Kruskal.\n" +
                "Para dígrafos, trabalha com o grafo subjacente.\n" +
                "A AGM conecta todos os vértices com o menor peso total possível.");
            
            btnShortestPath = CreateButton("Caminho Mínimo", 320, Color.FromArgb(220, 53, 69),
                "Calcula o caminho mínimo entre dois vértices usando o algoritmo de Dijkstra.\n" +
                "Selecione dois vértices clicando neles no grafo.\n" +
                "O caminho com menor peso total será destacado em vermelho.");
            
            btnTransitiveClosure = CreateButton("Fecho Transitivo", 360, Color.FromArgb(255, 193, 7),
                "Calcula o fecho transitivo de um vértice.\n" +
                "Fecho Direto: todos os vértices alcançáveis a partir do vértice selecionado.\n" +
                "Fecho Inverso: todos os vértices que alcançam o vértice selecionado (apenas para dígrafos).\n" +
                "Selecione um vértice clicando nele no grafo.");
            
            btnCycleDetection = CreateButton("Detectar Ciclos", 400, Color.FromArgb(138, 43, 226),
                "Detecta todos os ciclos presentes no grafo.\n" +
                "Para grafos não dirigidos: detecta ciclos simples.\n" +
                "Para dígrafos: detecta ciclos direcionados.\n" +
                "Todos os ciclos encontrados são listados, e o primeiro é destacado visualmente.");
            
            btnDFS = CreateButton("Busca em Profundidade", 440, Color.FromArgb(30, 144, 255),
                "Executa uma busca em profundidade (DFS) a partir de um vértice.\n" +
                "Explora o grafo o mais profundo possível antes de retroceder.\n" +
                "Mostra a ordem de visitação dos vértices.\n" +
                "Selecione um vértice clicando nele no grafo.");
            
            btnBFS = CreateButton("Busca em Largura", 480, Color.FromArgb(255, 140, 0),
                "Executa uma busca em largura (BFS) a partir de um vértice.\n" +
                "Explora todos os vértices na mesma distância antes de avançar.\n" +
                "Mostra a ordem de visitação e as distâncias de cada vértice.\n" +
                "Selecione um vértice clicando nele no grafo.");
            
            btnDegreeSequence = CreateButton("Sequência de Graus", 520, Color.FromArgb(50, 205, 50),
                "Calcula e exibe a sequência de graus do grafo.\n" +
                "Para grafos não dirigidos: mostra o grau de cada vértice.\n" +
                "Para dígrafos: mostra graus de entrada e saída, e a sequência do grafo subjacente.\n" +
                "A sequência é ordenada de forma decrescente.");
            
            btnClear = CreateButton("Limpar", 560, Color.FromArgb(108, 117, 125),
                "Limpa todos os destaques visuais do grafo e a área de saída.\n" +
                "Mantém o grafo atual, apenas remove as marcações de algoritmos.");
            
            btnReset = CreateButton("Reiniciar", 600, Color.FromArgb(220, 53, 69),
                "Reinicia completamente a aplicação.\n" +
                "Remove o grafo atual e limpa todas as seleções.\n" +
                "Use para começar do zero.");

            // Controles de Zoom
            var lblZoomTitle = new Label
            {
                Text = "Zoom:",
                ForeColor = Color.White,
                Location = new Point(10, 640),
                AutoSize = true
            };

            btnZoomOut = new Button
            {
                Text = "➖",
                Location = new Point(10, 660),
                Width = 145,
                Height = 30,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnZoomIn = new Button
            {
                Text = "➕",
                Location = new Point(165, 660),
                Width = 145,
                Height = 30,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            lblZoom = new Label
            {
                Text = "100%",
                ForeColor = Color.White,
                Location = new Point(10, 695),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            // Painel scrollável para os controles
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 10, 20) // Padding direito para não sobrepor o resizePanel, inferior para garantir que tudo seja visível
            };

            txtOutput = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen,
                Location = new Point(10, 720),
                Width = 300,
                Height = 100
            };

            lblStatus = new Label
            {
                Text = "Pronto",
                ForeColor = Color.White,
                Location = new Point(10, 830),
                AutoSize = true,
                Width = 300
            };

            // Adiciona todos os controles ao painel scrollável
            scrollPanel.Controls.AddRange(new Control[]
            {
                lblTitle, lblGraphType, cmbGraphType,
                lblVertices, numVertices, lblEdges, numEdges,
                btnGenerateGraph, btnMST, btnShortestPath,
                btnTransitiveClosure, btnCycleDetection, btnDFS,
                btnBFS, btnDegreeSequence, btnClear, btnReset,
                lblZoomTitle, btnZoomOut, btnZoomIn, lblZoom,
                txtOutput, lblStatus
            });

            // Adiciona o painel scrollável ao painel de controle (primeiro, para ficar atrás)
            controlPanel.Controls.Add(scrollPanel);
            
            // Adiciona resizePanel e btnTogglePanel por último para ficarem por cima
            controlPanel.Controls.Add(resizePanel);
            controlPanel.Controls.Add(btnTogglePanel);
            // Garante que fiquem por cima do scrollPanel
            controlPanel.Controls.SetChildIndex(resizePanel, controlPanel.Controls.Count - 1);
            controlPanel.Controls.SetChildIndex(btnTogglePanel, controlPanel.Controls.Count - 1);

            this.Controls.Add(controlPanel);
            this.Controls.Add(graphPanel);

            // Eventos
            btnGenerateGraph.Click += BtnGenerateGraph_Click;
            btnMST.Click += BtnMST_Click;
            btnShortestPath.Click += BtnShortestPath_Click;
            btnTransitiveClosure.Click += BtnTransitiveClosure_Click;
            btnCycleDetection.Click += BtnCycleDetection_Click;
            btnDFS.Click += BtnDFS_Click;
            btnBFS.Click += BtnBFS_Click;
            btnDegreeSequence.Click += BtnDegreeSequence_Click;
            btnClear.Click += BtnClear_Click;
            btnReset.Click += BtnReset_Click;
            btnZoomIn.Click += BtnZoomIn_Click;
            btnZoomOut.Click += BtnZoomOut_Click;
        }

        private Button CreateButton(string text, int y, Color backColor, string tooltip = "")
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(10, y),
                Width = 300,
                Height = 35,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            // Adiciona tooltip se fornecido
            if (!string.IsNullOrEmpty(tooltip))
            {
                var toolTip = new ToolTip
                {
                    IsBalloon = true,
                    ToolTipTitle = text,
                    UseAnimation = true,
                    UseFading = true
                };
                toolTip.SetToolTip(button, tooltip);
            }

            // Efeitos visuais modernos
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Min(255, backColor.R + 20),
                Math.Min(255, backColor.G + 20),
                Math.Min(255, backColor.B + 20)
            );
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(
                Math.Max(0, backColor.R - 20),
                Math.Max(0, backColor.G - 20),
                Math.Max(0, backColor.B - 20)
            );

            // Eventos para animação de hover
            button.MouseEnter += (s, e) => {
                button.Font = new Font(button.Font.FontFamily, button.Font.Size + 0.5f, button.Font.Style);
            };
            button.MouseLeave += (s, e) => {
                button.Font = new Font(button.Font.FontFamily, button.Font.Size - 0.5f, button.Font.Style);
            };

            return button;
        }

        private void InitializeGraph()
        {
            currentGraph = new Graph(false, true);
            graphPanel.Graph = currentGraph;
            graphPanel.ZoomFactor = zoomFactor; // Inicializa o zoom do grafo
            UpdateStatus("Grafo inicializado. Clique em 'Gerar Grafo Aleatório' para começar.");
        }

        private void BtnGenerateGraph_Click(object sender, EventArgs e)
        {
            bool isDirected = cmbGraphType.SelectedIndex == 1;
            int vertices = (int)numVertices.Value;
            int edges = (int)numEdges.Value;

            currentGraph = GraphGenerator.GenerateRandomGraph(vertices, edges, isDirected, true);
            graphPanel.Graph = currentGraph;
            // Garante que todos os vértices começam com amarelo claro
            foreach (var vertex in currentGraph.Vertices)
            {
                vertex.Color = Color.LightYellow;
            }
            selectedVertex1 = null;
            selectedVertex2 = null;
            
            // Reseta a detecção de ciclos quando um novo grafo é gerado
            detectedCycles.Clear();
            currentCycleIndex = -1;

            AppendOutput($"\n=== GRAFO GERADO ===");
            AppendOutput($"Tipo: {(isDirected ? "Dirigido (Dígrafo)" : "Não Dirigido")}");
            AppendOutput($"Vértices: {vertices}");
            AppendOutput($"Arestas: {edges}");
            AppendOutput($"Valorado: Sim (pesos aleatórios entre 1.0 e 10.0)");
            UpdateStatus("Grafo gerado com sucesso!");
        }

        private void BtnMST_Click(object sender, EventArgs e)
        {
            if (currentGraph == null || currentGraph.Vertices.Count == 0)
            {
                MessageBox.Show("Gere um grafo primeiro!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var mstAlgorithm = new MinimumSpanningTree();
            var mst = mstAlgorithm.Execute(currentGraph);

            // Destaca as arestas da MST
            foreach (var edge in currentGraph.Edges)
            {
                edge.IsHighlighted = mst.Contains(edge);
                edge.Color = edge.IsHighlighted ? Color.Red : Color.Black;
            }

            graphPanel.Invalidate();
            AppendOutput($"\n=== ÁRVORE GERADORA MÍNIMA (Kruskal) ===");
            AppendOutput($"Algoritmo: Kruskal (Union-Find)");
            if (currentGraph.IsDirected)
            {
                AppendOutput($"Nota: Trabalhando com o grafo subjacente (não dirigido)");
            }
            AppendOutput($"Arestas na AGM: {mst.Count}");
            double totalWeight = mst.Sum(e => e.Weight);
            AppendOutput($"Peso total: {totalWeight:F2}");
            AppendOutput($"\nArestas da AGM (destacadas em vermelho):");
            foreach (var edge in mst.OrderBy(e => e.Weight))
            {
                AppendOutput($"  {edge.From.Label} → {edge.To.Label} (peso: {edge.Weight:F2})");
            }
            UpdateStatus("AGM calculada e destacada no grafo!");
        }

        private void BtnShortestPath_Click(object sender, EventArgs e)
        {
            if (currentGraph == null || currentGraph.Vertices.Count == 0)
            {
                MessageBox.Show("Gere um grafo primeiro!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedVertex1 == null || selectedVertex2 == null)
            {
                MessageBox.Show("Selecione dois vértices clicando neles no grafo!", "Aviso", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus("Clique em dois vértices para calcular o caminho mínimo");
                return;
            }

            var shortestPathAlgorithm = new ShortestPath();
            var result = shortestPathAlgorithm.Execute(currentGraph, selectedVertex1, selectedVertex2);

            // Limpa destaques anteriores
            foreach (var edge in currentGraph.Edges)
            {
                edge.IsHighlighted = false;
                edge.Color = Color.Black;
            }

            if (result.Exists)
            {
                // Destaca o caminho
                for (int i = 0; i < result.Path.Count - 1; i++)
                {
                    var edge = currentGraph.Edges.FirstOrDefault(e =>
                        (e.From == result.Path[i] && e.To == result.Path[i + 1]) ||
                        (!currentGraph.IsDirected && e.From == result.Path[i + 1] && e.To == result.Path[i]));
                    if (edge != null)
                    {
                        edge.IsHighlighted = true;
                        edge.Color = Color.Red;
                    }
                }

                graphPanel.Invalidate();
                AppendOutput($"\n=== CAMINHO MÍNIMO (Dijkstra) ===");
                AppendOutput($"Algoritmo: Dijkstra");
                AppendOutput($"Origem: {selectedVertex1.Label}");
                AppendOutput($"Destino: {selectedVertex2.Label}");
                AppendOutput($"Peso total do caminho: {result.TotalWeight:F2}");
                AppendOutput($"Número de vértices no caminho: {result.Path.Count}");
                AppendOutput($"\nCaminho completo (destacado em vermelho):");
                AppendOutput($"  {string.Join(" → ", result.Path.Select(v => v.Label))}");
                UpdateStatus("Caminho mínimo calculado e destacado!");
            }
            else
            {
                AppendOutput($"\n=== CAMINHO MÍNIMO (Dijkstra) ===");
                AppendOutput($"Origem: {selectedVertex1.Label}");
                AppendOutput($"Destino: {selectedVertex2.Label}");
                AppendOutput($"\n❌ Não existe caminho conectando {selectedVertex1.Label} a {selectedVertex2.Label}");
                AppendOutput($"Os vértices estão em componentes diferentes do grafo.");
                UpdateStatus("Caminho não encontrado!");
            }
        }

        private void BtnTransitiveClosure_Click(object sender, EventArgs e)
        {
            if (currentGraph == null || currentGraph.Vertices.Count == 0)
            {
                MessageBox.Show("Gere um grafo primeiro!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedVertex1 == null)
            {
                MessageBox.Show("Selecione um vértice clicando nele no grafo!", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus("Clique em um vértice para calcular o fecho transitivo");
                return;
            }

            var closureAlgorithm = new TransitiveClosure();
            var result = closureAlgorithm.Execute(currentGraph, selectedVertex1);

            AppendOutput($"\n=== FECHO TRANSITIVO ===");
            AppendOutput($"Vértice selecionado: {selectedVertex1.Label}");
            AppendOutput($"\nFecho Direto (vértices alcançáveis a partir de {selectedVertex1.Label}):");
            if (result.DirectClosure.Count > 0)
            {
                AppendOutput($"  Total: {result.DirectClosure.Count} vértices");
                AppendOutput($"  Vértices: {string.Join(", ", result.DirectClosure.Select(v => v.Label))}");
            }
            else
            {
                AppendOutput($"  Nenhum vértice alcançável (vértice isolado ou sem arestas de saída)");
            }
            
            if (currentGraph.IsDirected)
            {
                AppendOutput($"\nFecho Inverso (vértices que alcançam {selectedVertex1.Label}):");
                if (result.InverseClosure.Count > 0)
                {
                    AppendOutput($"  Total: {result.InverseClosure.Count} vértices");
                    AppendOutput($"  Vértices: {string.Join(", ", result.InverseClosure.Select(v => v.Label))}");
                }
                else
                {
                    AppendOutput($"  Nenhum vértice alcança este vértice (vértice fonte ou isolado)");
                }
            }

            UpdateStatus("Fecho transitivo calculado!");
        }

        private void BtnCycleDetection_Click(object sender, EventArgs e)
        {
            if (currentGraph == null || currentGraph.Vertices.Count == 0)
            {
                MessageBox.Show("Gere um grafo primeiro!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Se não há ciclos detectados ou o grafo mudou, detecta novamente
            bool needsNewDetection = detectedCycles.Count == 0 || 
                                    currentCycleIndex == -1 ||
                                    (detectedCycles.Count > 0 && currentCycleIndex >= detectedCycles.Count);

            if (needsNewDetection)
            {
                var cycleAlgorithm = new CycleDetection();
                var result = cycleAlgorithm.Execute(currentGraph);
                
                detectedCycles = result.AllCycles;
                currentCycleIndex = 0; // Começa no primeiro ciclo
            }
            else
            {
                // Avança para o próximo ciclo
                currentCycleIndex = (currentCycleIndex + 1) % detectedCycles.Count;
            }

            // Limpa destaques anteriores
            foreach (var edge in currentGraph.Edges)
            {
                edge.IsHighlighted = false;
                edge.Color = Color.Black;
            }

            if (detectedCycles.Count > 0)
            {
                // Destaca o ciclo atual
                var currentCycle = detectedCycles[currentCycleIndex];
                
                for (int i = 0; i < currentCycle.Count - 1; i++)
                {
                    var edge = currentGraph.Edges.FirstOrDefault(e =>
                        (e.From == currentCycle[i] && e.To == currentCycle[i + 1]) ||
                        (!currentGraph.IsDirected && e.From == currentCycle[i + 1] && e.To == currentCycle[i]));
                    if (edge != null)
                    {
                        edge.IsHighlighted = true;
                        edge.Color = Color.Purple;
                    }
                }

                graphPanel.Invalidate();
                AppendOutput($"\n=== DETECÇÃO DE CICLOS ===");
                AppendOutput($"Total de ciclos encontrados: {detectedCycles.Count}");
                AppendOutput($"\nCiclo {currentCycleIndex + 1} de {detectedCycles.Count} (destacado no grafo):");
                AppendOutput($"  {string.Join(" → ", currentCycle.Select(v => v.Label))}");
                
                if (detectedCycles.Count > 1)
                {
                    AppendOutput($"\nTodos os ciclos encontrados:");
                    for (int i = 0; i < detectedCycles.Count && i < 15; i++) // Limita a 15 ciclos para não sobrecarregar
                    {
                        string marker = i == currentCycleIndex ? "►" : " ";
                        AppendOutput($"  {marker} Ciclo {i + 1}: {string.Join(" → ", detectedCycles[i].Select(v => v.Label))}");
                    }
                    if (detectedCycles.Count > 15)
                    {
                        AppendOutput($"  ... e mais {detectedCycles.Count - 15} ciclos");
                    }
                    AppendOutput($"\n💡 Dica: Clique novamente em 'Detectar Ciclos' para ver o próximo ciclo!");
                }
                
                UpdateStatus($"Ciclo {currentCycleIndex + 1} de {detectedCycles.Count} destacado! Clique novamente para ver o próximo.");
            }
            else
            {
                AppendOutput($"\n=== DETECÇÃO DE CICLOS ===");
                AppendOutput($"Nenhum ciclo encontrado no grafo.");
                AppendOutput($"O grafo é acíclico (árvore ou DAG).");
                currentCycleIndex = -1;
                UpdateStatus("Grafo não contém ciclos!");
            }
        }

        private void BtnDFS_Click(object sender, EventArgs e)
        {
            if (currentGraph == null || currentGraph.Vertices.Count == 0)
            {
                MessageBox.Show("Gere um grafo primeiro!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedVertex1 == null)
            {
                MessageBox.Show("Selecione um vértice clicando nele no grafo!", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus("Clique em um vértice para executar DFS");
                return;
            }

            var dfsAlgorithm = new DepthFirstSearch();
            var result = dfsAlgorithm.Execute(currentGraph, selectedVertex1);

            // Limpa destaques anteriores
            foreach (var edge in currentGraph.Edges)
            {
                edge.IsHighlighted = false;
                edge.Color = Color.Black;
            }

            // Destaca o caminho DFS
            for (int i = 0; i < result.Path.Count - 1; i++)
            {
                var edge = currentGraph.Edges.FirstOrDefault(e =>
                    (e.From == result.Path[i] && e.To == result.Path[i + 1]) ||
                    (!currentGraph.IsDirected && e.From == result.Path[i + 1] && e.To == result.Path[i]));
                if (edge != null)
                {
                    edge.IsHighlighted = true;
                    edge.Color = Color.Blue;
                }
            }

            graphPanel.Invalidate();
            AppendOutput($"\n=== BUSCA EM PROFUNDIDADE (DFS) ===");
            AppendOutput($"Algoritmo: Depth-First Search (Busca em Profundidade)");
            AppendOutput($"Vértice inicial: {selectedVertex1.Label}");
            AppendOutput($"\nOrdem de visitação (destacada em azul):");
            AppendOutput($"  {string.Join(" → ", result.Path.Select(v => v.Label))}");
            AppendOutput($"\nEstatísticas:");
            AppendOutput($"  Total de vértices visitados: {result.Path.Count}");
            AppendOutput($"  Vértices não visitados: {currentGraph.Vertices.Count - result.Path.Count}");
            if (result.Path.Count < currentGraph.Vertices.Count)
            {
                var notVisited = currentGraph.Vertices.Except(result.Path).Select(v => v.Label);
                AppendOutput($"  Vértices não alcançáveis: {string.Join(", ", notVisited)}");
            }
            UpdateStatus("DFS executado e caminho destacado!");
        }

        private void BtnBFS_Click(object sender, EventArgs e)
        {
            if (currentGraph == null || currentGraph.Vertices.Count == 0)
            {
                MessageBox.Show("Gere um grafo primeiro!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedVertex1 == null)
            {
                MessageBox.Show("Selecione um vértice clicando nele no grafo!", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus("Clique em um vértice para executar BFS");
                return;
            }

            var bfsAlgorithm = new BreadthFirstSearch();
            var result = bfsAlgorithm.Execute(currentGraph, selectedVertex1);

            // Limpa destaques anteriores
            foreach (var edge in currentGraph.Edges)
            {
                edge.IsHighlighted = false;
                edge.Color = Color.Black;
            }

            // Destaca o caminho BFS
            foreach (var vertex in result.Path)
            {
                if (result.Parent.ContainsKey(vertex) && result.Parent[vertex] != null)
                {
                    var parent = result.Parent[vertex];
                    var edge = currentGraph.Edges.FirstOrDefault(e =>
                        (e.From == parent && e.To == vertex) ||
                        (!currentGraph.IsDirected && e.From == vertex && e.To == parent));
                    if (edge != null)
                    {
                        edge.IsHighlighted = true;
                        edge.Color = Color.Orange;
                    }
                }
            }

            graphPanel.Invalidate();
            AppendOutput($"\n=== BUSCA EM LARGURA (BFS) ===");
            AppendOutput($"Algoritmo: Breadth-First Search (Busca em Largura)");
            AppendOutput($"Vértice inicial: {selectedVertex1.Label}");
            AppendOutput($"\nOrdem de visitação (destacada em laranja):");
            AppendOutput($"  {string.Join(" → ", result.Path.Select(v => v.Label))}");
            AppendOutput($"\nDistâncias (número de arestas do vértice inicial):");
            foreach (var kvp in result.Distance.OrderBy(d => d.Value))
            {
                string distanceStr = kvp.Value == 0 ? "Origem" : kvp.Value.ToString();
                AppendOutput($"  {kvp.Key.Label}: {distanceStr}");
            }
            AppendOutput($"\nEstatísticas:");
            AppendOutput($"  Total de vértices visitados: {result.Path.Count}");
            AppendOutput($"  Vértices não visitados: {currentGraph.Vertices.Count - result.Path.Count}");
            if (result.Path.Count < currentGraph.Vertices.Count)
            {
                var notVisited = currentGraph.Vertices.Except(result.Path).Select(v => v.Label);
                AppendOutput($"  Vértices não alcançáveis: {string.Join(", ", notVisited)}");
            }
            UpdateStatus("BFS executado e caminho destacado!");
        }

        private void BtnDegreeSequence_Click(object sender, EventArgs e)
        {
            if (currentGraph == null || currentGraph.Vertices.Count == 0)
            {
                MessageBox.Show("Gere um grafo primeiro!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var degreeAlgorithm = new DegreeSequence();
            var result = degreeAlgorithm.Execute(currentGraph);

            AppendOutput($"\n=== SEQUÊNCIA DE GRAUS ===");
            
            if (currentGraph.IsDirected)
            {
                AppendOutput($"Tipo: Dígrafo (Grafo Dirigido)");
                AppendOutput($"\nGraus de Saída (out-degree):");
                foreach (var kvp in result.OutDegrees.OrderByDescending(d => d.Value))
                {
                    AppendOutput($"  {kvp.Key.Label}: {kvp.Value}");
                }
                AppendOutput($"\nGraus de Entrada (in-degree):");
                foreach (var kvp in result.InDegrees.OrderByDescending(d => d.Value))
                {
                    AppendOutput($"  {kvp.Key.Label}: {kvp.Value}");
                }
                AppendOutput($"\nSequência de Graus (Grafo Subjacente - não dirigido):");
                AppendOutput($"  [{string.Join(", ", result.DegreeSequence)}]");
            }
            else
            {
                AppendOutput($"Tipo: Grafo Não Dirigido");
                AppendOutput($"\nGraus dos Vértices:");
                foreach (var kvp in result.VertexDegrees.OrderByDescending(d => d.Value))
                {
                    AppendOutput($"  {kvp.Key.Label}: {kvp.Value}");
                }
                AppendOutput($"\nSequência de Graus (ordenada decrescentemente):");
                AppendOutput($"  [{string.Join(", ", result.DegreeSequence)}]");
            }
            
            AppendOutput($"\nTotal de vértices: {currentGraph.Vertices.Count}");
            AppendOutput($"Total de arestas: {currentGraph.Edges.Count}");
            UpdateStatus("Sequência de graus calculada!");
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (currentGraph != null)
            {
                foreach (var edge in currentGraph.Edges)
                {
                    edge.IsHighlighted = false;
                    edge.Color = Color.Black;
                }
                // Reseta cores dos vértices para amarelo claro
                foreach (var vertex in currentGraph.Vertices)
                {
                    vertex.Color = Color.LightYellow;
                }
                graphPanel.Invalidate();
            }
            selectedVertex1 = null;
            selectedVertex2 = null;
            txtOutput.Clear();
            
            // Não reseta os ciclos detectados, apenas limpa a visualização
            // Para resetar completamente, use o botão Reiniciar
            UpdateStatus("Limpo!");
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            // Faz tudo que o limpar faz
            BtnClear_Click(sender, e);
            
            // Além disso, apaga o grafo completamente
            if (currentGraph != null)
            {
                currentGraph.Clear();
                graphPanel.Graph = currentGraph;
                graphPanel.Invalidate();
            }
            
            // Reseta seleções
            selectedVertex1 = null;
            selectedVertex2 = null;
            
            // Reseta a detecção de ciclos
            detectedCycles.Clear();
            currentCycleIndex = -1;
            
            UpdateStatus("Reiniciado! Gere um novo grafo.");
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {
            if (zoomFactor < maxZoom)
            {
                zoomFactor = Math.Min(zoomFactor + zoomStep, maxZoom);
                ApplyZoom();
            }
        }

        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            if (zoomFactor > minZoom)
            {
                zoomFactor = Math.Max(zoomFactor - zoomStep, minZoom);
                ApplyZoom();
            }
        }

        private void ApplyZoom()
        {
            // Atualiza o tamanho do formulário
            this.Size = new Size(
                (int)(baseFormSize.Width * zoomFactor),
                (int)(baseFormSize.Height * zoomFactor)
            );

            // Atualiza a largura do painel de controle (se não estiver colapsado)
            if (!isControlPanelCollapsed)
            {
                controlPanel.Width = (int)(baseControlPanelWidth * zoomFactor);
                savedControlPanelWidth = controlPanel.Width;
                btnTogglePanel.Location = new Point(controlPanel.Width - 40, 10);
            }

            // Atualiza fontes e tamanhos dos controles
            UpdateControlsZoom();

            // Atualiza o zoom do painel do grafo
            graphPanel.ZoomFactor = zoomFactor;

            // Atualiza label de zoom
            lblZoom.Text = $"{(int)(zoomFactor * 100)}%";

            // Redesenha o painel do grafo
            graphPanel.Invalidate();
        }

        private void UpdateControlsZoom()
        {
            // Encontra o painel scrollável
            Panel? scrollPanel = null;
            foreach (Control control in controlPanel.Controls)
            {
                if (control is Panel panel && panel.AutoScroll)
                {
                    scrollPanel = panel;
                    break;
                }
            }

            if (scrollPanel == null) return;

            // Aplica zoom em todos os controles dentro do painel scrollável
            foreach (Control control in scrollPanel.Controls)
            {
                // Se não tem tag, armazena valores base
                if (control.Tag == null)
                {
                    // Armazena valores base no Tag como string serializada
                    string baseDataStr = $"{control.Location.X}|{control.Location.Y}|{control.Width}|{control.Height}|{control.Font?.Size ?? 9}";
                    control.Tag = baseDataStr;
                }

                // Recupera valores base do Tag
                if (control.Tag is string storedBaseData)
                {
                    string[] parts = storedBaseData.Split('|');
                    if (parts.Length >= 5)
                    {
                        int baseX = int.Parse(parts[0]);
                        int baseY = int.Parse(parts[1]);
                        int baseWidth = int.Parse(parts[2]);
                        int baseHeight = int.Parse(parts[3]);
                        float baseFontSize = float.Parse(parts[4]);

                        // Aplica zoom
                        control.Location = new Point(
                            (int)(baseX * zoomFactor),
                            (int)(baseY * zoomFactor)
                        );
                        control.Width = (int)(baseWidth * zoomFactor);
                        control.Height = (int)(baseHeight * zoomFactor);

                        if (control.Font != null)
                        {
                            control.Font = new Font(
                                control.Font.FontFamily,
                                baseFontSize * zoomFactor,
                                control.Font.Style
                            );
                        }
                    }
                }
            }
        }

        private void GraphPanel_VertexClicked(object sender, Vertex vertex)
        {
            if (selectedVertex1 == null)
            {
                selectedVertex1 = vertex;
                vertex.Color = Color.LightGreen;
                UpdateStatus($"Vértice {vertex.Label} selecionado. Selecione outro para caminho mínimo.");
            }
            else if (selectedVertex2 == null && selectedVertex1 != vertex)
            {
                selectedVertex2 = vertex;
                vertex.Color = Color.Red;
                UpdateStatus($"Vértices {selectedVertex1.Label} e {vertex.Label} selecionados.");
            }
            else
            {
                // Reset
                if (selectedVertex1 != null) selectedVertex1.Color = Color.LightYellow;
                if (selectedVertex2 != null) selectedVertex2.Color = Color.LightYellow;
                selectedVertex1 = vertex;
                selectedVertex2 = null;
                vertex.Color = Color.LightGreen;
                UpdateStatus($"Vértice {vertex.Label} selecionado.");
            }
            graphPanel.Invalidate();
        }

        private void AppendOutput(string text)
        {
            txtOutput.AppendText(text + Environment.NewLine);
            txtOutput.SelectionStart = txtOutput.Text.Length;
            txtOutput.ScrollToCaret();
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = message;
        }

        private void ControlPanel_Paint(object sender, PaintEventArgs e)
        {
            // Desenha gradiente no painel de controle
            using (var brush = new LinearGradientBrush(
                controlPanel.ClientRectangle,
                Color.FromArgb(50, 50, 55),
                Color.FromArgb(40, 40, 45),
                90f))
            {
                e.Graphics.FillRectangle(brush, controlPanel.ClientRectangle);
            }
        }

        private void ResizePanel_Paint(object sender, PaintEventArgs e)
        {
            // Desenha indicadores visuais na barra de redimensionamento
            var panel = sender as Panel;
            if (panel == null) return;

            int centerX = panel.Width / 2;
            int spacing = 3;
            using (var pen = new Pen(Color.FromArgb(120, 120, 125), 1))
            {
                for (int i = 0; i < 3; i++)
                {
                    int y = panel.Height / 2 - spacing + (i * spacing * 2);
                    e.Graphics.DrawLine(pen, centerX - 1, y, centerX - 1, y + spacing);
                    e.Graphics.DrawLine(pen, centerX + 1, y, centerX + 1, y + spacing);
                }
            }
        }

        private bool isResizing = false;
        private int resizeStartX = 0;
        private int resizeStartWidth = 0;

        private void ResizePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !isControlPanelCollapsed)
            {
                isResizing = true;
                resizeStartX = Control.MousePosition.X;
                resizeStartWidth = controlPanel.Width;
                resizePanel.Capture = true;
            }
        }

        private void ResizePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                // Calcula a nova largura baseado na posição atual do mouse
                int currentMouseX = Control.MousePosition.X;
                int deltaX = currentMouseX - resizeStartX;
                int newWidth = resizeStartWidth + deltaX;
                
                if (newWidth >= minControlPanelWidth && newWidth <= maxControlPanelWidth)
                {
                    controlPanel.Width = newWidth;
                    savedControlPanelWidth = newWidth;
                    btnTogglePanel.Location = new Point(controlPanel.Width - 40, 10);
                }
            }
            else if (!isControlPanelCollapsed)
            {
                // Feedback visual ao passar o mouse
                resizePanel.BackColor = Color.FromArgb(80, 80, 85);
            }
        }

        private void ResizePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isResizing = false;
                resizePanel.Capture = false;
                if (!isControlPanelCollapsed)
                {
                    resizePanel.BackColor = Color.FromArgb(60, 60, 65);
                }
            }
        }
        
        // Handler global para continuar o redimensionamento mesmo quando o mouse sai do painel
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isResizing)
            {
                int currentMouseX = Control.MousePosition.X;
                int deltaX = currentMouseX - resizeStartX;
                int newWidth = resizeStartWidth + deltaX;
                
                if (newWidth >= minControlPanelWidth && newWidth <= maxControlPanelWidth)
                {
                    controlPanel.Width = newWidth;
                    savedControlPanelWidth = newWidth;
                    btnTogglePanel.Location = new Point(controlPanel.Width - 40, 10);
                }
            }
        }
        
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (isResizing && e.Button == MouseButtons.Left)
            {
                isResizing = false;
                resizePanel.Capture = false;
                if (!isControlPanelCollapsed)
                {
                    resizePanel.BackColor = Color.FromArgb(60, 60, 65);
                }
            }
        }
        
        private void ResizePanel_MouseLeave(object sender, EventArgs e)
        {
            if (!isResizing && !isControlPanelCollapsed)
            {
                resizePanel.BackColor = Color.FromArgb(60, 60, 65);
            }
        }

        private void BtnTogglePanel_Click(object sender, EventArgs e)
        {
            if (isControlPanelCollapsed)
            {
                // Expande o painel
                controlPanel.Width = savedControlPanelWidth;
                btnTogglePanel.Text = "◀";
                btnTogglePanel.Location = new Point(controlPanel.Width - 40, 10);
                isControlPanelCollapsed = false;
                
                // Mostra o resizePanel primeiro
                resizePanel.Visible = true;
                
                // Mostra todos os controles
                foreach (Control ctrl in controlPanel.Controls)
                {
                    if (ctrl != resizePanel && ctrl != btnTogglePanel)
                        ctrl.Visible = true;
                }
            }
            else
            {
                // Colapsa o painel
                savedControlPanelWidth = controlPanel.Width;
                controlPanel.Width = collapsedWidth;
                btnTogglePanel.Text = "▶";
                btnTogglePanel.Location = new Point(10, 10);
                isControlPanelCollapsed = true;
                
                // Esconde todos os controles exceto o botão de toggle e resizePanel
                // Primeiro encontra o scrollPanel
                Panel? scrollPanel = null;
                foreach (Control ctrl in controlPanel.Controls)
                {
                    if (ctrl is Panel panel && panel.AutoScroll && ctrl != resizePanel)
                    {
                        scrollPanel = panel;
                        break;
                    }
                }
                
                // Esconde o scrollPanel e todos os outros controles
                foreach (Control ctrl in controlPanel.Controls)
                {
                    if (ctrl != resizePanel && ctrl != btnTogglePanel)
                        ctrl.Visible = false;
                }
                
                // Esconde também o resizePanel quando colapsado
                resizePanel.Visible = false;
            }
            
            graphPanel.Invalidate();
        }
    }
}

