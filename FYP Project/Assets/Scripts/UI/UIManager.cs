using TMPro;
using UnityEngine;

// Handles updating on-screen UI elements
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI baseHealthText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI buildModeText;
    public TextMeshProUGUI buildHintText;
    public GameObject gameOverText;

    private void Awake()
    {
        Instance = this;

        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
        }

        if (buildHintText != null)
        {
            buildHintText.text = "1 = Wall    2 = Turret    3 = Trap    4 = Anti-Air";
        }
    }

    public void UpdateBaseHealth(int currentHealth)
    {
        if (baseHealthText != null)
        {
            baseHealthText.text = "Base HP: " + currentHealth;
        }
    }

    public void UpdateWave(int currentWave)
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + currentWave;
        }
    }

    public void UpdateMoney(int currentMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = "Money: " + currentMoney;
        }
    }

    public void UpdateBuildMode(string modeName)
    {
        if (buildModeText != null)
        {
            buildModeText.text = "Build Mode: " + modeName;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }
    }
}