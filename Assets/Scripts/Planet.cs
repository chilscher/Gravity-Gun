using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour{

    private Player player;
    public float rotationSpeed = 1f; //the speed that the planet rotates around its own center, positive is counterclockwise
    public float coefficientOfFriction = 0.6f; //0 is no friction, 1 is equal to normal force
    
    void Start()    {
        player = GameObject.FindObjectOfType<Player>();
    }
    
    void Update()    {
        rotateSelf();
    }

    void OnMouseDown() { player.clickedPlanet(this);    }

    void rotateSelf() {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
    

    
}
