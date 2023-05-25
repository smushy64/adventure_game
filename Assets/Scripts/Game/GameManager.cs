/**
 * Description:  Game Manager script
 * Author:       Alicia Amarilla (smushyaa@gmail.com)
 * File Created: May 23, 2023
*/
using System;
using UnityEngine;

public class GameOptions {
    public int window_width;
    public int window_height;

    public float resolution_scale;

    // ExclusiveFullScreen = 0,
    // FullScreenWindow    = 1,
    // MaximizedWindow     = 2,
    // Windowed            = 3
    public FullScreenMode fullscreen_mode;

    public bool invert_look_x;
    public bool invert_look_y;

    public float look_sensitivity_x;
    public float look_sensitivity_y;

    // default options
    public GameOptions() {
        window_width  = 1280;
        window_height = 720;

        resolution_scale = 1.0f;

        fullscreen_mode = FullScreenMode.Windowed;

        look_sensitivity_x = 1f;
        look_sensitivity_y = 1f;
    }

}

public class GameManager : MonoBehaviour {

    public Action<int, int> on_screen_resize;
    public Action on_scene_load;

    public static GameManager instance { get; private set; }
    public InputState input_state { get; private set; } = new InputState();
    public bool is_cursor_locked { get; private set; } = true;

    public bool is_in_game { get; private set; } = true;


    public int screen_width  { get; private set; } = 0;
    public int screen_height { get; private set; } = 0;

    public int render_resolution_x { get; private set; } = 0;
    public int render_resolution_y { get; private set; } = 0;
    public float render_resolution_scale { get; private set; } = 1f;

    public FullScreenMode fullscreen_mode { get; private set; }

    public void set_screen_size(
        int width, int height,
        FullScreenMode fullscreen_mode
    ) {
        screen_width  = width;
        screen_height = height;
        recalculate_render_resolution();

        this.fullscreen_mode = fullscreen_mode;

        Screen.SetResolution( width, height, this.fullscreen_mode );
        if( on_screen_resize != null ) {
            on_screen_resize.Invoke( render_resolution_x, render_resolution_y );
        }
    }

    public void set_screen_size( int width, int height ) {
        set_screen_size( width, height, fullscreen_mode );
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

    void OnEnable() {
        on_scene_load += scene_init;
    }
    void OnDisable() {
        on_scene_load -= scene_init;
    }

    void Awake() {

        if( GameManager.instance != null && GameManager.instance != this ) {
            Destroy(this);
            return;
        } else {
            GameManager.instance = this;
        }

        DontDestroyOnLoad(this.gameObject);

        #if UNITY_EDITOR
            GameOptions options = new GameOptions();
            IO.serialize_game_options( options );
        #else
            GameOptions options = IO.deserialize_game_options();
        #endif

        screen_width    = options.window_width;
        screen_height   = options.window_height;
        fullscreen_mode = options.fullscreen_mode;

    }

    void Start() {
        scene_init();

        // TODO(alicia): temp!
        cursor_lock();

    }

    // called every time a scene is loaded
    void scene_init() {
        set_screen_size(
            screen_width,
            screen_height,
            fullscreen_mode
        );
    }

    void Update() {
        // react to changing the resolution in editor
        #if UNITY_EDITOR
            if(
                screen_width != Screen.width ||
                screen_height != Screen.height
            ) {
                set_screen_size( Screen.width, Screen.height );
            }
        #endif
        this.update_input_state();

    }

    void recalculate_render_resolution() {
        const int MIN_RESOLUTION = 1;

        render_resolution_x = Mathf.Max((int)((float)screen_width  * render_resolution_scale), MIN_RESOLUTION);
        render_resolution_y = Mathf.Max((int)((float)screen_height * render_resolution_scale), MIN_RESOLUTION);
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
