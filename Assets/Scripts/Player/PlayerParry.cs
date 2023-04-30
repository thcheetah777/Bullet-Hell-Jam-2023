using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerParry : MonoBehaviour
{

    [SerializeField] private float parryAngleSpeed = 0.5f;
    [SerializeField] private float parryKnockback = 20;
    [SerializeField] private float parrySpeed = 5;
    [SerializeField] private float runDelay = 1;
    [SerializeField] private Transform arrowAnchor;

    private float parryInput;
    private float parryAngleInput;

    private bool isParrying;
    private Parryable currentParryable;
    private float parryAngle = 0;
    private float previousParryableGravity = 1;

    Rigidbody2D playerBody;
    PlayerMovement playerMovement;
    float originalGravityScale;

    void Start() {
        playerBody = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();

        originalGravityScale = playerBody.gravityScale;
        arrowAnchor.gameObject.SetActive(false);
    }

    void Update() {
        if (isParrying)
        {
            parryAngle -= parryAngleInput * parryAngleSpeed * Time.deltaTime;
            arrowAnchor.rotation = Quaternion.Euler(0, 0, parryAngle);
        }
    }

    private void StartParry() {
        isParrying = true;

        playerMovement.gravityEnabled = false;
        playerMovement.canRun = false;
        playerMovement.canJump = false;

        playerBody.velocity = Vector2.zero;

        arrowAnchor.gameObject.SetActive(true);
        arrowAnchor.position = currentParryable.transform.position;

        Vector2 direction = currentParryable.transform.position - transform.position;
        parryAngle = Quaternion.LookRotation(Vector3.forward, Quaternion.identity * direction).eulerAngles.z;

        if (currentParryable.TryGetComponent<Rigidbody2D>(out Rigidbody2D parryableBody))
        {
            previousParryableGravity = parryableBody.gravityScale;
            parryableBody.velocity = Vector2.zero;
            parryableBody.gravityScale = 0;
        }
    }

    private IEnumerator EndParry() {
        isParrying = false;

        playerMovement.gravityEnabled = true;
        playerMovement.canJump = true;

        arrowAnchor.gameObject.SetActive(false);

        Vector2 direction = Quaternion.Euler(0, 0, parryAngle) * Vector2.up;
        playerBody.velocity = direction.normalized;
        playerBody.AddForce(-direction.normalized * parryKnockback, ForceMode2D.Impulse);

        if (currentParryable.TryGetComponent<Rigidbody2D>(out Rigidbody2D parryableBody))
        {
            parryableBody.gravityScale = previousParryableGravity;
            parryableBody.AddForce(direction.normalized * parrySpeed, ForceMode2D.Impulse);
        }

        currentParryable.PlayerClose(false);
        currentParryable = null;

        yield return new WaitForSeconds(runDelay);

        playerMovement.canRun = true;
    }

    private Vector2 AngleToDirection(float angle) {
        return Quaternion.Euler(0, 0, angle) * Vector2.up;
    }

    void OnTriggerEnter2D(Collider2D trigger) {
        if (trigger.TryGetComponent<Parryable>(out Parryable parryable))
        {
            if (!isParrying)
            {
                parryable.PlayerClose(true);
                currentParryable = parryable;
            }
        }
    }

    void OnTriggerExit2D(Collider2D trigger) {
        if (trigger.TryGetComponent<Parryable>(out Parryable parryable))
        {
            if (parryable == currentParryable)
            {
                parryable.PlayerClose(false);
                currentParryable = null;
            }
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawRay(new Ray(transform.position, Quaternion.Euler(0, 0, parryAngle) * Vector2.up));
    }

    void OnParry(InputValue value) {
        parryInput = value.Get<float>();
        if (currentParryable != null)
        {
            if (parryInput > 0)
            {
                StartParry();
            } else
            {
                StartCoroutine(EndParry());
            }
        }
    }

    void OnMovement(InputValue value) {
        parryAngleInput = value.Get<float>();
    }

}
