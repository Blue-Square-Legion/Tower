using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostWwiseEvent : MonoBehaviour
{
    public AK.Wwise.Event RobotFootsteps;
    // Start is called before the first frame update
    public void PlayRobotFootsteps()
    {
        RobotFootsteps.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
