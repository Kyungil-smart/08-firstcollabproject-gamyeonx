using System.Collections.Generic;
using UnityEngine;

public class TurnGuestExitManager : MonoBehaviour
{
    [Header("시간 설정")]
    [SerializeField] private float _turnEndTime = 180f;       // 3분
    [SerializeField] private float _forceCloseTime = 240f;    // 4분

    [Header("참조")]
    [SerializeField] private GameTime _gameTime;
    [SerializeField] private GuestSpawner _guestSpawner;

    private bool _hasTurnEnded;
    private bool _hasForceClosed;
    private bool _hasFinishedTurn;

    private readonly HashSet<GuestController> _aliveGuests = new HashSet<GuestController>();

    private void Awake()
    {
        if (_gameTime == null)
        {
            _gameTime = FindFirstObjectByType<GameTime>();
        }
    }

    private void OnEnable()
    {
        GuestController.OnGuestRemoved += HandleGuestRemoved;
    }

    private void OnDisable()
    {
        GuestController.OnGuestRemoved -= HandleGuestRemoved;
    }

    private void Update()
    {
        if (_gameTime == null)
        {
            return;
        }

        if (_hasFinishedTurn)
        {
            return;
        }

        float currentTurnTime = _gameTime._userTime;

        if (!_hasTurnEnded && currentTurnTime >= _turnEndTime)
        {
            StartTurnEnding();
        }

        if (!_hasForceClosed && currentTurnTime >= _forceCloseTime)
        {
            ForceCloseTurn();
        }
    }

    public void ResetTurnState()
    {
        _hasTurnEnded = false;
        _hasForceClosed = false;
        _hasFinishedTurn = false;
        _aliveGuests.Clear();

        Debug.Log("[TurnGuestExitManager] 턴 상태 초기화");
    }

    public void RegisterGuest(GuestController guest)
    {
        if (guest == null)
        {
            return;
        }

        if (_hasFinishedTurn)
        {
            return;
        }

        _aliveGuests.Add(guest);
        Debug.Log($"[TurnGuestExitManager] 손님 등록 | Count={_aliveGuests.Count}");
    }

    private void StartTurnEnding()
    {
        _hasTurnEnded = true;
        Debug.Log("[TurnGuestExitManager] 3분 도달 -> 턴 종료 시작");

        foreach (GuestController guest in _aliveGuests)
        {
            if (guest == null)
            {
                continue;
            }

            guest.NotifyTurnEnded();
        }

        TryFinishTurn();
    }

    private void ForceCloseTurn()
    {
        _hasForceClosed = true;
        Debug.Log("[TurnGuestExitManager] 4분 도달 -> 강제 종료");

        List<GuestController> guests = new List<GuestController>(_aliveGuests);

        for (int i = 0; i < guests.Count; i++)
        {
            if (guests[i] == null)
            {
                continue;
            }

            guests[i].ForceRemoveGuest();
        }

        FinishTurn();
    }

    private void HandleGuestRemoved(GuestController guest)
    {
        if (guest == null)
        {
            return;
        }

        if (_aliveGuests.Remove(guest))
        {
            Debug.Log($"[TurnGuestExitManager] 손님 제거 감지 | Count={_aliveGuests.Count}");
        }

        TryFinishTurn();
    }

    private void TryFinishTurn()
    {
        if (!_hasTurnEnded)
        {
            return;
        }

        if (_aliveGuests.Count > 0)
        {
            return;
        }

        FinishTurn();
    }

    private void FinishTurn()
    {
        if (_hasFinishedTurn)
        {
            return;
        }

        _hasFinishedTurn = true;
        Debug.Log("[TurnGuestExitManager] 영업 종료 -> 다음 주차 테스트 시작");

        if (_gameTime != null)
        {
            _gameTime.HandleTurnFinishedForTest();
        }
    }
}