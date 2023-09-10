using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreWindow : MonoBehaviour
{
    private static ScoreWindow instance;
    private TextMeshProUGUI scoreText;

    private void Awake() {
        instance = this;
        this.scoreText = this.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
    }

    public static void UpdateCoin(int coin) {
        instance.scoreText.text = $"{coin}";
    }
    
}
