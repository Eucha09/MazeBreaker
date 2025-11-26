using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class StarManager
{
    Dictionary<Vector2Int, int> _intersectionsCount = new Dictionary<Vector2Int, int>();

    public void Init()
    {

    }

    public bool IsIntersecting(Vector2Int gridPos)
    {
        return _intersectionsCount.ContainsKey(gridPos);
    }

    public void Intersect(Vector2Int gridPos)
    {
        if (_intersectionsCount.ContainsKey(gridPos))
            _intersectionsCount[gridPos]++;
        else
            _intersectionsCount.Add(gridPos, 1);
    }

    public void Separate(Vector2Int gridPos)
    {
        if (_intersectionsCount.ContainsKey(gridPos))
        {
            _intersectionsCount[gridPos]--;
            if (_intersectionsCount[gridPos] == 0)
                _intersectionsCount.Remove(gridPos);
        }
    }

    public void DrawConstellation(Vector3 curPos)
    {
        List<StarPiece> stars = new List<StarPiece>();
        List<StarEdge> edges = new List<StarEdge>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (Minimap_Marker obj in Managers.Minimap.Markers)
            if (obj.SyncPositionTarget != null &&  obj.SyncPositionTarget.GetComponent<StarPiece>() != null)
                if ((obj.SyncPositionTarget.position - curPos).magnitude <= 18.0f * 5.0f)
                    stars.Add(obj.SyncPositionTarget.GetComponent<StarPiece>());

        for (int i = 0; i < stars.Count; i++)
        {
            for (int j = i + 1; j < stars.Count; j++)
            {
                StarEdge edge = stars[i].GetEdgeConnectedTo(stars[j]);
                if (edge != null)
                {
                    edge.Start = i;
                    edge.End = j;
                    edges.Add(edge);
                }
            }
        }

        edges.Sort((e1, e2) => e1.Distance.CompareTo(e2.Distance));

        UnionFind uf = new UnionFind(stars.Count);

        foreach (var edge in edges)
        {
            if (uf.Union(edge.Start, edge.End))
            {
                foreach (MazeCell cell in edge.Cells)
                    if (!visited.Contains(cell.GridPos))
                        visited.Add(cell.GridPos);
                edge.DrawEdge();
                continue;
            }

            bool necessary = false;
            foreach (MazeCell cell in edge.Cells)
            {
                if (_intersectionsCount.ContainsKey(cell.GridPos) && !visited.Contains(cell.GridPos))
                {
                    necessary = true;
                    break;
                }
            }

            if (necessary)
            {
                foreach (MazeCell cell in edge.Cells)
                    if (!visited.Contains(cell.GridPos))
                        visited.Add(cell.GridPos);
                edge.DrawEdge();
            }
            else
            {
                edge.EraseEdge();
            }
        }

    }

    public void CreateEdgesAround(Vector3 curPos)
    {
        List<StarPiece> stars = new List<StarPiece>();

        foreach (Minimap_Marker obj in Managers.Minimap.Markers)
            if (obj.SyncPositionTarget != null && obj.SyncPositionTarget.GetComponent<StarPiece>() != null)
                if ((obj.SyncPositionTarget.position - curPos).magnitude <= 18.0f * 5.0f)
                    stars.Add(obj.SyncPositionTarget.GetComponent<StarPiece>());

        for (int i = 0; i < stars.Count; i++)
        {
            for (int j = i + 1; j < stars.Count; j++)
            {
                StarEdge edge = stars[i].GetEdgeConnectedTo(stars[j]);
                if (edge != null)
                    continue;

                Vector3 dir = stars[j].transform.position - stars[i].transform.position;
                if (dir.magnitude > 18.0f * 5.0f) // 5칸
                    continue;

                if (Physics.Raycast(stars[i].transform.position + Vector3.up, dir, dir.magnitude, 1 << (int)Define.Layer.Block))
                    continue;

                // edge 생성
                StarEdge starEdge = Managers.Resource.Instantiate("Nature/StarEdge").GetComponent<StarEdge>();
                starEdge.Init(stars[i].gameObject, stars[j].gameObject);
                stars[i].Edges.Add(starEdge);
                stars[j].Edges.Add(starEdge);
            }
        }

        DrawConstellation(curPos);
    }

    public List<StarPiece> FindPath(StarPiece start, StarPiece dest)
    {
        List<StarPiece> path = new List<StarPiece>();
        HashSet<StarPiece> visited = new HashSet<StarPiece>();
        Dictionary<StarPiece, StarPiece> parent = new Dictionary<StarPiece, StarPiece>();
        Queue<StarPiece> q = new Queue<StarPiece>();

        visited.Add(start);
        q.Enqueue(start);
        parent[start] = start;
        while (q.Count > 0)
        {
            StarPiece cur = q.Dequeue();

            if (cur == dest)
                break;

            foreach (StarPiece next in cur.GetConnectedStars())
            {
                if (visited.Contains(next))
                    continue;

                parent[next] = cur;
                visited.Add(next);
                q.Enqueue(next);
            }
        }

        if (visited.Contains(dest))
        {
            StarPiece cur = dest;
            while (parent[cur] != cur)
            {
                path.Add(cur);
                cur = parent[cur];
            }
            path.Add(cur);
            path.Reverse();
        }

        return path;
	}

	public List<StarPiece> FindPath(StarPiece start, Vector2Int destGridPos)
	{
        StarPiece dest = null;
		List<StarPiece> path = new List<StarPiece>();
		HashSet<StarPiece> visited = new HashSet<StarPiece>();
		Dictionary<StarPiece, StarPiece> parent = new Dictionary<StarPiece, StarPiece>();
		Queue<StarPiece> q = new Queue<StarPiece>();

		visited.Add(start);
		q.Enqueue(start);
		parent[start] = start;
		while (q.Count > 0)
		{
			StarPiece cur = q.Dequeue();
            Vector2Int curGridPos = Managers.Map.WorldToGrid(cur.transform.position);

            if ((curGridPos - destGridPos).sqrMagnitude <= 1)
            {
                dest = cur;
				break;
            }

			foreach (StarPiece next in cur.GetConnectedStars())
			{
				if (visited.Contains(next))
					continue;

				parent[next] = cur;
				visited.Add(next);
				q.Enqueue(next);
			}
		}

		if (visited.Contains(dest))
		{
			StarPiece cur = dest;
			while (parent[cur] != cur)
			{
				path.Add(cur);
				cur = parent[cur];
			}
			path.Add(cur);
			path.Reverse();
		}

		return path;
	}

	public List<GameObject> FindAllConnectedStarsAndEdges(StarPiece start)
    {
        List<GameObject> ret = new List<GameObject>();
		HashSet<StarPiece> visited = new HashSet<StarPiece>();
		Queue<StarPiece> q = new Queue<StarPiece>();

		ret.Add(start.gameObject);
		visited.Add(start);
		q.Enqueue(start);
		while (q.Count > 0)
		{
			StarPiece cur = q.Dequeue();

			foreach (StarPiece next in cur.GetConnectedStars())
			{
				if (visited.Contains(next))
					continue;

                ret.Add(cur.GetEdgeConnectedTo(next).gameObject);
				ret.Add(next.gameObject);
				visited.Add(next);
				q.Enqueue(next);
			}
		}

        return ret;
	}

	public void Clear()
    {

    }
}

public class UnionFind
{
    private int[] _parent;

    public UnionFind(int size)
    {
        _parent = new int[size];
        for (int i = 0; i < size; i++)
        {
            _parent[i] = i;
        }
    }

    public int Find(int x)
    {
        if (_parent[x] != x)
        {
            _parent[x] = Find(_parent[x]);
        }
        return _parent[x];
    }

    public bool Union(int x, int y)
    {
        int rootX = Find(x);
        int rootY = Find(y);
        if (rootX != rootY)
        {
            _parent[rootY] = rootX;
            return true;
        }
        return false;
    }
}