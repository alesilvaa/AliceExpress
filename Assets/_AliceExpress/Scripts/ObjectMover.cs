using System;
using System.Collections;
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
            StartCoroutine(SmoothMoveToY(selectedObject, .6f, 0.2f));


            // Libera las celdas ocupadas por este objeto al comenzar a moverlo
            ClearOccupiedCells(selectedObject);
        }
    }
}

    void OnMouseUp()
    {
        if (selectedObject != null)
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

            // Inicia el descenso suave con un tiempo fijo de 0.6 segundos
            StartCoroutine(SmoothMoveToY(selectedObject, 0f, 0.6f));
            selectedObject = null;
        }
    }





void OnMouseDrag()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (dragPlane.Raycast(ray, out float distance))
    {
        Vector3 targetPosition = ray.GetPoint(distance) + offset;

        // Tamaño del objeto y su collider
        Vector3 objSize = selectedObject.transform.localScale;
        Vector3 halfExtents = objSize / 2;

        // Límites de la grilla ajustados al tamaño del objeto
        float minX = objSize.x / 2;
        float maxX = gridManager.cols * gridManager.cellSize - objSize.x / 2;
        float minZ = objSize.z / 2;
        float maxZ = gridManager.rows * gridManager.cellSize - objSize.z / 2;

        // Clampea la posición objetivo para que no salga de los bordes
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

        // Posición actual
        Vector3 currentPosition = selectedObject.transform.position;

        // Variables para determinar si cada eje está bloqueado
        bool blockX = false;
        bool blockZ = false;

        // Chequear colisiones en el eje X (manteniendo Z)
        Vector3 checkPositionX = new Vector3(targetPosition.x, currentPosition.y, currentPosition.z);
        Collider[] hitCollidersX = Physics.OverlapBox(checkPositionX, halfExtents, Quaternion.identity);
        foreach (var collider in hitCollidersX)
        {
            // Ignorar colliders de tipo trigger o el mismo objeto
            if (collider.isTrigger || collider.gameObject == selectedObject)
                continue;

            blockX = true;
            break;
        }

        // Chequear colisiones en el eje Z (manteniendo X)
        Vector3 checkPositionZ = new Vector3(currentPosition.x, currentPosition.y, targetPosition.z);
        Collider[] hitCollidersZ = Physics.OverlapBox(checkPositionZ, halfExtents, Quaternion.identity);
        foreach (var collider in hitCollidersZ)
        {
            // Ignorar colliders de tipo trigger o el mismo objeto
            if (collider.isTrigger || collider.gameObject == selectedObject)
                continue;

            blockZ = true;
            break;
        }

        // Ajustar la posición en cada eje basado en colisiones y bordes
        float newX = blockX ? currentPosition.x : targetPosition.x;
        float newZ = blockZ ? currentPosition.z : targetPosition.z;

        // Actualizar la posición del objeto
        selectedObject.transform.position = new Vector3(newX, .6f, newZ);
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
    private IEnumerator SmoothMoveToY(GameObject obj, float targetY, float duration, AnimationCurve curve = null)
    {
        float elapsedTime = 0f;
        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            // Aplica curva si está disponible
            if (curve != null)
            {
                t = curve.Evaluate(t);
            }

            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = targetPos; // Asegura que alcance el destino final
    }



}
