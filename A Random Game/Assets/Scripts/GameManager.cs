using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    //player
    private GameObject player;

    //Music
    public AudioClip[] songs;

    //Menu
    public int playerSelect;
    public int gameSelect;
    public TextMeshProUGUI gameText;
    public Image playerImage;
    public Image hand;
    public GameObject[] handPos;
    /*public Image[] stars;
    public Vector3[] starsPos, starsStart;*/
    public float starSpeed;
    public GameObject starsGroup;
    private Vector3 canvasPos;

    //Ui
    public float timer;
    public TextMeshProUGUI timerText;

    public GameObject loseMenu;

    //Pause
    public bool paused;
    public GameObject pauseMenu;

    //Map
    public GameObject enemiesGroup, itemsGroup;
    public Grid grid;
    public GameObject[] mapGen, enemies, items;
    public LayerMask tileMapFilter;
    public Tile[] tiles, topTiles, backTiles, decorTiles;

    // Start is called before the first frame update
    void Start()
    {
        playerSelect = PlayerPrefs.GetInt("playerSelect", 1);
        gameSelect = PlayerPrefs.GetInt("gameSelect", 5);
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            timer = 90 * gameSelect;
            Generate();
        }
        /*else
        {
            starsStart[0] = stars[0].transform.position;
            starsStart[1] = stars[1].transform.position;
            starsPos[0] = stars[0].transform.position.x * Vector3.right;
            starsPos[1] = stars[1].transform.position.x * Vector3.right;
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            gameText.text = "" + (gameSelect - 2);
            playerImage.GetComponent<Animator>().SetInteger("State", playerSelect - 1);
            playerImage.SetNativeSize();
            hand.transform.position = handPos[playerSelect - 1].transform.position;
            //Menu sliding stuff
            canvasPos += Vector3.right * (starSpeed * Time.deltaTime);
            starsGroup.transform.localPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) / 1.2f + canvasPos;
            //MenuSlide(0);
            //MenuSlide(1);
        }
        else
        {
            if (!GetComponents<AudioSource>()[0].isPlaying && Time.timeScale == 1)
            {
                GetComponents<AudioSource>()[0].clip = songs[Random.Range(1, songs.Length)];
                GetComponents<AudioSource>()[0].Play();
            }
            timer -= Time.deltaTime;
            int min = Mathf.RoundToInt(timer) / 60;
            int sec = Mathf.RoundToInt(timer) % 60;
            timerText.text = min + ":" + (sec < 10 ? "0" + sec : sec);
            if (Input.GetKeyDown(KeyCode.Escape) && !player.GetComponent<PlayerController>().endMenu.activeSelf)
            {
                if (!paused && Time.timeScale == 1)
                {
                    paused = true;
                    pauseMenu.SetActive(true);
                    GetComponents<AudioSource>()[1].Play();
                    GetComponents<AudioSource>()[0].Pause();
                    GetComponents<AudioSource>()[2].Stop();
                    Time.timeScale = 0;
                }
                else
                {
                    paused = false;
                    pauseMenu.SetActive(false);
                    GetComponents<AudioSource>()[1].Play();
                    Time.timeScale = 1;
                }
            }

            if (timer < 0)
            {
                loseMenu.SetActive(true);
                GetComponents<AudioSource>()[0].Stop();
                GetComponents<AudioSource>()[2].Stop();
                Time.timeScale = 0;
            }

            if (timer <= 12 && !GetComponents<AudioSource>()[2].isPlaying && Time.timeScale == 1)
                GetComponents<AudioSource>()[2].Play();
        }
    }

    public void LoadScene(int num)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            PlayerPrefs.SetInt("playerSelect", playerSelect);
            PlayerPrefs.SetInt("gameSelect", gameSelect);
            GetComponents<AudioSource>()[2].Play();
        }
        Time.timeScale = 1;
        SceneManager.LoadScene(num);
    }

    public void PlayerSelect(int amount)
    {
        if (playerSelect + amount >= 1 && playerSelect + amount <= 4)
            playerSelect += amount;
        GetComponents<AudioSource>()[1].Play();
    }

    public void GameSelect(int amount)
    {
        if (gameSelect + amount >= 5 && gameSelect + amount <= 75)
            gameSelect += amount;
        GetComponents<AudioSource>()[1].Play();
    }

    public void Generate()
    {
        Debug.Log("Generating Terrain");
        Vector2 location = Vector2.down * 10;
        for (int i = 0; i < gameSelect; i++)
        {
            GameObject seed = mapGen[i == 0 || i == gameSelect - 1 ? (i == 0 ? 0 : 1) : Random.Range(2, mapGen.Length)];
            GameObject t = Instantiate(seed, location, Quaternion.identity, grid.transform);
            Tilemap map = t.GetComponent<Tilemap>();
            int biome = 0;
            if (i != 0 && i != gameSelect - 1)
                biome = Random.Range(1, tiles.Length);
            map.SwapTile(tiles[0], tiles[biome]);
            map.SwapTile(topTiles[0], topTiles[biome]);
            if (t.GetComponentsInChildren<Tilemap>().Length > 1)
            {
                t.GetComponentsInChildren<Tilemap>()[1].SwapTile(backTiles[0], backTiles[biome]);
                t.GetComponentsInChildren<Tilemap>()[1].SwapTile(decorTiles[0], decorTiles[biome]);
            }
            t.GetComponent<Terrain>().biome = biome;
            location += new Vector2(map.size.x - 2, 0);
        }

        //spawning prep
        float posX = mapGen[0].GetComponent<Tilemap>().size.x;
        float itemposX = posX;
        bool stop = false;

        Debug.Log("Spawning Enemies");
        while (!stop)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Random.Range(posX + gameSelect * .5f, posX + gameSelect * 3), 25), Vector2.down, 40, tileMapFilter);
            if (hit)
                Instantiate(enemies[Random.Range(0, enemies.Length)], hit.point + (Vector2.up * 2), Quaternion.identity, enemiesGroup.transform);
            else
                stop = true;
            posX = hit.point.x + 2;
        }

        stop = false;
        Debug.Log("Spawning Items");
        while (!stop)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Random.Range(itemposX + gameSelect * 1.5f, itemposX + gameSelect * 3.8f), 25), Vector2.down, 40, tileMapFilter);
            if (hit)
                Instantiate(items[Random.Range(0, items.Length)], hit.point + Vector2.up, Quaternion.identity, itemsGroup.transform);
            else
                stop = true;
            itemposX = hit.point.x + 3;
        }
    }

    /*void MenuSlide(int num)
    {
        starsPos[num] += Vector3.right * (starSpeed * Time.deltaTime);
        stars[num].transform.localPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) / 1.2f + starsPos[num];
        if (stars[num].transform.position.x >= starsStart[1].x * -2.5f)
        {
            stars[num].transform.position = starsStart[1] * 2.5f;
            starsPos[num] = starsStart[1].x * Vector3.right * 2.5f;
        }
    }*/
}
