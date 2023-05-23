/**
 * Description:  Player Controller script
 * Author:       Alicia Amarilla (smushyaa@gmail.com)
 * File Created: May 23, 2023
*/
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public bool is_trying_to_move   { get; private set; }
    public bool is_trying_to_sprint { get; private set; }
    public bool is_holding_jump     { get; private set; }

    public bool is_moving    { get; private set; }
    public bool is_grounded  { get; private set; }
    public bool is_sprinting { get; private set; }

    public Action OnTouchGround;
    public Action OnLeaveGround;

    [Header("Movement")]

    [SerializeField]
    float maxGroundedWalkVelocity = 10f;
    [SerializeField]
    float maxGroundedSprintVelocity = 10f;

    [SerializeField]
    float groundedAcceleration = 10f;
    [SerializeField]
    float aerialAcceleration   = 0.5f;

    [SerializeField]
    float stopDrag = 100f;

    [Header("Camera")]

    [SerializeField]
    Transform cameraPivot;
    [SerializeField]
    float maxPitchAngle = 80f;
    [SerializeField, Min(1f)]
    float baseLookSensitivity = 50f;

    [Header("Physics")]

    [SerializeField]
    float jumpForce = 200f;
    [SerializeField]
    float extraGravity = 2f;
    [SerializeField]
    float maxGroundCheckDistance = 0.2f;
    [SerializeField]
    float groundCheckVerticalOffset = 0.1f;
    [SerializeField]
    LayerMask groundCheckLayer = 1 << 3;
    [SerializeField, Range(0f, 1f)]
    float raycastInsetPercentage = 0.8f;
    [SerializeField]
    float maxAerialVelocity = 500f;

    float collider_radius;

    float camera_pitch;
    float camera_yaw;

    Rigidbody r3d;
    CapsuleCollider capsule_collider;

    float acceleration;
    float max_velocity;

    Vector3 last_movement;
    Vector3 movement;

    void OnEnable() {
        OnTouchGround += on_touch_ground;
    }
    void OnDisable() {
        OnTouchGround -= on_touch_ground;
    }

    void Awake() {
        r3d = GetComponent<Rigidbody>();
        capsule_collider = GetComponent<CapsuleCollider>();

        Debug.Assert( cameraPivot, "Camera Pivot needs to be set on Player!" );

        is_grounded  = true;
        acceleration = groundedAcceleration;
        max_velocity = maxGroundedWalkVelocity;

        collider_radius = capsule_collider.radius;
    }

    void Update() {
        acceleration = is_grounded ? groundedAcceleration : aerialAcceleration;

        InputState input_state = GameManager.instance.input_state;
        is_holding_jump = input_state.jump_hold;

        if( input_state.jump && can_jump() ) {
            is_grounded = false;
            r3d.position = new Vector3( r3d.position.x, r3d.position.y + 0.5f, r3d.position.z );
            r3d.AddForce( Vector3.up * jumpForce, ForceMode.Impulse );
        }

        Vector3 current_movement = transform.rotation * input_state.movement * acceleration;
        movement = Vector3.Slerp( last_movement, current_movement, Time.deltaTime * 10f );
        float movement_magnitude = movement.magnitude;

        is_trying_to_move   = movement_magnitude != 0f;
        is_trying_to_sprint = input_state.sprint;
        is_sprinting        = is_trying_to_sprint && can_sprint();

        const float MAX_VELOCITY_UPDATE_SPEED = 10f;
        if( is_grounded ) {
            max_velocity = Mathf.Lerp(
                max_velocity,
                is_sprinting ? maxGroundedSprintVelocity : maxGroundedWalkVelocity,
                Time.deltaTime * MAX_VELOCITY_UPDATE_SPEED
            );
        } else {
            max_velocity = maxAerialVelocity;
        }

        camera_yaw   = (camera_yaw + input_state.look.x * baseLookSensitivity * (input_state.invert_look_x ? -1f :  1f) * Time.deltaTime) % 360f;
        camera_pitch += input_state.look.y * baseLookSensitivity * (input_state.invert_look_y ?  1f : -1f) * Time.deltaTime;

        camera_pitch = Mathf.Min( Mathf.Abs( camera_pitch ), maxPitchAngle ) * Mathf.Sign( camera_pitch );

        last_movement = current_movement;
    }

    void FixedUpdate() {
        is_grounded = ground_check();

        if( !is_grounded ) {
            if( r3d.velocity.y < 0f && !is_holding_jump ) {
                r3d.AddForce( Vector3.down * extraGravity );
            }
        }

        r3d.AddForce( movement );

        float velocity = r3d.velocity.magnitude;
        const float MIN_VELOCITY = float.Epsilon;
        is_moving = velocity > MIN_VELOCITY;
        if( velocity >= max_velocity ) {
            r3d.velocity = Vector3.ClampMagnitude( r3d.velocity, max_velocity );
        }

        r3d.drag = is_trying_to_move ? 0f : (is_grounded ? stopDrag : 0f);

        transform.rotation        = Quaternion.AngleAxis( camera_yaw, Vector3.up );
        cameraPivot.localRotation = Quaternion.AngleAxis( camera_pitch, Vector3.right );

        r3d.useGravity = !is_grounded;

    }

    bool ground_check() {

        Vector3 position = transform.position + (Vector3.up * groundCheckVerticalOffset);

        bool hit_center = Physics.Raycast(
            position,
            Vector3.down,
            maxGroundCheckDistance,
            groundCheckLayer
        );
        bool hit_left = Physics.Raycast(
            position + ((Vector3.left * collider_radius) * raycastInsetPercentage),
            Vector3.down,
            maxGroundCheckDistance,
            groundCheckLayer
        );
        bool hit_right = Physics.Raycast(
            position + ((Vector3.right * collider_radius) * raycastInsetPercentage),
            Vector3.down,
            maxGroundCheckDistance,
            groundCheckLayer
        );
        bool hit_forward = Physics.Raycast(
            position + ((Vector3.forward * collider_radius) * raycastInsetPercentage),
            Vector3.down,
            maxGroundCheckDistance,
            groundCheckLayer
        );
        bool hit_back = Physics.Raycast(
            position + ((Vector3.back * collider_radius) * raycastInsetPercentage),
            Vector3.down,
            maxGroundCheckDistance,
            groundCheckLayer
        );

        bool result = hit_center || hit_forward || hit_left || hit_right || hit_back;

        if( result != is_grounded ) {
            if( result ) {
                Utilities.safe_invoke( OnTouchGround );
            } else {
                Utilities.safe_invoke( OnLeaveGround );
            }
        }

        return result;
    }

    bool can_jump() {
        return is_grounded;
    }

    bool can_sprint() {
        return is_grounded;
    }

    void on_touch_ground() {
        max_velocity = is_trying_to_sprint ? maxGroundedSprintVelocity : maxGroundedWalkVelocity;
    }

    void OnDrawGizmosSelected() {
    }

    void draw_ground_check() {
        Vector3 position = transform.position + (Vector3.up * groundCheckVerticalOffset);

        Gizmos.color = is_grounded ? Color.green : Color.red;

        Gizmos.DrawLine(
            position,
            position + (Vector3.down * maxGroundCheckDistance)
        );

        Gizmos.DrawLine(
            (position + ((Vector3.forward * collider_radius) * raycastInsetPercentage)),
            (position + ((Vector3.forward * collider_radius) * raycastInsetPercentage)) + (Vector3.down * maxGroundCheckDistance)
        );

        Gizmos.DrawLine(
            (position + ((Vector3.back * collider_radius) * raycastInsetPercentage)),
            (position + ((Vector3.back * collider_radius) * raycastInsetPercentage)) + (Vector3.down * maxGroundCheckDistance)
        );

        Gizmos.DrawLine(
            (position + ((Vector3.right * collider_radius) * raycastInsetPercentage)),
            (position + ((Vector3.right * collider_radius) * raycastInsetPercentage)) + (Vector3.down * maxGroundCheckDistance)
        );

        Gizmos.DrawLine(
            (position + ((Vector3.left * collider_radius) * raycastInsetPercentage)),
            (position + ((Vector3.left * collider_radius) * raycastInsetPercentage)) + (Vector3.down * maxGroundCheckDistance)
        );
    }

}
