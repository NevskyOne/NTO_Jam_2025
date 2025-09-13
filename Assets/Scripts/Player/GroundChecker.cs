using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GroundChecker : MonoBehaviour
{
    [SerializeField] private LayerMask _groundLayer;

    public event Action<bool> GroundStateChanged;

    private int _contacts;
    private bool _isGrounded;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsGround(other.gameObject))
        {
            _contacts++;
            UpdateState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsGround(other.gameObject))
        {
            _contacts = Mathf.Max(0, _contacts - 1);
            UpdateState();
        }
    }

    private bool IsGround(GameObject go)
    {
        return (_groundLayer.value & (1 << go.layer)) != 0;
    }

    private void UpdateState()
    {
        bool newState = _contacts > 0;
        if (newState == _isGrounded) return;
        _isGrounded = newState;
        GroundStateChanged?.Invoke(_isGrounded);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
    }
}
