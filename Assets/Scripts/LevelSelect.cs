using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One button per level, locked until unlocked, with its best time. Reads SaveData only: it never spells a key.
public class LevelSelect : MonoBehaviour
{
    [SerializeField] Button[] buttons;      // Level 1 to 5, in order
    [SerializeField] TMP_Text[] labels;

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int level = i + 1;                                   // a copy per button: the lambda must not see the loop variable
            buttons[i].onClick.AddListener(() => LevelLoader.Instance.Load(level));   // Level01..05 are build indices 1..5
        }
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int level = i + 1;
            bool open = SaveData.IsUnlocked(level);
            buttons[i].interactable = open;
            if (!open) labels[i].text = "Level " + level + "\nLocked";
            else if (SaveData.HasBest(level)) labels[i].text = "Level " + level + "\nBest " + SaveData.FormatTime(SaveData.GetBest(level));
            else labels[i].text = "Level " + level + "\nNo time yet";
        }
    }

    public GameObject FirstSelectable()
    {
        for (int i = 0; i < buttons.Length; i++)
            if (buttons[i].interactable) return buttons[i].gameObject;
        return buttons[0].gameObject;
    }
}
