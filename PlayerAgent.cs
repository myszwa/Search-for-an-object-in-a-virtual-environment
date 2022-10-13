using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PlayerAgent : Agent
{
    #region Expose Instance Variables
    [SerializeField]
    private float speed = 1.0f;

    [SerializeField]
    private GameObject target;

    [SerializeField]
    private float distanceRequired = 1.5f;

    [SerializeField]
    private MeshRenderer groundMeshRenderer;

    [SerializeField]
    private Material successMaterial;

    [SerializeField]
    private Material failureMaterial;

    [SerializeField]
    private Material defualtMaterial;

    public int stepTimeout = 300;

    private float nextStepTimeout;

    #endregion

    #region Private Instance Variables

    private Rigidbody playerRigidbody;

    private Vector3 orginalPosition;

    private Vector3 orginalTargetPosition;

    private float distanceAtStart;

    #endregion

    public override void Initialize()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        orginalPosition = transform.localPosition;
        orginalTargetPosition = target.transform.localPosition;
        MaxStep = 50000;
    }

    public override void OnEpisodeBegin()
    {
        transform.LookAt(target.transform);
        target.transform.localPosition = orginalTargetPosition;
        transform.localPosition = orginalPosition;
        transform.localPosition = new Vector3(orginalPosition.x, orginalPosition.y, Random.Range(22,24));
        nextStepTimeout = StepCount + stepTimeout;

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


        // Nagradzanie agenta

        AddReward(-1f / MaxStep);

        if (distanceFromTarget <= distanceRequired)
        {
            SetReward(1.0f);
            StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));
            EndEpisode();
            
        }
        // Karanie agenta
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