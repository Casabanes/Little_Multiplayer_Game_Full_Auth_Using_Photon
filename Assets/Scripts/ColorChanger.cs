using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    [SerializeField] private Renderer[] r;
    public void ChangeColor(Color c)
    {
        foreach (var item in r)
        {
            item.material.color = c;
        }
    }
}
