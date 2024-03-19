using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


//Handles the projectiles
public class Projectile : NetworkBehaviour
{
    //Visual effect when projectile hits something
    public GameObject impactVFX;

    //Sound Effect for impact
    public List<AudioClip> impactSFX;

    //Tracking if the projectile has already collided.
    private bool collided;
    

    // Handles collision events for the projectile.
    void OnCollisionEnter (Collision co) 
    {
        // Checks collision is not with Bullet or Player and hasn't already collided.
        if (co.gameObject.tag != "Bullet" && co.gameObject.tag != "Player" && !collided)
        {
            collided = true;

            if (impactVFX != null)
            {
                ContactPoint contact = co.contacts[0];
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
                Vector3 pos = contact.point;

                var hitVFX = Instantiate(impactVFX, pos, rot) as GameObject;
                var num = Random.Range (0, impactSFX.Count);
                hitVFX.GetComponent<AudioSource>().PlayOneShot(impactSFX[num]);

                // Destroys the VFX after 2 seconds.
                Destroy (hitVFX, 2);
            }
            // Destroys the projectile upon collision.
            Destroy(gameObject);
        }
    }
}
