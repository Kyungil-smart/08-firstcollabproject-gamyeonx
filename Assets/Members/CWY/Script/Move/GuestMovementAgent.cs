using System;
using System.Collections.Generic;
using UnityEngine;

public class GuestMovementAgent : MonoBehaviour
{
    [Header("외부 이동 속도")]
    [SerializeField] private float _outsideMoveSpeed = 3f;

    [Header("내부 이동 속도")]
    [SerializeField] private float _insideMoveSpeed = 1.5f;

    [Header("애니메이션")]
    [SerializeField] private CharacterAnimatorController _characterAnimatorController;

    [Header("디버그")]
    [SerializeField] private bool _showLog = false;

    private Vector3 _targetWorldPos;
    private bool _isMoving;
    private bool _isInsideMove;

    private Queue<Vector3Int> _pathQueue;
    private AStarPathfinder _pathfinder;

    public bool IsMoving => _isMoving;
    public bool IsInsideMove => _isInsideMove;

    public event Action OnMoveCompleted;
    public event Action OnMoveFailed;

    private void Reset()
    {
        _characterAnimatorController = GetComponentInChildren<CharacterAnimatorController>();
    }

    private void Awake()
    {
        _pathQueue = new Queue<Vector3Int>();
        _pathfinder = new AStarPathfinder();

        if (_characterAnimatorController == null)
        {
            _characterAnimatorController = GetComponentInChildren<CharacterAnimatorController>();
        }
    }

    private void Update()
    {
        if (!_isMoving)
        {
            UpdateAnimation(Vector2.zero);
            return;
        }

        Vector3 moveDirection3D = _targetWorldPos - transform.position;
        Vector2 moveDirection = new Vector2(moveDirection3D.x, moveDirection3D.y).normalized;

        UpdateAnimation(moveDirection);

        float speed = _isInsideMove ? _insideMoveSpeed : _outsideMoveSpeed;
        transform.position = Vector3.MoveTowards(transform.position, _targetWorldPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _targetWorldPos) > 0.01f)
        {
            return;
        }

        transform.position = _targetWorldPos;

        if (_isInsideMove)
        {
            _isMoving = false;
            UpdateAnimation(Vector2.zero);
            OnMoveCompleted?.Invoke();

            if (_showLog)
            {
                Debug.Log("[GuestMovementAgent] 내부 이동 완료");
            }

            return;
        }

        MoveNextOutsideCell();
    }

    public bool MoveToRoadCell(Vector3Int targetCell)
    {
        if (GridBuildingSystem.Instance == null)
        {
            OnMoveFailed?.Invoke();
            return false;
        }

        Vector3Int startCell = GridBuildingSystem.Instance.gridLayout.WorldToCell(transform.position);

        if (GridBuildingSystem.Instance.GetTileType(startCell) != TileType.Road)
        {
            OnMoveFailed?.Invoke();
            return false;
        }

        if (GridBuildingSystem.Instance.GetTileType(targetCell) != TileType.Road)
        {
            OnMoveFailed?.Invoke();
            return false;
        }

        List<Vector3Int> path = _pathfinder.FindPath(startCell, targetCell);

        if (path == null || path.Count == 0)
        {
            OnMoveFailed?.Invoke();
            return false;
        }

        _pathQueue.Clear();

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == startCell)
            {
                continue;
            }

            _pathQueue.Enqueue(path[i]);
        }

        _isInsideMove = false;
        MoveNextOutsideCell();

        return true;
    }

    public void MoveInsideTo(Transform targetPoint)
    {
        if (targetPoint == null)
        {
            OnMoveFailed?.Invoke();
            return;
        }

        _isInsideMove = true;
        _isMoving = true;
        _targetWorldPos = targetPoint.position;
        _targetWorldPos.z = 0f;

        Vector3 moveDirection3D = _targetWorldPos - transform.position;
        Vector2 moveDirection = new Vector2(moveDirection3D.x, moveDirection3D.y).normalized;
        UpdateAnimation(moveDirection);
    }

    public void TeleportTo(Transform targetPoint)
    {
        Vector3 pos = targetPoint.position;
        pos.z = 0f;
        transform.position = pos;

        UpdateAnimation(Vector2.zero);
    }

    public void StopMove()
    {
        _isMoving = false;
        _isInsideMove = false;
        _pathQueue.Clear();

        UpdateAnimation(Vector2.zero);
    }

    private void MoveNextOutsideCell()
    {
        if (_pathQueue.Count == 0)
        {
            _isMoving = false;
            UpdateAnimation(Vector2.zero);
            OnMoveCompleted?.Invoke();

            return;
        }

        Vector3Int nextCell = _pathQueue.Dequeue();

        if (GridBuildingSystem.Instance.GetTileType(nextCell) != TileType.Road)
        {
            _isMoving = false;
            UpdateAnimation(Vector2.zero);
 
            OnMoveFailed?.Invoke();
            return;
        }

        _targetWorldPos = GridBuildingSystem.Instance.gridLayout.CellToLocalInterpolated(
            nextCell + new Vector3(0.5f, 0.5f, 0f)
        );
        _targetWorldPos.z = 0f;

        _isMoving = true;

        Vector3 moveDirection3D = _targetWorldPos - transform.position;
        Vector2 moveDirection = new Vector2(moveDirection3D.x, moveDirection3D.y).normalized;
        UpdateAnimation(moveDirection);
    }

    private void UpdateAnimation(Vector2 moveInput)
    {
        if (_characterAnimatorController == null)
        {
            return;
        }

        _characterAnimatorController.UpdateAnimation(moveInput);
    }
}