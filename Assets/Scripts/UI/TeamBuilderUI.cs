using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TeamBuilderUI : MonoBehaviour
{
    [Header("Veteranos disponibles")]
    public List<VeteranData> misVeteranos;
    public GameObject characterCardPrefab;
    public Transform contentParent;

    [Header("Slot jugador")]
    public Image slot0Retrato;
    public TMP_Text slot0Nombre;

    [Header("Slot rival")]
    public Image slot1Retrato;
    public TMP_Text slot1Nombre;

    [Header("Botones")]
    public Button botonCombate;
    public Button botonVolver;

    private VeteranData seleccionado;
    private VeteranData rival;

    void Start()
    {
        if (TeamManager.Instance == null)
        {
            Debug.LogError("TeamManager no encontrado.");
            return;
        }

        if (misVeteranos == null || misVeteranos.Count == 0)
        {
            Debug.LogError("No hay veteranos asignados en TeamBuilderUI.");
            return;
        }

        if (characterCardPrefab == null || contentParent == null)
        {
            Debug.LogError("Faltan referencias para crear las cards en TeamBuilderUI.");
            return;
        }

        if (botonCombate == null)
        {
            Debug.LogError("BotonCombate no esta asignado en TeamBuilderUI.");
            return;
        }

        TeamManager.Instance.AsignarActivo(null, 0);
        TeamManager.Instance.AsignarActivo(null, 1);

        CrearCards();

        botonCombate.interactable = false;
        botonCombate.onClick.RemoveAllListeners();
        botonCombate.onClick.AddListener(IrACombate);

        if (botonVolver != null)
        {
            botonVolver.onClick.RemoveAllListeners();
            botonVolver.onClick.AddListener(Volver);
        }

        RefrescarSlots();
    }

    void CrearCards()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (VeteranData veterano in misVeteranos)
        {
            if (veterano == null || veterano.baseData == null)
            {
                Debug.LogWarning("Veterano invalido en TeamBuilderUI.");
                continue;
            }

            GameObject card = Instantiate(characterCardPrefab, contentParent);
            CharacterCardUI cardUI = card.GetComponent<CharacterCardUI>();
            if (cardUI == null)
            {
                Debug.LogError("El prefab de personaje no tiene CharacterCardUI.");
                Destroy(card);
                continue;
            }

            cardUI.SetupForTeamBuilder(veterano, SeleccionarPersonaje);
        }
    }

    void SeleccionarPersonaje(VeteranData veterano)
    {
        if (veterano == null || veterano.baseData == null) return;

        seleccionado = veterano;
        rival = CrearRivalAutomatico(seleccionado);

        if (rival != null)
        {
            TeamManager.Instance.AsignarActivo(seleccionado, 0);
            TeamManager.Instance.AsignarActivo(rival, 1);
        }
        else
        {
            Debug.LogError("No hay rival valido para el combate 1v1.");
        }

        RefrescarSlots();
    }

    VeteranData CrearRivalAutomatico(VeteranData jugador)
    {
        foreach (VeteranData veterano in misVeteranos)
        {
            if (veterano != null && veterano != jugador && veterano.baseData != null)
                return veterano;
        }

        return null;
    }

    void RefrescarSlots()
    {
        MostrarSlot(slot0Retrato, slot0Nombre, seleccionado, "Elige personaje");
        MostrarSlot(slot1Retrato, slot1Nombre, rival, "Rival automatico");

        if (botonCombate != null)
            botonCombate.interactable = seleccionado != null && rival != null;
    }

    void MostrarSlot(Image retrato, TMP_Text nombre, VeteranData veterano, string textoVacio)
    {
        if (veterano == null || veterano.baseData == null)
        {
            if (retrato != null)
            {
                retrato.sprite = null;
                retrato.enabled = false;
            }

            if (nombre != null) nombre.text = textoVacio;
            return;
        }

        CharacterData data = veterano.baseData;
        Sprite sprite = data.portrait != null ? data.portrait : data.fullArt;

        if (retrato != null)
        {
            retrato.sprite = sprite;
            retrato.enabled = sprite != null;
            retrato.preserveAspect = true;
        }

        if (nombre != null)
            nombre.text = data.characterName;
    }

    public void IrACombate()
    {
        if (seleccionado == null || rival == null || TeamManager.Instance == null) return;

        TeamManager.Instance.AsignarActivo(seleccionado, 0);
        TeamManager.Instance.AsignarActivo(rival, 1);

        if (!TeamManager.Instance.EquipoListo()) return;

        SceneManager.LoadScene("Combat");
    }

    public void Volver()
    {
        SceneManager.LoadScene("MainHub");
    }
}
