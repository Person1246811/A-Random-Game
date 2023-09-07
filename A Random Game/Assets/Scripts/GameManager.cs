using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int playerSelect;
    public int gameSelect;
    public TextMeshProUGUI gameText;
    public int[] mapGen;
    public GameObject[] mapSeeds;

    // Start is called before the first frame update
    void Start()
    {
        playerSelect = PlayerPrefs.GetInt("playerSelect", 0);
        gameSelect = PlayerPrefs.GetInt("gameSelect", 0);
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
        }
    }

    public void LoadScene(int num)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            PlayerPrefs.SetInt("playerSelect", playerSelect);
            PlayerPrefs.SetInt("gameSelect", gameSelect);
        }
        SceneManager.LoadScene(num);
    }

    public void PlayerSelect(int amount)
    {
        playerSelect += amount;
    }

    public void GameSelect(int amount)
    {
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
