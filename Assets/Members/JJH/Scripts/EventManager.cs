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

    [Header("이벤트 캔버스")]
    [SerializeField] private Transform _eventContentParent;

    [Header("스폰 이벤트 수치")]
    [SerializeField] private int _increaseVisitorBaseAmount = 3;
    [SerializeField] private int _increaseVisitorWeeklyAmount = 5;

    [Header("모험가 증가 주간(C, B, A)")]
    [SerializeField] private int _weeklyAdventurerCBonusWeight = 15;
    [SerializeField] private int _weeklyAdventurerBBonusWeight = 10;
    [SerializeField] private int _weeklyAdventurerABonusWeight = 5;

    [Header("고등급 모험가 증가(A)")]
    [SerializeField] private int _highTierAdventurerABonusWeight = 20;

    // 이번 주차에 적용되는 보정값
    public int CurrentCycleVisitorBonus { get; private set; }
    public int CurrentCycleAdventurerCBonusWeight { get; private set; }
    public int CurrentCycleAdventurerBBonusWeight { get; private set; }
    public int CurrentCycleAdventurerABonusWeight { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RegisterActionHandlers();
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
                EventsCanvasActive("TUTORIAL");
            }

            Debug.Log("[EventManager] 튜토리얼 실행");
        };

        _actionHandlers["INCREASE_VISITOR_BASE"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("INCREASE_VISITOR_BASE");
            }

            CurrentCycleVisitorBonus += _increaseVisitorBaseAmount;
            Debug.Log(
                $"[EventManager] 기본 방문객 증가 이벤트 실행 완료 | " +
                $"Action=INCREASE_VISITOR_BASE, " +
                $"Add={_increaseVisitorBaseAmount}, " +
                $"CurrentCycleVisitorBonus={CurrentCycleVisitorBonus}");
        };

        _actionHandlers["ENABLE_MERCHANT_BUFF"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("ENABLE_MERCHANT_BUFF");
            }

            Debug.Log("[EventManager] 상인 버프 이벤트 실행");
        };

        _actionHandlers["INCREASE_VISITOR_WEEKLY"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("INCREASE_VISITOR_WEEKLY");
            }

            CurrentCycleVisitorBonus += _increaseVisitorWeeklyAmount;
            CurrentCycleAdventurerCBonusWeight += _weeklyAdventurerCBonusWeight;
            CurrentCycleAdventurerBBonusWeight += _weeklyAdventurerBBonusWeight;
            CurrentCycleAdventurerABonusWeight += _weeklyAdventurerABonusWeight;

            Debug.Log(
                $"[EventManager] 모험가 증가 주간 적용 | " +
                $"Visitor+={_increaseVisitorWeeklyAmount}, " +
                $"C+={_weeklyAdventurerCBonusWeight}, " +
                $"B+={_weeklyAdventurerBBonusWeight}, " +
                $"A+={_weeklyAdventurerABonusWeight}");
        };

        _actionHandlers["INCREASE_UPKEEP_COST"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("INCREASE_UPKEEP_COST");
            }

            Debug.Log("[EventManager] 유지비 증가 이벤트 실행");
        };

        _actionHandlers["REPUTATION_BONUS_EVENT"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("REPUTATION_BONUS_EVENT");
            }

            Debug.Log("[EventManager] 축제 이벤트 실행");
        };

        _actionHandlers["INCREASE_RESOURCE_GAIN"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("INCREASE_RESOURCE_GAIN");
            }

            Debug.Log("[EventManager] 자원 획득 증가 이벤트 실행");
        };

        _actionHandlers["HIGH_TIER_VISITOR_RATE_UP"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("HIGH_TIER_VISITOR_RATE_UP");
            }

            CurrentCycleAdventurerABonusWeight += _highTierAdventurerABonusWeight;

            Debug.Log(
                $"[EventManager] 고등급 모험가 증가 이벤트 실행 완료 | " +
                $"Action=HIGH_TIER_VISITOR_RATE_UP, " +
                $"A+={_highTierAdventurerABonusWeight}, " +
                $"CurrentCycleAdventurerABonusWeight={CurrentCycleAdventurerABonusWeight}");
        };

        _actionHandlers["HERO_VISIT_TRIGGER"] = () =>
        {
            if (!IsLoading)
            {
                EventsCanvasActive("HERO_VISIT_TRIGGER");
            }

            Debug.Log("[EventManager] 영웅 방문 이벤트 실행");
        };
    }

    // 조건 체크 - 주차 변경 시 호출
    public void CheckWeekEvents(int currentWeek)
    {
        // 이 구조는 유지하고, 시작/종료 처리만 앞뒤에 추가
        CurrentCycleVisitorBonus = 0;
        CurrentCycleAdventurerCBonusWeight = 0;
        CurrentCycleAdventurerBBonusWeight = 0;
        CurrentCycleAdventurerABonusWeight = 0;


        foreach (var eventData in _eventDataList)
        {
            if (eventData.triggerType != "WEEK")
            {
                Debug.Log($"[EventManager] WEEK 타입 아님 스킵 | EventID={eventData.eventId}");
                continue;
            }
            if (eventData.triggerValue != currentWeek)
            {
                Debug.Log($"[EventManager] 주차 불일치 스킵 | EventID={eventData.eventId}, Need={eventData.triggerValue}, Current={currentWeek}");
                continue;
            }
            if (eventData.isOneTime && UIManager.Instance._triggeredEvents.Contains(eventData.eventId))
            {
                Debug.Log($"[EventManager] 이미 실행된 일회성 이벤트 스킵 | EventID={eventData.eventId}");
                continue; 
            }

            TriggerEvent(eventData);
        }
    }

    //조건체크 - 골드 변경 시 호출
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
        {
            if (!UIManager.Instance._triggeredEvents.Contains(eventData.eventId))
            {
                UIManager.Instance._triggeredEvents.Add(eventData.eventId);
            }
        }

        foreach (var action in eventData.actions.Split(';'))
        {
            string actionKey = action.Trim();

            if (_actionHandlers.TryGetValue(actionKey, out var handler))
            {
                handler.Invoke();
            }
            else
            {
                Debug.LogWarning($"등록되지 않은 액션: {actionKey}");
            }
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

        Debug.Log($"현재 실행된 이벤트 개수 {UIManager.Instance._triggeredEvents.Count}");

        if (UIManager.Instance != null && UIManager.Instance._triggeredEvents.Count > 0)
        {
            LoadTriggerEvents();
        }

        yield return new WaitForSeconds(2.0f);

        if (UIManager.Instance._triggeredEvents.Count == 0)
        {
            CheckWeekEvents(0);
        }
    }

    private void EventsCanvasActive(string eventName)
    {
        if (_eventContentParent == null)
        {
            Debug.LogWarning("[EventManager] Event Content Parent가 비어 있습니다.");
            return;
        }

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

        Button enterBtn = eventObj.Find("Enter")?.GetComponent<Button>();
        if (enterBtn != null)
        {
            enterBtn.onClick.RemoveAllListeners();
            enterBtn.onClick.AddListener(() =>
            {
                eventObj.gameObject.SetActive(false);
            });
        }
    }
}