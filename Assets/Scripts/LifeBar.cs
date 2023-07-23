using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LifeBar : MonoBehaviour
{
    PCharacter _target;

    [SerializeField] Image _myLifebar;

    [SerializeField] float _yOffSet;

    public LifeBar SetTarget(PCharacter target)
    {
        _target = target;

        _target.OnLifebarUpdate += UpdateLifebar;
        _target.OnDeath += () => Destroy(gameObject);
        return this;
    }

    void UpdateLifebar(float amount)
    {
        _myLifebar.fillAmount = amount;
    }

    public void UpdatePosition()
    {
        transform.position = _target.transform.position + Vector3.up * _yOffSet;
    }
}
