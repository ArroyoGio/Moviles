using UnityEngine;
using TMPro;
using Unity.Services.Economy;
using System.Threading.Tasks;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    [Header("UI")]
    public TMP_Text fichasText;
    public TMP_Text fichasDoradasText;

    void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        await LoadBalances();
    }

    public async Task LoadBalances()
    {
        try
        {
            var balances = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();

            foreach (var b in balances.Balances)
            {
                if (b.CurrencyId == "FICHAS" && fichasText != null)
                    fichasText.text = "Fichas: " + b.Balance;

                if (b.CurrencyId == "FICHAS_DORADAS" && fichasDoradasText != null)
                    fichasDoradasText.text = "Doradas: " + b.Balance;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error cargando monedas: " + e.Message);
        }
    }

    public async Task MakeVirtualPurchase(string purchaseId)
    {
        try
        {
            await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync(purchaseId);
            Debug.Log("Compra exitosa: " + purchaseId);
            await LoadBalances();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error en compra: " + e.Message);
            throw;
        }
    }

    public async Task AddFichas(int amount)
    {
        try
        {
            await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync("FICHAS", amount);
            await LoadBalances();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error añadiendo fichas: " + e.Message);
            throw;
        }
    }
}