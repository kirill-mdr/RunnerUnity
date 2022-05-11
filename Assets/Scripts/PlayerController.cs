using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator animator;
    private Vector3 startGamePosition;
    private Quaternion startGameRotation;
    private float laneOffset;
    private float laneChangeSpeed = 15f;
    private Rigidbody rb;
    private float pointStart;
    private float pointFinish;
    private bool isMoving = false;
    private Coroutine movingCorourine;
    private float lastVectorX;
    private bool isJumping;
    public float jumpPower = 17;
    private float jumpGravity = -40;
    private float realGravity = -9.8f;
    void Start()
    {
        // animator = GetComponent<Animator>();
        laneOffset = MapGenerator.Instance.laneOffset;
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0.0f;
        startGamePosition = transform.position;
        startGameRotation = transform.rotation;
        SwipeManager.Instance.MoveEvent += MovePlayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && pointFinish > -laneOffset)
        {
            MoveHorizontal(-laneChangeSpeed);
        }
        if (Input.GetKeyDown(KeyCode.D) && pointFinish < laneOffset)
        {
            MoveHorizontal(laneChangeSpeed);
        }
        if (Input.GetKeyDown(KeyCode.W) && isJumping == false)
        {
            Jump();
        }
    }

    void MovePlayer(bool[] swipes)
    {
        if (swipes[(int)SwipeManager.Direction.Left] && pointFinish > -laneOffset)
        {
            MoveHorizontal(-laneChangeSpeed);
        }
        if (swipes[(int)SwipeManager.Direction.Right] && pointFinish < laneOffset)
        {
            MoveHorizontal(laneChangeSpeed);
        }
        if (swipes[(int)SwipeManager.Direction.Up] && isJumping == false)
        {
            Jump();
        }
    }

    private void Jump()
    {
        isJumping = true;
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        Physics.gravity = new Vector3(0, jumpGravity, 0);
        StartCoroutine(StopJumpCoroutine());
    }

    IEnumerator StopJumpCoroutine()
    {
        do
        {
            yield return new WaitForSeconds(0.02f);
        } while (rb.velocity.y != 0);
        isJumping = false;
        Physics.gravity = new Vector3(0, realGravity, 0);
    }
    


    void MoveHorizontal(float speed)
    {
        pointStart = pointFinish;
        pointFinish += Mathf.Sign(speed) * laneOffset;

        if (isMoving)
        {
            StopCoroutine(movingCorourine);
            isMoving = false;
        }
        movingCorourine = StartCoroutine(MoveCoroutine(speed));

    }

    IEnumerator MoveCoroutine(float vectorX)
    {
        isMoving = true;
        while (Mathf.Abs(pointStart - transform.position.x) < laneOffset)
        {
            yield return new WaitForFixedUpdate();

            rb.velocity = new Vector3(vectorX, rb.velocity.y, rb.velocity.z);
            lastVectorX = vectorX;
            float x = Mathf.Clamp(transform.position.x, Mathf.Min(pointStart, pointFinish), Mathf.Max(pointStart, pointFinish));
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }
        rb.velocity = Vector3.zero;
        transform.position = new Vector3(pointFinish, transform.position.y, transform.position.z);
        if (transform.position.y > 1)
        {
            rb.velocity = new Vector3(rb.velocity.x, -10, rb.velocity.z);
        }
        isMoving = false;
    }

    public void StartGame()
    {
        // animator.SetTrigger("");
        RoadGenerator.Instance.StartLevel();
    }
    public void ResetGame()
    {
        // animator.SetTrigger("");
        rb.velocity = Vector3.zero;
        pointStart = 0;
        pointFinish = 0;
        transform.position = startGamePosition;
        transform.rotation = startGameRotation;
        RoadGenerator.Instance.ResetLevel();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ramp"))
        {
            rb.constraints |= RigidbodyConstraints.FreezePositionZ;
            Debug.Log("ddd");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ramp"))
        {
            rb.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }

        if (collision.gameObject.CompareTag("NotLose"))
        {
            MoveHorizontal(-lastVectorX);
        }
        
        if (collision.gameObject.CompareTag("Lose"))
        {
            ResetGame();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("RampPlane"))
        {
            if (rb.velocity.x == 0 && isJumping == false)
            {
                rb.velocity = new Vector3(rb.velocity.x, -10, rb.velocity.z);
            }
        }
    }
}
