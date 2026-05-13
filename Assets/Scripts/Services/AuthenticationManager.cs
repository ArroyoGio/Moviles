using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class AuthenticationManager : MonoBehaviour
{
    [Header("UI Refs")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public TMP_Text statusText;
    public GameObject loadingSpinner;

    [Header("Feedback Visual")]
    public Image usernameBorder;
    public Image passwordBorder;

    private Color colorError = new Color(1f, 0.2f, 0.2f);
    private Color colorOK = new Color(0.2f, 1f, 0.5f);
    private Color colorDefault = new Color(1f, 1f, 1f, 0.3f);

    async void Start()
    {
        statusText.transform.parent.gameObject.SetActive(false); // oculta StatusPanel al inicio

        await UnityServices.InitializeAsync();

        loginButton.onClick.AddListener(Login);
        registerButton.onClick.AddListener(Register);

        // Recordar último usuario
        string savedUser = PlayerPrefs.GetString("last_username", "");
        if (!string.IsNullOrEmpty(savedUser))
            usernameInput.text = savedUser;

        SetLoading(false);
        ResetBorders();
    }

    async void Login()
    {
        if (!ValidarCampos()) return;

        SetLoading(true);
        statusText.text = "Iniciando sesión...";
        statusText.color = Color.white;

        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(
                usernameInput.text.Trim(), passwordInput.text);

            PlayerPrefs.SetString("last_username", usernameInput.text.Trim());
            PlayerPrefs.Save();

            statusText.text = "¡Bienvenid@! Cargando...";
            statusText.color = colorOK;

            if (EconomyManager.Instance != null)
                await EconomyManager.Instance.LoadBalances();

            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterInventory");
        }
        catch (AuthenticationException e)
        {
            MostrarError(TraducirError(e.ErrorCode));
        }
        catch (System.Exception e)
        {
            MostrarError("Error inesperado. Intenta de nuevo.");
            Debug.LogError($"Login error: {e.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    async void Register()
    {
        if (!ValidarCampos()) return;

        SetLoading(true);
        statusText.text = "Creando cuenta...";
        statusText.color = Color.white;

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(
                usernameInput.text.Trim(), passwordInput.text);

            statusText.text = "¡Cuenta creada! Ya puedes iniciar sesión.";
            statusText.color = colorOK;
            SetBorderColor(usernameBorder, colorOK);
            SetBorderColor(passwordBorder, colorOK);
        }
        catch (AuthenticationException e)
        {
            MostrarError(TraducirError(e.ErrorCode));
        }
        catch (System.Exception e)
        {
            MostrarError("No se pudo crear la cuenta. Intenta de nuevo.");
            Debug.LogError($"Register error: {e.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    bool ValidarCampos()
    {
        ResetBorders();

        bool ok = true;

        if (string.IsNullOrWhiteSpace(usernameInput.text))
        {
            SetBorderColor(usernameBorder, colorError);
            ok = false;
        }

        if (string.IsNullOrWhiteSpace(passwordInput.text) || passwordInput.text.Length < 8)
        {
            SetBorderColor(passwordBorder, colorError);
            if (!ok == false)
                MostrarError("La contraseña debe tener al menos 8 caracteres.");
            ok = false;
        }

        if (!ok && string.IsNullOrWhiteSpace(usernameInput.text))
            MostrarError("Completa todos los campos.");

        return ok;
    }

    string TraducirError(int code)
    {
        return code switch
        {
            10002 => "Usuario o contraseña incorrectos.",
            10003 => "El nombre de usuario ya existe.",
            10009 => "La contraseña debe tener al menos 8 caracteres.",
            10013 => "Nombre de usuario inválido.",
            _ => $"Error ({code}). Intenta de nuevo."
        };
    }

    void MostrarError(string msg)
    {
        statusText.text = msg;
        statusText.color = colorError;
        statusText.transform.parent.gameObject.SetActive(true); // activa StatusPanel
        SetBorderColor(usernameBorder, colorError);
        SetBorderColor(passwordBorder, colorError);
    }

    void SetLoading(bool estado)
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(estado);
        loginButton.interactable = !estado;
        registerButton.interactable = !estado;
        usernameInput.interactable = !estado;
        passwordInput.interactable = !estado;
    }

    void SetBorderColor(Image img, Color c)
    {
        if (img != null) img.color = c;
    }

    void ResetBorders()
    {
        SetBorderColor(usernameBorder, colorDefault);
        SetBorderColor(passwordBorder, colorDefault);
    }
}