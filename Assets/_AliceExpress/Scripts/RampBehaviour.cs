using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RampBehaviour : MonoBehaviour
{
    [SerializeField] private List<ObjectsBehaviour> objects = new List<ObjectsBehaviour>();
    [SerializeField] private MaterialOffsetChanger materialOffsetChanger;
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

            int spacesAvailable = boxData.PosObjetos.Count - boxData.posicionesOcupadas.Count;
            int objectsToMove = Mathf.Min(matchingObjects.Count, spacesAvailable);

            if (matchingObjects.Count > 0)
            {
                if (objectsToMove > 0)
                {
                    isProcessing = true; // Marcar como procesando

                    StartCoroutine(MoveObjectsToBox(boxData, matchingObjects, objectsToMove));
                    StartCoroutine(materialOffsetChanger.ChangeOffsetGradually(0.29f));
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
          

            if (objectsToMove > 0 && !isProcessing) // Evitar iniciar mientras ya está procesando
            {
                isProcessing = true; // Marcar como procesando
                StartCoroutine(MoveObjectsToBox(boxData, matchingObjects, objectsToMove));
                StartCoroutine(materialOffsetChanger.ChangeOffsetGradually(0.29f));
            }
            else if (objectsToMove == 0)
            {
                Debug.LogWarning("No hay suficientes posiciones disponibles en la caja o no hay objetos para mover.");
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
                //StartCoroutine(boxData.HandleAllPositionsOccupied());
            }
        }
    }


    private IEnumerator MoveObjectsToBox(BoxBehaviour boxData, List<ObjectsBehaviour> matchingObjects, int objectsToMove)
    {
        yield return new WaitForSeconds(0.05f);

        int movedObjects = 0; // Contador para objetos movidos

        // Asegurar que el bucle itera hasta que se muevan todos los permitidos
        for (int i = 0; i < matchingObjects.Count && movedObjects < objectsToMove; i++)
        {
            var objBehaviour = matchingObjects[i];

            Transform targetPosition = boxData.GetNextAvailablePosition();

            if (targetPosition == null)
            {
                Debug.LogWarning("No hay más posiciones disponibles en la caja.");
                break;
            }

            // Marcar la posición como ocupada inmediatamente
            boxData.MarkPositionAsOccupied(targetPosition);

            if (!isBoxInTrigger)
            {
                Debug.LogWarning("La caja salió del trigger. Deteniendo el movimiento.");
                isProcessing = false;
                yield break;
            }

            GameObject obj = objBehaviour.gameObject;

            float jumpHeight = 2.5f;

            // Mover el objeto
            obj.transform
                .DOJump(targetPosition.position, jumpHeight, 1, 0.155f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    SoundManager.Instance.PlayPop();
                    obj.transform.SetParent(targetPosition);
                });

            // Remover de la lista principal
            objects.Remove(objBehaviour);
            movedObjects++; // Incrementar el contador de objetos movidos

            // Actualizar las posiciones de los objetos restantes en la rampa
            UpdateObjectsPositions();

            // Esperar antes de mover el siguiente objeto
            yield return new WaitForSeconds(0.155f);
        }

        // Si se movieron todos los objetos permitidos, liberar la bandera de procesamiento
        isProcessing = false;
    }



    private void UpdateObjectsPositions()
    {
        for (int i = 0; i < objects.Count; i++)
        {
            Transform targetPosition = transform.GetChild(i);

            // Reorganizar los objetos restantes en la rampa
            if (objects[i].transform.position != targetPosition.position)
            {
                objects[i].transform
                    .DOMove(targetPosition.position, 0.12f)
                    .SetEase(Ease.InOutQuad);
            }
        }
    }


}