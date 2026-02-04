using UnityEngine;
using UnityEngine.UI;

public class ItemViewer : MonoBehaviour
{
    public Button button;

    public void Init()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        Init();
    }
}
