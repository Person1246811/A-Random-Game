using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    //Basic Variables
    private Rigidbody2D RB;
    private GameObject player;
    public float range;
    public float speed;
    public float death;

    //Jump variables
    public LayerMask envlayer;
    public float detectRange;
    public float detectRangeDown;
    public float jumpPower;

    //Bullet Variables
    public LayerMask playerLayer;
    public GameObject bullet;
    private float angle;
    public float bulletRange;
    public float bulletSpeed;
    public float bulletLifespan;

    //Bullet counters and canshoot
    private bool canShoot = true;
    private float fireCountdown = 0;
    public float fireRate;

    void Start()
    {
        //Assigns the enemies rigidbody and the players position
        RB = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        //Declares a temp velocity
        Vector2 velocity = RB.velocity;

        //Detects if the player is in range
        if (transform.position.x - player.transform.position.x <= range)
        {
            //Makes the enemy move towards the player
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.transform.position.x, transform.position.y), speed * Time.deltaTime);

            //if the enemy hits the ground and a wall it jumps
            if (Physics2D.Raycast(transform.position, Vector2.down, detectRangeDown, envlayer) &&
               ((Physics2D.Raycast(transform.position, Vector2.left, detectRange, envlayer) || (Physics2D.Raycast(transform.position, Vector2.right, detectRange, envlayer)))))
                velocity.y = jumpPower;
        }

        //Finds the angle needed to shoot at the player
        Vector2 distance = new Vector2(transform.position.y - player.transform.position.y, transform.position.x - player.transform.position.x);
        angle = (Mathf.Atan2(distance.x, distance.y) * Mathf.Rad2Deg) + 180;

        //If the player is in range and the enemy can shoot it shoots
        RaycastHit2D sight = Physics2D.Raycast(transform.position, player.transform.position - transform.position, bulletRange, (envlayer | playerLayer));
        if (sight.collider != null && sight.collider.tag == "Player" && canShoot)
        {
            GameObject b = Instantiate(bullet, transform.position, Quaternion.Euler(0, 0, angle));
            b.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.right * bulletSpeed);
            canShoot = false;
            Destroy(b, bulletLifespan);
        }

        //Countdown for the fire rate
        else if (!canShoot)
        {
            fireCountdown += Time.deltaTime;
            if (fireCountdown >= fireRate)
            {
                fireCountdown = 0;
                canShoot = true;
            }
        }

        //If the enemy falls below the death number it dies
        if (transform.position.y <= death)
            Destroy(gameObject);

        //Makes the velocity the temp velocity
        RB.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //When the enemy collides with a player bullet it dies and destroys the bullet
        if (collision.gameObject.tag == "PlayerBullet")
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
