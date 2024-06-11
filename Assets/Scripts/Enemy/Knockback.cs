using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    [SerializeField] private float _knockBackTime = 0.25f;
    [SerializeField] private float _knockBackForce = 50f;

    private Rigidbody2D _rb;

    private bool _isKnockingBack;

    private float _timer;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_isKnockingBack)
        {
            _timer = Time.deltaTime;

            if (_timer > _knockBackTime) 
            { 
                _rb.velocity = new Vector2(0f, _rb.velocity.y);
                _rb.angularVelocity = 0f;
                _isKnockingBack = false;
            }
        }
    }

    public void StartKnockBack(Vector2 dir)
    {
        _isKnockingBack = true;
        _timer = 0f;
        _rb.AddForce(dir * _knockBackForce, ForceMode2D.Impulse);
    }

}
