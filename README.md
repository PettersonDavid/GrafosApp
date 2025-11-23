# Aplicação de Algoritmos em Grafos

Aplicação desktop desenvolvida em C# (.NET 8.0) com Windows Forms para visualização e execução de algoritmos clássicos da teoria dos grafos.

## 📋 Funcionalidades

- ✅ **Geração de Grafos Aleatórios**: Grafos dirigidos/não dirigidos, valorados
- ✅ **Árvore Geradora Mínima (AGM)**: Algoritmo de Kruskal
- ✅ **Caminho Mínimo**: Algoritmo de Dijkstra
- ✅ **Fecho Transitivo**: Direto e inverso (para dígrafos)
- ✅ **Detecção de Ciclos**: Encontra todos os ciclos simples
- ✅ **Busca em Profundidade (DFS)**: Exploração em profundidade
- ✅ **Busca em Largura (BFS)**: Exploração em largura
- ✅ **Sequência de Graus**: Cálculo de graus para grafos dirigidos e não dirigidos

## 🛠️ Tecnologias

- **Linguagem**: C# (.NET 8.0)
- **Framework**: Windows Forms
- **Paradigma**: Programação Orientada a Objetos

## 🚀 Como Executar

### Pré-requisitos

- .NET 8.0 SDK instalado
- Windows (aplicação Windows Forms)

### Passos

1. Clone o repositório:
```bash
git clone https://github.com/PettersonDavid/GrafosApp.git
cd GrafosApp
```

2. Restaure as dependências:
```bash
dotnet restore
```

3. Compile o projeto:
```bash
dotnet build
```

4. Execute a aplicação:
```bash
dotnet run
```

Ou abra o projeto no Visual Studio e pressione F5.

## 📁 Estrutura do Projeto

```
GrafosApp/
├── Algorithms/          # Implementação dos algoritmos
│   ├── BreadthFirstSearch.cs
│   ├── CycleDetection.cs
│   ├── DepthFirstSearch.cs
│   ├── DegreeSequence.cs
│   ├── GraphGenerator.cs
│   ├── MinimumSpanningTree.cs
│   ├── ShortestPath.cs
│   └── TransitiveClosure.cs
├── Models/              # Modelos de dados
│   ├── Edge.cs
│   ├── Graph.cs
│   └── Vertex.cs
├── UI/                  # Componentes de interface
│   ├── AnimatedGraphPanel.cs
│   └── GraphPanel.cs
├── MainForm.cs          # Formulário principal
└── Program.cs           # Ponto de entrada
```

## 🎯 Algoritmos Implementados

| Algoritmo | Complexidade | Descrição |
|-----------|--------------|-----------|
| **Kruskal (AGM)** | O(E log E) | Árvore geradora mínima |
| **Dijkstra** | O(V²) | Caminho mínimo |
| **Fecho Transitivo** | O(V + E) | Vértices alcançáveis |
| **Detecção de Ciclos** | O(V + E) a O(V!) | Todos os ciclos simples |
| **DFS** | O(V + E) | Busca em profundidade |
| **BFS** | O(V + E) | Busca em largura |
| **Sequência de Graus** | O(V + E) | Cálculo de graus |

## 🎨 Interface

A aplicação possui uma interface gráfica intuitiva com painel lateral expansível, visualização interativa do grafo com animações, área de saída formatada com resultados, tooltips explicativos, controles de zoom e destaques coloridos para diferentes algoritmos.


## 👥 Autores

Desenvolvido por David Petterson e Arthur Gomes Valverde como projeto acadêmico para a disciplina de Algoritmos em Grafos. 

## 📄 Licença

Este projeto foi desenvolvido como parte da disciplina de Algoritmos em Grafos da PUC Minas - Campus Contagem.



