using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections;

public class CrossTheRoadAgent : Agent
{
    [SerializeField]
    private float speed = 5.0f;

    [SerializeField, Tooltip("This is the offset amount from the local agent position the agent will move on every step")]
    private float stepAmount = 5.0f;

    [SerializeField]
    private TextMeshProUGUI rewardValue = null;

    [SerializeField]
    private TextMeshProUGUI episodesValue = null;

    [SerializeField]
    private TextMeshProUGUI stepValue = null;

    [SerializeField]
    private Material successMaterial;

    [SerializeField]
    private Material failureMaterial;

    private CrossTheRoadGoal goal = null;

    private float overallReward = 0;

    private float overallSteps = 0;

    private Vector3 moveTo = Vector3.zero;

    private Vector3 originalPosition = Vector3.zero;

    private Rigidbody agentRigidbody;

    private bool moveInProgress = false;

    private int direction = 0;

    private float[] spawnPositionsX = { 0.01f, 5.01f, 10.01f, -5.01f, -10.01f };

    public enum MoveToDirection
    { 
        Idle,
        Left,
        Right,
        Forward
    }

    private MoveToDirection moveToDirection = MoveToDirection.Idle;

    // void Awake()
    // {
    //     goal = transform.parent.GetComponentInChildren<CrossTheRoadGoal>();
    //     originalPosition = transform.localPosition;
    //     agentRigidbody = GetComponent<Rigidbody>();
    // }
    void Awake()
    {
        goal = transform.parent.GetComponentInChildren<CrossTheRoadGoal>();
        originalPosition = new Vector3(0f, transform.localPosition.y, transform.localPosition.z);
        agentRigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Randomly select a spawn position
        float randomX = spawnPositionsX[Random.Range(0, spawnPositionsX.Length)];
        
        // Set the new spawn position
        Vector3 newSpawnPosition = new Vector3(randomX, originalPosition.y, originalPosition.z);
        
        // Update the agent's position and moveTo
        transform.localPosition = moveTo = newSpawnPosition;
        transform.localRotation = Quaternion.identity;
        agentRigidbody.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 3 observations - x, y, z
        sensor.AddObservation(transform.localPosition);

        // 3 observations - x, y, z
        sensor.AddObservation(goal.transform.localPosition);
    }

    void Update()
    {
        if (!moveInProgress)
            return;

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, moveTo, Time.deltaTime * speed);

        if (Vector3.Distance(transform.localPosition, moveTo) <= 0.00001f)
        {
            moveInProgress = false;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (moveInProgress)
            return;

        direction = actionBuffers.DiscreteActions[0];

        // Check if the agent is on the road
        bool isOnRoad = transform.localPosition.z > 5;

        if (isOnRoad)
        {
            // On the road, only allow forward movement or idle
            switch (direction)
            {
                case 0: // idle
                    moveTo = transform.localPosition;
                    moveToDirection = MoveToDirection.Idle;
                    break;
                case 1: // left
                case 2: // right
                    // Ignore left and right movements
                    moveTo = transform.localPosition;
                    moveToDirection = MoveToDirection.Idle;
                    break;
                case 3: // forward
                    moveTo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + stepAmount);
                    moveToDirection = MoveToDirection.Forward;
                    moveInProgress = true;
                    break;
            }
        }
        else
        {
            // Off the road, allow all movements
            switch (direction)
            {
                case 0: // idle
                    moveTo = transform.localPosition;
                    moveToDirection = MoveToDirection.Idle;
                    break;
                case 1: // left
                    moveTo = new Vector3(transform.localPosition.x - stepAmount, transform.localPosition.y, transform.localPosition.z);
                    moveToDirection = MoveToDirection.Left;
                    moveInProgress = true;
                    break;
                case 2: // right
                    moveTo = new Vector3(transform.localPosition.x + stepAmount, transform.localPosition.y, transform.localPosition.z);
                    moveToDirection = MoveToDirection.Right;
                    moveInProgress = true;
                    break;
                case 3: // forward
                    moveTo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + stepAmount);
                    moveToDirection = MoveToDirection.Forward;
                    moveInProgress = true;
                    break;
            }
        }
    }

    public void GivePoints()
    {
        AddReward(1.0f);
        
        UpdateStats();

        EndEpisode();

        StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));
    }

    public void TakeAwayPoints()
    {
        AddReward(-0.025f);
        
        UpdateStats();
     
        EndEpisode();

        StartCoroutine(SwapGroundMaterial(failureMaterial, 0.5f));
    }

    private void UpdateStats()
    {
        overallReward += GetCumulativeReward();
        overallSteps += StepCount;
        rewardValue.text = $"{overallReward.ToString("F2")}";
        episodesValue.text = $"{CompletedEpisodes}";
        stepValue.text = $"{overallSteps}";
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //idle
        discreteActionsOut[0] = 0;

        //move left
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 1;
        }

        //move right
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 2;
        }

        //move forward
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 3;
        }
    }

    private IEnumerator SwapGroundMaterial(Material material, float duration)
    {
        // Implement this method based on your requirements
        yield return new WaitForSeconds(duration);
    }
}



// using TMPro;
// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
// using Unity.MLAgents.Sensors;
// using UnityEngine;
// using System.Collections;
// public class CrossTheRoadAgent : Agent
// {
//     [SerializeField]
//     private float trainingSpeed = 25.0f;
//     [SerializeField]
//     private float deployedSpeed = 15.0f;  // Increased speed for deployed model
//     [SerializeField, Tooltip("This is the offset amount from the local agent position the agent will move on every step")]
//     private float stepAmount = 1.0f;
//     [SerializeField]
//     private TextMeshProUGUI rewardValue = null;
//     [SerializeField]
//     private TextMeshProUGUI episodesValue = null;
//     [SerializeField]
//     private TextMeshProUGUI stepValue = null;
//     [SerializeField]
//     private Material successMaterial;
//     [SerializeField]
//     private Material failureMaterial;
//     [SerializeField]
//     private Renderer groundRenderer;
    


//     private CrossTheRoadGoal goal = null;
//     private float overallReward = 0;
//     private float overallSteps = 0;
//     private Vector3 moveTo = Vector3.zero;
//     private Vector3 originalPosition = Vector3.zero;
//     private Rigidbody agentRigidbody;
//     private bool moveInProgress = false;
//     private int direction = 0;

//     private float[] spawnPositionsX = { 0f, 5f, 10f, -5f, -10f };
//     private float currentSpeed;

//     public enum MoveToDirection
//     { 
//         Idle,
//         Left,
//         Right,
//         Forward
//     }

//     private MoveToDirection moveToDirection = MoveToDirection.Idle;

//     void Awake()
//     {
//         goal = transform.parent.GetComponentInChildren<CrossTheRoadGoal>();
//         originalPosition = new Vector3(0f, transform.localPosition.y, transform.localPosition.z);
//         agentRigidbody = GetComponent<Rigidbody>();
//         currentSpeed = Application.isEditor ? trainingSpeed : deployedSpeed;
//     }

//     public override void OnEpisodeBegin()
//     {
//         float randomX = spawnPositionsX[Random.Range(0, spawnPositionsX.Length)];
//         Vector3 newSpawnPosition = new Vector3(randomX, originalPosition.y, originalPosition.z);
//         transform.localPosition = moveTo = newSpawnPosition;
//         transform.localRotation = Quaternion.identity;
//         agentRigidbody.velocity = Vector3.zero;
//     }

//     // public override void CollectObservations(VectorSensor sensor)
//     // {
//     //     sensor.AddObservation(transform.localPosition);
//     //     sensor.AddObservation(goal.transform.localPosition);

//     //     // Add observations for nearby cars and trees
//     //     Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 5f);
//     //     foreach (Collider col in nearbyObjects)
//     //     {
//     //         if (col.CompareTag("car") || col.CompareTag("tree"))
//     //         {
//     //             sensor.AddObservation(col.transform.position - transform.position);
//     //         }
//     //     }
//     // }


//     public override void CollectObservations(VectorSensor sensor)
//     {
//         sensor.AddObservation(transform.localPosition);
//         sensor.AddObservation(goal.transform.localPosition);

//         // Add a fixed number of observations for nearby cars and trees
//         int maxNearbyObjects = 10;
//         Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 5f);
//         int count = 0;

//         foreach (Collider col in nearbyObjects)
//         {
//             if (count >= maxNearbyObjects) break;
//             if (col.CompareTag("car") || col.CompareTag("tree"))
//             {
//                 sensor.AddObservation(col.transform.position - transform.position);
//                 count++;
//             }
//         }

//         // Fill the rest with zero observations if less than maxNearbyObjects
//         for (int i = count; i < maxNearbyObjects; i++)
//         {
//             sensor.AddObservation(Vector3.zero);
//         }
//     }


//     void Update()
//     {
//         if (!moveInProgress)
//             return;

//         transform.localPosition = Vector3.MoveTowards(transform.localPosition, moveTo, Time.deltaTime * currentSpeed);

//         if (Vector3.Distance(transform.localPosition, moveTo) <= 0.00001f)
//         {
//             moveInProgress = false;
//         }
//     }

//     public override void OnActionReceived(ActionBuffers actionBuffers)
//     {
//         if (moveInProgress)
//             return;

//         direction = actionBuffers.DiscreteActions[0];

//         switch (direction)
//         {
//             case 0: // idle
//                 moveTo = transform.localPosition;
//                 moveToDirection = MoveToDirection.Idle;
//                 break;
//             case 1: // left
//                 moveTo = new Vector3(transform.localPosition.x - stepAmount, transform.localPosition.y, transform.localPosition.z);
//                 moveToDirection = MoveToDirection.Left;
//                 moveInProgress = true;
//                 break;
//             case 2: // right
//                 moveTo = new Vector3(transform.localPosition.x + stepAmount, transform.localPosition.y, transform.localPosition.z);
//                 moveToDirection = MoveToDirection.Right;
//                 moveInProgress = true;
//                 break;
//             case 3: // forward
//                 moveTo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + stepAmount);
//                 moveToDirection = MoveToDirection.Forward;
//                 moveInProgress = true;
//                 break;
//         }
//         if (direction == 3) // forward
//         {
//             AddReward(0.01f); // Small reward for moving forward
//         }
//     }

//     public void GivePoints()
//     {
//         AddReward(1.0f);
//         UpdateStats();
//         EndEpisode();
//         StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));
//     }

//     public void TakeAwayPoints()
//     {
//         AddReward(-0.1f);
//         UpdateStats();
//         EndEpisode();
//         StartCoroutine(SwapGroundMaterial(failureMaterial, 0.5f));
//     }

//     private void UpdateStats()
//     {
//         overallReward += this.GetCumulativeReward();
//         overallSteps += this.StepCount;
//         rewardValue.text = $"{overallReward.ToString("F2")}";
//         episodesValue.text = $"{this.CompletedEpisodes}";
//         stepValue.text = $"{overallSteps}";
//     }

//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         var discreteActionsOut = actionsOut.DiscreteActions;
//         //idle
//         discreteActionsOut[0] = 0;

//         //move left
//         if (Input.GetKeyDown(KeyCode.LeftArrow))
//         {
//             discreteActionsOut[0] = 1;
//         }

//         //move right
//         if (Input.GetKeyDown(KeyCode.RightArrow))
//         {
//             discreteActionsOut[0] = 2;
//         }

//         //move forward
//         if (Input.GetKeyDown(KeyCode.UpArrow))
//         {
//             discreteActionsOut[0] = 3;
//         }
//     }

//     private IEnumerator SwapGroundMaterial(Material material, float duration)
//     {
//         if (groundRenderer != null)
//         {
//             Material originalMaterial = groundRenderer.material;
//             groundRenderer.material = material;
//             yield return new WaitForSeconds(duration);
//             groundRenderer.material = originalMaterial;
//         }
//         else
//         {
//             Debug.LogWarning("Ground renderer is not assigned!");
//             yield return null;
//         }
//     }

//     void OnDrawGizmos()
//     {
//         if (!Application.isPlaying) return;

//         // Visualize movement direction
//         Gizmos.color = Color.blue;
//         Gizmos.DrawLine(transform.position, moveTo);

//         // Visualize ray perceptions
//         RayPerceptionSensorComponent3D rayPerception = GetComponent<RayPerceptionSensorComponent3D>();
//         if (rayPerception != null)
//         {
//             float[] rayAngles = GetRayAngles(rayPerception);
//             for (int i = 0; i < rayAngles.Length; i++)
//             {
//                 Vector3 direction = Quaternion.Euler(0, rayAngles[i], 0) * transform.forward;
//                 RaycastHit hit;
//                 if (Physics.Raycast(transform.position, direction, out hit, rayPerception.RayLength))
//                 {
//                     Gizmos.color = Color.red;
//                     Gizmos.DrawLine(transform.position, hit.point);
//                 }
//                 else
//                 {
//                     Gizmos.color = Color.green;
//                     Gizmos.DrawRay(transform.position, direction * rayPerception.RayLength);
//                 }
//             }
//         }
//     }

//     private float[] GetRayAngles(RayPerceptionSensorComponent3D rayPerception)
//     {
//         float[] angles = new float[rayPerception.RaysPerDirection * 2 + 1];
//         float increment = rayPerception.MaxRayDegrees / rayPerception.RaysPerDirection;
        
//         for (int i = 0; i < rayPerception.RaysPerDirection; i++)
//         {
//             angles[i] = -rayPerception.MaxRayDegrees + i * increment;
//             angles[angles.Length - 1 - i] = rayPerception.MaxRayDegrees - i * increment;
//         }
//         angles[rayPerception.RaysPerDirection] = 0f;

//         return angles;
//     }
// }



// using TMPro;
// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
// using Unity.MLAgents.Sensors;
// using UnityEngine;
// using System.Collections;

// public class CrossTheRoadAgent : Agent
// {
//     [SerializeField] private float trainingSpeed = 25.0f;
//     [SerializeField] private float deployedSpeed = 15.0f;
//     [SerializeField, Tooltip("This is the offset amount from the local agent position the agent will move on every step")]
//     private float stepAmount = 1.0f;
//     [SerializeField] private TextMeshProUGUI rewardValue = null;
//     [SerializeField] private TextMeshProUGUI episodesValue = null;
//     [SerializeField] private TextMeshProUGUI stepValue = null;
//     [SerializeField] private Material successMaterial;
//     [SerializeField] private Material failureMaterial;
//     [SerializeField] private Renderer groundRenderer;
//     [SerializeField] private Vector3 crosswalkPosition = new Vector3(5.01f, 0.5f, 0f);
//     [SerializeField] private float roadWidth = 10f;

//     private CrossTheRoadGoal goal = null;
//     private float overallReward = 0;
//     private float overallSteps = 0;
//     private Vector3 moveTo = Vector3.zero;
//     private Rigidbody agentRigidbody;
//     private bool moveInProgress = false;
//     private int direction = 0;
//     private float currentSpeed;
//     private float[] spawnPositionsX = { 0.01f, 5.01f, 10.01f, -5.01f, -10.01f };


//     private enum AgentState { MovingToCrosswalk, CrossingRoad }
//     private AgentState currentState = AgentState.MovingToCrosswalk;

//     public enum MoveToDirection
//     { 
//         Idle,
//         Left,
//         Right,
//         Forward,
//         Backward
//     }

//     private MoveToDirection moveToDirection = MoveToDirection.Idle;

//     void Awake()
//     {
//         goal = transform.parent.GetComponentInChildren<CrossTheRoadGoal>();
//         agentRigidbody = GetComponent<Rigidbody>();
//         currentSpeed = Application.isEditor ? trainingSpeed : deployedSpeed;
//     }

//     public override void OnEpisodeBegin()
//     {
//         // Randomly select a spawn position
//         float randomX = spawnPositionsX[Random.Range(0, spawnPositionsX.Length)];
        
//         // Set the new spawn position
//         Vector3 newSpawnPosition = new Vector3(randomX, 0.05f, 0f);
        
//         transform.localPosition = moveTo = newSpawnPosition;
//         transform.localRotation = Quaternion.identity;
//         agentRigidbody.velocity = Vector3.zero;
//         currentState = AgentState.MovingToCrosswalk;
//     }

//     public override void CollectObservations(VectorSensor sensor)
//     {
//         sensor.AddObservation(transform.localPosition);
//         sensor.AddObservation(crosswalkPosition);
//         sensor.AddObservation(goal.transform.localPosition);
//         sensor.AddObservation((int)currentState);

//         int maxNearbyObjects = 10;
//         Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 5f);
//         int count = 0;

//         foreach (Collider col in nearbyObjects)
//         {
//             if (count >= maxNearbyObjects) break;
//             if (col.CompareTag("car") || col.CompareTag("tree"))
//             {
//                 sensor.AddObservation(col.transform.position - transform.position);
//                 count++;
//             }
//         }

//         for (int i = count; i < maxNearbyObjects; i++)
//         {
//             sensor.AddObservation(Vector3.zero);
//         }
//     }

//     void Update()
//     {
//         if (!moveInProgress)
//             return;

//         transform.localPosition = Vector3.MoveTowards(transform.localPosition, moveTo, Time.deltaTime * currentSpeed);

//         if (Vector3.Distance(transform.localPosition, moveTo) <= 0.00001f)
//         {
//             moveInProgress = false;
//         }
//     }

//     public override void OnActionReceived(ActionBuffers actionBuffers)
//     {
//         if (moveInProgress)
//             return;

//         direction = actionBuffers.DiscreteActions[0];
//         bool isOnCrosswalk = Vector3.Distance(transform.localPosition, crosswalkPosition) < 0.1f;

//         switch (currentState)
//         {
//             case AgentState.MovingToCrosswalk:
//                 MoveToCrosswalk(direction, isOnCrosswalk);
//                 break;
//             case AgentState.CrossingRoad:
//                 CrossRoad(direction, isOnCrosswalk);
//                 break;
//         }

//         AddReward(-0.01f);
//     }

//     private void MoveToCrosswalk(int direction, bool isOnCrosswalk)
//     {
//         if (isOnCrosswalk)
//         {
//             currentState = AgentState.CrossingRoad;
//             AddReward(0.5f);
//             return;
//         }

//         switch (direction)
//         {
//             case 1: // left
//                 moveTo = new Vector3(transform.localPosition.x - stepAmount, transform.localPosition.y, transform.localPosition.z);
//                 moveToDirection = MoveToDirection.Left;
//                 break;
//             case 2: // right
//                 moveTo = new Vector3(transform.localPosition.x + stepAmount, transform.localPosition.y, transform.localPosition.z);
//                 moveToDirection = MoveToDirection.Right;
//                 break;
//             case 3: // forward
//                 moveTo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + stepAmount);
//                 moveToDirection = MoveToDirection.Forward;
//                 break;
//             default:
//                 moveToDirection = MoveToDirection.Idle;
//                 return;
//         }

//         moveInProgress = true;
//     }

//     private void CrossRoad(int direction, bool isOnCrosswalk)
//     {
//         if (!isOnCrosswalk)
//         {
//             AddReward(-0.5f);
//             EndEpisode();
//             return;
//         }

//         switch (direction)
//         {
//             case 3: // forward
//                 moveTo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + stepAmount);
//                 moveToDirection = MoveToDirection.Forward;
//                 moveInProgress = true;
//                 break;
//             case 4: // backward
//                 moveTo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - stepAmount);
//                 moveToDirection = MoveToDirection.Backward;
//                 moveInProgress = true;
//                 break;
//             default:
//                 moveToDirection = MoveToDirection.Idle;
//                 return;
//         }

//         AddReward(0.01f);
//     }

//     public void GivePoints()
//     {
//         AddReward(1.0f);
//         UpdateStats();
//         EndEpisode();
//         StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));
//     }

//     public void TakeAwayPoints()
//     {
//         AddReward(-0.1f);
//         UpdateStats();
//         EndEpisode();
//         StartCoroutine(SwapGroundMaterial(failureMaterial, 0.5f));
//     }

//     private void UpdateStats()
//     {
//         overallReward += this.GetCumulativeReward();
//         overallSteps += this.StepCount;
//         rewardValue.text = $"{overallReward.ToString("F2")}";
//         episodesValue.text = $"{this.CompletedEpisodes}";
//         stepValue.text = $"{overallSteps}";
//     }

//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         var discreteActionsOut = actionsOut.DiscreteActions;
//         discreteActionsOut[0] = 0; // idle

//         if (Input.GetKeyDown(KeyCode.LeftArrow))
//             discreteActionsOut[0] = 1; // left
//         else if (Input.GetKeyDown(KeyCode.RightArrow))
//             discreteActionsOut[0] = 2; // right
//         else if (Input.GetKeyDown(KeyCode.UpArrow))
//             discreteActionsOut[0] = 3; // forward
//         else if (Input.GetKeyDown(KeyCode.DownArrow))
//             discreteActionsOut[0] = 4; // backward
//     }

//     private IEnumerator SwapGroundMaterial(Material material, float duration)
//     {
//         if (groundRenderer != null)
//         {
//             Material originalMaterial = groundRenderer.material;
//             groundRenderer.material = material;
//             yield return new WaitForSeconds(duration);
//             groundRenderer.material = originalMaterial;
//         }
//         else
//         {
//             Debug.LogWarning("Ground renderer is not assigned!");
//             yield return null;
//         }
//     }

//     void OnDrawGizmos()
//     {
//         if (!Application.isPlaying) return;

//         Gizmos.color = Color.blue;
//         Gizmos.DrawLine(transform.position, moveTo);

//         RayPerceptionSensorComponent3D rayPerception = GetComponent<RayPerceptionSensorComponent3D>();
//         if (rayPerception != null)
//         {
//             float[] rayAngles = GetRayAngles(rayPerception);
//             for (int i = 0; i < rayAngles.Length; i++)
//             {
//                 Vector3 direction = Quaternion.Euler(0, rayAngles[i], 0) * transform.forward;
//                 RaycastHit hit;
//                 if (Physics.Raycast(transform.position, direction, out hit, rayPerception.RayLength))
//                 {
//                     Gizmos.color = Color.red;
//                     Gizmos.DrawLine(transform.position, hit.point);
//                 }
//                 else
//                 {
//                     Gizmos.color = Color.green;
//                     Gizmos.DrawRay(transform.position, direction * rayPerception.RayLength);
//                 }
//             }
//         }
//     }

//     private float[] GetRayAngles(RayPerceptionSensorComponent3D rayPerception)
//     {
//         float[] angles = new float[rayPerception.RaysPerDirection * 2 + 1];
//         float increment = rayPerception.MaxRayDegrees / rayPerception.RaysPerDirection;
        
//         for (int i = 0; i < rayPerception.RaysPerDirection; i++)
//         {
//             angles[i] = -rayPerception.MaxRayDegrees + i * increment;
//             angles[angles.Length - 1 - i] = rayPerception.MaxRayDegrees - i * increment;
//         }
//         angles[rayPerception.RaysPerDirection] = 0f;

//         return angles;
//     }
// }