using System.Collections;
using UnityEngine;

public class GuestSpawner : MonoBehaviour
{
    [Header("손님 프리팹")]
    [SerializeField] private GameObject _guestPrefab;

    [Header("사이클 설정")]
    [SerializeField] private float _spawnOpenDuration = 60f;
    [SerializeField] private float _cycleDuration = 180f;
    [SerializeField] private bool _spawnOnStart = true;

    [Header("사이클별 목표 스폰 수")]
    [Tooltip("0번 = 1사이클, 1번 = 2사이클, 2번 = 3사이클")]
    [SerializeField] private int[] _spawnCountPerCycle;

    [Header("Visitor ID 설정")]
    [SerializeField] private bool _useRandomVisitorID = true;
    [SerializeField] private int _fixedVisitorID = 1;
    [SerializeField] private int[] _randomVisitorIDPool;

    [Header("디버그")]
    [SerializeField] private bool _enableDebugLog = true;

    private Coroutine _spawnCycleCoroutine;
    private bool _isSpawnRunning;
    private int _cycleIndex;

    private void Start()
    {
        if(_spawnOnStart)
        {
            StartSpawn();
        }
    }

    public void StartSpawn()
    {
        if(_guestPrefab == null)
        {
            Debug.LogWarning("[GuestSpawner] Guest Prefab이 비어 있습니다.");
            return;
        }

        if(_cycleDuration <= 0f)
        {
            Debug.LogWarning("[GuestSpawner] Cycle Duration은 0보다 커야 합니다.");
            return;
        }

        if(_spawnOpenDuration <= 0f || _spawnOpenDuration > _cycleDuration)
        {
            Debug.LogWarning("[GuestSpawner] Spawn Open Duration은 0보다 크고 Cycle Duration 이하여야 합니다.");
            return;
        }

        if(_spawnCountPerCycle == null || _spawnCountPerCycle.Length == 0)
        {
            Debug.LogWarning("[GuestSpawner] SpawnCountPerCycle이 비어 있습니다.");
            return;
        }

        StopSpawn();

        _isSpawnRunning = true;
        _cycleIndex = 0;
        _spawnCycleCoroutine = StartCoroutine(SpawnCycleRoutine());

        Log("[GuestSpawner] 스폰 시스템 시작");
    }

    public void StopSpawn()
    {
        _isSpawnRunning = false;

        if (_spawnCycleCoroutine != null)
        {
            StopCoroutine(_spawnCycleCoroutine);
            _spawnCycleCoroutine = null;
        }

        Log("[GuestSpawner] 스폰 시스템 중지");
    }

    private IEnumerator SpawnCycleRoutine()
    {
        while (_isSpawnRunning)
        {
            _cycleIndex++;

            int currentCycleTargetCount = GetTargetSpawnCountForCurrentCycle();

            Log($"[GuestSpawner] 사이클 시작 | Cycle={_cycleIndex}, TargetSpawn={currentCycleTargetCount}");

            yield return StartCoroutine(SpawnWindowRoutine(currentCycleTargetCount));

            if (!_isSpawnRunning)
            {
                yield break;
            }

            float closedDuration = _cycleDuration - _spawnOpenDuration;

            if (closedDuration > 0f)
            {
                Log($"[GuestSpawner] 스폰 닫힘 구간 시작 | Duration={closedDuration:F1}s");
                yield return new WaitForSeconds(closedDuration);
            }
        }
    }

    private IEnumerator SpawnWindowRoutine(int targetSpawnCount)
    {
        Log($"[GuestSpawner] 스폰 가능 구간 시작 | Duration={_spawnOpenDuration:F1}s, Target={targetSpawnCount}");

        if (targetSpawnCount <= 0)
        {
            Log("[GuestSpawner] 이번 사이클 목표 스폰 수가 0이므로 생성하지 않습니다.");
            yield return new WaitForSeconds(_spawnOpenDuration);
            yield break;
        }

        // 목표 수에 맞춰 간격 자동 계산
        float spawnIntervalForThisCycle = _spawnOpenDuration / targetSpawnCount;

        Log($"[GuestSpawner] 이번 사이클 자동 스폰 간격 = {spawnIntervalForThisCycle:F2}s");

        for(int i = 0; i < targetSpawnCount; i++)
        {
            if(!_isSpawnRunning)
            {
                yield break;
            }

            SpawnGuest();

            // 마지막 스폰 뒤에는 대기 안 함
            if(i == targetSpawnCount - 1)
            {
                break;
            }

            yield return new WaitForSeconds(spawnIntervalForThisCycle);
        }

        Log($"[GuestSpawner] 스폰 가능 구간 종료 | Spawned={targetSpawnCount}");

        // 남은 시간 보정
        float estimatedUsedTime = spawnIntervalForThisCycle * targetSpawnCount;
        float remainTime = _spawnOpenDuration - estimatedUsedTime;

        if(remainTime > 0f)
        {
            yield return new WaitForSeconds(remainTime);
        }
    }

    private int GetTargetSpawnCountForCurrentCycle()
    {
        if(_spawnCountPerCycle == null || _spawnCountPerCycle.Length == 0)
        {
            return 0;
        }

        int arrayIndex = _cycleIndex - 1;

        if(arrayIndex < 0)
        {
            return 0;
        }

        if(arrayIndex >= _spawnCountPerCycle.Length)
        {
            // 배열을 넘어가면 마지막 값 계속 사용
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

        Log($"[GuestSpawner] 손님 생성 완료 | Cycle={_cycleIndex}, VisitorID={visitorID}");

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
