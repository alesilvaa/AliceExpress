using System.Collections;
using UnityEngine;

public class MaterialOffsetChanger : MonoBehaviour
{
    [SerializeField] private Material material; // El material a modificar

    public float offsetIncrement = 4f; // Incremento del offset en X
    public float changeSpeed = 1f; // Velocidad con la que cambia el offset

    private Vector2 currentOffset;

    void Start()
    {
        // Inicializa el valor del offset
        if (material != null)
        {
            currentOffset = material.mainTextureOffset;
        }
    }

    void Update()
    {
        // Verifica si se ha presionado la tecla espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(ChangeOffsetGradually(0.33f));
        }
    }

    public IEnumerator ChangeOffsetGradually(float duration)
    {
        yield return new WaitForSeconds(0.1f);
        // Asegúrate de que el material esté asignado
        if (material != null)
        {
            // Calculamos el valor final del offset en X según la cantidad de incremento
            float targetOffsetX = currentOffset.x + offsetIncrement;

            // Calculamos cuántos segundos debe durar el cambio
            float startTime = Time.time;
            float endTime = startTime + duration;

            // Realizamos el cambio gradualmente hasta que el tiempo haya pasado
            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / duration; // Normalizamos el tiempo
                float newOffsetX = Mathf.Lerp(currentOffset.x, targetOffsetX, t); // Interpolación lineal
                material.mainTextureOffset = new Vector2(newOffsetX, material.mainTextureOffset.y);
                yield return null;
            }

            // Asegura que el offset final sea exacto
            material.mainTextureOffset = new Vector2(targetOffsetX, material.mainTextureOffset.y);
            currentOffset = material.mainTextureOffset; // Actualiza el valor del offset
        }
    }
}