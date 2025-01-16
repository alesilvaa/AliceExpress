using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Necesario para usar DoTween

public class BoxBehaviour : MonoBehaviour
{
    [SerializeField] private Box box;
    private string nombreCaja;
    [SerializeField] private List<Transform> posObjetos;
    public HashSet<Transform> posicionesOcupadas = new HashSet<Transform>();
    [SerializeField] List<GameObject> boxCover;
    public List<Transform> PosObjetos => posObjetos;
    public GameObject starTrail;
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

        //StartCoroutine( HandleAllPositionsOccupied()); // Si no hay posiciones disponibles, manejarlo.
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
        ActivateBoxCovers();
        starTrail.SetActive(true);
        SoundManager.Instance.PlayFullBox();
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
    public void ActivateBoxCovers()
    {
        foreach (var cover in boxCover)
        {
            if (cover != null)
            {
                // Activar el objeto si no está activo
                cover.SetActive(true);

                // Obtener la rotación actual en el eje X
                float currentRotationX = cover.transform.rotation.eulerAngles.x;
                float targetRotation = 0f;

                // Si la rotación es exactamente 270 o -270, manejar específicamente
                if (Mathf.Approximately(currentRotationX, 270f))
                {
                    // Para 270, mantener el comportamiento original (antihorario)
                    targetRotation = 0f;
                }
                else if (Mathf.Approximately(currentRotationX, 90f)) // -270 se lee como 90 en Unity
                {
                    // Para -270 (90), ir en sentido horario
                    targetRotation = 360f;
                }

                // Aplicar la rotación gradual
                cover.transform.DORotate(
                    new Vector3(targetRotation, 
                        cover.transform.rotation.eulerAngles.y, 
                        cover.transform.rotation.eulerAngles.z),
                    0.18f,
                    RotateMode.FastBeyond360
                );
            }
        }
    }



}