using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RampBehaviour : MonoBehaviour
{
    [SerializeField] private List<ObjectsBehaviour> objects = new List<ObjectsBehaviour>();
    private bool isProcessing = false; // Bandera para controlar el procesamiento
    private bool isBoxInTrigger = false; // Bandera para verificar si la caja está en el trigger

    void Start()
    {
        foreach (Transform child in transform)
        {
            ObjectsBehaviour objBehaviour = child.GetComponent<ObjectsBehaviour>();
            if (objBehaviour != null)
            {
                objects.Add(objBehaviour);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        BoxBehaviour boxData = other.GetComponent<BoxBehaviour>();

        if (boxData != null)
        {
            isBoxInTrigger = true; // Registrar que la caja está en el trigger

            if (isProcessing) return; // Si ya está procesando, no hacer nada más

            if (objects == null || objects.Count == 0)
            {
                Debug.LogWarning("La lista de objetos no está asignada o está vacía.");
                return;
            }

            if (objects[0].name != boxData.NombreCaja)
            {
                Debug.LogWarning("El primer objeto en la lista no coincide con el nombre de la caja.");
                return;
            }

            List<ObjectsBehaviour> matchingObjects = new List<ObjectsBehaviour>();

            foreach (var obj in objects)
            {
                if (obj == null) continue;

                if (obj.name == boxData.NombreCaja)
                {
                    matchingObjects.Add(obj);
                }
            }

            if (matchingObjects.Count > 0)
            {
                int spacesAvailable = boxData.PosObjetos.Count;
                int objectsToMove = Mathf.Min(matchingObjects.Count, spacesAvailable);

                if (objectsToMove > 0)
                {
                    isProcessing = true; // Marcar como procesando
                    StartCoroutine(MoveObjectsToBox(boxData, matchingObjects, objectsToMove));
                }
                else
                {
                    Debug.LogWarning("No hay suficientes posiciones disponibles en la caja.");
                }
            }
            else
            {
                Debug.Log("No se encontraron objetos que coincidan con el nombre de la caja.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BoxBehaviour boxData = other.GetComponent<BoxBehaviour>();
        if (boxData != null)
        {
            isBoxInTrigger = false; 
            isProcessing = false;

            // Validar si todas las posiciones están ocupadas al salir del trigger
            if (boxData.GetNextAvailablePosition() == null)
            {
                Debug.Log("Todas las posiciones están ocupadas al salir del trigger.");
                StartCoroutine(boxData.HandleAllPositionsOccupied());
            }
        }
    }


    private IEnumerator MoveObjectsToBox(BoxBehaviour boxData, List<ObjectsBehaviour> matchingObjects, int objectsToMove)
    {
        yield return new WaitForSeconds(0.45f);

        for (int i = 0; i < objectsToMove; i++)
        {
            if (!isBoxInTrigger)
            {
                Debug.LogWarning("La caja salió del trigger. Deteniendo el movimiento.");
                isProcessing = false;
                yield break;
            }

            GameObject obj = matchingObjects[i].gameObject;
            Transform targetPosition = boxData.GetNextAvailablePosition();

            if (targetPosition == null)
            {
                Debug.LogWarning("No hay posiciones disponibles en la caja.");
                break;
            }

            // Marcar la posición como ocupada inmediatamente
            boxData.MarkPositionAsOccupied(targetPosition);

            float jumpHeight = 2.0f;

            obj.transform
                .DOJump(targetPosition.position, jumpHeight, 1, 0.2f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    obj.transform.SetParent(targetPosition);
                    Debug.Log($"Objeto {obj.name} movido a posición {targetPosition.name}.");
                });

            objects.Remove(matchingObjects[i]);
            UpdateObjectsPositions();

            yield return new WaitForSeconds(0.2f);
        }

        isProcessing = false;
    }


    private void UpdateObjectsPositions()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            Transform targetPosition = transform.GetChild(i);

            if (objects[i].transform.position != targetPosition.position)
            {
                objects[i].transform.DOMove(targetPosition.position, 0.12f).SetEase(Ease.InOutQuad);
            }
        }
    }
}
