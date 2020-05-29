using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour{
    
    private Camera mainCamera;
    private Vector2 prevCameraPosition; // camera position at the last frame
    public float parallax = .1f; //the background texture scrolling is scaled down by this amount

    // Start is called before the first frame update
    void Start(){
        mainCamera = Camera.main;
        setPrevCameraPosition();
    }

    // Update is called once per frame
    void Update() {
        moveWithCamera();
        setPrevCameraPosition();
    }
    void setPrevCameraPosition() { prevCameraPosition = mainCamera.transform.position;    }

    void moveWithCamera() {
        //get change in camera position
        Vector2 currentCameraPos = mainCamera.transform.position;
        Vector2 cameraPosChange = currentCameraPos - prevCameraPosition;

        //move background to camera
        Vector2 pos = transform.position;
        transform.position = pos + cameraPosChange;

        //determine how much the background should scroll
        Vector2 scrollAmount = cameraPosChange * parallax;

        //scroll background
        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material mat = mr.material;
        Vector2 offset = mat.mainTextureOffset;
        offset += scrollAmount;
        mat.mainTextureOffset = offset;

    }
    
}
