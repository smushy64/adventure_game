/**
 * Description:  Game Manager script
 * Author:       Alicia Amarilla (smushyaa@gmail.com)
 * File Created: May 23, 2023
*/
using System;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance { get; private set; }
    public InputState input_state { get; private set; } = new InputState();

    void Awake() {
        if( GameManager.instance != null && GameManager.instance != this ) {
            Destroy(this);
        } else {
            GameManager.instance = this;
        }
    }

    void Start() {
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        this.update_input_state();
    }

    void update_input_state() {

        // TODO(alicia): more sophisticated system :)

        int left    = Convert.ToInt32( Input.GetKey( KeyCode.A ) );
        int right   = Convert.ToInt32( Input.GetKey( KeyCode.D ) );
        int forward = Convert.ToInt32( Input.GetKey( KeyCode.W ) );
        int back    = Convert.ToInt32( Input.GetKey( KeyCode.S ) );

        this.input_state.menu_movement.x = ( right - left );
        this.input_state.menu_movement.y = ( forward - back );

        this.input_state.movement.x = (float)this.input_state.menu_movement.x;
        this.input_state.movement.z = (float)this.input_state.menu_movement.y;

        this.input_state.movement = this.input_state.movement.normalized;

        const int LEFT_MOUSE_BUTTON = 0;

        this.input_state.sprint    = Input.GetKey( KeyCode.LeftShift );
        this.input_state.interact  = Input.GetKey( KeyCode.E );
        this.input_state.shoot = Input.GetMouseButton( LEFT_MOUSE_BUTTON );

        this.input_state.jump_hold = Input.GetKey( KeyCode.Space );
        this.input_state.jump      = Input.GetKeyDown( KeyCode.Space );

        this.input_state.look.x = Input.GetAxis( "Mouse X" );
        this.input_state.look.y = Input.GetAxis( "Mouse Y" );

    }

};
