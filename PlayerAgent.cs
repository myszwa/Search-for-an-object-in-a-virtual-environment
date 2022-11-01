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
    // add goAwayMaterial
    [SerializeField]
    private Material goAwayMaterial;

    [SerializeField]
    private Material failureMaterial;

    [SerializeField]
    private Material defualtMaterial;

    public int stepTimeout = 300;

    private float nextStepTimeout;

    public int MaxStep = 500000000;

    public static float GRID_MAX_SIZE = 40;

    public static double score(double distance)
    {
        return normal_distribution(distance / GRID_MAX_SIZE, 0, 0.4);
    }

    public static double normal_distribution(double x, double mi, double sigma)
    {
        return 1.0 / (sigma * Math.Sqrt(2 * Math.PI)) * Math.Pow(Math.E, -Math.Pow(x - mi, 2) / (2 * sigma * sigma));
    }

    #endregion

    #region Private Instance Variables

    private Rigidbody playerRigidbody;

    private Vector3 orginalPosition;

    private Vector3 lastPlayerPosition;

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
    }

    public override void OnEpisodeBegin()
    {
        transform.LookAt(target.transform);

        target.transform.localPosition = orginalTargetPosition;

        transform.localPosition = orginalPosition;

        transform.localPosition = new Vector3(orginalPosition.x, orginalPosition.y, Random.Range(22f, 24f));

        // nextStepTimeout = StepCount + stepTimeout; // we don't have to keep checkpoints


        // Measure the distance from target at start
        distanceAtStart = Vector3.Distance(transform.localPosition, orginalTargetPosition);

        // reset player velocity
        playerRigidbody.velocity = Vector3.zero;

        //lastPlayerPosition = transform.localPosition;
       
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

        //var lastDistanceFromTarget = Vector3.Distance(lastPlayerPosition, target.transform.localPosition);
        Debug.Log("distanceAtStart " + distanceAtStart);

        // Actions
        var vectorForce = new Vector3();

        vectorForce.x = actions.ContinuousActions[0];

        vectorForce.z = actions.ContinuousActions[1];

        playerRigidbody.AddForce(vectorForce * speed);
        //Debug.Log("currentDistanceFromTarget " + lastDistanceFromTarget);

        //lastPlayerPosition = transform.localPosition;
        var currentDistanceFromTarget = Vector3.Distance(transform.localPosition, target.transform.localPosition);
        Debug.Log("lastDistance " + currentDistanceFromTarget);

        // Reward function
        var reward = (float)score(currentDistanceFromTarget);
        // Set reward once
        SetReward(reward);

        //Debug.Log("reward "+ reward);

        // if (currentDistanceFromTarget < lastDistanceFromTarget) //good condition?
        if (currentDistanceFromTarget > distanceAtStart)
        {
            AddReward(-0.1f);
            StartCoroutine(SwapGroundMaterial(goAwayMaterial, 0.5f));
            EndEpisode();
            return;
        }

        if (currentDistanceFromTarget <= distanceRequired)
        {
            StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));
            EndEpisode();
            return;
        }

       // Punish the agent
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