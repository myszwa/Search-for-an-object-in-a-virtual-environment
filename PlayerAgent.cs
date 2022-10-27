using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;




public class PlayerAgent : Agent
{
    #region Expose Instance Variables
    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private GameObject target;

    [SerializeField]
    private float distanceRequired = 5.5f;

    [SerializeField]
    private MeshRenderer groundMeshRenderer;

    [SerializeField]
    private Material successMaterial;

    [SerializeField]
    private Material failureMaterial;

    [SerializeField]
    private Material defualtMaterial;

    public int stepTimeout = 300;

    // private float nextStepTimeout;

    public int MaxStep = 500000000;

    
    public static float GRID_MAX_SIZE = 50;

    public static double score(float distance)
    {
        return normal_distribution(distance / GRID_MAX_SIZE, 0, 0.2);
    }

    public static double normal_distribution(double x, double mi, double sigma)
    {
        return 1.0 / (sigma * Math.Sqrt(2 * Math.PI)) * Math.Pow(Math.E, -Math.Pow(x - mi, 2) / (2 * sigma * sigma));
    }

    #endregion

    #region Private Instance Variables

    private Rigidbody playerRigidbody;

    private Vector3 orginalPosition;

    private Vector3 orginalTargetPosition;

    private float distanceAtStart;

    #endregion

    public static void Main()
    {

    }

    public override void Initialize()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        orginalPosition = transform.localPosition;
        orginalTargetPosition = target.transform.localPosition;
        distanceAtStart = Vector3.Distance(transform.localPosition, orginalTargetPosition);
    }

    public override void OnEpisodeBegin()
    {
        transform.LookAt(target.transform);
        target.transform.localPosition = orginalTargetPosition;

        transform.localPosition = orginalPosition;
        transform.localPosition = new Vector3(orginalPosition.x, orginalPosition.y, Random.Range(22f, 24f));
        //nextStepTimeout = StepCount + stepTimeout;

        

        // reset player velocity
        playerRigidbody.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 3 obserwacje
        sensor.AddObservation(transform.localPosition);

        // 3 obserwacje
        sensor.AddObservation(target.transform.localPosition);

        // 1 obserwacja
        sensor.AddObservation(playerRigidbody.velocity.x);

        // 1 obserwacja
        sensor.AddObservation(playerRigidbody.velocity.z);

        // 1 obserwacja
        sensor.AddObservation(Vector3.Distance(transform.localPosition, target.transform.localPosition));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var vectorForce = new Vector3();
        vectorForce.x = actions.ContinuousActions[0];
        vectorForce.z = actions.ContinuousActions[1];

        playerRigidbody.AddForce(vectorForce * speed);

        var distanceFromTarget = Vector3.Distance(transform.localPosition, target.transform.localPosition);

        var mi = 0;

        var sigma = 0.2;
        //float y = distanceAtStart;


        // Debug.Log("distanceFromTarget " + distanceFromTarget);
        // Debug.Log("x  " + x);
        var atam = (float)score(distanceFromTarget);
        normal_distribution(atam, mi, sigma);
        // Debug.Log("reward " + atam);
         Debug.Log("reward " + atam);
        AddReward(Math.Max((-9f/200), atam));
        // Debug.Log("atam " + atam);
        // var reward = Math.Max(-1.0 / MaxStep, atam);

        // Debug.Log("reward " + atam);

        // Debug.Log("reward " + reward1);
        // Debug.Log("Continous [0] " + actions.ContinuousActions[0]);
        // Debug.Log("Continous [1] " + actions.ContinuousActions[1]);
        // Debug.Log(score(distanceFromTarget));

        // Nagradzanie agenta

        // AddReward(0.00001f / reward1);

        if (distanceFromTarget <= distanceRequired)
        {
             SetReward(1.0f);
             StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));
       
            EndEpisode();
            return;
        }
       // Karanie agenta

       // AddReward(-1f / MaxStep);
        if (transform.localPosition.y < -0.5)
        {
            SetReward(-1.0f);
            StartCoroutine(SwapGroundMaterial(failureMaterial, 0.5f));

            EndEpisode();

        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        continousActions[0] = Input.GetAxisRaw("Horizontal");
        continousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private IEnumerator SwapGroundMaterial(Material mat, float time)
    {
        groundMeshRenderer.material = mat;
        yield return new WaitForSeconds(time);
        groundMeshRenderer.material = defualtMaterial;
    }
}