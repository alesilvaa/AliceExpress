using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    private static UiManager instance;
    [SerializeField] private GameObject uiWin;
    
    
    public static UiManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EventsManager");
                instance = go.AddComponent<UiManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowWinUi()
    {
        uiWin.SetActive(true);
        
    }
}
