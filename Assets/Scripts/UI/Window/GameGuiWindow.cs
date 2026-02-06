using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameGuiWindow : NetworkBehaviour
{
    [SerializeField] Image fighter1Image, talisman1Image, elixir1Image;
    [SerializeField] Image fighter2Image, talisman2Image, elixir2Image;

    [SerializeField] Image talismanEffect;
    private Coroutine _effectRoutine;

    public void InitPlayer(SelectedPlayerData data, FighterEntity entity)
    {
        FighterSettings fighter = PrefabBuffer.GetFighter(data.FighterId);
        FightingTalisman talisman = PrefabBuffer.GetTalisman(data.TalismanId);
        FightingElixir elixir = PrefabBuffer.GetElixir(data.ElixirId);

        fighter1Image.sprite = fighter.Icon;
        if (talisman) talisman1Image.sprite = talisman.Icon;
        if (elixir) elixir1Image.sprite = elixir.Icon;
        entity.OnTalismanEffectUsed.AddListener(SetTalismanEffect);
    }

    public void InitEnemy(SelectedPlayerData data, FighterEntity entity)
    {
        FighterSettings fighter = PrefabBuffer.GetFighter(data.FighterId);
        FightingTalisman talisman = PrefabBuffer.GetTalisman(data.TalismanId);
        FightingElixir elixir = PrefabBuffer.GetElixir(data.ElixirId);

        fighter2Image.sprite = fighter.Icon;
        if (talisman) talisman2Image.sprite = talisman.Icon;
        if (elixir) elixir2Image.sprite = elixir.Icon;
        entity.OnTalismanEffectUsed.AddListener(SetEnemyTalismanEffect);
    }

    private void SetTalismanEffect(int talismanId)
    {
        FightingTalisman talisman = PrefabBuffer.GetTalisman(talismanId);
        if (talisman.Name == "IceTalisman")
            return;

        talismanEffect.sprite = talisman.EffectSprite;

        if (_effectRoutine != null)
        {
            StopCoroutine(_effectRoutine);
        }
        _effectRoutine = StartCoroutine(FadeRoutine(talisman.UseTime));
    }

    private void SetEnemyTalismanEffect(int talismanId)
    {
        FightingTalisman talisman = PrefabBuffer.GetTalisman(talismanId);
        if (talisman.Name != "IceTalisman")
            return;

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
