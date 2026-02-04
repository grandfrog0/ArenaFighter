using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameGuiWindow : MonoBehaviour
{
    [SerializeField] Image fighter1Image;
    [SerializeField] Image fighter2Image;

    [SerializeField] Image talismanEffect;
    private Coroutine _effectRoutine;

    public void Init(FighterSettings fighter, FighterEntity entity)
    {
        fighter1Image.sprite = fighter.Icon;
        entity.OnTalismanEffectUsed.AddListener(SetTalismanEffect);
    }

    private void SetTalismanEffect(FightingTalisman talisman)
    {
        talismanEffect.sprite = talisman.EffectSprite;

        if (_effectRoutine != null)
        {
            StopCoroutine(_effectRoutine);
        }    
        _effectRoutine = StartCoroutine(FadeRoutine(talisman.UseTime));
    }

    private IEnumerator FadeRoutine(float time)
    {
        talismanEffect.enabled = true;
        talismanEffect.color = Color.white;

        yield return new WaitForSeconds(time);
        for (float t = 1; t >= 0; t -= Time.deltaTime * 3)
        {
            talismanEffect.color = Color.Lerp(Color.clear, Color.white, t);
            yield return null;
        }

        talismanEffect.enabled = false;
    }
}
