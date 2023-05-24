/**
 * Description:  Camera Render Texture resizer
 * Author:       Alicia Amarilla (smushyaa@gmail.com)
 * File Created: May 24, 2023
*/
using UnityEngine;

public class CameraRT : MonoBehaviour {
    [SerializeField]
    RenderTexture mainCameraRenderTexture;
    [SerializeField]
    RenderTexture firstPersonRenderTexture;

    void OnEnable() {
        GameManager.instance.on_screen_resize += resize_camera_render_texture;
    }
    void OnDisable() {
        GameManager.instance.on_screen_resize -= resize_camera_render_texture;
    }

    public void resize_camera_render_texture( int width, int height ) {

        Debug.Log( "Screen resolution changed. Resizing camera render textures to " + width + "x" + height );

        mainCameraRenderTexture.Release();
        firstPersonRenderTexture.Release();

        mainCameraRenderTexture.width  = width;
        mainCameraRenderTexture.height = height;

        firstPersonRenderTexture.width  = width;
        firstPersonRenderTexture.height = height;

        mainCameraRenderTexture.Create();
        firstPersonRenderTexture.Create();
    }

}
