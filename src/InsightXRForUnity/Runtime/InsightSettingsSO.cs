using UnityEngine;

[CreateAssetMenu(fileName = "InsightSettings", menuName = "InsightDesk/InsightSettings")]
public class InsightSettingsSO : ScriptableObject
{
    public string customerID;
    public string userID;
    public string apiKey;
}