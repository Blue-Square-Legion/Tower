using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostWwiseEventSquidMovement : MonoBehaviour
{
    public AK.Wwise.Event Squid;
    // Start is called before the first frame update
    public void PlaySquid()
    {
        Squid.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
