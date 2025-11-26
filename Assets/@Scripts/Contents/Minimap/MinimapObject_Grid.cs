using System.Collections.Generic;
using UnityEngine;

public class MinimapObject_Grid : MonoBehaviour
{
    LineRenderer lr;
    [SerializeField]
    int _rowCount, _colCount;
    [SerializeField]
    float _gridSize;
    [SerializeField]
    Color _color;

    void initLineRenderer(LineRenderer lr)
    {
        lr.startWidth = lr.endWidth = 0.5f;
        lr.material.color = _color;
    }

    void makeGrid(LineRenderer lr, int rowCount, int colCount, float gridSize)
    {
        List<Vector3> gridPos = new List<Vector3>();

        float sr = transform.position.z + rowCount / 2.0f * _gridSize;
        float sc = transform.position.x - colCount / 2.0f * _gridSize;

        float ec = sc + colCount * _gridSize;

        gridPos.Add(new Vector3(sc, this.transform.position.y, sr));
        gridPos.Add(new Vector3(ec, this.transform.position.y, sr));

        int toggle = -1;
        Vector3 currentPos = new Vector3(ec, this.transform.position.y, sr);
        for (int i = 0; i < rowCount; i++)
        {
            Vector3 nextPos = currentPos;

            nextPos.z -= _gridSize;
            gridPos.Add(nextPos);

            nextPos.x += (colCount * toggle * _gridSize);
            gridPos.Add(nextPos);

            currentPos = nextPos;
            toggle *= -1;
        }

        currentPos.z = sr;
        gridPos.Add(currentPos);

        int colToggle = toggle = -1;
        if (currentPos.x == sc) colToggle = 1;

        for (int i = 0; i < colCount; i++)
        {
            Vector3 nextPos = currentPos;

            nextPos.x += (colToggle * _gridSize);
            gridPos.Add(nextPos);

            nextPos.z += (rowCount * toggle * _gridSize);
            gridPos.Add(nextPos);

            currentPos = nextPos;
            toggle *= -1;
        }

        lr.positionCount = gridPos.Count;
        lr.SetPositions(gridPos.ToArray());
    }

    void Start()
    {
        lr = this.GetComponent<LineRenderer>();
        initLineRenderer(lr);

        transform.position = new Vector3(0.0f, -20.0f, 0.0f);
        transform.SetParent(Managers.Minimap.Root.transform);

        _rowCount = Managers.Map.RowSize / 2;
        _colCount = Managers.Map.ColSize / 2;
        _gridSize = Managers.Map.CellSize * 2;
        makeGrid(lr, _rowCount, _colCount, _gridSize);
    }
}
