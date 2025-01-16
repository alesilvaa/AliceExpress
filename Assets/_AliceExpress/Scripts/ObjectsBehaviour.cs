using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsBehaviour : MonoBehaviour
{
    public Objects objects;
    public string name;

    private void Awake()
    {
        name = objects.name;
    }
}
