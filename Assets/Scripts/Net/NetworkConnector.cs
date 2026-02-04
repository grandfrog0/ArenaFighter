using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkConnector : MonoBehaviour
{
    private Coroutine startAutoRoutine;

    public void StartHost()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
            return;

        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started.");
    }
    public void StartClient()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
            return;

        NetworkManager.Singleton.StartClient();
        Debug.Log("Client started.");
    }
    public void Stop()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Stopped.");
        }
    }

    /// <summary>
    /// Запуск хоста или клиента в зависимости от ситуации
    /// </summary>
    public void StartAuto()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
            return;

        if (startAutoRoutine != null)
        {
            StopCoroutine(startAutoRoutine);
            startAutoRoutine = null;
        }
        startAutoRoutine = StartCoroutine(StartAutoRoutine());
    }

    private IEnumerator StartAutoRoutine()
    {
        NetworkManager.Singleton.StartClient();

        yield return new WaitForSeconds(1);

        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
            yield return new WaitForSeconds(0.5f);
            StartHost();
        }
        else Debug.Log("Client started.");
    }
    public void StopAuto()
    {
        if (startAutoRoutine != null)
        {
            StopCoroutine(startAutoRoutine);
            startAutoRoutine = null;
        }
        Stop();
    }
}
