using UnityEngine;
using System.Collections.Generic;

public class CharacterInventoryManager : MonoBehaviour
{
    public List<VeteranData> misVeteranos;   // asigna aqui Dot y Hana
    public GameObject characterCardPrefab;
    public Transform contentParent;

    // Guarda el veterano que el jugador toco
    public static VeteranData selectedVeteran;

    void Start()
    {
        // Limpia cards anteriores antes de instanciar
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (VeteranData veterano in misVeteranos)
        {
            GameObject card = Instantiate(characterCardPrefab, contentParent);
            card.GetComponent<CharacterCardUI>().SetupVeteran(veterano);
        }
    }
}
