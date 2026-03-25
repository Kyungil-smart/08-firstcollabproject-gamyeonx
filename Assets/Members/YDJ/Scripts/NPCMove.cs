using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class NPCMove : MonoBehaviour
{
    [Header("목표 지점")]
    public Vector3Int TargetPosition; // 최종 도착 지점의 그리드 좌표
    [Header("NPC 이동 속도")]
    [SerializeField] private float _speed = 5f; 

    private Vector3 _targetPos; // 그리드의 실제 월드 좌표
    private bool _isMoving = false;

    private Queue<Vector3Int> _pathQueue;// 이동할 경로의 그리드 좌표 저장
    private AStarPathfinder _pathfinder;

    void Awake()
    {
        Init();
    }
    
    void Update()
    {
        // T 누르면 이동(테스트 용)
        if (Input.GetKeyDown(KeyCode.T))
            GoToTarget(TargetPosition);
        
        // _isMoving이 true면  Move 실행
        Move();
        
    }
   
    public void GoToTarget(Vector3Int targetCell)
    {
        // 현재 NPC의 월드 좌표를 그리드 좌표로 변환
        Vector3Int startCell = GridBuildingSystem.Instance.gridLayout.WorldToCell(transform.position);
        Debug.Log($"Start: {startCell}");
        Debug.Log($"Target: {targetCell}");
        Debug.Log($"Start Tile: {GridBuildingSystem.Instance.GetTileType(startCell)}");
        Debug.Log($"Target TileType: {GridBuildingSystem.Instance.GetTileType(targetCell)}");
        
        // 시작 그리드 좌표의 TileType이 Road가 아니면 실행 안 함
        if (GridBuildingSystem.Instance.GetTileType(startCell) != TileType.Road)
        {
            Debug.Log("시작 위치가 길이 아님");
            return;
        }
        
        // 도착 그리드 좌표의 TileType이 Road가 아니면 실행 안 함
        if (GridBuildingSystem.Instance.GetTileType(targetCell) != TileType.Road)
        {
            Debug.Log("목표가 길이 아님");
            return;
        }
        
        // _pathfinder로부터 경로를 받아옴
        List<Vector3Int> path = _pathfinder.FindPath(startCell, targetCell);

        if (path == null)
        {
            Debug.Log("길 없음");
            _isMoving = false; // 가는 도중에 길이 사라지면 NPC 움직임 멈춤
            return;
        }
        
        // 전에 쓴 경로는 사용하면 안 되므로 Queue 초기화 해줌
        _pathQueue.Clear();

        // path를 Queue에 담음
        foreach (Vector3Int cell in path)
        {
            // startcell과 같은 그리드 좌표면 안 넣어줘도 됨
            if (cell == startCell) continue;
            _pathQueue.Enqueue(cell);
        }

        MoveNext(); // 다음 그리드로 이동
    }
    
    void MoveNext()
    {
        // Queue안에 더 이상 경로가 없으면 안 움직임
        if (_pathQueue.Count == 0)
        {
            _isMoving = false;
            return;
        }
        
        // Queue에서 꺼내서 MoveToGrid로 전달
        Vector3Int nextCell = _pathQueue.Dequeue();
        MoveToGrid(nextCell);
    }
    
    // 해당 그리드 위치로 이동을 시작하는 메서드
    public void MoveToGrid(Vector3Int cellPos)
    {
        // 이동 가능 체크, 그리드의 TileType이 Road가 아니라면 갈 수 없음
        if (!IsWalkable(cellPos))
        {
            Debug.Log("이동 불가");
            GoToTarget(TargetPosition); // 최단거리로 이동 중 길이 없어질 경우 다른 길 탐색
            return;
        }
        
        // 도착점의 그리드 좌표를 월드 좌표로 변환, x,y에 각 0.5를 더해줌으로 NPC가 그리드의 중앙에 오도록 만들어줌
        _targetPos = GridBuildingSystem.Instance.gridLayout
            .CellToLocalInterpolated(cellPos + new Vector3(0.5f, 0.5f, 0));

        _targetPos.z = 0;

        _isMoving = true;
    }

    // 실제 NPC 이동 메서드
    void Move()
    {
        if (!_isMoving) return;
        
        transform.position = Vector3.MoveTowards(transform.position, _targetPos, _speed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, _targetPos) < 0.01f)
        {
            transform.position = _targetPos;
            MoveNext(); // 다음 그리드로 이동
        }
    }

    // 그리드의 TileType이 Road인지 아닌지 판별
    bool IsWalkable(Vector3Int pos)
    {
        return GridBuildingSystem.Instance.GetTileType(pos) == TileType.Road;
    }
    
    void Init()
    {
        _pathQueue = new Queue<Vector3Int>();
        _pathfinder = new AStarPathfinder();
    }
}
