/**
 * Description:  Useful static functions
 * Author:       Alicia Amarilla (smushyaa@gmail.com)
 * File Created: May 23, 2023
*/
using System;
using UnityEngine;

static public class Utilities {

    static public void safe_invoke( Action action ) {
        if( action != null ) {
            action.Invoke();
        }
    }

};

static public class MathfEx {

    static public float remap(
        float in_min, float in_max,
        float out_min, float out_max,
        float v
    ) {
        float t = Mathf.InverseLerp( in_min, in_max, v );
        return Mathf.Lerp( out_min, out_max, t );
    }

};
