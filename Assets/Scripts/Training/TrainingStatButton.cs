using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingStatButton : MonoBehaviour
{
    private TrainingManager manager;
    private TrainingStatType stat;
    private Button button;
    private TMP_Text label;

    public void Setup(TrainingManager trainingManager, TrainingStatType trainingStat)
    {
        manager = trainingManager;
        stat = trainingStat;
        button = GetComponent<Button>();
        label = GetComponentInChildren<TMP_Text>();

        if (label != null)
            label.text = $"{manager.GetStatName(stat)}\nEnergia {manager.GetEnergyCost(stat)}";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => manager.Train(stat));
        }
    }

    public void Refresh()
    {
        if (button != null)
            button.interactable = manager != null && manager.SelectedVeteran != null && !manager.IsComplete;
    }
}
