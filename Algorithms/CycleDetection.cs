using System;
using System.Collections.Generic;
using System.Linq;
using GrafosApp.Models;

namespace GrafosApp.Algorithms
{
    /// <summary>
    /// Implementa detecção de ciclos em grafos (dirigidos e não dirigidos)
    /// </summary>
    public class CycleDetection : IGraphAlgorithm
    {
        public string Name => "Detecção de Ciclos";
        public string Description => "Detecta se o grafo contém ciclos";

        public class CycleResult
        {
            public bool HasCycle { get; set; }
            public List<List<Vertex>> AllCycles { get; set; }
            public List<Vertex> FirstCycle { get; set; } // Para compatibilidade e visualização

            public CycleResult()
            {
                HasCycle = false;
                AllCycles = new List<List<Vertex>>();
                FirstCycle = new List<Vertex>();
            }
        }

        public CycleResult Execute(Graph graph)
        {
            var result = new CycleResult();

            if (graph.Vertices.Count == 0)
                return result;

            if (graph.IsDirected)
            {
                result = DetectCycleDirected(graph);
            }
            else
            {
                result = DetectCycleUndirected(graph);
            }

            return result;
        }

        private CycleResult DetectCycleUndirected(Graph graph)
        {
            var result = new CycleResult();
            var cyclesFound = new HashSet<string>(); // Para evitar ciclos duplicados
            var processedVertices = new HashSet<Vertex>(); // Para evitar processar o mesmo vértice múltiplas vezes

            // Para cada vértice, tenta encontrar todos os ciclos que passam por ele
            // Mas só processa vértices que ainda não foram processados como ponto de partida
            foreach (var startVertex in graph.Vertices.OrderBy(v => v.Label))
            {
                // Pula se já processamos um vértice equivalente (para reduzir duplicatas)
                // Mas ainda precisamos verificar todos para garantir que encontramos todos os ciclos
                var path = new List<Vertex> { startVertex };
                var visited = new HashSet<Vertex> { startVertex };
                FindAllCyclesUndirected(graph, startVertex, startVertex, path, visited, result, cyclesFound);
            }

            result.HasCycle = result.AllCycles.Count > 0;
            if (result.AllCycles.Count > 0)
            {
                result.FirstCycle = result.AllCycles[0];
            }

            return result;
        }
        
        private void FindAllCyclesUndirected(Graph graph, Vertex start, Vertex current, 
            List<Vertex> path, HashSet<Vertex> visited, CycleResult result, HashSet<string> cyclesFound)
        {
            var neighbors = graph.GetNeighbors(current);
            
            foreach (var neighbor in neighbors)
            {
                // Se encontramos o vértice inicial e o caminho tem pelo menos 3 vértices, temos um ciclo
                if (neighbor == start && path.Count >= 3)
                {
                    var cycle = new List<Vertex>(path);
                    cycle.Add(start); // Fecha o ciclo
                    
                    var normalized = NormalizeCycle(cycle);
                    if (!cyclesFound.Contains(normalized))
                    {
                        cyclesFound.Add(normalized);
                        result.AllCycles.Add(cycle);
                    }
                }
                // Se o vizinho não foi visitado e não é o início (exceto quando fechamos o ciclo)
                else if (!visited.Contains(neighbor))
                {
                    path.Add(neighbor);
                    visited.Add(neighbor);
                    FindAllCyclesUndirected(graph, start, neighbor, path, visited, result, cyclesFound);
                    path.RemoveAt(path.Count - 1);
                    visited.Remove(neighbor);
                }
            }
        }

        private CycleResult DetectCycleDirected(Graph graph)
        {
            var result = new CycleResult();
            var visited = new HashSet<Vertex>();
            var recStack = new HashSet<Vertex>();
            var parent = new Dictionary<Vertex, Vertex>();
            var cyclesFound = new HashSet<string>(); // Para evitar ciclos duplicados

            foreach (var vertex in graph.Vertices)
            {
                if (!visited.Contains(vertex))
                {
                    DFSDirected(graph, vertex, visited, recStack, parent, result, cyclesFound);
                }
            }

            result.HasCycle = result.AllCycles.Count > 0;
            if (result.AllCycles.Count > 0)
            {
                result.FirstCycle = result.AllCycles[0];
            }

            return result;
        }

        private void DFSDirected(Graph graph, Vertex current, HashSet<Vertex> visited, 
            HashSet<Vertex> recStack, Dictionary<Vertex, Vertex> parent, CycleResult result, HashSet<string> cyclesFound)
        {
            visited.Add(current);
            recStack.Add(current);

            var neighbors = graph.GetEdgesFromVertex(current).Select(e => e.To);
            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    parent[neighbor] = current;
                    DFSDirected(graph, neighbor, visited, recStack, parent, result, cyclesFound);
                }
                else if (recStack.Contains(neighbor))
                {
                    // Ciclo encontrado - constrói o caminho do ciclo
                    var cycle = BuildCyclePath(current, neighbor, parent, graph);
                    if (cycle != null && cycle.Count > 0)
                    {
                        // Normaliza o ciclo para evitar duplicatas
                        var normalized = NormalizeCycle(cycle);
                        if (!cyclesFound.Contains(normalized))
                        {
                            cyclesFound.Add(normalized);
                            result.AllCycles.Add(cycle);
                        }
                    }
                }
            }

            recStack.Remove(current);
        }

        private List<Vertex> BuildCyclePath(Vertex start, Vertex end, Dictionary<Vertex, Vertex> parent, Graph graph)
        {
            // Para grafos dirigidos, usa a mesma lógica mas mais simples
            var path = new List<Vertex>();
            var current = start;
            
            // Constrói o caminho do ciclo
            while (current != null && current != end)
            {
                path.Add(current);
                if (!parent.TryGetValue(current, out current))
                    return new List<Vertex>();
                if (path.Count > graph.Vertices.Count) return new List<Vertex>(); // Previne loop infinito
            }
            
            if (current == end)
            {
                path.Add(end);
                path.Add(start); // Fecha o ciclo
                return path;
            }
            
            return new List<Vertex>();
        }

        private string NormalizeCycle(List<Vertex> cycle)
        {
            if (cycle.Count == 0) return "";
            
            // Remove o último vértice se for igual ao primeiro (fechamento do ciclo)
            var cycleVertices = new List<Vertex>(cycle);
            if (cycleVertices.Count > 1 && cycleVertices[0] == cycleVertices[cycleVertices.Count - 1])
            {
                cycleVertices.RemoveAt(cycleVertices.Count - 1);
            }
            
            if (cycleVertices.Count < 3) return "";
            
            // Encontra o menor vértice (por Label) para começar a normalização
            // Isso garante que ciclos idênticos tenham a mesma representação, independente do vértice inicial
            int minIndex = 0;
            string minLabel = cycleVertices[0].Label;
            for (int i = 1; i < cycleVertices.Count; i++)
            {
                if (string.Compare(cycleVertices[i].Label, minLabel, StringComparison.Ordinal) < 0)
                {
                    minIndex = i;
                    minLabel = cycleVertices[i].Label;
                }
            }
            
            // Reconstrói o ciclo começando do menor vértice em ambas as direções
            var forward = new List<string>();
            var backward = new List<string>();
            
            // Direção para frente (sentido horário)
            for (int i = 0; i < cycleVertices.Count; i++)
            {
                int idx = (minIndex + i) % cycleVertices.Count;
                forward.Add(cycleVertices[idx].Label);
            }
            
            // Direção para trás (sentido anti-horário) - começa do mesmo vértice mínimo
            for (int i = 0; i < cycleVertices.Count; i++)
            {
                int idx = (minIndex - i + cycleVertices.Count) % cycleVertices.Count;
                backward.Add(cycleVertices[idx].Label);
            }
            
            // Compara lexicograficamente e retorna a menor representação
            string forwardStr = string.Join("->", forward);
            string backwardStr = string.Join("->", backward);
            
            // Compara lexicograficamente - escolhe a menor representação
            // Isso garante que V3->V5->V4->V3 e V3->V4->V5->V3 sejam normalizados da mesma forma
            // E também garante que ciclos encontrados começando de vértices diferentes sejam normalizados igualmente
            return string.Compare(forwardStr, backwardStr, StringComparison.Ordinal) <= 0 
                ? forwardStr 
                : backwardStr;
        }
    }
}

