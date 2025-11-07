using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GDD3400.Labyrinth
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyAgent : MonoBehaviour
    {
        [Header("Vision Settings")]
        public float viewRadius = 10f;
        [Range(0, 360)]
        public float viewAngle = 90f;

        [Header("Detection")]
        public LayerMask playerMask;
        public LayerMask obstacleMask;

        [Header("Debug")]
        public bool showVisionCone = true;
        public Color visionColor = new Color(1, 1, 0, 0.15f);

        [SerializeField] GameObject player;
        private bool playerVisible;

        bool patrol = true;

        [SerializeField] private LevelManager _levelManager;

        GameObject barrel;

        [SerializeField] GameObject patrolPoint1;
        [SerializeField] GameObject patrolPoint2;

        bool firstNode = true;
        float playerDetectedTimer;
        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }
        [SerializeField] private float _TurnRate = 10f;
        [SerializeField] private float _MaxSpeed = 5f;
        [SerializeField] private float _SightDistance = 25f;

        [SerializeField] private float _StoppingDistance = 1.5f;
        
        [Tooltip("The distance to the destination before we start leaving the path")]
        [SerializeField] private float _LeavingPathDistance = 2f; // This should not be less than 1

        [Tooltip("The minimum distance to the destination before we start using the pathfinder")]
        [SerializeField] private float _MinimumPathDistance = 6f;


        private Vector3 _velocity;
        private Vector3 _floatingTarget;
        private Vector3 _destinationTarget;
        List<PathNode> _path;

        private Rigidbody _rb;

        private LayerMask _wallLayer;

        private bool DEBUG_SHOW_PATH = true;

        bool isHiding = false;
        float barrelSearchTimer;

        GameEvent alert = new GameEvent();
        GameEvent playerLost = new GameEvent();
        public void Awake()
        {
            // Grab and store the rigidbody component
            _rb = GetComponent<Rigidbody>();

            // Grab and store the wall layer
            _wallLayer = LayerMask.GetMask("Walls");
        }

        public void Start()
        {
            // If we didn't manually set the level manager, find it
            if (_levelManager == null) _levelManager = FindAnyObjectByType<LevelManager>();

            // If we still don't have a level manager, throw an error
            if (_levelManager == null) Debug.LogError("Unable To Find Level Manager");

            EventManager.AddInvoker(GameplayEvent.Lost, playerLost);
            EventManager.AddInvoker(GameplayEvent.Alert, alert);
            EventManager.AddListener(GameplayEvent.Alert, HearAlert);
            EventManager.AddListener(GameplayEvent.Hide, Hide);
            EventManager.AddListener(GameplayEvent.UnHide, UnHide);
            EventManager.AddListener(GameplayEvent.Lost, PlayerLost);
        }

        public void Update()
        {
            if (!_isActive) return;
            
            if (isHiding == false)
            {
                Perception();
            }
            else if (barrelSearchTimer >= 1)
            {
                BarrelPerception();
            }
            DecisionMaking();

            if (playerDetectedTimer > 0)
            {
                SetDestinationTarget(player.transform.position);
            }
            else
            {
                patrol = true;
            }
        }

        private void Perception()
        {
            playerVisible = false;
            if (player == null) return;

            Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);


            // Check if player is within FOV
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                // Check if obstacle is blocking view
                if (!Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleMask))
                {
                    if (distanceToPlayer <= viewRadius)
                    {
                        playerVisible = true;
                        patrol = false;
                        SetDestinationTarget(player.transform.position);
                        alert.Invoke(alert.Data);
                    }
                    else
                    {
                        patrol = true;
                    }
                }
            }
            else if (distanceToPlayer <= 2)
            {
                playerVisible = true;
                patrol = false;
                SetDestinationTarget(player.transform.position);
            }
        }

        private void BarrelPerception()
        {
            playerVisible = false;
            if (player == null) return;

            Vector3 dirToPlayer = (barrel.transform.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, barrel.transform.position);


            // Check if player is within FOV
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                // Check if obstacle is blocking view
                if (!Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleMask))
                {
                    if (distanceToPlayer <= viewRadius)
                    {
                        playerVisible = true;
                        patrol = false;
                        SetDestinationTarget(player.transform.position);
                        alert.AddData(GameplayEventData.Player, player);
                        alert.Invoke(alert.Data);
                    }
                    else
                    {
                        patrol = true;
                    }
                }
            }
            else if (distanceToPlayer <= 1)
            {
                playerVisible = true;
                patrol = false;
                SetDestinationTarget(barrel.transform.position);
            }
        }

        private void DecisionMaking()
        {

            if (_path != null && _path.Count > 0)
            {
                if (Vector3.Distance(transform.position, _destinationTarget) < _LeavingPathDistance)
                {
                    _path = null;
                    _floatingTarget = _destinationTarget;
                    if (firstNode == true)
                    {
                        firstNode = false;
                    }
                    else
                    {
                        firstNode = true;
                    }
                }
                else
                {
                    PathFollowing();
                }
            }
            else if (patrol == true)
            {
                if (firstNode == true)
                {
                    SetDestinationTarget(patrolPoint1.transform.position);
                }
                else
                {
                    SetDestinationTarget(patrolPoint2.transform.position);
                }
            }
        }

        #region Path Following

        // Perform path following
        private void PathFollowing()
        {
            // TODO: Implement path following
            int closestNodeIndex = GetClosestNode();
            int nextNodeIndex = closestNodeIndex + 1;

            PathNode targetNode = null;

            if (nextNodeIndex < _path.Count) targetNode = _path[nextNodeIndex];
            else targetNode = _path[closestNodeIndex];

            _floatingTarget = targetNode.transform.position;
        }

        // Public method to set the destination target
        public void SetDestinationTarget(Vector3 destination)
        {

            _destinationTarget = destination;

            //if the straight line distance is greater than our minimum required, pathfind
            if(Vector3.Distance(transform.position, destination) > _MinimumPathDistance)
            {
                // TODO: Implement destination target setting
                PathNode startNode = _levelManager.GetNode(transform.position);
                PathNode endNode = _levelManager.GetNode(destination);

                if (startNode == null || endNode == null) return;

                _path = Pathfinder.FindPath(startNode, endNode);

                StartCoroutine(DrawPathDebugLines(_path));
            }
            //Otherwise move Directly to destination
            else
            {
                _floatingTarget = destination;
            }
        }

        // Get the closest node to the player's current position
        private int GetClosestNode()
        {
            int closestNodeIndex = 0;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < _path.Count; i++)
            {
                float distance = Vector3.Distance(transform.position, _path[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNodeIndex = i;
                }
            }
            return closestNodeIndex;
        }

        #endregion

        #region Action
        private void FixedUpdate()
        {
            if (!_isActive) return;

            playerDetectedTimer--;

            Debug.DrawLine(this.transform.position, _floatingTarget, Color.green);

            // If we have a floating target and we are not close enough to it, move towards it
            if (_floatingTarget != Vector3.zero && Vector3.Distance(transform.position, _floatingTarget) > _StoppingDistance)
            {
                // Calculate the direction to the target position
                Vector3 direction = (_floatingTarget - transform.position).normalized;

                // Calculate the movement vector
                _velocity = direction * _MaxSpeed;                
            }

            // If we are close enough to the floating target, slow down
            else
            {
                _velocity *= .95f;
            }

            // Calculate the desired rotation towards the movement vector
            if (_velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_velocity);

                // Smoothly rotate towards the target rotation based on the turn rate
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _TurnRate);
            }

            _rb.linearVelocity = _velocity;

            barrelSearchTimer--;
        }
        #endregion

        private IEnumerator DrawPathDebugLines(List<PathNode> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i].transform.position, path[i + 1].transform.position, Color.red, 3.5f);
                yield return new WaitForSeconds(0.1f);
            }
        }


        public void PlayerDetected()
        {

        }

        public void PlayerLost(Dictionary<System.Enum, object> data)
        {
            patrol = true;
        }

        void OnDrawGizmos()
        {
            if (!showVisionCone) return;

            Gizmos.color = visionColor;
            Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

            Gizmos.DrawRay(transform.position, leftBoundary * viewRadius);
            Gizmos.DrawRay(transform.position, rightBoundary * viewRadius);

            // Draw filled fan shape (optional)
            int segments = 20;
            float step = viewAngle / segments;
            Vector3 lastPoint = transform.position;
            for (int i = 0; i <= segments; i++)
            {
                float angle = -viewAngle / 2 + step * i;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                Vector3 point = transform.position + direction * viewRadius;
                Gizmos.DrawLine(transform.position, point);
                if (i > 0)
                    Gizmos.DrawLine(lastPoint, point);
                lastPoint = point;
            }

            if (playerVisible)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.transform.position);
            }
        }

        void HearAlert(Dictionary<System.Enum, object> data)
        {
            playerDetectedTimer = 180;
            patrol = false;
        }

        void Hide(Dictionary<System.Enum, object> data)
        {
            isHiding = true;
            barrel = GameObject.FindWithTag("Barrel");
            barrelSearchTimer = 30;
            playerDetectedTimer = 0;
        }

        void UnHide(Dictionary<System.Enum, object> data)
        {
            isHiding = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Barrel"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
