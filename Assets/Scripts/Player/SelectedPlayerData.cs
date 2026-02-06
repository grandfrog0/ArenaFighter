using Unity.Netcode;

public struct SelectedPlayerData : INetworkSerializable
{
    public int FighterId;
    public int TalismanId;
    public int ElixirId;
    public bool IsReady;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref FighterId);
        serializer.SerializeValue(ref TalismanId);
        serializer.SerializeValue(ref ElixirId);
        serializer.SerializeValue(ref IsReady);
    }
}