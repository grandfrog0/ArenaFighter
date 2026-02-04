using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
    static string FolderPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArenaFighter");
    static string UserDataPath => Path.Combine(FolderPath, "userData.json");
    static string UserGameSettingsPath => Path.Combine(FolderPath, "userGameSettings.json");
    static string StoreConfigPath => Path.Combine(FolderPath, "storeConfig.json");
    static string FighterSettingsPath(string name) => Path.Combine(FolderPath, $"fighter{name}Config.json");

    [SerializeField] UserData userData;
    [SerializeField] UserGameSettings userGameSettings;
    [SerializeField] StoreConfig storeConfig;
    [SerializeField] List<FighterSettings> fightersSettings;

    public void Load()
    {
        try
        {
            string json = File.ReadAllText(UserDataPath);
            JsonUtility.FromJsonOverwrite(json, userData);

            //json = File.ReadAllText(StoreConfigPath);
            //Debug.Log(string.Join(";  ", storeConfig.Items));
            //Debug.Log(json);
            //JsonUtility.FromJsonOverwrite(json, storeConfig);
            //Debug.Log(string.Join(";  ", storeConfig.Items));

            json = File.ReadAllText(UserGameSettingsPath);
            JsonUtility.FromJsonOverwrite(json, userGameSettings);

            foreach (var fighterSettings in fightersSettings)
            {
                json = File.ReadAllText(FighterSettingsPath(fighterSettings.Name));
                JsonUtility.FromJsonOverwrite(json, storeConfig);
            }
        }
        catch (Exception e) 
        {
            Debug.Log("Error on load occured: " + e);
        }
        Debug.Log("Loaded.");
    }
    public void Save()
    {
        try
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            string json = JsonUtility.ToJson(userData, true);
            File.WriteAllText(UserDataPath, json);

            json = JsonUtility.ToJson(storeConfig, true);
            File.WriteAllText(StoreConfigPath, json);

            json = JsonUtility.ToJson(userGameSettings, true);
            File.WriteAllText(UserGameSettingsPath, json);

            foreach (var fighterSettings in fightersSettings)
            {
                json = JsonUtility.ToJson(fighterSettings, true);
                File.WriteAllText(FighterSettingsPath(fighterSettings.Name), json);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error on save occured: " + e);
        }
    }

    private void Awake() => Load();
    private void OnApplicationQuit() => Save();
}