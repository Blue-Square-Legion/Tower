using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostWwiseEventRobotMovement : MonoBehaviour
{
    public AK.Wwise.Event RobotMovement;
    // Start is called before the first frame update
    public void PlayRobotMovement()
    {
        RobotMovement.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
