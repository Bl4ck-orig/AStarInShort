using System;
using System.Collections.Generic;
using System.Linq;

internal class AStar
{

   static int xLength = 20;
   static int yLength = 10;
   static int obstacles = 20;
   static int seed = 0;
   
   static Node startNode, endNode;
   static Node[,] nodes;
   
   static void Main(string[] args)
   {
       Console.WriteLine("X start:");
       int xStart = int.Parse(Console.ReadLine());
       Console.WriteLine("Y start:");
       int yStart = int.Parse(Console.ReadLine());
       Console.WriteLine("X end:");
       int xEnd = int.Parse(Console.ReadLine()); 
       Console.WriteLine("Y end:");
       int yEnd = int.Parse(Console.ReadLine());

       endNode = new Node(xEnd, yEnd);
       startNode = new Node(xStart, yStart);

       CreateMatrix();

       try
       {
           List<Node> path = AStart();
           PrintMatrix(path);
       }
       catch(Exception e)
       {
           Console.WriteLine("Path not possible!");
           PrintMatrix(new List<Node>());
       }

       Console.ReadLine();
   }


    
    static void CreateMatrix()
    {
        nodes = new Node[xLength, yLength];
    
        for(int x = 0; x < xLength; x++)
            for(int y = 0; y < yLength; y++)
                nodes[x,y] = new Node(x,y);

        obstacles = Math.Min(obstacles, xLength * yLength);
        Random rng = new Random(seed);
        for(int i = 0; i < obstacles; i++)
        {
            int xRand = rng.Next(xLength);
            int yRand = rng.Next(yLength);

            Node n = nodes[xRand, yRand];
    
            if(n.IsObstacle || n == startNode || n == endNode)
            {
                i--;
                continue;
            }
    
            nodes[xRand,yRand].IsObstacle = true;
        }
    }

    static void PrintMatrix(List<Node> _p)
    {
        for (int y = 0; y < yLength; y++)
        {
            for (int x = 0; x < xLength; x++)
                Console.Write(((nodes[x, y].IsObstacle) ? 'O' : ((_p.Contains(nodes[x, y])) ? 'X' : '*')));

            Console.WriteLine();
        }
    }

    static List<Node> AStart()
    {
        List<Node> openNodes = new List<Node>();
        openNodes.Add(startNode);

        startNode.G = 0f;
        startNode.SetT();

        nodes[startNode.X, startNode.Y] = startNode;
        nodes[endNode.X, endNode.Y] = endNode;

        for(Node currentNode = null; openNodes.Count != 0; endNode.CameFrom = currentNode)
        {
            currentNode = openNodes.OrderBy(x => x.T + x.StepCost).First();

            if (currentNode == endNode)
                return SetPath(endNode);

            openNodes.Remove(currentNode);

            float nextG = 0f;
            float stepCost = 0f;
            foreach (var neighbour in GetNeighbours(currentNode)
                .Where(x => currentNode.GetStepCost(x, out stepCost) && (nextG = currentNode.G + stepCost) < x.G))
            {
                neighbour.StepCost = stepCost;
                neighbour.CameFrom = currentNode;
                neighbour.G = nextG;
                neighbour.SetT();
            
                if (!openNodes.Contains(neighbour))
                    openNodes.Add(neighbour);
            }
        }

        throw new InvalidOperationException();
    }

    static List<Node> SetPath(Node _eN)
    {
        List<Node> path = new List<Node>();

        for (; _eN != null; _eN = _eN.CameFrom)
            path.Add(_eN);

        return path;
    }

    private static List<Node> GetNeighbours(Node _n)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = Math.Max(_n.X - 1, 0); x <= Math.Min(_n.X + 1, xLength - 1); x++)
            for (int y = Math.Max(_n.Y - 1, 0); y <= Math.Min(_n.Y + 1, yLength - 1); y++)
                neighbours.Add(nodes[x, y]);

        neighbours.Remove(_n);
        return neighbours;
    }

    private class Node
    {
        float vertCost = 1;
        float diagCost = 1.4f;

        public int X, Y;
        public float T, G = float.MaxValue;
        public float H, StepCost;
        public bool IsObstacle;
        public Node CameFrom;

        public Node(int _x, int _y)
        {
            X = _x;
            Y = _y;
            IsObstacle = false;
            SetH();
        }

        public void SetH()
        {
            if (endNode == null)
                return;

            int xDist = Math.Abs(X - endNode.X);
            int yDist = Math.Abs(Y - endNode.Y);

            int verts = Math.Abs(xDist - yDist);
            int diags = (Math.Max(xDist, yDist) - verts);

            H = verts * vertCost + diags * diagCost;
        }

        public void SetT() => T = G + H;

        public bool GetStepCost(Node _nN, out float _c)
        {
            _c = (X == _nN.X || Y == _nN.Y) ? vertCost : diagCost;
            return !_nN.IsObstacle;
        }
    }
}
