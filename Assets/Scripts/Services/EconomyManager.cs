using UnityEngine;
using TMPro;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("UI")]
    public TMP_Text fichasText;
    public TMP_Text fichasDoradasText;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // Llama esto después del login
    public async Task LoadBalances()
    {
        var balances = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
        foreach (var b in balances.Balances)
        {
            if (b.CurrencyId == "FICHAS" && fichasText != null)
                fichasText.text = $"Fichas: {b.Balance}";
            if (b.CurrencyId == "FICHAS_DORADAS" && fichasDoradasText != null)
                fichasDoradasText.text = $"Fichas doradas: {b.Balance}";
        }
    }

    // Compra virtual
    public async Task MakeVirtualPurchase(string purchaseId)
    {
        try
        {
            var result = await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync(purchaseId);
            Debug.Log($"Compra exitosa: {purchaseId}");
            await LoadBalances(); // refresca los balances
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error en compra: {e.Message}");
        }
    }

    // Dar fichas al jugador (para pruebas)
    public async Task AddFichas(int amount)
    {
        try
        {
            await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync("FICHAS", amount);
            Debug.Log($"Fichas añadidas: {amount}");
            await LoadBalances();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error añadiendo fichas: {e.Message}");
        }
    }
}