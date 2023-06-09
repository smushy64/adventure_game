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

    public Action on_touch_ground;
    public Action on_leave_ground;

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

    [SerializeField, Min(0f)]
    float maxStairHeight = 1f;
    [SerializeField, Min(0f)]
    float stairJumpHeight = 1f;

    // NOTE(alicia): I can't think of a better name rn oops
    [SerializeField, Range(0f, 0.999999f)]
    float maxSlopeDot = 0.5f;

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

    #if UNITY_EDITOR

        [Header("Debug Only")]

        [SerializeField]
        bool drawGroundCheck = false;
        [SerializeField]
        bool drawSlopeRaycast = false;
        [SerializeField]
        bool drawStairRaycast = false;

    #endif

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
        on_touch_ground += reset_max_ground_velocity;
    }
    void OnDisable() {
        on_touch_ground -= reset_max_ground_velocity;
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

        if( is_grounded ) {

            float movement_sqr_mag = movement.sqrMagnitude;
            if( movement_sqr_mag > 0f ) {
                Vector3 movement_direction = movement / Mathf.Sqrt( movement_sqr_mag );
                handle_slopes_and_stairs( movement_direction );
            }

        } else {

            if( r3d.velocity.y < 0f || !is_holding_jump ) {
                r3d.AddForce( Vector3.down * extraGravity );
            }

        }
        is_grounded = ground_check();
        r3d.useGravity = !is_grounded;

        r3d.AddForce( movement );

        Vector2 lateral_velocity_vector = new Vector2( r3d.velocity.x, r3d.velocity.z );
        float velocity = lateral_velocity_vector.magnitude;

        const float MIN_VELOCITY = float.Epsilon;
        is_moving = velocity > MIN_VELOCITY;

        if( velocity >= max_velocity ) {

            const float CLAMP_THRESHOLD = 0.1f;
            // if the difference is less than a threshold,
            // just clamp down to max velocity
            if( velocity - max_velocity < CLAMP_THRESHOLD ) {
                lateral_velocity_vector = Vector2.ClampMagnitude( lateral_velocity_vector, max_velocity );
            } else {
                // instead of just clamping all the way down to max ground velocity
                // clamp to halfway between max and the current velocity so
                // the effect of the velocity suddenly dropping isn't as jarring
                float mid_point = Mathf.Lerp( max_velocity, velocity, 0.5f );
                lateral_velocity_vector = Vector2.ClampMagnitude( lateral_velocity_vector, mid_point );
            }

        }

        r3d.velocity = new Vector3( lateral_velocity_vector.x, r3d.velocity.y, lateral_velocity_vector.y );

        r3d.drag = is_trying_to_move ? 0f : (is_grounded ? stopDrag : 0f);

        transform.rotation        = Quaternion.AngleAxis( camera_yaw, Vector3.up );
        cameraPivot.localRotation = Quaternion.AngleAxis( camera_pitch, Vector3.right );

    }

    bool stair_check_lower = false;
    bool stair_check_upper = false;

    void handle_slopes_and_stairs( Vector3 movement_direction ) {
        
        // stairs
        stair_check_lower = false;
        stair_check_upper = false;

        float stair_check_length = 1.2f;

        Vector3 raycast_start_position = transform.position + (movement_direction * 0.25f);

        if(Physics.Raycast(
            raycast_start_position,
            movement_direction,
            out RaycastHit hit_info,
            stair_check_length,
            groundCheckLayer
        )) {
            stair_check_lower = true;
            if( !Physics.Raycast(
                raycast_start_position + (Vector3.up * maxStairHeight),
                movement_direction,
                stair_check_length + 0.1f,
                groundCheckLayer
            )) {
                stair_check_upper = true;
                movement += new Vector3( 0f, stairJumpHeight, 0f );
                is_grounded = true;
                return;
            }

            float movement_dot_surface_normal = Mathf.Abs( Vector3.Dot( movement_direction, hit_info.normal ) );
            if( movement_dot_surface_normal >= maxSlopeDot ) {
                float remaped_dot = MathfEx.remap( maxSlopeDot, 1.0f, 0.0f, 1.0f, movement_dot_surface_normal );
                movement -= remaped_dot * movement;
            }

        }
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
            position + ((-transform.right * collider_radius) * raycastInsetPercentage),
            Vector3.down,
            maxGroundCheckDistance,
            groundCheckLayer
        );
        bool hit_right = Physics.Raycast(
            position + ((transform.right * collider_radius) * raycastInsetPercentage),
            Vector3.down,
            maxGroundCheckDistance,
            groundCheckLayer
        );
        bool hit_forward = Physics.Raycast(
            position + ((transform.forward * collider_radius) * raycastInsetPercentage),
            Vector3.down,
            maxGroundCheckDistance,
            groundCheckLayer
        );
        bool hit_back = Physics.Raycast(
            position + ((-transform.forward * collider_radius) * raycastInsetPercentage),
            Vector3.down,
            maxGroundCheckDistance,
            groundCheckLayer
        );

        bool result = hit_center || hit_forward || hit_left || hit_right || hit_back;

        if( result != is_grounded ) {
            if( result ) {
                Utilities.safe_invoke( on_touch_ground );
            } else {
                Utilities.safe_invoke( on_leave_ground );
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

    void reset_max_ground_velocity() {
        max_velocity = is_trying_to_sprint ? maxGroundedSprintVelocity : maxGroundedWalkVelocity;
    }

    #if UNITY_EDITOR

        void OnDrawGizmosSelected() {
            if( drawGroundCheck ) {
                draw_ground_check();
            }

            if( drawSlopeRaycast ) {
                draw_slope_raycast();
            }

            if( drawStairRaycast ) {
                draw_stair_raycast();
            }

        }

        void draw_stair_raycast() {
            Gizmos.color = stair_check_lower ? Color.red : Color.green;
            Gizmos.DrawLine(
                transform.position,
                transform.position + movement.normalized
            );

            Gizmos.color = stair_check_upper ? Color.red : Color.green;
            Vector3 stair_check_position = transform.position + (Vector3.up * maxStairHeight);
            Gizmos.DrawLine(
                stair_check_position,
                stair_check_position + movement.normalized
            );
        }

        void draw_slope_raycast() {
            Gizmos.color = Color.blue;

            Vector3 line_position = transform.position;

            Gizmos.DrawLine(
                line_position,
                line_position + movement.normalized
            );
        }

        void draw_ground_check() {
            if( capsule_collider == null ) {
                capsule_collider = GetComponent<CapsuleCollider>();
                collider_radius = capsule_collider.radius;
            }

            Vector3 position = transform.position + (Vector3.up * groundCheckVerticalOffset);

            Gizmos.color = is_grounded ? Color.green : Color.red;

            Gizmos.DrawLine(
                position,
                position + (Vector3.down * maxGroundCheckDistance)
            );

            Gizmos.DrawLine(
                (position + ((transform.forward * collider_radius) * raycastInsetPercentage)),
                (position + ((transform.forward * collider_radius) * raycastInsetPercentage)) + (Vector3.down * maxGroundCheckDistance)
            );

            Gizmos.DrawLine(
                (position + ((-transform.forward * collider_radius) * raycastInsetPercentage)),
                (position + ((-transform.forward * collider_radius) * raycastInsetPercentage)) + (Vector3.down * maxGroundCheckDistance)
            );

            Gizmos.DrawLine(
                (position + ((transform.right * collider_radius) * raycastInsetPercentage)),
                (position + ((transform.right * collider_radius) * raycastInsetPercentage)) + (Vector3.down * maxGroundCheckDistance)
            );

            Gizmos.DrawLine(
                (position + ((-transform.right * collider_radius) * raycastInsetPercentage)),
                (position + ((-transform.right * collider_radius) * raycastInsetPercentage)) + (Vector3.down * maxGroundCheckDistance)
            );
        }

    #endif

}
