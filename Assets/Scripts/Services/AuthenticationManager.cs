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
    public Button guestButton;
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
        guestButton.onClick.AddListener(LoginAnonimo);

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

            // esto ya lo tienes
            if (EconomyManager.Instance != null)
                await EconomyManager.Instance.LoadBalances();

            // AGREGA esto después:
            if (CloudCodeManager.Instance != null)
            {
                // Guardar que el jugador entró
                await CloudCodeManager.Instance.GuardarDatos(0, 0);

                // Leer sus datos guardados
                var datos = await CloudCodeManager.Instance.LeerDatos();
                Debug.Log("Datos del jugador cargados");
            }

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

    async void LoginAnonimo()
    {
        SetLoading(true);

        statusText.transform.parent.gameObject.SetActive(true);
        statusText.text = "Entrando como invitado...";
        statusText.color = Color.white;

        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log(AuthenticationService.Instance.PlayerId);

            statusText.text = "¡Bienvenid@ invitad@!";
            statusText.color = colorOK;

            if (EconomyManager.Instance != null)
                await EconomyManager.Instance.LoadBalances();
            if (CloudCodeManager.Instance != null)
            {
                try
                {
                    // Pequeña espera para asegurar que el token esté listo
                    await System.Threading.Tasks.Task.Delay(500);

                    await CloudCodeManager.Instance.GuardarDatos(0, 0);
                    await CloudCodeManager.Instance.LeerDatos();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Cloud Code opcional falló: " + e.Message);
                }
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterInventory");

            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterInventory");
        }
        catch (AuthenticationException e)
        {
            MostrarError($"Error Auth ({e.ErrorCode})");
        }
        catch (System.Exception e)
        {
            MostrarError("No se pudo iniciar como invitado.");
            Debug.LogError(e.Message);
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