using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Necesario para usar DoTween

public class BoxBehaviour : MonoBehaviour
{
    [SerializeField] private Box box;
    private string nombreCaja;
    [SerializeField] private List<Transform> posObjetos;
    private HashSet<Transform> posicionesOcupadas = new HashSet<Transform>();

    public List<Transform> PosObjetos => posObjetos;

    public Box Box => box;

    public string NombreCaja
    {
        get => nombreCaja;
        set => nombreCaja = value;
    }

    private void Awake()
    {
        nombreCaja = box.name;
    }

    public Transform GetNextAvailablePosition()
    {
        foreach (var pos in posObjetos)
        {
            if (!posicionesOcupadas.Contains(pos))
            {
                return pos;
            }
        }

        StartCoroutine( HandleAllPositionsOccupied()); // Si no hay posiciones disponibles, manejarlo.
        return null;
    }

    public void MarkPositionAsOccupied(Transform position)
    {
        if (!posicionesOcupadas.Contains(position))
        {
            posicionesOcupadas.Add(position);
        }

        if (posicionesOcupadas.Count == posObjetos.Count)
        {
            StartCoroutine(HandleAllPositionsOccupied()); // Verificar si están todas ocupadas.
        }
    }


    public IEnumerator HandleAllPositionsOccupied()
    {
        yield return new WaitForSeconds(0.4f);
        EventsManager.Instance.BoxCleared(this.gameObject);
        // Elevarse en Y hasta 3
        transform.DOMoveY(3f, .4f)
            .OnComplete(() =>
            {
                // Luego moverse en X hasta 20
                transform.DOMoveX(20f, .5f)
                    .OnComplete(() =>
                    {
                       
                        // Opcional: destruir el objeto después de salir de la escena
                        Destroy(gameObject);
                    });
            });
    }
}