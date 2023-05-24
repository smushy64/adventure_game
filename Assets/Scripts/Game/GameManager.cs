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
    public bool is_cursor_locked { get; private set; } = true;

    public bool is_in_game { get; private set; } = true;

    public Action<int, int> on_screen_resize;

    public int screen_width  { get; private set; } = 0;
    public int screen_height { get; private set; } = 0;

    public int render_resolution_x { get; private set; } = 0;
    public int render_resolution_y { get; private set; } = 0;
    public float render_resolution_scale { get; private set; } = 1f;

    public FullScreenMode fullscreen_mode { get; private set; }

    void Awake() {
        if( GameManager.instance != null && GameManager.instance != this ) {
            Destroy(this);
            return;
        } else {
            GameManager.instance = this;
        }

        DontDestroyOnLoad(this.gameObject);

        screen_width    = Screen.width;
        screen_height   = Screen.height;

        const int MIN_RESOLUTION = 1;

        render_resolution_x = Mathf.Max((int)((float)screen_width  * render_resolution_scale), MIN_RESOLUTION);
        render_resolution_y = Mathf.Max((int)((float)screen_height * render_resolution_scale), MIN_RESOLUTION);

        fullscreen_mode = Screen.fullScreenMode;

    }

    void Start() {
        // TODO(alicia): temp!
        cursor_lock();

        if( on_screen_resize != null ) {
            on_screen_resize.Invoke( render_resolution_x, render_resolution_y );
        }
    }

    void Update() {
        this.update_input_state();
    }

    public void cursor_lock() {
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Locked;
        is_cursor_locked = true;
    }
    public void cursor_unlock() {
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
        is_cursor_locked = false;
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
