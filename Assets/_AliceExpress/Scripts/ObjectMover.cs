using System;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public GridManager gridManager;
    private GameObject selectedObject;
    private Vector3 offset;
    private Plane dragPlane;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        else if (Input.GetMouseButton(0) && selectedObject != null)
        {
            OnMouseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            OnMouseUp();
        }
    }

    private void Start()
    {
        EventsManager.Instance.OnBoxCleared += ReleaseCellsForObject;
    }
    
    private void OnDestroy()
    {
        EventsManager.Instance.OnBoxCleared -= ReleaseCellsForObject;
    }

    void OnMouseDown()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit))
    {
        if (hit.collider.CompareTag("Movable"))
        {
            selectedObject = hit.collider.gameObject;
            dragPlane = new Plane(Vector3.up, hit.point);
            offset = selectedObject.transform.position - hit.point;

            // Eleva el objeto en el eje Y al seleccionarlo
            Vector3 elevatedPosition = selectedObject.transform.position;
            elevatedPosition.y = 1f;
            selectedObject.transform.position = elevatedPosition;

            // Libera las celdas ocupadas por este objeto al comenzar a moverlo
            ClearOccupiedCells(selectedObject);
        }
    }
}

    void OnMouseUp()
    {
        Vector3 currentPosition = selectedObject.transform.position;

        // Tamaño del objeto en celdas
        Vector3 scale = selectedObject.transform.localScale;
        int objWidth = Mathf.CeilToInt(scale.x / gridManager.cellSize);
        int objHeight = Mathf.CeilToInt(scale.z / gridManager.cellSize);

        // Posición inicial en la grilla, ajustada al tamaño del objeto
        Vector2Int gridPosition = CalculateGridPosition(currentPosition, objWidth, objHeight);

        // Verifica si el objeto cabe dentro de la grilla y no ocupa espacio inválido
        if (IsPlacementValid(gridPosition, objWidth, objHeight))
        {
            // Alinea al centro de las celdas que ocupa
            Vector3 alignedPosition = AlignToGrid(gridPosition, objWidth, objHeight);
            selectedObject.transform.position = alignedPosition;

            // Marca las nuevas celdas como ocupadas
            MarkOccupiedCells(gridPosition, objWidth, objHeight);
        }
        else
        {
            Debug.Log("No se puede colocar el objeto aquí.");
        }

        selectedObject = null;
    }



void OnMouseDrag()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (dragPlane.Raycast(ray, out float distance))
    {
        Vector3 point = ray.GetPoint(distance) + offset;

        // Tamaño del objeto en celdas
        float objWidth = selectedObject.transform.localScale.x;
        float objHeight = selectedObject.transform.localScale.z;

        // Límites de la grilla ajustados al tamaño del objeto
        float minX = objWidth / 2;
        float maxX = gridManager.cols * gridManager.cellSize - objWidth / 2;
        float minZ = objHeight / 2;
        float maxZ = gridManager.rows * gridManager.cellSize - objHeight / 2;

        // Clampea la posición para que no salga de los bordes
        point.x = Mathf.Clamp(point.x, minX, maxX);
        point.z = Mathf.Clamp(point.z, minZ, maxZ);

        // Calcula las celdas que ocuparía el objeto
        int objWidthInCells = Mathf.CeilToInt(objWidth / gridManager.cellSize);
        int objHeightInCells = Mathf.CeilToInt(objHeight / gridManager.cellSize);
        Vector2Int gridPosition = CalculateGridPosition(point, objWidthInCells, objHeightInCells);

        // Verifica si el movimiento llevaría a celdas ocupadas o inválidas
        if (IsPlacementValid(gridPosition, objWidthInCells, objHeightInCells))
        {
            // Si es válido, mueve el objeto libremente
            selectedObject.transform.position = point;
        }
        else
        {
            // Bloquea el movimiento manteniendo la posición previa válida
            Vector2Int previousGridPos = CalculateGridPosition(selectedObject.transform.position, objWidthInCells, objHeightInCells);
            Vector3 alignedPosition = AlignToGrid(previousGridPos, objWidthInCells, objHeightInCells);
            selectedObject.transform.position = alignedPosition;

            Debug.Log("Movimiento bloqueado: fuera de límites o celda ocupada.");
        }
    }
}



    /*void OnMouseUp()
    {
        Vector3 currentPosition = selectedObject.transform.position;

        // Tamaño del objeto en celdas
        Vector3 scale = selectedObject.transform.localScale;
        int objWidth = Mathf.CeilToInt(scale.x / gridManager.cellSize);
        int objHeight = Mathf.CeilToInt(scale.z / gridManager.cellSize);

        // Posición inicial en la grilla, ajustada al tamaño del objeto
        Vector2Int gridPosition = CalculateGridPosition(currentPosition, objWidth, objHeight);

        // Verifica si el objeto cabe dentro de la grilla y no ocupa espacio inválido
        if (IsPlacementValid(gridPosition, objWidth, objHeight))
        {
            // Alinea al centro de las celdas que ocupa
            Vector3 alignedPosition = AlignToGrid(gridPosition, objWidth, objHeight);
            selectedObject.transform.position = alignedPosition;

            // Marca las nuevas celdas como ocupadas
            MarkOccupiedCells(gridPosition, objWidth, objHeight);
        }
        else
        {
            Debug.Log("No se puede colocar el objeto aquí.");
        }

        selectedObject = null;
    }*/

    bool IsPlacementValid(Vector2Int gridPos, int width, int height)
    {
        if (gridPos.x < 0 || gridPos.y < 0 || gridPos.x + height > gridManager.rows || gridPos.y + width > gridManager.cols)
        {
            return false; // El objeto se sale de los límites.
        }

        for (int x = gridPos.x; x < gridPos.x + height; x++)
        {
            for (int y = gridPos.y; y < gridPos.y + width; y++)
            {
                if (!gridManager.IsCellEmpty(x, y))
                {
                    return false;
                }
            }
        }

        return true;
    }

    void ClearOccupiedCells(GameObject obj)
    {
        if (obj != null)
        {
            Vector3 scale = obj.transform.localScale;
            int objWidth = Mathf.CeilToInt(scale.x / gridManager.cellSize);
            int objHeight = Mathf.CeilToInt(scale.z / gridManager.cellSize);
            Vector2Int gridPosition = CalculateGridPosition(obj.transform.position, objWidth, objHeight);

            for (int x = gridPosition.x; x < gridPosition.x + objHeight; x++)
            {
                for (int y = gridPosition.y; y < gridPosition.y + objWidth; y++)
                {
                    gridManager.SetCellOccupied(x, y, false);
                }
            }
        }
    }

    public void ReleaseCellsForObject(GameObject obj)
    {
        ClearOccupiedCells(obj);
    }

    void MarkOccupiedCells(Vector2Int gridPos, int width, int height)
    {
        for (int x = gridPos.x; x < gridPos.x + height; x++)
        {
            for (int y = gridPos.y; y < gridPos.y + width; y++)
            {
                gridManager.SetCellOccupied(x, y, true);
            }
        }
    }

    Vector2Int CalculateGridPosition(Vector3 worldPosition, int width, int height)
    {
        int col = Mathf.FloorToInt((worldPosition.x - (width - 1) * gridManager.cellSize / 2) / gridManager.cellSize);
        int row = Mathf.FloorToInt((worldPosition.z - (height - 1) * gridManager.cellSize / 2) / gridManager.cellSize);
        return new Vector2Int(row, col);
    }

    Vector3 AlignToGrid(Vector2Int gridPos, int width, int height)
    {
        float centerX = (gridPos.y + (float)width / 2) * gridManager.cellSize;
        float centerZ = (gridPos.x + (float)height / 2) * gridManager.cellSize;
        return new Vector3(centerX, 0, centerZ);
    }
}
