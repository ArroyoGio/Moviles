using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.CloudCode;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CloudCodeManager : MonoBehaviour
{
    public static CloudCodeManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // ?? GUARDAR DATOS — directo a Cloud Save ??????????????????
    public async Task GuardarDatos(int nivelPvE, int totalPartidas)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { "nivelPvE",      nivelPvE      },
                { "totalPartidas", totalPartidas }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("Guardado OK en Cloud Save");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("GuardarDatos fallo: " + e.Message);
        }
    }

    // ?? LEER DATOS — directo desde Cloud Save ?????????????????
    public async Task<Dictionary<string, object>> LeerDatos()
    {
        try
        {
            var keys = new HashSet<string> { "nivelPvE", "totalPartidas" };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            var datos = new Dictionary<string, object>();

            foreach (var kvp in result)
                datos[kvp.Key] = kvp.Value.Value.GetAs<object>();

            Debug.Log("Datos leidos desde Cloud Save");
            return datos;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("LeerDatos fallo: " + e.Message);
            return new Dictionary<string, object>();
        }
    }

    // ?? TRANSACCIÓN — esta sí va por Cloud Code ???????????????
    public async Task<bool> EjecutarTransaccionGacha(int costo)
    {
        try
        {
            var parametros = new Dictionary<string, object>
            {
                { "costo", costo }
            };

            var resultado = await CloudCodeService.Instance
                .CallEndpointAsync<Dictionary<string, object>>(
                    "TransaccionGacha", parametros);

            bool exito = false;

            if (resultado.ContainsKey("success"))
                exito = (bool)resultado["success"];

            if (exito)
                Debug.Log("Transaccion OK. Fichas: " + resultado["fichasRestantes"]);
            else
                Debug.Log("Transaccion fallida: " + resultado["mensaje"]);

            return exito;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Transaccion fallo: " + e.Message);
            return false;
        }
    }
}