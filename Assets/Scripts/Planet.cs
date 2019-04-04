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
    private float ignoreCollisionTimer = 0f;
    public float ignoreCollisionDuration = 1f; //number of seconds that a collision with the player is ignored for

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

    public void startIgnoreTimer() { ignoreCollisionTimer = ignoreCollisionDuration;    }

    public void countDownIgnoreTimer() { ignoreCollisionTimer -= Time.deltaTime;    }

    public void clearIgnoreTimer() { ignoreCollisionTimer = 0f;    }

    public void renewIgnoreTimer() { ignoreCollisionTimer = ignoreCollisionDuration;    }

    public bool hasIgnoreTimerFinished() { return (ignoreCollisionTimer <= 0);    }

    
}
