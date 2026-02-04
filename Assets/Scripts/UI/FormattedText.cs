using TMPro;
using UnityEngine;

public class FormattedText : MonoBehaviour
{
    [SerializeField] string formatString = "{0}";
    private TMP_Text _text;

    public void SetValue(float value)
    {
        _text.text = string.Format(formatString, value);
    }
    public void SetValue(int value)
    {
        _text.text = string.Format(formatString, value);
    }
    public void SetValue(params object[] objects)
    {
        _text.text = string.Format(formatString, objects);
    }

    private void OnValidate()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        _text = GetComponent<TMP_Text>();
    }
}
