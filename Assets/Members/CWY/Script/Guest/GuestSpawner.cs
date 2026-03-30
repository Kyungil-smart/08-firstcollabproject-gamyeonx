using System.Collections;
using System.Reflection;
using UnityEngine;

public class GuestSpawner : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject _guestPrefab;
    [SerializeField] private GameTime _gameTime;

    [Header("입장 가능 시간")]
    [SerializeField] private float _spawnOpenDuration = 60f;

    [Header("사이클별 목표 스폰 수")]
    [Tooltip("0번 = 1사이클, 1번 = 2사이클, 2번 = 3사이클")]
    [SerializeField] private int[] _spawnCountPerCycle;

    [Header("Visitor ID 설정")]
    [SerializeField] private bool _useRandomVisitorID = true;
    [SerializeField] private int _fixedVisitorID = 1;
    [SerializeField] private int[] _randomVisitorIDPool;

    [Header("디버그")]
    [SerializeField] private bool _enableDebugLog = true;

    private Coroutine _spawnRoutine;

    private int _currentCycleIndex = 1;
    private bool _wasSpawnWindowOpen;
    private bool _wasTurnInitialized;

    // GameTime private 필드 접근용
    private FieldInfo _userTimeField;
    private FieldInfo _userTimeUnitField;

    private void Awake()
    {
        if(_gameTime == null)
        {
            _gameTime = FindFirstObjectByType<GameTime>();
        }

        CacheGameTimeFields();
    }

    private void Update()
    {
        if(_gameTime == null)
        {
            return;
        }

        float currentTurnTime = GetGameTimeValue("_userTime");
        float currentTurnDuration = GetGameTimeValue("_userTimeUnit");

        if(currentTurnDuration <= 0f)
        {
            return;
        }

        bool isSpawnWindowOpen = currentTurnTime < _spawnOpenDuration;

        // 첫 시작 처리
        if(!_wasTurnInitialized)
        {
            _wasTurnInitialized = true;
            _wasSpawnWindowOpen = isSpawnWindowOpen;

            if (isSpawnWindowOpen)
            {
                StartSpawnForCurrentCycle();
            }

            Log($"[GuestSpawner] 초기화 완료 | Cycle={_currentCycleIndex}, TurnTime={currentTurnTime:F1}");
            return;
        }

        // 스폰 가능 구간 시작 감지
        if(!_wasSpawnWindowOpen && isSpawnWindowOpen)
        {
            _currentCycleIndex++;
            StartSpawnForCurrentCycle();

            Log($"[GuestSpawner] 새 사이클 시작 감지 | Cycle={_currentCycleIndex}, TurnTime={currentTurnTime:F1}");
        }

        // 스폰 가능 구간 종료 감지
        if(_wasSpawnWindowOpen && !isSpawnWindowOpen)
        {
            StopSpawnRoutine();
            Log($"[GuestSpawner] 스폰 가능 구간 종료 | Cycle={_currentCycleIndex}, TurnTime={currentTurnTime:F1}");
        }

        _wasSpawnWindowOpen = isSpawnWindowOpen;
    }

    private void CacheGameTimeFields()
    {
        if(_gameTime == null)
        {
            Debug.LogWarning("[GuestSpawner] GameTime 참조가 없습니다.");
            return;
        }

        System.Type gameTimeType = typeof(GameTime);

        _userTimeField = gameTimeType.GetField("_userTime", BindingFlags.NonPublic | BindingFlags.Instance);
        _userTimeUnitField = gameTimeType.GetField("_userTimeUnit", BindingFlags.NonPublic | BindingFlags.Instance);

        if(_userTimeField == null || _userTimeUnitField == null)
        {
            Debug.LogWarning("[GuestSpawner] GameTime의 private 필드를 찾지 못했습니다. 필드명이 변경되었는지 확인하세요.");
        }
    }

    private float GetGameTimeValue(string fieldName)
    {
        if(_gameTime == null)
        {
            return 0f;
        }

        FieldInfo field = fieldName switch
        {
            "_userTime" => _userTimeField,
            "_userTimeUnit" => _userTimeUnitField,
            _ => null
        };

        if(field == null)
        {
            return 0f;
        }

        object value = field.GetValue(_gameTime);

        if(value is float floatValue)
        {
            return floatValue;
        }

        return 0f;
    }

    private void StartSpawnForCurrentCycle()
    {
        if(_guestPrefab == null)
        {
            Debug.LogWarning("[GuestSpawner] Guest Prefab이 비어 있습니다.");
            return;
        }

        if(_spawnCountPerCycle == null || _spawnCountPerCycle.Length == 0)
        {
            Debug.LogWarning("[GuestSpawner] SpawnCountPerCycle이 비어 있습니다.");
            return;
        }

        StopSpawnRoutine();
        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private void StopSpawnRoutine()
    {
        if(_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        int targetSpawnCount = GetTargetSpawnCountForCurrentCycle();

        Log($"[GuestSpawner] 스폰 시작 | Cycle={_currentCycleIndex}, TargetSpawn={targetSpawnCount}");

        if(targetSpawnCount <= 0)
        {
            Log("[GuestSpawner] 이번 사이클 목표 스폰 수가 0이므로 생성하지 않습니다.");
            yield break;
        }

        float spawnInterval = _spawnOpenDuration / targetSpawnCount;
        Log($"[GuestSpawner] 자동 스폰 간격 = {spawnInterval:F2}s");

        for(int i = 0; i < targetSpawnCount; i++)
        {
            float currentTurnTime = GetGameTimeValue("_userTime");

            if(currentTurnTime >= _spawnOpenDuration)
            {
                Log("[GuestSpawner] 스폰 가능 시간이 종료되어 스폰을 중단합니다.");
                yield break;
            }

            SpawnGuest();

            if(i == targetSpawnCount - 1)
            {
                break;
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        Log($"[GuestSpawner] 스폰 완료 | Cycle={_currentCycleIndex}, Spawned={targetSpawnCount}");
    }

    private int GetTargetSpawnCountForCurrentCycle()
    {
        if(_spawnCountPerCycle == null || _spawnCountPerCycle.Length == 0)
        {
            return 0;
        }

        int arrayIndex = _currentCycleIndex - 1;

        if(arrayIndex < 0)
        {
            return 0;
        }

        if(arrayIndex >= _spawnCountPerCycle.Length)
        {
            return Mathf.Max(0, _spawnCountPerCycle[_spawnCountPerCycle.Length - 1]);
        }

        return Mathf.Max(0, _spawnCountPerCycle[arrayIndex]);
    }

    public GameObject SpawnGuest()
    {
        if(_guestPrefab == null)
        {
            Debug.LogWarning("[GuestSpawner] Guest Prefab이 비어 있어 스폰할 수 없습니다.");
            return null;
        }

        GameObject guestObject = Instantiate(_guestPrefab, transform.position, Quaternion.identity);
        GuestController guestController = guestObject.GetComponent<GuestController>();

        if(guestController == null)
        {
            Debug.LogWarning("[GuestSpawner] 생성된 오브젝트에 GuestController가 없습니다.");
            Destroy(guestObject);
            return null;
        }

        int visitorID = GetSpawnVisitorID();
        guestController.SetupSpawn(visitorID);

        Log($"[GuestSpawner] 손님 생성 완료 | Cycle={_currentCycleIndex}, VisitorID={visitorID}");

        return guestObject;
    }

    private int GetSpawnVisitorID()
    {
        if(!_useRandomVisitorID)
        {
            return _fixedVisitorID;
        }

        if(_randomVisitorIDPool == null || _randomVisitorIDPool.Length == 0)
        {
            Debug.LogWarning("[GuestSpawner] 랜덤 VisitorID Pool이 비어 있어 FixedVisitorID를 사용합니다.");
            return _fixedVisitorID;
        }

        int randomIndex = Random.Range(0, _randomVisitorIDPool.Length);
        return _randomVisitorIDPool[randomIndex];
    }

    private void Log(string message)
    {
        if(_enableDebugLog)
        {
            Debug.Log(message);
        }
    }
}