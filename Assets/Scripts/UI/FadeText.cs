using System.Collections;
using TMPro;
using UnityEngine;

public class FadeText : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] float waitTime = 1f;
    [SerializeField] float fadeTime = 0.5f;

    public void Play()
    {
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        Color color = new Color(text.color.r, text.color.g, text.color.b, 1);
        text.color = color;

        yield return new WaitForSeconds(waitTime);

        for (float t = 0; t <= 1; t += Time.deltaTime / fadeTime)
        {
            text.color = Color.Lerp(color, Color.clear, t);
            yield return null;
        }

        text.color = Color.clear;
    }
}
