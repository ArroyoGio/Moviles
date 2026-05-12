using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class AuthenticationManager : MonoBehaviour
{
    [Header("UI Login")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public TMP_Text statusText;

    async void Start()
    {
        // Inicializa Unity Services
        await UnityServices.InitializeAsync();
        loginButton.onClick.AddListener(Login);
        registerButton.onClick.AddListener(Register);
    }

    async void Login()
    {
        try
        {
            statusText.text = "Iniciando sesión...";

            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(
                usernameInput.text, passwordInput.text);

            statusText.text = "¡Bienvenida!";

            // Solo carga balances si EconomyManager existe
            if (EconomyManager.Instance != null)
                await EconomyManager.Instance.LoadBalances();

            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterInventory");
        }
        catch (System.Exception e)
        {
            statusText.text = $"Error: {e.Message}";
            Debug.LogError($"Login error: {e.Message}");
        }
    }

    async void Register()
    {
        try
        {
            statusText.text = "Registrando...";
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(
                usernameInput.text, passwordInput.text);
            statusText.text = "¡Cuenta creada! Inicia sesión.";
        }
        catch (System.Exception e)
        {
            statusText.text = $"Error: {e.Message}";
        }
    }
}