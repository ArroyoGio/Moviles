using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;

public class StoreUI : MonoBehaviour
{
    [Header("Botones de compra")]
    public Button buyPunoSportButton;
    public Button buyVendaProButton;
    public Button buyGaseosaButton;

    [Header("Botón prueba")]
    public Button darFichasButton;

    [Header("Textos")]
    public TMP_Text fichasText;
    public TMP_Text doradasText;
    public TMP_Text resultText;

    void Start()
    {
        ConfigurarBoton(buyPunoSportButton, () => Comprar("COMPRAR_PUNO_SPORT", buyPunoSportButton, "Puño Sport"));
        ConfigurarBoton(buyVendaProButton, () => Comprar("COMPRAR_VENDA_PRO", buyVendaProButton, "Venda Pro"));
        ConfigurarBoton(buyGaseosaButton, () => Comprar("COMPRAR_GASEOSA", buyGaseosaButton, "Gaseosa"));

        if (darFichasButton != null)
        {
            darFichasButton.onClick.RemoveAllListeners();
            darFichasButton.onClick.AddListener(DarFichasPrueba);
        }

        StartCoroutine(ActualizarMonedasRoutine());
    }

    void ConfigurarBoton(Button boton, UnityEngine.Events.UnityAction accion)
    {
        if (boton == null) return;
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(accion);
    }

    void Comprar(string purchaseId, Button boton, string nombreItem)
    {
        boton.interactable = false;
        StartCoroutine(BuyRoutine(purchaseId, boton, nombreItem));
    }

    IEnumerator BuyRoutine(string purchaseId, Button boton, string nombreItem)
    {
        if (resultText != null)
            resultText.text = "Comprando " + nombreItem + "...";

        var task = EconomyManager.Instance.MakeVirtualPurchase(purchaseId);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            if (resultText != null)
                resultText.text = "No tienes fichas suficientes.";
        }
        else
        {
            if (resultText != null)
                resultText.text = "¡Compraste " + nombreItem + "!";
        }

        yield return ActualizarMonedasRoutine();

        boton.interactable = true;
    }

    public void DarFichasPrueba()
    {
        if (darFichasButton != null)
            darFichasButton.interactable = false;

        StartCoroutine(DarFichasRoutine());
    }

    IEnumerator DarFichasRoutine()
    {
        var task = EconomyManager.Instance.AddFichas(500);
        yield return new WaitUntil(() => task.IsCompleted);

        if (resultText != null)
            resultText.text = "¡500 fichas añadidas!";

        yield return ActualizarMonedasRoutine();

        if (darFichasButton != null)
            darFichasButton.interactable = true;
    }

    IEnumerator ActualizarMonedasRoutine()
    {
        var task = EconomyService.Instance.PlayerBalances.GetBalancesAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("Error cargando monedas: " + task.Exception.Message);
            yield break;
        }

        foreach (PlayerBalance balance in task.Result.Balances)
        {
            if (balance.CurrencyId == "FICHAS" && fichasText != null)
                fichasText.text = "Fichas: " + balance.Balance;

            if (balance.CurrencyId == "FICHAS_DORADAS" && doradasText != null)
                doradasText.text = "Doradas: " + balance.Balance;
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainHub");
    }
}