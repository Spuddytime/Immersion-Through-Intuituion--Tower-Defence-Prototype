using UnityEngine;

// Handles player money for building and rewards
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    public int startingMoney = 50; //chunp change for now

    private int currentMoney;

    void Awake()
    {
        Instance = this;
        currentMoney = startingMoney;
    }

    void Start()
    {
        UpdateMoneyUI();
    }

    public int GetMoney()
    {
        return currentMoney;
    }

    public bool CanAfford(int amount)
    {
        return currentMoney >= amount;
    }

    public bool SpendMoney(int amount)
    {
        if (!CanAfford(amount))
            return false;

        currentMoney -= amount;
        UpdateMoneyUI();
        return true;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoney(currentMoney);
        }
    }
}