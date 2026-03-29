using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCEnter : MonoBehaviour
{
    public Transform Exit;
    public WaitForSeconds WaitForSeconds;
    public float WaitTime = 0.5f;
    private Collider2D _collider2D;
    private Rigidbody2D _rigidbody2D;

    void Awake()
    {
        Enit();
    }

    void Enit()
    {
        _collider2D = GetComponent<Collider2D>();
        _rigidbody2D =  GetComponent<Rigidbody2D>();
        WaitForSeconds = new WaitForSeconds(WaitTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enter"))
        {
            StartCoroutine(WaitForTime());
        }
    }

    public IEnumerator WaitForTime()
    {
        yield return WaitForSeconds;
        transform.position = Exit.position;
        yield break;
    }
    
}
