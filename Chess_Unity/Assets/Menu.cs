using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
   public void PlayGame()
   {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
   }

   public void QuitGame()
   {
        Application.Quit();
   }

    public static int netMode;
    private Text myText;
    void Start()
    {
       myText = GetComponent<Text>();
    }

    void Update()
    {

        netMode = int.Parse(myText.text);
    }

}
