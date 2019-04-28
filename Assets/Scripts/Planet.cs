using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{

    private Player player;
    public float autoRotationSpeed = 1f; //the speed that the planet rotates around its own center, positive is counterclockwise
    //public Planet orbitingAround;
    //public float orbitalgSpeed = 1f; //the speed that the planet revolves around its 
    //public float orbitalEccentricity = 1f;
    public float coefficientOfFriction = 0.6f; //0 is no friction, 1 is equal to normal force

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        rotateSelf();
    }

    void OnMouseDown() { player.clickedPlanet(this);    }

    void rotateSelf() { transform.Rotate(Vector3.forward * autoRotationSpeed * Time.deltaTime);    }

    
}
