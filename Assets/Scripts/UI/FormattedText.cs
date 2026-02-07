using TMPro;
using UnityEngine;

public class FormattedText : MonoBehaviour
{
    [SerializeField] string formatString = "{0}";
    private TMP_Text _text;

    public void SetValue(float value)
    {
        if (!_text) _text = GetComponent<TMP_Text>();
        _text.text = string.Format(formatString, value);
    }
    public void SetValue(int value)
    {
        if (!_text) _text = GetComponent<TMP_Text>();
        _text.text = string.Format(formatString, value);
    }
    public void SetValue(params object[] objects)
    {
        if (!_text) _text = GetComponent<TMP_Text>();
        _text.text = string.Format(formatString, objects);
    }

    private void OnEnable()
    {
        _text = GetComponent<TMP_Text>();
    }
}
