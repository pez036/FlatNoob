using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField]
    private float moveForce = 5f;

    [SerializeField]
    private float jumpForce = 5f;

    [SerializeField]
    private float dashForce = 20f;
    
    private float horizontalInput;

    private Vector2 dashDirection;

    private Rigidbody2D body;
    private Animator animator;
    private string BOUND_TAG = "bound";

    private bool isGrounded = true;
    private int remainingJumpTime = 1;

    private bool isDashing = false;
    private int remainingDashTime = 1;

    // Start is called before the first frame update
    void Start() {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        PlayerMoveKeyboard();
        PlayerJump();
        LimitSpeed();
        PlayerDash();
    }

    void LimitSpeed() {
        Vector2 tempVel = body.velocity;
        tempVel.y = Mathf.Clamp(tempVel.y, -PlayerConstants.MAX_VERTICAL_SPEED, PlayerConstants.MAX_VERTICAL_SPEED);
        body.velocity = tempVel;
    }

    void PlayerMoveKeyboard() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        body.AddForce(new Vector2(horizontalInput, 0f) * moveForce, ForceMode2D.Impulse);
    }

    void PlayerJump() {
        if (Input.GetButtonDown("Jump") && remainingJumpTime > 0) {
            if (!isGrounded) {
                remainingJumpTime--;
            }
            body.velocity = new Vector2(0f, jumpForce);
        }
    }

    void PlayerDash() {
        if (Input.GetKeyDown(KeyCode.LeftShift) && remainingDashTime > 0) {
            remainingDashTime--;
            isDashing = true;
            dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            dashDirection.Normalize();
            StartCoroutine(EndDash());
        }

        if (isDashing) {
            body.velocity = dashDirection * dashForce;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag(BOUND_TAG)) {
            isGrounded = true;
            remainingJumpTime = PlayerConstants.MAX_JUMP_TIME;
            remainingDashTime = PlayerConstants.MAX_DASH_TIME;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag(BOUND_TAG)) {
            StartCoroutine(SetOffGround());
        }

    }

    /**
     * Allow a free jump within MAX_SEC_FREE_JUMP seconds leaving the ground
     */
    IEnumerator SetOffGround() {
        yield return new WaitForSeconds(PlayerConstants.MAX_SEC_FREE_JUMP);
        isGrounded = false;
    }

    /**
     * Slow down after dash
     */
    IEnumerator EndDash() {
        yield return new WaitForSeconds(PlayerConstants.DASH_DURATION);
        isDashing = false;
        body.velocity.Normalize();
        body.velocity = dashDirection * PlayerConstants.AFTER_DASH_SPEED;
    }
}
