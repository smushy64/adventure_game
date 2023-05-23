/**
 * Description:  Input structures
 * Author:       Alicia Amarilla (smushyaa@gmail.com)
 * File Created: May 23, 2023
*/
using UnityEngine;

public class InputState {
    public Vector3 movement;
    public Vector2 look;

    public bool jump;
    public bool jump_hold;

    public bool shoot;
    public bool interact;
    public bool sprint;

    public Vector2Int menu_movement;
    public bool menu_ok;
    public bool menu_cancel;

    public bool invert_look_x;
    public bool invert_look_y;

    public float look_sensitivity_x = 1f;
    public float look_sensitivity_y = 1f;
};
