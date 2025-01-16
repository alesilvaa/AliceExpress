using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // Singleton para acceso global

    public AudioSource effectsSource;   // Fuente para efectos de sonido
    public AudioClip touchSound;        // Sonido al tocar la pantalla
    public AudioClip fullBoxSound;      // Sonido al llenar la caja
    private bool once;

    private void Awake()
    {
        Application.targetFrameRate = 60; // Fijar la tasa de refresco a 60 FPS
        
        // Implementar el patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistir entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayPop()
    {
        effectsSource.PlayOneShot(touchSound);
    }
    
    public void PlayFullBox()
    {
        effectsSource.PlayOneShot(fullBoxSound);
    }
    
}