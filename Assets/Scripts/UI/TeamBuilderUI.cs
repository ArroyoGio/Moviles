using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TeamBuilderUI : MonoBehaviour
{
    [Header("Slots del equipo")]
    public Image slot0Retrato;
    public TMP_Text slot0Nombre;
    public Image slot1Retrato;
    public TMP_Text slot1Nombre;

    [Header("Botón combate")]
    public Button botonCombate;

    void Start()
    {
        if (TeamManager.Instance == null)
        {
            Debug.LogError("TeamManager no encontrado — inicia desde CharacterInventory");
            return;
        }

        var equipo = TeamManager.Instance.equipoActual;

        if (equipo.activos[0] != null) MostrarSlot(0, equipo.activos[0]);
        if (equipo.activos[1] != null) MostrarSlot(1, equipo.activos[1]);

        ActualizarBoton();
    }

    void MostrarSlot(int slot, VeteranData veterano)
    {
        CharacterData data = veterano.baseData;

        if (slot == 0)
        {
            if (data.portrait != null) slot0Retrato.sprite = data.portrait;
            slot0Nombre.text = data.characterName;
        }
        else
        {
            if (data.portrait != null) slot1Retrato.sprite = data.portrait;
            slot1Nombre.text = data.characterName;
        }
    }

    void ActualizarBoton()
    {
        botonCombate.interactable = TeamManager.Instance.EquipoListo();
    }

    public void IrACombate()
    {
        if (!TeamManager.Instance.EquipoListo()) return;
        SceneManager.LoadScene("Combat");
    }

    public void Volver()
    {
        SceneManager.LoadScene("CharacterInventory");
    }
}