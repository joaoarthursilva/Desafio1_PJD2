using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private Animator _anim;
    private Vector3 _direction;
    public float speed;
    private bool _isFacingRight;

    private void Start()
    {
        _isFacingRight = true;
        transform.localEulerAngles = new Vector3(1, 1, 1);
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        GetInput();
        HandleAnim();
    }

    private void FixedUpdate()
    {
        if (_direction != Vector3.zero)
            Move();
    }

    private void GetInput()
    {
        _direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0);
    }

    private void Move()
    {
        _rb.AddForce(_direction * speed * 10);
    }

    private void HandleAnim()
    {
        if ((_isFacingRight && _direction.x < 0) || (!_isFacingRight && _direction.x > 0))
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
            _isFacingRight = !_isFacingRight;
        }

        // _anim.SetBool("running", _direction != Vector3.zero);
    }
}