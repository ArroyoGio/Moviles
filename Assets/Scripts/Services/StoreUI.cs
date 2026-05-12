using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreUI : MonoBehaviour
{
    [Header("Botones de compra")]
    public Button buyPunoSportButton;
    public Button darFichasButton;
    public TMP_Text resultText;

    void Start()
    {
        // RemoveAllListeners evita que se registre dos veces
        buyPunoSportButton.onClick.RemoveAllListeners();
        buyPunoSportButton.onClick.AddListener(OnBuyPunoSport);

        if (darFichasButton != null)
        {
            darFichasButton.onClick.RemoveAllListeners();
            darFichasButton.onClick.AddListener(DarFichasPrueba);
        }
    }

    public void OnBuyPunoSport()
    {
        // Desactiva el botón para evitar doble click
        buyPunoSportButton.interactable = false;
        StartCoroutine(BuyRoutine());
    }

    System.Collections.IEnumerator BuyRoutine()
    {
        if (resultText != null)
            resultText.text = "Comprando...";

        var task = EconomyManager.Instance.MakeVirtualPurchase("COMPRAR_PUNO_SPORT");
        yield return new UnityEngine.WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            if (resultText != null)
                resultText.text = "Error en compra";
            Debug.LogError(task.Exception.Message);
        }
        else
        {
            if (resultText != null)
                resultText.text = "¡Compra exitosa!";
        }

        // Reactiva el botón al terminar
        buyPunoSportButton.interactable = true;
    }

    public void DarFichasPrueba()
    {
        if (darFichasButton != null)
            darFichasButton.interactable = false;
        StartCoroutine(DarFichasRoutine());
    }

    System.Collections.IEnumerator DarFichasRoutine()
    {
        var task = EconomyManager.Instance.AddFichas(500);
        yield return new UnityEngine.WaitUntil(() => task.IsCompleted);
        if (resultText != null)
            resultText.text = "¡500 fichas añadidas!";
        if (darFichasButton != null)
            darFichasButton.interactable = true;
    }

    public void GoBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterInventory");
    }
}