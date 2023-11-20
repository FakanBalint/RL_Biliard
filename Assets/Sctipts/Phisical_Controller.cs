using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class Phisical_Controller : Agent, IObserver<BallEvent>
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float innerVelocityThreshold = 0.1f;

    public Team team = Team.none;

    private void Start()
    {
        BallEventManager.Instance.Subscribe(this);
    }

    private void OnDestroy()
    {
        BallEventManager.Instance.Unsubscribe(this);
    }

    private void SetTeam(object sender, BallEvent e)
    {
        if ((e.team == Team.none) || (e.team == Team.Both))
        {
            return;
        }
        else if (team == Team.Striped || team == Team.Full){
            return;
        }
        this.team = e.team;
    }

    // Implement the Heuristic method for manual control during training
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
        continuousActionsOut[1] = Input.GetAxisRaw("Vertical");
    }

    public override void OnEpisodeBegin()
    {
        // Reset relevant variables at the beginning of each episode
        team = Team.none;
        BallEventManager.Instance.Reset();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Extract direction and force from actions
        float direction = actions.ContinuousActions[0];
        float force = actions.ContinuousActions[1];

        ShootBall(direction, force);
    }


    void ShootBall(float direction, float force)
    {
        

        // Check the inner velocity
        if (rb.velocity.magnitude <= innerVelocityThreshold)
        {
            // Convert the direction from degrees to radians
            float radian = direction * Mathf.Deg2Rad;

            // Calculate the horizontal and vertical force components
            float horizontalForce = Mathf.Cos(radian) * force * 1000;
            float verticalForce = Mathf.Sin(radian) * force * 1000;

            // Apply the forces to shoot the ball
            rb.AddForce(new Vector3(horizontalForce, 0, verticalForce));
        }
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        IHitable hitable = collision.gameObject.GetComponent<IHitable>();
        if (hitable != null)
        {
            hitable.OnHit();
            //a little reward for hitting any balls
            AddReward(+0.01f);
        }
    }

    public void OnNotify(object sender, BallEvent e)
    {
        if (team == Team.none)
        {
            // Set the team of the Ai for the first ball that was hit down first
            SetTeam(sender, e);
        }

        if (e.team == this.team)
        {
            // Add reward for hitting the right balls
            AddReward(1f);
        }
        else if (e.team == Team.Both)
        {
            // Add penalty for hitting down the Black or White balls
            AddReward(-10f);
            Debug.Log("Episode end");
            EndEpisode();
        }
        else
        {
            // Add penalty for hitting the wrong team's balls
            AddReward(-1f);
        }
    }
}
