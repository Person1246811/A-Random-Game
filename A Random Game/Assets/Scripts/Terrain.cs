using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    public int biome;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //When a car bullet hits the terrain it freezes, changes sprite, then expands
        if (collision.gameObject.tag == "CarBullet")
        {
            collision.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
            collision.gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
            collision.gameObject.GetComponent<CircleCollider2D>().radius = 2.3f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //When the bullets hits the terrain it gets destroyed
        if (collision.gameObject.tag == "PlayerBullet")
            Destroy(collision.gameObject);

        if (collision.gameObject.tag == "EnemyBullet")
            Destroy(collision.gameObject);
    }
}
