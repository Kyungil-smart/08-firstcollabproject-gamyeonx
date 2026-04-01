

using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    
    private List<EventData> _eventDataList = new List<EventData>();
    
    private Dictionary<string, Action> _actionHandlers = new Dictionary<string, Action>();
    
    public bool IsLoading { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        RegisterActionHandlers();  // 실행 가능한 액션 미리 등록
    }

    private void Start()
    {
        StartCoroutine(GetComponent<EventDataLoader>().LoadEvents(OnEventsLoaded));
    }

    private void OnEventsLoaded(List<EventData> events)
    {
        _eventDataList = events;
        Debug.Log($"이벤트 {events.Count}개 로드 완료");
    }
    
    private void RegisterActionHandlers()
    {
        _actionHandlers["SHOW_MESSAGE_WEEK10"] = () =>
        {
            if (IsLoading) return;
            Debug.Log("메시지 표시");
            // 메세지 표시 로직
        };
        _actionHandlers["UNLOCK_BUILDING_asdf"] = () => 
        {
            Debug.Log("건물 해금");
        };
        _actionHandlers["SHOW_MESSAGE_GOLD10000"] = () => 
        {
            Debug.Log("");
        };
    }

    // 조건 체크 - 주차 변경 시 호출
    public void CheckWeekEvents(int currentWeek)
    {
        foreach (var eventData in _eventDataList)
        {
            if (eventData.triggerType != "WEEK") continue;
            if (eventData.triggerValue != currentWeek) continue;
            if (eventData.isOneTime && UIManager.Instance._triggeredEvents.Contains(eventData.eventId)) continue;

            TriggerEvent(eventData);
        }
    }

    // 조건 체크 - 골드 변경 시 호출
    public void CheckGoldEvents(int currentGold)
    {
        foreach (var eventData in _eventDataList)
        {
            if (eventData.triggerType != "GOLD") continue;
            if (currentGold < eventData.triggerValue) continue;
            if (eventData.isOneTime && UIManager.Instance._triggeredEvents.Contains(eventData.eventId)) continue;

            TriggerEvent(eventData);
        }
    }

    private void TriggerEvent(EventData eventData)
    {
        if (eventData.isOneTime)
            UIManager.Instance._triggeredEvents.Add(eventData.eventId);

        // 콤마로 구분된 액션 순차 실행
        foreach (var action in eventData.actions.Split(','))
        {
            string actionKey = action.Trim();
            if (_actionHandlers.TryGetValue(actionKey, out var handler))
                handler.Invoke();
            else
                Debug.LogWarning($"등록되지 않은 액션: {actionKey}");
        }
    }

    public void LoadTriggerEvents()
    {
        IsLoading = true;
        var triggeredIds = UIManager.Instance._triggeredEvents; 

        foreach (var eventData in _eventDataList)
        {
            if (triggeredIds.Contains(eventData.eventId))
            {
                foreach (var action in eventData.actions.Split(','))
                {
                    string actionKey = action.Trim();
                    if (_actionHandlers.TryGetValue(actionKey, out var handler))
                    {
                        handler.Invoke();
                    }
                }
            }
        }
        
        IsLoading = false;
    }
}