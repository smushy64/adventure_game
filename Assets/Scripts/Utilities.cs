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
