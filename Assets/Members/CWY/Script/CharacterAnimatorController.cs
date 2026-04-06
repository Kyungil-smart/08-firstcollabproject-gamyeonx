using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimatorController : MonoBehaviour
{
    [Header("ÂüÁ¶")]
    [SerializeField] private Animator _animator;

    private static readonly int SpeedHash = Animator.StringToHash("speed");
    private static readonly int OrientationHash = Animator.StringToHash("orientation");

    public enum Direction
    {
        South = 0,
        West = 1,
        East = 2,
        North = 3
    }

    private Direction _currentDirection = Direction.South;

    private void Reset()
    {
        _animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _animator.SetInteger(OrientationHash, (int)_currentDirection);
        _animator.SetFloat(SpeedHash, 0f);
    }

    public void UpdateAnimation(Vector2 moveInput)
    {
        if (_animator == null)
        {
            return;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            _currentDirection = GetDirection(moveInput);
            _animator.SetInteger(OrientationHash, (int)_currentDirection);
        }

        _animator.SetFloat(SpeedHash, isMoving ? 1f : 0f);
    }

    public void SetDirection(Direction direction)
    {
        if (_animator == null)
        {
            return;
        }

        _currentDirection = direction;
        _animator.SetInteger(OrientationHash, (int)_currentDirection);
    }

    private Direction GetDirection(Vector2 moveInput)
    {
        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            return moveInput.x > 0f ? Direction.East : Direction.West;
        }

        return moveInput.y > 0f ? Direction.North : Direction.South;
    }
}