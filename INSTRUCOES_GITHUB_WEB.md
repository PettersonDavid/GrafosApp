# Instruções para Publicar no GitHub - Interface Web

## Passo 1: Criar o Repositório

1. Acesse: https://github.com/new
2. Faça login se necessário
3. Preencha:
   - **Repository name**: `GrafosApp`
   - **Description**: `Aplicação desktop para visualização e execução de algoritmos em grafos`
   - **Visibility**: Escolha **Public** ou **Private**
   - **NÃO marque** "Add a README file" (já temos um)
   - **NÃO marque** "Add .gitignore" (já temos um)
   - **NÃO marque** "Choose a license"
4. Clique em **"Create repository"**

---

## Passo 2: Upload dos Arquivos

### 2.1. Upload das Pastas

1. Na página do repositório recém-criado, clique no botão **"uploading an existing file"** (ou arraste arquivos para a área)

2. **Arraste e solte as PASTAS inteiras**:
   - Pasta `Algorithms` (com todos os arquivos .cs dentro)
   - Pasta `Models` (com todos os arquivos .cs dentro)
   - Pasta `UI` (com todos os arquivos .cs dentro)

3. **Arraste e solte os ARQUIVOS**:
   - `MainForm.cs`
   - `Program.cs`
   - `GrafosApp.csproj`
   - `GrafosApp.sln`
   - `README.md`
   - `RELATORIO_CORRIGIDO.md`
   - `.gitignore`

### 2.2. Finalizar o Upload

1. Role até o final da página
2. No campo **"Commit message"**, escreva:
   ```
   Initial commit: Aplicação de Algoritmos em Grafos
   ```
3. Clique em **"Commit changes"**

---

## Passo 3: Verificar

1. Acesse: `https://github.com/davidoliveiradev/GrafosApp`
2. Você deve ver todos os arquivos e pastas
3. O README.md deve aparecer na página inicial do repositório

---

## Dica Importante

Se você arrastar uma pasta inteira, o GitHub vai fazer upload de todos os arquivos dentro dela automaticamente. É mais rápido do que fazer arquivo por arquivo.

---

## Estrutura Final no GitHub

Seu repositório deve ter esta estrutura:

```
GrafosApp/
├── Algorithms/
│   ├── BreadthFirstSearch.cs
│   ├── CycleDetection.cs
│   ├── DepthFirstSearch.cs
│   ├── DegreeSequence.cs
│   ├── GraphGenerator.cs
│   ├── IGraphAlgorithm.cs
│   ├── MinimumSpanningTree.cs
│   ├── ShortestPath.cs
│   └── TransitiveClosure.cs
├── Models/
│   ├── Edge.cs
│   ├── Graph.cs
│   └── Vertex.cs
├── UI/
│   ├── AnimatedGraphPanel.cs
│   └── GraphPanel.cs
├── MainForm.cs
├── Program.cs
├── GrafosApp.csproj
├── GrafosApp.sln
├── README.md
├── RELATORIO_CORRIGIDO.md
└── .gitignore
```

---

**Pronto! Seu projeto estará no GitHub! 🎉**

