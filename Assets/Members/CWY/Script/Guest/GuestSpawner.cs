using System.Collections.Generic;
using UnityEngine;

public class GuestSpawner : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameTime _gameTime;
    [SerializeField] private GuestDataDatabaseSO _guestDataDatabase;
    [SerializeField] private TurnGuestExitManager _turnGuestExitManager;

    [Header("입장 가능 시간")]
    [SerializeField] private float _spawnOpenDuration = 60f;

    [Header("턴 전체 시간")]
    [SerializeField] private float _turnDuration = 180f;

    [Header("정규분포 예약 스폰 설정")]
    [SerializeField] private float _spawnMean = 30f;
    [SerializeField] private float _spawnStdDev = 12f;

    [Header("테스트 스폰 설정")]
    [SerializeField] private bool _useFixedVisitorIDForTest = false;
    [SerializeField] private int _fixedVisitorID = 1;

    [Header("디버그")]
    [SerializeField] private bool _enableDebugLog = true;

    [SerializeField] private TurnEndUI _turnEndUI;

    private bool _wasSpawnWindowOpen;
    private bool _wasTurnInitialized;

    private readonly List<float> _spawnTimes = new List<float>();
    private int _nextSpawnIndex;

    private void Awake()
    {
        if (_gameTime == null)
        {
            _gameTime = FindFirstObjectByType<GameTime>();
        }
    }

    private void Update()
    {
        if (_gameTime == null)
        {
            return;
        }

        float currentTurnTime = _gameTime._userTime;
        float currentTurnDuration = _turnDuration;

        if (currentTurnDuration <= 0f)
        {
            return;
        }

        bool isSpawnWindowOpen = currentTurnTime < _spawnOpenDuration;

        if (!_wasTurnInitialized)
        {
            _wasTurnInitialized = true;
            _wasSpawnWindowOpen = isSpawnWindowOpen;

            if (_turnGuestExitManager != null)
            {
                _turnGuestExitManager.ResetTurnState();
            }

            if (isSpawnWindowOpen)
            {
                StartSpawnPlanForCurrentWeek();
            }

            return;
        }

        if (!_wasSpawnWindowOpen && isSpawnWindowOpen)
        {
            if (_turnGuestExitManager != null)
            {
                _turnGuestExitManager.ResetTurnState();
            }

            StartSpawnPlanForCurrentWeek();
        }

        if (_wasSpawnWindowOpen && !isSpawnWindowOpen)
        {
            ClearSpawnPlan();
        }

        if (isSpawnWindowOpen)
        {
            ProcessReservedSpawns(currentTurnTime);
        }

        _wasSpawnWindowOpen = isSpawnWindowOpen;
    }

    private void StartSpawnPlanForCurrentWeek()
    {
        if (_gameTime == null)
        {
            Debug.LogWarning("[GuestSpawner] GameTime 참조가 없습니다.");
            return;
        }

        if (_gameTime._userWeek <= 0)
        {
            ClearSpawnPlan();
            Log($"[GuestSpawner] 0주차이므로 손님 스폰을 진행하지 않습니다. | Week={_gameTime._userWeek}");
            return;
        }

        if (_guestDataDatabase == null)
        {
            Debug.LogWarning("GuestDataDatabaseSO가 비어 있습니다.");
            return;
        }

        if (GuestPoolManager.Instance == null)
        {
            Debug.LogWarning("없습니다.");
            return;
        }

        int currentWeek = _gameTime._userWeek;
        int baseSpawnCount = GetBaseSpawnCount(currentWeek);
        int eventBonusSpawnCount = GetEventVisitorBonusForCurrentWeek();
        int finalSpawnCount = Mathf.Max(0, baseSpawnCount + eventBonusSpawnCount);

        Debug.Log(
            $"[GuestSpawner] 스폰 수 계산 | " +
            $"Week={currentWeek}, " +
            $"Base={baseSpawnCount}, " +
            $"EventBonus={eventBonusSpawnCount}, " +
            $"Final={finalSpawnCount}");

        CreateSpawnTimes(finalSpawnCount);
        _nextSpawnIndex = 0;

        Log($"[GuestSpawner] 스폰 계획 생성 | Week={currentWeek}, Base={baseSpawnCount}, Bonus={eventBonusSpawnCount}, Final={finalSpawnCount}");
    }

    private int GetBaseSpawnCount(int week)
    {
        return Mathf.RoundToInt(10f + 14.4f * (Mathf.Sqrt(week) - 1f));
    }

    private int GetEventVisitorBonusForCurrentWeek()
    {
        if (EventManager.Instance == null)
        {
            return 0;
        }

        Debug.Log($"[GuestSpawner] 현재 이벤트 스폰 보너스 사용 | Bonus={EventManager.Instance.CurrentCycleVisitorBonus}");
        return EventManager.Instance.CurrentCycleVisitorBonus;
    }

    private void CreateSpawnTimes(int targetSpawnCount)
    {
        _spawnTimes.Clear();

        for (int i = 0; i < targetSpawnCount; i++)
        {
            float spawnTime;

            do
            {
                spawnTime = GetGaussianRandom(_spawnMean, _spawnStdDev);
            }
            while (spawnTime < 0f || spawnTime > _spawnOpenDuration);

            _spawnTimes.Add(spawnTime);
        }

        _spawnTimes.Sort();

        for (int i = 0; i < _spawnTimes.Count; i++)
        {
            Log($"[GuestSpawner] SpawnTimes[{i}] = {_spawnTimes[i]:F2}");
        }
    }

    private float GetGaussianRandom(float mean, float stdDev)
    {
        float u1 = 1f - Random.value;
        float u2 = 1f - Random.value;

        float randStdNormal =
            Mathf.Sqrt(-2f * Mathf.Log(u1)) *
            Mathf.Sin(2f * Mathf.PI * u2);

        return mean + stdDev * randStdNormal;
    }

    private void ProcessReservedSpawns(float currentTurnTime)
    {
        while (_nextSpawnIndex < _spawnTimes.Count &&
               currentTurnTime >= _spawnTimes[_nextSpawnIndex] &&
               currentTurnTime < _spawnOpenDuration)
        {
            Debug.Log($"[GuestSpawner] 실제 스폰 실행 | TurnTime={currentTurnTime:F2}, SpawnTime={_spawnTimes[_nextSpawnIndex]:F2}");
            SpawnGuest();
            _nextSpawnIndex++;
        }
    }

    private void ClearSpawnPlan()
    {
        _spawnTimes.Clear();
        _nextSpawnIndex = 0;
    }

    public GameObject SpawnGuest()
    {
        if (GuestPoolManager.Instance == null)
        {
            Debug.LogWarning("스폰할 수 없습니다.");
            return null;
        }

        int visitorID = GetSpawnVisitorID();

        if (visitorID <= 0)
        {
            Debug.LogWarning("VisitorID를 찾지 못했습니다.");
            return null;
        }

        GameObject guestObject = GuestPoolManager.Instance.GetGuest(transform.position, Quaternion.identity);

        if (guestObject == null)
        {
            Debug.LogWarning("Guest를 가져오지 못했습니다.");
            return null;
        }

        GuestController guestController = guestObject.GetComponent<GuestController>();

        if (guestController == null)
        {
            Debug.LogWarning("GuestController가 없습니다.");
            GuestPoolManager.Instance.ReturnGuest(guestObject);
            return null;
        }

        guestController.SetupSpawn(visitorID);
        //추가
        _turnEndUI.AddVisitor();

        if (_turnGuestExitManager != null)
        {
            _turnGuestExitManager.RegisterGuest(guestController);
        }

        Log($"[GuestSpawner] 풀 스폰 완료 | VisitorID={visitorID}, Name={guestObject.name}");
        return guestObject;
    }

    private int GetSpawnVisitorID()
    {
        if (_useFixedVisitorIDForTest)
        {
            return _fixedVisitorID;
        }

        return GetWeightedRandomVisitorID();
    }

    private int GetWeightedRandomVisitorID()
    {
        IReadOnlyList<GuestDataRow> rows = _guestDataDatabase.GuestDataRows;

        if (rows == null || rows.Count == 0)
        {
            return -1;
        }

        int cBonus = EventManager.Instance != null ? EventManager.Instance.CurrentCycleAdventurerCBonusWeight : 0;
        int bBonus = EventManager.Instance != null ? EventManager.Instance.CurrentCycleAdventurerBBonusWeight : 0;
        int aBonus = EventManager.Instance != null ? EventManager.Instance.CurrentCycleAdventurerABonusWeight : 0;
        int aHightierBouns = EventManager.Instance != null ? EventManager.Instance.CurrentHighTierAdventurerABounsWeight : 0;
        int totalWeight = 0;
        List<int> finalWeights = new List<int>(rows.Count);

        for (int i = 0; i < rows.Count; i++)
        {
            GuestDataRow row = rows[i];

            if (row == null)
            {
                finalWeights.Add(0);
                continue;
            }

            int finalWeight = Mathf.Max(0, row.SpawnWeight);

            TryApplyAdventurerGradeBonus(row, ref finalWeight, cBonus, bBonus, aBonus, aHightierBouns);

            finalWeights.Add(finalWeight);
            totalWeight += finalWeight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning("[GuestSpawner] 모든 최종 가중치가 0 이하입니다.");
            return -1;
        }

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            int weight = finalWeights[i];

            if (weight <= 0 || rows[i] == null)
            {
                continue;
            }

            cumulativeWeight += weight;

            if (randomValue < cumulativeWeight)
            {
                Log($"[GuestSpawner] 가중치 선택 완료 | VisitorID={rows[i].VisitorID}, FinalWeight={weight}, Roll={randomValue}/{totalWeight}");
                return rows[i].VisitorID;
            }
        }

        Debug.LogWarning("[GuestSpawner] 가중치 선택 실패. 마지막 fallback 사용");
        return rows[rows.Count - 1] != null ? rows[rows.Count - 1].VisitorID : -1;
    }

    private bool TryApplyAdventurerGradeBonus(
        GuestDataRow row,
        ref int finalWeight,
        int cBonus,
        int bBonus,
        int aBonus,
        int aHightierBouns)
    {
        bool applied = false;

        if (!row.IsAdventurer)
        {
            return false;
        }

        switch (row.AdventurerGrade)
        {
            case "C":
                finalWeight += cBonus;
                applied = true;
                break;

            case "B":
                finalWeight += bBonus;
                applied = true;
                break;

            case "A":
                if(aHightierBouns > 0)
                {
                    finalWeight += aHightierBouns;
                }
                else
                {
                    finalWeight += aBonus;
                }
                applied = true;
                break;
        }

        return applied;
    }

    private void Log(string message)
    {
        if (_enableDebugLog)
        {
            Debug.Log(message);
        }
    }
}