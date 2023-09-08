using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    //Menu
    public int playerSelect;
    public int gameSelect;
    public TextMeshProUGUI gameText;
    public Image playerImage;

    //Pause
    public bool paused;
    public GameObject pauseMenu;

    //Map
    public int[] mapGen;
    public GameObject[] mapSeeds;

    // Start is called before the first frame update
    void Start()
    {
        playerSelect = PlayerPrefs.GetInt("playerSelect", 1);
        gameSelect = PlayerPrefs.GetInt("gameSelect", 3);
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            Generate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            gameText.text = "" + gameSelect;
            if (playerSelect == 1)
                playerImage.color = Color.black;
            else if (playerSelect == 2)
                playerImage.color = Color.red;
            else if (playerSelect == 3)
                playerImage.color = Color.blue;
            else if (playerSelect == 4)
                playerImage.color = Color.white;
        }
        else
        {
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
        if (gameSelect + amount >= 3)
            gameSelect += amount;
    }

    public void Generate()
    {
        Debug.Log("Generating Seeds");
        mapGen = new int[gameSelect];
        for (int i = 0; i < gameSelect; i++)
            mapGen[i] = Random.Range(0, 6);
        Debug.Log("Generating Terrain");
        Vector2 location = Vector2.zero;
        for (int i = 0;  i < gameSelect; i++)
        {
            Instantiate(mapSeeds[mapGen[i]], location, Quaternion.identity);
            location += new Vector2(10, 0);
        }
    }
}
