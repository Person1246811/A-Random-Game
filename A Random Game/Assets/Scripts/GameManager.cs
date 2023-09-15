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
    public Tile[] tiles;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            gameText.text = "" + (gameSelect - 2);
            playerImage.GetComponent<Animator>().SetInteger("State", playerSelect + 1);
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
        Vector2 location = Vector2.zero;
        for (int i = 0;  i < gameSelect; i++)
        {
            GameObject t = Instantiate(mapGen[i == 0 || i == gameSelect - 1 ? (i == 0 ? 0 : 1) : Random.Range(2, mapGen.Length)], location, Quaternion.identity, grid.transform);
            if (i != 0 && i != gameSelect - 1)
                t.GetComponent<Tilemap>().SwapTile(tiles[0], tiles[Random.Range(1, tiles.Length)]);
            location += new Vector2(t.GetComponent<Tilemap>().size.x, 0);
        }

        Debug.Log("Spawning Enemies");
        float posX = 10;
        for (int i = 0; i < gameSelect * 2; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Random.Range(posX + 1, posX + 15), 12), Vector2.down, 22, tileMapFilter);
            if (hit)
                Instantiate(enemies[Random.Range(0, enemies.Length)], hit.point + (Vector2.up * 3), Quaternion.identity);
            posX = hit.point.x + 2;
        }

        Debug.Log("Spawning Items");
        float itemposX = 10;
        for (int i = 0; i < gameSelect; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Random.Range(itemposX + 1, itemposX + 15), 12), Vector2.down, 22, tileMapFilter);
            if (hit)
                Instantiate(items[Random.Range(0, items.Length)], hit.point + Vector2.up, Quaternion.identity);
            itemposX = hit.point.x + gameSelect * 1.7f;
        }
    }
}
