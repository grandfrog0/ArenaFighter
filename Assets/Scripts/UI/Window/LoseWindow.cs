using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoseWindow : MonoBehaviour
{
    [SerializeField] DataSaver dataSaver;
    [SerializeField] UserData userData;
    [SerializeField] FormattedText statisticsText;

    [SerializeField] AudioSource au;

    public void Lose()
    {
        userData.Loses++;
        statisticsText.SetValue(userData.Wins, userData.Loses);
        dataSaver.Save();

        au.Play();
    }
}
