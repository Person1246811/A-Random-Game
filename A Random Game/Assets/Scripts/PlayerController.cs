using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Basic variables
    private Rigidbody2D myRB;
    public float groundDetectDistance = .1f;
    public float jumpHeight;
    public float speed;
    public float hp;
    public float death;

    public bool speedBoost;
    public bool canFly;

    //Variable for mouse angle
    private float angle = 0;

    //Grapple Variables
    public bool canGrapple;
    public Camera mainCamera;
    public LineRenderer line;
    public SpringJoint2D joint;
    public GameObject grapplePos;
    public LayerMask tileMapFilter;
    public float jointSpeed;
    public float minDistance;
    private Vector3 direction;

    //Bullet Variables
    public GameObject bullet;
    public float bulletSpeed;
    public float bulletLifespan;

    //Bullet counters and canshoot
    private bool canShoot = true;
    private float fireCountdown = 0;
    public float fireRate;

    void Start()
    {
        //Assigns myRB to the players rigidbody
        myRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Basic controls
        Vector2 groundDetection = new Vector2(transform.position.x, transform.position.y - .51f);
        Vector2 velocity = myRB.velocity;
        float moveInputX = (speedBoost ? speed * 2 : speed) * Input.GetAxisRaw("Horizontal");
        if (Physics2D.Raycast(groundDetection, Vector2.down, groundDetectDistance) || canFly)
        {
            velocity.x = moveInputX;
            if (Input.GetKeyDown(KeyCode.Space))
                velocity.y = jumpHeight;
        }
        else
        {
            float moveDirection = Input.GetAxisRaw("Horizontal") * myRB.velocity.x;
            if (moveDirection != Mathf.Abs(moveDirection))
                velocity.x += moveInputX * 2 * Time.deltaTime;
        }
        myRB.velocity = velocity;
        Debug.DrawRay(groundDetection, Vector2.down);
        if (myRB.velocity.x > .1)
            transform.rotation = new Quaternion(0, 0, 0, 0);
        else if (myRB.velocity.x < -.1)
            transform.rotation = new Quaternion(0, 180, 0, 0);

        //mousePos on screen
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 distance = new Vector2(transform.position.y - mousePos.y, transform.position.x - mousePos.x);
        //rotation towards mouse
        angle = (Mathf.Atan2(distance.x, distance.y) * Mathf.Rad2Deg) + 180;

        //Finds the direction in which the raycast needs to go
        direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

        //Sets the position of the line to the correct position
        line.SetPosition(0, grapplePos.transform.position);
        //Detects if the grapple needs to pull, then it pulls
        if (joint.enabled && joint.distance > minDistance)
            joint.distance -= jointSpeed;
        //Gets the right click input then sends a hit raycast, puts the grapple point to the hit point, and activating the line and spring joint, making the grapple
        if (Input.GetMouseButtonDown(1) && canGrapple)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10, tileMapFilter);
            if (hit)
            {
                line.enabled = true;
                joint.enabled = true;
                line.SetPosition(1, hit.point);
                joint.connectedAnchor = hit.point;
            }
        }
        //Detects if the player releases right click and disables the line renderer and spring joint
        if (Input.GetMouseButtonUp(1))
        {
            line.enabled = false;
            joint.enabled = false;
        }

        //If the player clicks it spawns the bullet and makes it go towards the mouse until it runs out of bullet lifespan time
        if (Input.GetKey(KeyCode.Mouse0) && canShoot)
        {
            GameObject b = Instantiate(bullet, transform.position, Quaternion.Euler(0, 0, angle));
            b.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.right * bulletSpeed);
            canShoot = false;
            Destroy(b, bulletLifespan);
        }

        //The countdown until the player can shoot again
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
            hp = 0;

        //When the player has less than or equal to 0 hp, the player gets disabled
        if (hp <= 0)
        {
            transform.position = Vector2.zero;
            hp = 3;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //When the player hits a enemy bullet it subtracts 1 hp and destroys the bullet
        if (collision.gameObject.tag == "EnemyBullet")
        {
            Destroy(collision.gameObject);
            hp--;
        }
    }
}
