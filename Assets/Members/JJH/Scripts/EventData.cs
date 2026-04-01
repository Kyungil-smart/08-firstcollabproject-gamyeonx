[System.Serializable]
public class EventData
{
    public string eventId;
    public string triggerType;  // WEEK, GOLD
    public int triggerValue;
    public string actions;      // 콤마로 구분된 액션 목록
    public bool isOneTime;      // 1회성 여부
}
