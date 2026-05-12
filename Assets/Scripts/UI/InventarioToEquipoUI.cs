using UnityEngine;
using UnityEngine.SceneManagement;

public class InventarioToEquipoUI : MonoBehaviour
{
    public void IrAArmarEquipo()
    {
        SceneManager.LoadScene("TeamBuilder");
    }

    public void IrATienda()
    {
        SceneManager.LoadScene("Store");
    }

}