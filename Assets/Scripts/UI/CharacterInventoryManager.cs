using UnityEngine;
using System.Collections.Generic;

public class CharacterInventoryManager : MonoBehaviour
{
    public List<VeteranData> misVeteranos;   // asigna aquí Dot y Hana
    public GameObject characterCardPrefab;
    public Transform contentParent;

    // Guarda el veterano que el jugador tocó
    public static VeteranData selectedVeteran;

    void Start()
    {
        // Limpia cards anteriores antes de instanciar
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (TeamManager.Instance != null)
        {
            if (misVeteranos.Count > 0)
                TeamManager.Instance.AsignarActivo(misVeteranos[0], 0);
            if (misVeteranos.Count > 1)
                TeamManager.Instance.AsignarActivo(misVeteranos[1], 1);
        }

        foreach (VeteranData veterano in misVeteranos)
        {
            GameObject card = Instantiate(characterCardPrefab, contentParent);
            card.GetComponent<CharacterCardUI>().SetupVeteran(veterano);
        }
    }
}