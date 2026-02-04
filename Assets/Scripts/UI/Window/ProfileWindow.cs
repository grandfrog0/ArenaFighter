using TMPro;
using UnityEngine;

public class ProfileWindow : MonoBehaviour
{
    [SerializeField] DataSaver dataSaver;
    [SerializeField] UserData userData;
    [SerializeField] TMP_InputField usernameField;
    [SerializeField] FormattedText statisticsText;

    private void OnEnable()
    {
        usernameField.text = userData.Name;
        statisticsText.SetValue(userData.Wins, userData.Loses);

        usernameField.onValueChanged.AddListener(SetUsername);
    }
    private void OnDisable()
    {
        usernameField.onValueChanged.RemoveListener(SetUsername);
        dataSaver.Save();
    }
    private void SetUsername(string value)
    {
        userData.Name = value;
    }
}
