using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationUI : MonoBehaviour
{
    public void IrALogin() => SceneManager.LoadScene("Login");

    public void IrAMainHub() => SceneManager.LoadScene("MainHub");

    public void IrAPersonajes() => SceneManager.LoadScene("CharacterInventory");

    public void IrADetallePersonaje() => SceneManager.LoadScene("CharacterDetail");

    public void IrAArmarEquipo() => SceneManager.LoadScene("TeamBuilder");

    public void IrATienda() => SceneManager.LoadScene("Store");

    public void IrACombate() => SceneManager.LoadScene("Combat");
}