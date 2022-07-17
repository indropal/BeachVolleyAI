using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolleyRaycast : MonoBehaviour
{

    // crate Layer Mask to trigger the Raycast for specific layer
    public LayerMask l_mask; // default value set as 'Ball_land'

    public GameObject marker_object;

    // instantiating volleyball raycast
    private Ray volley_ray;
    private Rigidbody volley_body;
    private Vector3 ball_velo;
    private RaycastHit raycast_hit_info;

    void FixedUpdate()
    {
        /*
         Why 'FixedUpdate()' is used instead of 'Update()' ? 
            -> FixedUpdate is more consistent than Update method in terms of intervals of consecutive calls.
            -> Game Physics related updates for Rigid Bodies for eaxmple; should be handled in fixed update rather than update method.
        */

        volley_body = GetComponent<Rigidbody>();
        ball_velo = volley_body.velocity;

        // create a raycast in the direction of movement of the ball
        volley_ray = new Ray(transform.position, ball_velo); // transform.forward ? ~ not necessary

        /*
           Create a Ray with a set direction with length of 100 units
           - 'l_mask' being the Layer mask which should interact with the Raycast

           - 'QueryTriggerInteraction' has 3 methods 'Ignore', 'UseGlobal' & 'Collide'
                -> 'Ignore' being doing nothing to the ray even if it collides with a specified layer mask
                -> 'Collide' describe collision behaviors
        */
        if ( Physics.Raycast(volley_ray, out raycast_hit_info, 100, l_mask) )
        {
            // For any collisons detected, Draw a line (in red) in the Editor ~ doesn't appear in the game preview
            Debug.DrawLine(volley_ray.origin, raycast_hit_info.point, Color.red); // Start of ray: origin | End of ray: point (infinite)
            
            // Log the name of the game object the Ball collided with
            // Debug.Log(raycast_hit_info.collider.gameObject.name);
            // Debug.Log(raycast_hit_info.point); // 'point' attribute is where the ray hits the Layer / trigger object

            Vector3 hit_loc = new Vector3(raycast_hit_info.point.x, raycast_hit_info.point.y + 0.75f, raycast_hit_info.point.z);

            // instantiate the marker object at the point of raycast collision - with no rotation
            GameObject clone_mark = (GameObject)Instantiate(marker_object, hit_loc, Quaternion.identity);
           
            Destroy(clone_mark, 0.05f);
        }
        else
        {
            // for no collisions detected, just draw a green line (visible only in Editor not in Game preview)
            Debug.DrawLine(volley_ray.origin, volley_ray.origin + volley_ray.direction * 100, Color.green);
        }


    }
}
