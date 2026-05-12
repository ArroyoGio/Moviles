using UnityEngine;

[System.Serializable]
public class TeamData
{
    public VeteranData[] activos = new VeteranData[2]; // Fase 1: solo 2

    public bool EsValido()
    {
        foreach (var v in activos)
            if (v == null) return false;
        return true;
    }
}