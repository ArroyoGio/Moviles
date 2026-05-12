using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;

    public TeamData equipoActual = new TeamData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public bool AsignarActivo(VeteranData veterano, int slot)
    {
        if (slot < 0 || slot >= equipoActual.activos.Length) return false;
        equipoActual.activos[slot] = veterano;
        return true;
    }

    public bool EquipoListo() => equipoActual.EsValido();
}