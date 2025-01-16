using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

public class InstantiatedObjectsMover : MonoBehaviour
{
    public SplineContainer splineContainer; // Asigna el SplineContainer aquí
    public float speed = 1f;                // Velocidad de movimiento
    public AnimationCurve rampCurve;       // Define la forma de la rampa

    private List<GameObject> instantiatedObjects = new List<GameObject>();
    private List<float> progresses = new List<float>(); // Progreso individual de cada objeto

    void Start()
    {
        // Captura los objetos instanciados por el Spline Instantiate
        SplineInstantiate splineInstantiate = GetComponent<SplineInstantiate>();
        if (splineInstantiate != null && splineContainer != null)
        {
            foreach (Transform child in transform)
            {
                instantiatedObjects.Add(child.gameObject);
                progresses.Add(0f); // Inicializar el progreso de cada objeto
            }
        }
    }

    void Update()
    {
        if (splineContainer == null || instantiatedObjects.Count == 0) return;

        // Mover cada objeto a lo largo del spline
        for (int i = 0; i < instantiatedObjects.Count; i++)
        {
            GameObject obj = instantiatedObjects[i];
            if (obj == null) continue;

            // Incrementar el progreso del objeto
            progresses[i] += speed * Time.deltaTime;
            if (progresses[i] > 1f) progresses[i] -= 1f; // Reiniciar al inicio si supera el final

            // Obtener posición y tangente del spline
            Vector3 position = splineContainer.Spline.EvaluatePosition(progresses[i]);
            Vector3 tangent = splineContainer.Spline.EvaluateTangent(progresses[i]);

            // Aplicar altura según la curva de rampa
            float rampHeight = rampCurve.Evaluate(progresses[i]);
            position.y += rampHeight;

            // Actualizar posición y rotación
            obj.transform.position = position;
            obj.transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
        }
    }
}
