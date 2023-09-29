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
    private float timer;
    public TextMeshProUGUI timerText;

    //Pause
    public bool paused;
    public GameObject pauseMenu;

    //Map
    public Grid grid;
    public GameObject[] mapGen, enemies, items;
    public LayerMask tileMapFilter;
    public Tile[] tiles, topTiles, backTiles;

    // Start is called before the first frame update
    void Start()
    {
        playerSelect = PlayerPrefs.GetInt("playerSelect", 1);
        gameSelect = PlayerPrefs.GetInt("gameSelect", 5);
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            timer = 30 * gameSelect;
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
            timer -= Time.deltaTime;
            int min = Mathf.RoundToInt(timer) / 60;
            int sec = Mathf.RoundToInt(timer) % 60;
            timerText.text = min + ":" + (sec < 10 ? "0" + sec : sec);
            if (Input.GetKeyDown(KeyCode.Escape))
            { 
                if (paused)
                {
                    paused = false;
                    pauseMenu.SetActive(false);
                    Time.timeScale = 1;
                }
                else
                {
                    paused = true;
                    pauseMenu.SetActive(true);
                    Time.timeScale = 0;
                }
            }
        }
    }

    public void LoadScene(int num)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            PlayerPrefs.SetInt("playerSelect", playerSelect);
            PlayerPrefs.SetInt("gameSelect", gameSelect);
        }
        Time.timeScale = 1;
        SceneManager.LoadScene(num);
    }

    public void PlayerSelect(int amount)
    {
        if (playerSelect + amount >= 1 && playerSelect + amount <= 4)
            playerSelect += amount;
    }

    public void GameSelect(int amount)
    {
        if (gameSelect + amount >= 5)
            gameSelect += amount;
    }

    public void Generate()
    {
        Debug.Log("Generating Terrain");
        Vector2 location = Vector2.down * 10;
        for (int i = 0;  i < gameSelect; i++)
        {
            GameObject seed = mapGen[i == 0 || i == gameSelect - 1 ? (i == 0 ? 0 : 1) : Random.Range(2, mapGen.Length)];
            GameObject t = Instantiate(seed, location, Quaternion.identity, grid.transform);
            if (i != 0 && i != gameSelect - 1)
            {
                int biome = Random.Range(1, tiles.Length);
                t.GetComponent<Tilemap>().SwapTile(tiles[0], tiles[biome]);
                t.GetComponent<Tilemap>().SwapTile(topTiles[0], topTiles[biome]);
                if (t.GetComponentsInChildren<Tilemap>()[1] != null)
                    t.GetComponentsInChildren<Tilemap>()[1].SwapTile(backTiles[0], backTiles[biome]);
            }
            location += new Vector2(t.GetComponent<Tilemap>().size.x, 0);
        }

        //spawning prep
        float posX = mapGen[0].GetComponent<Tilemap>().size.x;
        float itemposX = posX;
        bool stop = false;

        Debug.Log("Spawning Enemies");
        while (!stop)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Random.Range(posX + gameSelect, posX + gameSelect * 4), 15), Vector2.down, 35, tileMapFilter);
            if (hit)
                Instantiate(enemies[Random.Range(0, enemies.Length)], hit.point + (Vector2.up * 2), Quaternion.identity);
            else
                stop = true;
            posX = hit.point.x + 2;
        }

        stop = false;
        Debug.Log("Spawning Items");
        while (!stop)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Random.Range(itemposX + gameSelect * 2.5f, itemposX + gameSelect * 5.5f), 15), Vector2.down, 35, tileMapFilter);
            if (hit)
                Instantiate(items[Random.Range(0, items.Length)], hit.point + Vector2.up, Quaternion.identity);
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
