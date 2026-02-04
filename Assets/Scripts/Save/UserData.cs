using UnityEngine;

[CreateAssetMenu(fileName = "userData", menuName = "SO/User Data")]
public class UserData : ScriptableObject
{
    public string Name;
    public int Wins;
    public int Loses;
    public float Diamonds;
    public float Coins;
}
