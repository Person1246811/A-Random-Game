using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //End menu
    public GameObject endMenu;

    //Basic variables
    private Animator anim;
    private Rigidbody2D myRB;
    public float groundDetectDistance = .1f;
    public float jumpHeight;
    public float speed;
    private float maxhp = 3;
    public float hp;
    public GameObject health;
    public float death;
    private float hurtTimer;
    private float regenTimer;
    public int score;

    //Player Select Variables
    public int playerSelect;

    //Power ups
    private float powerTimer;
    public bool speedBoost;
    public bool canFly;
    public GameObject wings;
    public bool canGrapple;

    //Variable for mouse angle
    private float angle = 0;

    //Grapple Variables
    public Camera mainCamera;
    private LineRenderer line;
    private SpringJoint2D joint;
    public GameObject grapplePos;
    public LayerMask tileMapFilter;
    public float jointSpeed;
    public float minDistance;
    private Vector3 direction;

    //Bullet Variables
    public float bulletSpeed;
    public float bulletLifespan;
    private bool bulletFlip;

    //Bullet counters and canshoot
    private bool canShoot = true;
    private float fireCountdown = 0;
    public float fireRate;

    //bullets
    public GameObject bullet;
    public GameObject car;
    public GameObject orbital;
    public GameObject rollingPin;
    public GameObject weaponRotation;
    public SpriteRenderer weaponSprite;
    public Sprite[] weaponSprites;
    public Vector2[] handPos;

    //rolling pin active timer
    private float pinTimer;

    void Start()
    {
        anim = GetComponent<Animator>();
        myRB = GetComponent<Rigidbody2D>();
        playerSelect = PlayerPrefs.GetInt("playerSelect", 1);
        anim.SetInteger("Player", playerSelect - 1);
        line = GetComponent<LineRenderer>();
        joint = GetComponent<SpringJoint2D>();
        hp = maxhp;
        weaponRotation.transform.localPosition = handPos[playerSelect - 1];
        weaponSprite.sprite = weaponSprites[playerSelect - 1];
        
        //Sets the collider of the player to match the size of the image
        GetComponent<BoxCollider2D>().size = GetComponent<SpriteRenderer>().sprite.bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale != 0)
        {
            //Basic controls
            Vector2 groundDetection = new Vector2(transform.position.x, transform.position.y - (GetComponent<SpriteRenderer>().sprite.bounds.size.y / 2) - .1f);
            Vector2 velocity = myRB.velocity;
            float moveInputX = (speedBoost ? speed * 1.5f : speed) * Input.GetAxisRaw("Horizontal");
            Debug.DrawRay(groundDetection, Vector2.down);
            if (Physics2D.Raycast(groundDetection, Vector2.down, groundDetectDistance) || canFly)
            {
                velocity.x = moveInputX;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    velocity.y = jumpHeight;
                    anim.SetTrigger("Jump");
                    GetComponents<AudioSource>()[1].Play();
                }
            }
            else
            {
                float moveDirection = Input.GetAxisRaw("Horizontal") * myRB.velocity.x;
                if (moveDirection != Mathf.Abs(moveDirection) || Mathf.Abs(velocity.x) < speed)
                    velocity.x += moveInputX * 2 * Time.deltaTime;
            }
            myRB.velocity = velocity;
            if (Mathf.Abs(myRB.velocity.x) >= .2f)
            {
                anim.SetBool("Walking", true);
                if (!GetComponents<AudioSource>()[0].isPlaying)
                    GetComponents<AudioSource>()[0].Play();
            }
            else
            {
                anim.SetBool("Walking", false);
                GetComponents<AudioSource>()[0].Stop();
            }
            Debug.DrawRay(groundDetection, Vector2.down);

            //adds the wings to the player
            if (canFly)
                wings.SetActive(true);
            else
                wings.SetActive(false);

            //mousePos on screen
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 distance = transform.position - mousePos;
            //rotation towards mouse
            angle = (Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg) + 180;

            //Rotates player and children
            weaponRotation.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (mousePos.x > transform.position.x)
            {
                transform.rotation = new Quaternion(0, 0, 0, 0);
                weaponSprite.transform.localRotation = new Quaternion(0, 0, 0, 0);
                bulletFlip = false;
            }
            else
            {
                transform.rotation = new Quaternion(0, 180, 0, 0);
                weaponSprite.transform.localRotation = new Quaternion(180, 0, 0, 0);
                bulletFlip = true;
            }

            //If the player clicks it spawns the bullet and makes it go towards the mouse until it runs out of bullet lifespan time
            if (Input.GetKey(KeyCode.Mouse0) && canShoot)
            {
                //Banana and car
                if (playerSelect == 1 || playerSelect == 2)
                {
                    GameObject b = Instantiate(playerSelect == 1 ? bullet : car, weaponSprite.transform.position, Quaternion.Euler(0, 0, angle));
                    b.GetComponent<SpriteRenderer>().flipY = bulletFlip;
                    b.GetComponent<Rigidbody2D>().AddRelativeForce(Vector2.right * (playerSelect == 1 ? bulletSpeed : bulletSpeed * .8f));
                    b.GetComponent<Rigidbody2D>().gravityScale = playerSelect == 1 ? .5f : 1.2f;
                    canShoot = false;
                    Destroy(b, playerSelect == 1 ? bulletLifespan : bulletLifespan * 1.8f);
                }
                //Orbital
                if (playerSelect == 3)
                {
                    RaycastHit2D hit = Physics2D.Raycast(new Vector3(mousePos.x, 12, mousePos.z), Vector2.down, 22, tileMapFilter);
                    if (hit)
                    {
                        Vector2 strikePoint = new Vector2(mousePos.x, hit.point.y);
                        GameObject b = Instantiate(orbital, new Vector2(0, 5) + strikePoint, Quaternion.identity);
                        b.GetComponent<LineRenderer>().SetPosition(0, new Vector2(0, 10) + strikePoint);
                        b.GetComponent<LineRenderer>().SetPosition(1, strikePoint);
                        canShoot = false;
                        Destroy(b, bulletLifespan / 2);
                    }
                }
                //Rolling pin
                if (playerSelect == 4)
                {
                    rollingPin.SetActive(true);
                    pinTimer = fireRate * .5f;
                    canShoot = false;
                }
            }

            //The countdown until the player can shoot again
            else if (!canShoot)
            {
                fireCountdown += Time.deltaTime;
                if (fireCountdown >= fireRate)
                {
                    fireCountdown = 0;
                    canShoot = true;
                    rollingPin.SetActive(false);
                }
            }

            if (rollingPin.activeSelf)
            {
                if (pinTimer >= 0)
                    pinTimer -= Time.deltaTime;
                else
                    rollingPin.SetActive(false);
            }

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
                    weaponSprite.sprite = weaponSprites[4];
                }
            }
            //Detects if the player releases right click and disables the line renderer and spring joint
            if (Input.GetMouseButtonUp(1))
            {
                line.enabled = false;
                joint.enabled = false;
                weaponSprite.sprite = weaponSprites[playerSelect - 1];
            }

            if (powerTimer >= 0)
                powerTimer -= Time.deltaTime;
            else
            {
                canFly = false;
                canGrapple = false;
                speedBoost = false;
            }

            //hp bar
            if (hp == maxhp)
                health.SetActive(false);
            else
            {
                health.GetComponent<Slider>().value = hp;
                health.GetComponent<Slider>().maxValue = maxhp;
                health.SetActive(true);
                if (regenTimer >= 0)
                    regenTimer -= Time.deltaTime;
                else
                {
                    regenTimer = 12.5f;
                    hp++;
                }
            }

            if (hurtTimer >= 0)
                hurtTimer -= Time.deltaTime;

            //If the enemy falls below the death number it dies
            if (transform.position.y <= death)
                hp = 0;

            //When the player has less than or equal to 0 hp, the player gets disabled
            if (hp <= 0)
            {
                transform.position = Vector2.right * 19;
                hp = 3;
            }
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && hurtTimer <= 0)
        {
            hp--;
            hurtTimer = .5f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "EndPortal")
        {
            Time.timeScale = 0;
            endMenu.SetActive(true);
        }

        if (!canFly && !canGrapple && !speedBoost)
        { 
            if (collision.gameObject.tag == "WingsItem")
            {
                Destroy(collision.gameObject);
                canFly = true;
                powerTimer = 8;
            }
            if (collision.gameObject.tag == "GrappleItem")
            {
                Destroy(collision.gameObject);
                canGrapple = true;
                powerTimer = 8;
            }
            if (collision.gameObject.tag == "SpeedItem")
            {
                Destroy(collision.gameObject);
                speedBoost = true;
                powerTimer = 8;
            } 
        }
    }
}
