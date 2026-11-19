using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Networking;
using GoodForBusiness.Slimes.Variants;
using GoodForBusiness.Stubs.Game;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

namespace GoodForBusiness.Slimes
{
    public enum SlimeState
    {
        Idle,
        Following,
        Eating,
        Growing,
        AbilityActive
    }

    public class SlimeAI : EnemyAI
    {
        public SlimeType SlimeType
        {
            get => (SlimeType)SyncSlimeType.Value;
            set => SyncSlimeType.Value = (int)value;
        }

        [Header("Network Sync")]
        public NetworkVariable<int> SyncState = new NetworkVariable<int>((int)SlimeState.Idle);
        public NetworkVariable<float> SyncGrowthProgress = new NetworkVariable<float>(0f);
        public NetworkVariable<bool> SyncIsGrown = new NetworkVariable<bool>(false);
        public NetworkVariable<int> SyncOwnerPlayerId = new NetworkVariable<int>(-1);
        public NetworkVariable<int> SyncSlimeType = new NetworkVariable<int>((int)SlimeType.None);

        [Header("Networking Components")]
        [SerializeField] private SlimeNetworkTransform? _networkTransform;
        [SerializeField] private SlimeFeedingSync? _feedingSync;
        [SerializeField] private SlimeAbilitySync? _abilitySync;
        [SerializeField] private SlimeStateSync? _stateSync;
        [SerializeField] private SlimeHatchingSync? _hatchingSync;
        [SerializeField] private SlimeItemSpawnSync? _itemSpawnSync;

        public SlimeState CurrentState
        {
            get => (SlimeState)SyncState.Value;
            set => SyncState.Value = (int)value;
        }

        public float GrowthProgress
        {
            get => SyncGrowthProgress.Value;
            set => SyncGrowthProgress.Value = Mathf.Clamp01(value);
        }

        public bool IsGrown
        {
            get => SyncIsGrown.Value;
            set => SyncIsGrown.Value = value;
        }

        public int OwnerPlayerId
        {
            get => SyncOwnerPlayerId.Value;
            set => SyncOwnerPlayerId.Value = value;
        }

        public ISlimeVariant? Variant => _variant;

        public ulong NetworkObjectId => NetworkObject?.NetworkObjectId ?? 0;

        [Header("References")]
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _creatureVoice;
        [SerializeField] private AudioSource _creatureSFX;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _hatchClip;
        [SerializeField] private AudioClip _eatClip;
        [SerializeField] private AudioClip _growClip;
        [SerializeField] private AudioClip _abilityClip;
        [SerializeField] private AudioClip _idleClip;

        [Header("Movement")]
        [SerializeField] private float _followDistance = 3f;
        [SerializeField] private float _idleWanderRadius = 2f;
        [SerializeField] private float _stateCheckInterval = 0.5f;

        [Header("Bobbing")]
        [SerializeField] private float _baseBobAmount = 0.5f;
        [SerializeField] private float _bobSpeed = 1.5f;

        private ISlimeVariant? _variant;
        private PluginConfig? _config;
        private Coroutine? _stateCoroutine;
        private Coroutine? _bobCoroutine;
        private Vector3 _basePosition;
        private float _bobPhase;
        private PlayerControllerB? _followTarget;
        private float _lastStateCheck;
        private bool _isInitialized;

        /// <summary>
        /// When true, the slime's daily ability variant controls movement directly
        /// (e.g., pathfinding to a beehive). LateUpdate will not reset the path.
        /// </summary>
        public bool AbilityControlsMovement { get; set; }

        public override void Start()
        {
            base.Start();
            InitializeComponents();
            InitializeVariant();
            _basePosition = transform.position;

            if (IsServer)
            {
                _stateCoroutine = StartCoroutine(StateMachineRoutine());
                _bobCoroutine = StartCoroutine(BobRoutine());
            }

            // Apply visuals after variant is initialized
            SlimeVisuals.ApplyVariantVisuals(this);

            _isInitialized = true;
        }

        private void InitializeComponents()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent == null)
                _agent = gameObject.AddComponent<NavMeshAgent>();

            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);

            var audioSources = GetComponents<AudioSource>();
            if (audioSources.Length >= 1)
                _creatureVoice = audioSources[0];
            if (audioSources.Length >= 2)
                _creatureSFX = audioSources[1];
            else
                _creatureSFX = gameObject.AddComponent<AudioSource>();

            _agent.speed = 2f;
            _agent.acceleration = 8f;
            _agent.stoppingDistance = _followDistance;
            _agent.autoBraking = true;

            // Initialize networking components
            _networkTransform = GetComponent<SlimeNetworkTransform>();
            if (_networkTransform == null)
                _networkTransform = gameObject.AddComponent<SlimeNetworkTransform>();

            _feedingSync = GetComponent<SlimeFeedingSync>();
            if (_feedingSync == null)
                _feedingSync = gameObject.AddComponent<SlimeFeedingSync>();

            _abilitySync = GetComponent<SlimeAbilitySync>();
            if (_abilitySync == null)
                _abilitySync = gameObject.AddComponent<SlimeAbilitySync>();

            _stateSync = GetComponent<SlimeStateSync>();
            if (_stateSync == null)
                _stateSync = gameObject.AddComponent<SlimeStateSync>();

            _hatchingSync = GetComponent<SlimeHatchingSync>();
            if (_hatchingSync == null)
                _hatchingSync = gameObject.AddComponent<SlimeHatchingSync>();

            _itemSpawnSync = GetComponent<SlimeItemSpawnSync>();
            if (_itemSpawnSync == null)
                _itemSpawnSync = gameObject.AddComponent<SlimeItemSpawnSync>();
        }

        public void InitializeVariant()
        {
            _config = GoodForBusinessPlugin.Instance?.Config;

            // Add the appropriate variant component based on slime type
            if (SlimeType != SlimeType.None)
            {
                switch (SlimeType)
                {
                    case SlimeType.Equipment:
                        _variant = gameObject.AddComponent<EquipmentSlimeVariant>();
                        break;
                    case SlimeType.BeeLover:
                        _variant = gameObject.AddComponent<BeeLoverSlimeVariant>();
                        break;
                    default:
                        UnityEngine.Debug.LogWarning($"[GoodForBusiness] Unknown slime type: {SlimeType}");
                        break;
                }
            }

            if (IsServer && _hatchClip != null)
            {
                PlaySoundServerRpc(_creatureVoice, _hatchClip);
            }
        }

        private System.Collections.IEnumerator StateMachineRoutine()
        {
            yield return new WaitForSeconds(1f);

            while (true)
            {
                if (!IsServer)
                {
                    yield return new WaitForSeconds(_stateCheckInterval);
                    continue;
                }

                UpdateState();
                _lastStateCheck = Time.time;
                yield return new WaitForSeconds(_stateCheckInterval);
            }
        }

        private void UpdateState()
        {
            if (CurrentState == SlimeState.Growing || CurrentState == SlimeState.AbilityActive || CurrentState == SlimeState.Eating)
                return;

            if (IsGrown)
            {
                if (CurrentState != SlimeState.Idle && CurrentState != SlimeState.Following)
                {
                    SetStateServerRpc(SlimeState.Idle);
                }
            }
            else
            {
                _followTarget = FindNearestPlayer();
                if (_followTarget != null)
                {
                    float dist = Vector3.Distance(transform.position, _followTarget.transform.position);
                    if (dist > _followDistance * 1.5f)
                    {
                        SetStateServerRpc(SlimeState.Following);
                    }
                    else
                    {
                        SetStateServerRpc(SlimeState.Idle);
                    }
                }
                else
                {
                    SetStateServerRpc(SlimeState.Idle);
                }
            }
        }

        private System.Collections.IEnumerator BobRoutine()
        {
            while (true)
            {
                _bobPhase += Time.deltaTime * _bobSpeed;
                float bobOffset = Mathf.Sin(_bobPhase) * _baseBobAmount * (IsGrown ? 0.3f : 1f);
                transform.position = _basePosition + Vector3.up * bobOffset;
                yield return null;
            }
        }

        private void LateUpdate()
        {
            if (!IsSpawned || !IsServer)
                return;

            UpdateAnimations();

            if (GoodForBusinessPlugin.Instance?.Config?.SlimesFacePlayers.Value == true)
            {
                FaceNearestPlayer();
            }

            switch (CurrentState)
            {
                case SlimeState.Following:
                    FollowTarget();
                    break;
                case SlimeState.Idle:
                    IdleWander();
                    break;
                case SlimeState.Eating:
                case SlimeState.Growing:
                    _agent.ResetPath();
                    break;
                case SlimeState.AbilityActive:
                    if (!AbilityControlsMovement)
                    {
                        _agent.ResetPath();
                    }
                    break;
            }
        }

        private void FollowTarget()
        {
            if (_followTarget == null)
            {
                _followTarget = FindNearestPlayer();
                if (_followTarget == null)
                {
                    SetStateServerRpc(SlimeState.Idle);
                    return;
                }
            }

            float dist = Vector3.Distance(transform.position, _followTarget.transform.position);
            if (dist > _followDistance)
            {
                _agent.SetDestination(_followTarget.transform.position);
            }
            else
            {
                _agent.ResetPath();
                if (dist < _followDistance * 0.5f)
                {
                    Vector3 away = (transform.position - _followTarget.transform.position).normalized;
                    Vector3 newPos = transform.position + away * _followDistance;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(newPos, out hit, 2f, NavMesh.AllAreas))
                    {
                        _agent.SetDestination(hit.position);
                    }
                }
            }
        }

        private void IdleWander()
        {
            if (_agent.hasPath || _agent.pathPending)
                return;

            if (Random.value < 0.1f)
            {
                Vector3 randomDir = Random.insideUnitSphere * _idleWanderRadius;
                randomDir += transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDir, out hit, _idleWanderRadius, NavMesh.AllAreas))
                {
                    _agent.SetDestination(hit.position);
                }
            }
        }

        private void UpdateAnimations()
        {
            if (_animator == null)
                return;

            _animator.SetInteger("SlimeState", (int)CurrentState);
            _animator.SetFloat("GrowthProgress", GrowthProgress);
            _animator.SetBool("IsGrown", IsGrown);
            _animator.SetFloat("Speed", _agent.velocity.magnitude);
        }

        private void FaceNearestPlayer()
        {
            if (StartOfRound.Instance?.allPlayerScripts == null)
                return;

            PlayerControllerB? nearestPlayer = null;
            float nearestDist = float.MaxValue;

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null)
                    continue;

                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestPlayer = player;
                }
            }

            if (nearestPlayer != null)
            {
                Vector3 direction = (nearestPlayer.transform.position - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }

        private PlayerControllerB? FindNearestPlayer()
        {
            if (StartOfRound.Instance?.allPlayerScripts == null)
                return null;

            PlayerControllerB? nearestPlayer = null;
            float nearestDist = float.MaxValue;

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null || player.isPlayerDead)
                    continue;

                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestPlayer = player;
                }
            }

            return nearestPlayer;
        }

        /// <summary>
        /// Called when a player feeds scrap to this slime.
        /// Uses ServerRpc to ensure proper network synchronization.
        /// </summary>
        public void OnFedScrap(int scrapValue, int playerId = -1)
        {
            if (IsGrown)
                return;

            // If called on client, forward to server
            if (!IsServer && NetworkObject != null)
            {
                _feedingSync?.FeedSlimeServerRpc(NetworkObjectId, scrapValue, playerId);
                return;
            }

            // Server-side logic
            _variant?.OnFedScrap(scrapValue);

            float previousProgress = GrowthProgress;
            GrowthProgress += scrapValue * 0.001f;

            if (GrowthProgress >= 1f && previousProgress < 1f)
            {
                GrowthProgress = 1f;
                IsGrown = true;
                SetStateServerRpc(SlimeState.Growing);
                PlayGrowEffects();
            }
            else
            {
                SetStateServerRpc(SlimeState.Eating);
                PlayEatEffects();
            }

            if (playerId >= 0)
                OwnerPlayerId = playerId;

            // Sync growth progress to all clients
            SyncGrowthServerRpc(GrowthProgress);
            _stateSync?.SyncGrowthProgressClientRpc(NetworkObjectId, GrowthProgress, IsGrown);
        }

        /// <summary>
        /// Triggers this slime's daily ability.
        /// Uses ServerRpc to ensure proper network synchronization.
        /// </summary>
        public void ExecuteDailyAbility()
        {
            if (!IsGrown)
                return;

            // If called on client, forward to server
            if (!IsServer && NetworkObject != null)
            {
                _abilitySync?.TriggerAbilityServerRpc(NetworkObjectId);
                return;
            }

            // Server-side logic
            _variant?.ExecuteDailyAbility(this);
            SetStateServerRpc(SlimeState.AbilityActive);
            PlayAbilityEffects();

            StartCoroutine(ReturnToIdleAfterAbility());
        }

        private System.Collections.IEnumerator ReturnToIdleAfterAbility()
        {
            yield return new WaitForSeconds(3f);
            if (IsServer)
            {
                SetStateServerRpc(IsGrown ? SlimeState.Idle : SlimeState.Following);
            }
        }

        public void PlayEatEffects()
        {
            if (_eatClip != null)
            {
                PlaySoundServerRpc(_creatureSFX, _eatClip);
            }
            TriggerAnimationClientRpc("Eat");
        }

        public void PlayGrowEffects()
        {
            if (_growClip != null)
            {
                PlaySoundServerRpc(_creatureVoice, _growClip);
            }
            TriggerAnimationClientRpc("Grow");
        }

        public void PlayAbilityEffects()
        {
            if (_abilityClip != null)
            {
                PlaySoundServerRpc(_creatureSFX, _abilityClip);
            }
            TriggerAnimationClientRpc("Ability");
        }

        public int GetGrowthCost()
        {
            return _variant?.GrowthCost ?? 300;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetStateServerRpc(SlimeState state)
        {
            CurrentState = state;
            _stateSync?.SyncStateClientRpc(NetworkObjectId, (int)state);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncGrowthServerRpc(float progress)
        {
            GrowthProgress = progress;
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaySoundServerRpc(AudioSource source, AudioClip clip)
        {
            PlaySoundClientRpc(source, clip);
        }

        [ClientRpc]
        public void PlaySoundClientRpc(AudioSource source, AudioClip clip)
        {
            if (source != null && clip != null)
            {
                source.PlayOneShot(clip);
            }
        }

        [ClientRpc]
        public void TriggerAnimationClientRpc(string trigger)
        {
            if (_animator != null)
            {
                _animator.SetTrigger(trigger);
            }
        }

        /// <summary>
        /// Spawns an item (equipment crate or slimed beehive) at the specified position.
        /// Called by slime variants when executing their daily ability.
        /// </summary>
        public void SpawnItemAt(string itemName, Vector3 position, int creditValue = 0)
        {
            if (!IsServer)
                return;

            _itemSpawnSync?.SpawnItemServerRpc(itemName, position, Quaternion.identity, creditValue);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_stateCoroutine != null)
                StopCoroutine(_stateCoroutine);
            if (_bobCoroutine != null)
                StopCoroutine(_bobCoroutine);
        }
    }
}