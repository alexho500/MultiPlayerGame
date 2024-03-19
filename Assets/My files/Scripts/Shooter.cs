using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Netcode;


//First Person Shooting Mechanics 
public class Shooter : NetworkBehaviour
{

    //References to camera, projectiles, and the effects 
    public Camera cam;
    public GameObject projectilePrefab;
    public GameObject muzzlePrefab;
    public float projectileSpeed = 10;

    //Fire points for hands 
    public Transform LHFirePoint;
    public Transform RHFirePoint;
    public float fireRate;

    //List of the shooting sound effects
    public List<AudioClip> shootSFX;
    [Space]
    [Header("SHAKE OPTIONS & PP")]

    //Post-processing and camera shake options 
    public Volume volume;
    public float chromaticGoal = 0.5f;
    public float chromaticRate = 0.1f;
    // For camera shake
    public CinemachineImpulseSource impulseSource;
    public float shakeDuration = 1;
    public float shakeAmplitude = 5;
    public float shakeFrequency = 2.5f;
    
    private ChromaticAberration chromatic;
    private AudioSource audioSource;
    private Animator anim;
    // Timer to manage firing rate
    private float timeToFire = 0;
    // Target point for shooting
    private Vector3 destination;
    // Controls which hand is firing.
    private bool leftHand;
    private bool chromaticIncrease;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        if (volume != null)
        {
            volume.profile.TryGet(out chromatic);
        }
    }

    void Start()
    {
        if (chromatic == null)
        {
            Debug.LogError("Chromatic Aberration component not found on the Volume.");
        }
    }


    void Update()
    {
        if (!IsOwner) return; 

        anim = GetComponent<Animator>();

        if(Input.GetButton("Fire1") && Time.time >= timeToFire) 
        {
            anim.SetBool("Idle", false);
            timeToFire = Time.time + 1 / fireRate;
            ShootProjectileServerRpc();     
        }

        if(Input.GetButtonUp("Fire1"))
        {
            anim.SetBool("Idle", true);
        } 
    }

    [ServerRpc]
    void ShootProjectileServerRpc()
    {
        
        ShootProjectile();
        ShootProjectileClientRpc();
    }

    [ClientRpc]
    void ShootProjectileClientRpc()
    {
        if (!IsOwner) 
        {
            ShootProjectile();
        }
    }

     // Main shooting logic, calculates direction and instantiates projectiles.
    void ShootProjectile ()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        RaycastHit hit;

        // Calculate hit point or default to a distant point
        if (Physics.Raycast(ray, out hit))
            destination = hit.point;
        else
            destination = ray.GetPoint(1000);
        
        // Alternate shooting between left and right hands.
        if(leftHand)
        {
            leftHand = false;
            anim.SetTrigger("Left");
            InstantiateProjectile(LHFirePoint);
        }
        else
        {
            leftHand = true;
            anim.SetTrigger("Right");
            InstantiateProjectile(RHFirePoint);
        }        
    }

    // Instantiates the projectile and muzzle flash, and plays shooting effects.
    void InstantiateProjectile(Transform handFirePoint)
    {

    ShakeCameraWithImpulse();
    StartCoroutine(ChromaticAberrationPunch());

    // Use projectilePrefab instead of projectile
    var projectileObj = Instantiate(projectilePrefab, handFirePoint.position, Quaternion.identity);
    var distance = destination - handFirePoint.position;
    projectileObj.GetComponent<Rigidbody>().velocity = distance.normalized * projectileSpeed;

    // Use muzzlePrefab instead of muzzle
    var muzzleObj = Instantiate(muzzlePrefab, handFirePoint.position, Quaternion.identity);
    // Destroy the muzzle effect after 2 seconds to clean up
    Destroy(muzzleObj, 2); 

    var time = distance.magnitude / projectileSpeed;
    iTween.PunchPosition(projectileObj, new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0), Random.Range(1, 4));

    var num = Random.Range(0, shootSFX.Count);
    audioSource.PlayOneShot(shootSFX[num]);
    }


    public void ShakeCameraWithImpulse()
    {
        impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = shakeDuration;
        impulseSource.m_ImpulseDefinition.m_AmplitudeGain = shakeAmplitude;
        impulseSource.m_ImpulseDefinition.m_FrequencyGain = shakeFrequency;
        impulseSource.GenerateImpulse();
    }

     // Gradually applies and then removes chromatic aberration effect.
    IEnumerator ChromaticAberrationPunch()
    {    
        if(!chromaticIncrease)
        {    
            chromaticIncrease = true;
            float amount = 0;
            while (amount < chromaticGoal)
            {
                amount += chromaticRate;
                chromatic.intensity.value = amount;
                yield return new WaitForSeconds (0.05f);
            }
            while (amount > 0)
            {
                amount -= chromaticRate;
                chromatic.intensity.value = amount;
                yield return new WaitForSeconds (0.05f);
            }
            chromaticIncrease = false;
        }
    }

}
