using UnityEngine;

public class SpriteRepeater : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // Sprite a replicar
    public int numberOfCopies = 5; // Número de copias a crear
    public float spacing = 1.0f; // Espacio entre cada sprite
    public float moveSpeed = 1.0f; // Velocidad del movimiento
    private GameObject[] spriteCopies; // Array para almacenar las copias
    private float spriteWidth; // Ancho del sprite

    private void Start()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer no asignado.");
            return;
        }

        spriteWidth = spriteRenderer.bounds.size.x;
        CreateSprites();
    }

    private void CreateSprites()
    {
        spriteCopies = new GameObject[numberOfCopies];

        for (int i = 0; i < numberOfCopies; i++)
        {
            // Crear un nuevo objeto con el mismo sprite
            GameObject newSprite = new GameObject($"SpriteCopy_{i}");
            newSprite.transform.SetParent(transform);

            // Agregar un SpriteRenderer y copiar los valores del original
            SpriteRenderer newSpriteRenderer = newSprite.AddComponent<SpriteRenderer>();
            newSpriteRenderer.sprite = spriteRenderer.sprite;
            newSpriteRenderer.color = spriteRenderer.color;
            newSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder;

            // Posicionar la copia
            Vector3 newPosition = new Vector3(i * (spriteWidth + spacing), 0, 0);
            newSprite.transform.localPosition = newPosition;
            newSprite.transform.rotation = Quaternion.Euler(90,0,0);
            // Guardar la referencia de la copia
            spriteCopies[i] = newSprite;
        }
    }

    private void Update()
    {
        for (int i = 0; i < spriteCopies.Length; i++)
        {
            if (spriteCopies[i] == null) continue;

            // Mover el sprite hacia la izquierda dentro del contenedor
            spriteCopies[i].transform.localPosition += Vector3.left * moveSpeed * Time.deltaTime;

            // Reposicionar el sprite si sale del área visible
            if (spriteCopies[i].transform.localPosition.x < -spriteWidth)
            {
                // Moverlo al final de la fila
                float maxX = GetMaxX();
                spriteCopies[i].transform.localPosition = new Vector3(maxX + spriteWidth + spacing, 0, 0);
            }
        }
    }

    // Obtener la posición más a la derecha entre los sprites
    private float GetMaxX()
    {
        float maxX = float.MinValue;

        foreach (GameObject sprite in spriteCopies)
        {
            if (sprite != null)
            {
                float x = sprite.transform.localPosition.x;
                if (x > maxX) maxX = x;
            }
        }

        return maxX;
    }
}
