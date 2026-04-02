using System.Collections.Generic;
using UnityEngine;

public class MoveWithWayPoints : MonoBehaviour
{
    public GuestController GuestController;
    [SerializeField] private List<Transform> _pathPoints = new List<Transform>();
    [SerializeField] private float _moveSpeed = 2f;
    private List<Transform> _currentPath;
    private int _currentIndex;
    private bool _isMoving;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var path = GetWaitToEnterancePath();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            var path = GetExitToFacilityExitPath();
        }
        
        MoveWithPathPoints();
    }

    public List<Transform> GetWaitToEnterancePath()
    {
        _pathPoints.Clear();

        FacilityRuntime facility = GuestController.CurrentFacilityRuntime;

        _pathPoints.Add(facility.WaitPoint);

        //foreach (var way in facility.EnteranceWayPoints)
        //{
        //    _pathPoints.Add(way);
        //}

        //_pathPoints.Add(facility.EnterancePoint);
        
        DebugPath(_pathPoints, "Wait -> Entrance");

        return _pathPoints;
    }

    public List<Transform> GetExitToFacilityExitPath()
    {
        _pathPoints.Clear();

        FacilityRuntime facility = GuestController.CurrentFacilityRuntime;

        //_pathPoints.Add(facility.ExitPoint);

        //foreach (var way in facility.ExitWayPoints)
        //{
        //    _pathPoints.Add(way);
        //}

        _pathPoints.Add(facility.FacilityExitPoint);

        DebugPath(_pathPoints, "Exit -> FacilityExit");

        return _pathPoints;
    }

    public void SettingPathAndMove(List<Transform> path)
    {
        if (path == null || path.Count == 0) return;
        
        _currentPath = path;
        _currentIndex = 0;
        _isMoving = true;
    }

    private void MoveWithPathPoints()
    {
        if (!_isMoving) return;

        Transform target = _currentPath[_currentIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            Time.deltaTime * _moveSpeed
        );

        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            _currentIndex++;

            if (_currentIndex >= _currentPath.Count)
            {
                _isMoving = false;
            }
        }
    }
    
    private void DebugPath(List<Transform> path, string label)
    {
        Debug.Log($"[{label}] 경로 개수: {path.Count}");

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == null)
            {
                Debug.LogError($"[{label}] {i}번째 null");
                continue;
            }

            Debug.Log($"[{label}] {i}번째: {path[i].name} / 위치: {path[i].position}");
        }
    }
}
    
