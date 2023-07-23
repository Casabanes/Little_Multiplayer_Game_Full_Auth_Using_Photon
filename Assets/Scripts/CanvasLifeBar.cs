using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CanvasLifeBar : MonoBehaviour
{
    [SerializeField] LifeBar _lifebarPrefab;

    public event Action UpdateBars = delegate { };

    public void SpawnBar(PCharacter target)
    {
        LifeBar lifebar = Instantiate(_lifebarPrefab, target.transform.position, Quaternion.identity, transform)
                          .SetTarget(target);

        UpdateBars += lifebar.UpdatePosition;

        target.OnDeath += () => UpdateBars -= lifebar.UpdatePosition;
    }

    void LateUpdate()
    {
        UpdateBars();
    }
}
