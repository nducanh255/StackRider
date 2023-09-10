using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class GameOverWindow : MonoBehaviour
{
    private static GameOverWindow instance;
    private Button gameOverButton;
    private TextMeshProUGUI gameOverText;

    private void Awake() {
        instance = this;
        this.gameOverButton = this.transform.Find("Button").GetComponent<Button>();
        this.gameOverText = this.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        Hide();
    }

    public static void RestartGame() {
        SceneManager.LoadScene(0);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public static void StaticShowWin() {
        instance.gameOverText.text = "You Win!!!";
        instance.Show();
    }

    public static void StaticShowLoose() {
        instance.gameOverText.text = "Game Over!";
        instance.Show();
    }
    
}
