

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    
    private List<EventData> _eventDataList = new List<EventData>();
    
    private Dictionary<string, Action> _actionHandlers = new Dictionary<string, Action>();
    
    public bool IsLoading { get; private set; }
    public bool IsTutorial = false;
    
    [Header("이벤트 캔버스")]
    [SerializeField] private Transform _eventContentParent;
    [Header("코루틴 실행 전 조작 방지 이미지")]
    [SerializeField] private Transform _dontTouchImage;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _dontTouchImage.gameObject.SetActive(true);
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
        
        StartCoroutine(WaitAndLoadEvents());
    }
    
    private void RegisterActionHandlers()
    {
        _actionHandlers["TUTORIAL"] = () =>
        {
            if (!IsLoading)
            {
                IsTutorial = true;
                EventsCanvasActive("TUTORIAL");
            }
            Debug.Log("튜토리얼 실행");
        };
        
        _actionHandlers["INCREASE_VISITOR_BASE"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("INCREASE_VISITOR_BASE");
            }
        };
        
        _actionHandlers["ENABLE_MERCHANT_BUFF"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("ENABLE_MERCHANT_BUFF");
            }
        };     
        
        _actionHandlers["INCREASE_VISITOR_WEEKLY"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("INCREASE_VISITOR_WEEKLY");
            }
        };     
        
        _actionHandlers["INCREASE_UPKEEP_COST"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("INCREASE_UPKEEP_COST");
            }
        };      
        
        _actionHandlers["REPUTATION_BONUS_EVENT"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("REPUTATION_BONUS_EVENT");
            }
        };      
        
        _actionHandlers["INCREASE_RESOURCE_GAIN"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("INCREASE_RESOURCE_GAIN");
            }
        };
                
        _actionHandlers["HIGH_TIER_VISITOR_RATE_UP"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("HIGH_TIER_VISITOR_RATE_UP");
            }
        };
                
        _actionHandlers["HERO_VISIT_TRIGGER"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("HERO_VISIT_TRIGGER");
            }
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

        // ;로 구분된 액션 순차 실행
        foreach (var action in eventData.actions.Split(';'))
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
        Debug.Log("트리거 로딩 실행");
        IsLoading = true;
        var triggeredIds = UIManager.Instance._triggeredEvents; 

        foreach (var eventData in _eventDataList)
        {
            if (triggeredIds.Contains(eventData.eventId))
            {
                foreach (var action in eventData.actions.Split(';'))
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
    
    private IEnumerator WaitAndLoadEvents()
    {
        yield return new WaitForSeconds(2.0f);

        Debug.Log($"현재 실행된 이벤트 개수{UIManager.Instance._triggeredEvents.Count}");
        
        if (UIManager.Instance != null && UIManager.Instance._triggeredEvents.Count > 0)
        {
            LoadTriggerEvents();
        }
        
        yield return new WaitForSeconds(2.0f);
        
        if (UIManager.Instance._triggeredEvents.Count == 0)
        {
            CheckWeekEvents(0);
        }
        
        _dontTouchImage.gameObject.SetActive(false);
        
    }

    private void EventsCanvasActive(string eventName)
    {
        Transform eventObj = null;
        
        for (int i = 0; i < _eventContentParent.childCount; i++)
        {
            if (_eventContentParent.GetChild(i).name == eventName)
            {
                eventObj = _eventContentParent.GetChild(i);
                break;
            }
        }

        if (eventObj == null)
        {
            Debug.LogWarning($"[UI] {eventName} 이라는 이름의 이벤트를 찾을 수 없습니다.");
            return;
        }
        
        eventObj.gameObject.SetActive(true);
        
        var conv = eventObj.GetComponent<EventUI>();
        if (conv != null)
        {
            conv.StartConversation();
        
            // 투명 '다음' 버튼 연결 (스크립트 내 OnClickNext 호출)
            // 이 버튼은 인스펙터에서 미리 연결해두거나 여기서 찾아서 등록합니다.
        }
    
        // 마지막 확인 버튼(Enter) 로직
        Button enterBtn = eventObj.Find("Enter")?.GetComponent<Button>();
        if (enterBtn != null)
        {
            enterBtn.onClick.RemoveAllListeners();
            enterBtn.onClick.AddListener(() => 
            {
                eventObj.gameObject.SetActive(false);
                if (IsTutorial) IsTutorial = false;
            });
        }
    }
}