using System;
using System.Collections;
using UnityEngine;

public class CarHandler : MonoBehaviour
{
    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    Transform gameModel;

    // Max values
    float maxSteerVelocity = 2;
    float maxForwardVelocity = 30;
    float carMaxSpeedPercentage = 0;

    // Multiplayers
    float accelerationMultiplier = 3;
    float breakMultiplier = 15;
    float steeringMultiplier = 5;
    

    Vector2 input = Vector2.zero;

    [Header("SFX")]
    [SerializeField]
    AudioSource carEngineAS;

    [SerializeField]
    AnimationCurve carPitchAnimationCurve;

    [SerializeField]
    AudioSource carSkidAS;

    bool isPlayer = true;

    float carStartPositionZ;
    float distanceTravelled = 0;
    public float DistanceTravelled => distanceTravelled;

    //Events
    public event Action<CarHandler> OnPlayerCrashed;

    void Start()
    {
        isPlayer = CompareTag("Player");

        if (isPlayer)
            carEngineAS.Play();

        carStartPositionZ = transform.position.z;
    }

    void Update()
    {
        // rotate car model when turning
        // the more i increase rb.linearVelocity.x * 5 the faster its gonna turn
        gameModel.transform.rotation = Quaternion.Euler(0, rb.linearVelocity.x * 5, 0);

        UpdateCarAudio();

        // update distance travelled
        distanceTravelled = transform.position.z - carStartPositionZ;
    }

    private void FixedUpdate()
    {
        // apply acceleration
        if(input.y > 0)
        {
            Accelerate();
        } else
        {
            // slow down automatically if the player dosent do anything
            rb.linearDamping = 0.2f;
        }

        // apply brake
        if (input.y < 0)
        {
            Brake();
        }

        Steer();

        // dont let the car go backwards
        if(rb.linearVelocity.z <= 0)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    void Accelerate()
    {
        // do not slow down
        rb.linearDamping = 0;

        // stay withn the speed limit
        if(rb.linearVelocity.z >= maxForwardVelocity)
        {
            return;
        }

        rb.AddForce(rb.transform.forward * accelerationMultiplier * input.y);
    }

    void Brake()
    {
        // dont brake unless we are going forward
        if(rb.linearVelocity.z <= 0)
        {
            return;
        }

        rb.AddForce(rb.transform.forward * breakMultiplier * input.y);
    }

    void Steer()
    {
        if(Mathf.Abs(input.x) > 0)
        {
            // move car sideways atleast 5 units we get 100% steering
            // clamp speed between 0 or 1
            float speedBaseSteerLimit = rb.linearVelocity.z / 0.5f;
            speedBaseSteerLimit = Mathf.Clamp01(speedBaseSteerLimit);

            rb.AddForce(rb.transform.right * steeringMultiplier * input.x * speedBaseSteerLimit);

            // normalize x velocity
            float normalizedX = rb.linearVelocity.x / maxSteerVelocity;

            // ensure that we dont allow to get biffer then 1 in magnitued 
            // when pressing left or right
            normalizedX = Mathf.Clamp(normalizedX, -1.0f, 1.0f);

            // turn speed limit
            rb.linearVelocity = new Vector3(normalizedX * maxSteerVelocity, 0, rb.linearVelocity.z);
        } else
        {
            // auto center car
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0, 0, rb.linearVelocity.z), Time.fixedDeltaTime * 3);
        }
    }

    IEnumerator SlowDownTimeCO()
    {
        while (Time.timeScale > 0.2f)
        {
            Time.timeScale -= Time.deltaTime * 2;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        while (Time.timeScale <= 1.0f)
        {
            Time.timeScale += Time.deltaTime;

            yield return null;
        }

        Time.timeScale = 1.0f;
    }

    void UpdateCarAudio()
    {
        if (!isPlayer) return;

        carMaxSpeedPercentage = rb.linearVelocity.z / maxForwardVelocity;

        if (carEngineAS != null)
            carEngineAS.pitch = carPitchAnimationCurve.Evaluate(carMaxSpeedPercentage);

        if (carSkidAS == null) return;

        if (input.y < 0 && carMaxSpeedPercentage > 0.2f)
        {
            if (!carSkidAS.isPlaying) carSkidAS.Play();
            carSkidAS.volume = Mathf.Lerp(carSkidAS.volume, 1.0f, Time.deltaTime * 10);
        }
        else
        {
            carSkidAS.volume = Mathf.Lerp(carSkidAS.volume, 0, Time.deltaTime * 30);
        }
    }


    public void SetInput(Vector2 inputVector)
    {
        // no cheating between -1 and 1 the speed
        inputVector.Normalize();

        input = inputVector;
    }

    //Events
    private void OnCollisionEnter(Collision collision)
    {

        //Trigger event
        OnPlayerCrashed?.Invoke(this);

        StartCoroutine(SlowDownTimeCO());
    }
}
