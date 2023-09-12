using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "CarBullet")
        {
            collision.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
            collision.gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
            collision.gameObject.GetComponent<CircleCollider2D>().radius = 2.3f;
        }
    }
}
