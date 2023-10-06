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
    private float maxhp = 2;
    public float hp;
    public GameObject healthGreen;
    public GameObject healthRed;
    public float death;
    public int type;

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
        hp = maxhp;
    }

    void Update()
    {
        //Declares a temp velocity
        Vector2 velocity = RB.velocity;

        //Detects if the player is in range
        if (transform.position.x - player.transform.position.x <= range)
        {
            //flips the sprite
            if (transform.position.x - player.transform.position.x > 0)
                GetComponent<SpriteRenderer>().flipX = true;
            else
                GetComponent<SpriteRenderer>().flipX = false;

            //Makes the enemy move towards the player
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.transform.position.x, type == 0 ? player.transform.position.y : transform.position.y), speed * Time.deltaTime);

            GetComponent<Animator>().SetBool("Walking", true);
            if (!GetComponents<AudioSource>()[0].isPlaying)
                GetComponents<AudioSource>()[0].Play();

            //if the enemy hits the ground and a wall it jumps
            if (Physics2D.Raycast(transform.position, Vector2.down, detectRangeDown, envlayer) && type != 0 &&
               ((Physics2D.Raycast(transform.position, Vector2.left, detectRange, envlayer) || (Physics2D.Raycast(transform.position, Vector2.right, detectRange, envlayer)))))
            {
                velocity.y = jumpPower;
                GetComponents<AudioSource>()[4].Play();
            }
        }
        else
        {
            GetComponent<Animator>().SetBool("Walking", false);
            GetComponents<AudioSource>()[0].Stop();
        }

        //Finds the angle needed to shoot at the player
        Vector2 distance = new Vector2(transform.position.y - player.transform.position.y, transform.position.x - player.transform.position.x);
        angle = (Mathf.Atan2(distance.x, distance.y) * Mathf.Rad2Deg) + 180;

        //If the player is in range and the enemy can shoot it shoots
        RaycastHit2D sight = Physics2D.Raycast(transform.position, player.transform.position - transform.position, bulletRange, (envlayer | playerLayer));
        if (sight.collider != null && sight.collider.tag == "Player" && canShoot)
        {
            GameObject b = Instantiate(bullet, transform.position + (Vector3.up * .4f), Quaternion.Euler(0, 0, angle));
            b.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.right * bulletSpeed);
            GetComponents<AudioSource>()[1].Play();
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

        //hp bar
        if (hp == maxhp)
        {
            healthGreen.SetActive(false);
            healthRed.SetActive(false);
        }
        else
        {
            Vector3 redTransform = healthRed.transform.localScale;
            healthGreen.transform.localScale = new Vector3((hp / maxhp) * redTransform.x, redTransform.y, redTransform.z);
            healthGreen.SetActive(true);
            healthRed.SetActive(true);
        }

        //If the enemy falls below the death number it dies and adds +1 score to the player
        if (transform.position.y <= death || hp <= 0)
        {
            player.GetComponent<PlayerController>().score++;
            Destroy(gameObject);
        }

        //Makes the velocity the temp velocity
        RB.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //When the enemy collides with a player bullet it takes damage and destroys the bullet
        if (collision.gameObject.tag == "PlayerBullet")
        {
            Destroy(collision.gameObject);
            GetComponents<AudioSource>()[2].Play();
            hp--;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //takes damage when the enemy touches the rolling pin
        if (collision.gameObject.tag == "PlayerBullet")
        {
            GetComponents<AudioSource>()[2].Play();
            hp = 0;
        }
        //When a car bullet hits the enemy it freezes, changes sprite, then expands, and also hurts the enemy
        if (collision.gameObject.tag == "CarBullet")
        {
            collision.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
            collision.gameObject.GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
            collision.gameObject.GetComponent<CircleCollider2D>().radius = 2.3f;
            GetComponents<AudioSource>()[2].Play();
            GetComponents<AudioSource>()[3].Play();
            hp--;
        }
        //kills the enemy when it touches the orbital laser
        if (collision.gameObject.tag == "Orbital")
        {
            GetComponents<AudioSource>()[2].Play();
            hp = 0;
        }
    }
}
