using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    public Dictionary<string, Action> Events = new Dictionary<string, Action>();
    private bool _loadEvents = false; // 1회성 이벤트의 경우 false일때만 실행되게 해야함
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitEvents();
    }
    
    // 이벤트들 등록 (모든 이벤트 다 해줘야함)
    private void InitEvents()
    {
        Events.Add("WEEK_10_EVENT", OnWeek10Event);
    }

    public void TriggerEvent(string eventName)
    {
        if (Events.TryGetValue(eventName, out Action action))
        {
            action.Invoke();
        }
        else
        {
            Debug.Log($"{eventName}에 연결된 함수가 없습니다.");
        }
    }
    
    private void OnWeek10Event()
    {
        Debug.Log("10주차 이벤트 발생! 특별 건물을 해금합니다.");
        if (!_loadEvents)
        {
            // 캔버스 활성화 해서 메세지 출력
            // 버튼 클릭시 캔버스 비활성화
        }
        // 이후 건물 건설 Content 활성화 -> 이거 어케 저장/로드 할 것 인지? -> 나중에 로드할때 해시셋에 등록되어 있으면 자동으로 활성화 되게 해야할듯
        // 위에 1회성 컨텐츠들 관련된 bool 타입 하나 선언해서 true 일때는 실행 안되게하고 밑에 이벤트들만 실행되게 해서 hashset에있는애들 순차적으로 다 활성화?
        // 이때는 해시셋 contain 빼고 그냥 등록되어있는애들 순차적으로 싹 다 실행하는 방향으로 하면 되긴 할 듯
    }
    
    // 해시셋 순회해서 하나씩 이벤트 활성화
    public void LoadEvents()
    {
        _loadEvents = true;
        foreach (string eventID in UIManager.Instance._triggeredEvents)
        {
            TriggerEvent(eventID);
        }
        _loadEvents = false;
    }
}
