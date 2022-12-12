using System;
using System.Collections.Generic;
using System.Linq;

internal class AStarClear
{
    static int xSize = 20;
    static int ySize = 10;
    static int obstacles = 100;
    static int seed = 0;
    static List<Node> openNodes = new List<Node>();
    static Node startNode;
    static Node endNode;
    static Node[,] nodeMatrix;

    static char pathChar = 'X';
    static char obstacleChar = 'O';
    static char emptyChar = '*';

    static void Main(string[] args)
    {
        int xStart, yStart, xEnd, yEnd;

        do
            Console.WriteLine("Enter x start:");
        while (!int.TryParse(Console.ReadLine(), out xStart));
            
        do
            Console.WriteLine("Enter y start:");
        while (!int.TryParse(Console.ReadLine(), out yStart));
            
        do
            Console.WriteLine("Enter x end:");
        while (!int.TryParse(Console.ReadLine(), out xEnd));
            
        do
            Console.WriteLine("Enter y end:");
        while (!int.TryParse(Console.ReadLine(), out yEnd));

        endNode = new Node(xEnd, yEnd, null);
        startNode = new Node(xStart, yStart, endNode);

        CreateMatrix();

        try
        {
            List<Node> path = AStart();
            PrintMap(path);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Path not possible!");
            PrintMap(new List<Node>());
        }

        Console.ReadLine();
    }

    static void CreateMatrix()
    {
        nodeMatrix = new Node[xSize, ySize];

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                nodeMatrix[x, y] = new Node(x, y, endNode);
            }
        }

        Random random = new Random(seed);
        for (int i = 0; i < obstacles; i++)
        {
            int nextObstacleX = random.Next(xSize);
            int nextObstacleY = random.Next(ySize);
            if (nodeMatrix[nextObstacleX, nextObstacleY].IsObstacle)
            {
                i--;
                continue;
            }

            if(nodeMatrix[nextObstacleX, nextObstacleY] == startNode)
            {
                i--;
                continue;
            }

            if (nodeMatrix[nextObstacleX, nextObstacleY] == endNode)
            {
                i--;
                continue;
            }

            nodeMatrix[nextObstacleX, nextObstacleY].SetObstacle();
        }
    }

    static void PrintMap(List<Node> _path)
    {
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (nodeMatrix[x, y].IsObstacle)
                    Console.Write(obstacleChar);
                else if (_path.Contains(nodeMatrix[x,y]))
                    Console.Write(pathChar);
                else
                    Console.Write(emptyChar);
            }
            Console.WriteLine();
        }
    }

    static List<Node> AStart()
    {
        startNode.SetCurrentCost(0f);
        startNode.RecalcTotalCost();
        nodeMatrix[startNode.X, startNode.Y] = startNode;
        nodeMatrix[endNode.X, endNode.Y] = endNode;
        openNodes.Add(startNode);

        Node currentNode = null;

        while(openNodes.Count != 0)
        {
            Node nextNode = openNodes.OrderBy(x => x.TotalCost).First();

            if (nextNode == endNode)
            {
                startNode.SetCameFrom(null);
                endNode.SetCameFrom(currentNode);
                return CreatePath(endNode);
            }
            currentNode = nextNode;

            openNodes.Remove(currentNode);

            List<Node> neighbourNodes = GetNeighbourNodes(currentNode);

            for (int i = 0; i < neighbourNodes.Count; i++)
            {
                if (!currentNode.TryGetCost(neighbourNodes[i], out float neighBourCost))
                    continue;

                float nextTotalCost = currentNode.CurrentCost + neighBourCost;

                if (nextTotalCost >= neighbourNodes[i].CurrentCost)
                    continue;

                neighbourNodes[i].SetCameFrom(currentNode);
                neighbourNodes[i].SetCurrentCost(nextTotalCost);
                neighbourNodes[i].RecalcTotalCost();

                if (!openNodes.Contains(neighbourNodes[i]))
                    openNodes.Add(neighbourNodes[i]);
            }
        }

        throw new InvalidOperationException();
    }

    private static List<Node> CreatePath(Node _endNode)
    {
        List<Node> path = new List<Node>();

        Node currentNode = _endNode;
        while (currentNode.CameFrom != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.CameFrom;
        }
        path.Add(currentNode);

        return path;
    }

    private static List<Node> GetNeighbourNodes(Node _currentNode)
    {
        List<Node> nodes = new List<Node>();

        int xMin = Math.Max(_currentNode.X - 1, 0);
        int xMax = Math.Min(_currentNode.X + 1, xSize - 1);
        int yMin = Math.Max(_currentNode.Y - 1, 0);
        int yMax = Math.Min(_currentNode.Y + 1, ySize - 1);

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                if (x != _currentNode.X || y != _currentNode.Y)
                    nodes.Add(nodeMatrix[x, y]);
            }
        }

        return nodes;
    }

    private class Node
    {
        public const float vertCost = 1f;
        public const float diagCost = 1.4f;

        public int X { get; }
        public int Y { get; }
        public bool IsObstacle { get; private set; }
        public float TotalCost { get; private set; } = float.MaxValue;
        public float CurrentCost { get; private set; } = float.MaxValue;
        public float HeuristicCost { get; private set; }
        public Node CameFrom { get; private set; }


        public Node(int _x, int _y, Node _endNode)
        {
            X = _x;
            Y = _y;
            IsObstacle = false;
            SetHeuristicCost(_endNode);
        }

        public void SetHeuristicCost(Node _endNode)
        {
            if (_endNode == null)
            {
                HeuristicCost = 0;
                return;
            }

            int xDistance = Math.Abs(X - _endNode.X);
            int yDistance = Math.Abs(Y - _endNode.Y);

            int verts = Math.Abs(xDistance - yDistance);
            int diags = Math.Max(xDistance, yDistance) - verts;
            HeuristicCost = verts * vertCost + diags * diagCost;
        }

        public void RecalcTotalCost()
        {
            TotalCost = CurrentCost + HeuristicCost;
        }

        public void SetCurrentCost(float _cost)
        {
            CurrentCost = _cost;
        }

        public void SetCameFrom(Node _cameFrom) => CameFrom = _cameFrom;

        public bool TryGetCost(Node _neighBourNode, out float _cost)
        {
            _cost = 0;
            if (_neighBourNode.IsObstacle)
                return false;

            if (X == _neighBourNode.X || Y == _neighBourNode.Y)
            {
                _cost = vertCost;
                return true;
            }

            _cost = diagCost;
            return true;
        }

        public void SetObstacle() => IsObstacle = true;
    }
}