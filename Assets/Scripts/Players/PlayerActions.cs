using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour, IDamageable
{
    [Header("Character Movement Attributes")]
    public int Team;
    public int Hp;

    [Header("Character Movement Attributes")]
    public float JumpForce = 0f;
    public float SprintSpeed = 1.5f;
    public float BaseSpeed = 1f;
    public float DashSpeed = 50f;
    public float WalljumpImpulse = 50f;
    public float inputLifespan = 2f;
    public float ChargeSpeed = 1f;

    [Header("Moveset")]
    public AttackCollection attackCollection;

    private bool isControlLock;
    private bool istouchingGround;

    // Player inputs
    private Dpad m_Dpad;
    private bool m_IsHoldingJump = false;
    private bool m_IsHoldingSprint = false;
    private float m_playerXDirection;
    private List<RegisteredInput> m_inputRegistry = new();

    // Player state
    private List<GameObject> m_GroundChunks = new();
    private float gravityScale;
    private float Timer;

    // Attack execution variables
    private bool _attackButtonHeld;
    private IEnumerator _attackCoroutine;
    public float _attackTimer;
    public float m_attackAccumulatedTimestamp;
    private float _attackCharge;
    private AttackStrengthType m_attackStrengthType;
    private Attack m_currentAttack;
    private Attack m_nextAttack;
    private AttackStatus m_attackStatus;
    private List<CastableObject> m_pendingEffects = new(); // Visual or technical effects to be casted
    private CastableHitbox m_climaxHitbox = new();     // Hitbox to be casted when attack reaches climax
    private List<GameObject> m_attackHeldObjects = new();    // Updates attack state for objects that persist, ex; lingering fire

    // Unity components
    private Rigidbody m_Rigidbody;
    private BoxCollider _boxCollider;
    [SerializeField] private Animator m_Animator;
    private LayerMask m_terrainMask;
    private Transform _modelTransform;

    // Networking components
    private ulong _netID;

    #region Unity MonoBehavior calls
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
        _modelTransform = transform.Find("ModelTransform").transform;
        m_Animator.runtimeAnimatorController = attackCollection.animatorController;

        m_terrainMask = LayerMask.NameToLayer("Terrain");
        gravityScale = 9f;
        Timer = 0f;

        Entity.DisableCollision(_boxCollider);

        Camera.main.GetComponent<CameraFocuser>().target = transform;
    }

    void Update()
    {
        if (TimerConditions())
        {
            Timer += Time.deltaTime;
            TimerTasks();
        } else {
            Timer = 0f;
        }
        istouchingGround = (m_GroundChunks.Count > 0);
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        Vector3 gravity = Physics.gravity * gravityScale;
        m_Rigidbody.AddForce(gravity, ForceMode.Acceleration);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == m_terrainMask.value)
        {
            Vector3 DistanceAvg = Vector3.zero;
            foreach (ContactPoint point in collision.contacts)
            {
                DistanceAvg += point.normal;
            }
            DistanceAvg /= collision.contactCount;
            if (Mathf.Abs(DistanceAvg.y) > Mathf.Abs(DistanceAvg.x) && Mathf.Abs(DistanceAvg.y) > Mathf.Abs(DistanceAvg.z))
            {
                // Deteced ground
                m_GroundChunks.Add(collision.gameObject);
            }
            else
            {
                // Detected wall
            }

        }
        else
        {
            // Contact with non-terrain object
            // Physics.IgnoreCollision(_boxCollider, collision.collider);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 3)
        {
            if(m_GroundChunks.Contains(collision.gameObject))
                m_GroundChunks.Remove(collision.gameObject);
        }
    }

    #endregion

    #region Movement
    void ApplyMovement()
    {
        if (!isControlLock)
        {
            Vector3 newSpeed = new(m_Dpad.Digitalized.x, 0f, m_Dpad.Digitalized.y);
            if (m_Dpad.Digitalized.x != 0)
                m_playerXDirection = m_Dpad.Digitalized.x;
            // Figure out better code to scale properly, ion like this
            _modelTransform.rotation = Quaternion.Euler(30, 180 - (90 * m_playerXDirection), 0);
            if (m_IsHoldingSprint)
            {
                newSpeed *= SprintSpeed;
                newSpeed.z = 0f;
            }
            else
                newSpeed *= BaseSpeed;
            if (m_IsHoldingJump)
                gravityScale = 6f;
            else
                gravityScale = 9f;
            m_Rigidbody.velocity += newSpeed;
        }

        Vector3 HorizontalVelocity = m_Rigidbody.velocity;
        HorizontalVelocity.y = 0f;
        m_Animator.SetFloat("SpeedX", HorizontalVelocity.magnitude);
        m_Animator.SetFloat("SpeedY", m_Rigidbody.velocity.y);

        m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x * 0.8f, m_Rigidbody.velocity.y, m_Rigidbody.velocity.z * 0.8f);

        if (m_attackStatus == AttackStatus.Release && m_currentAttack?.SpeedType == SpeedType.ConstantSpeed)
            ApplyImpulse(m_currentAttack.ImpulseVector, SpeedType.ConstantSpeed);
    }

    public void ApplyImpulse(Vector3 acc, SpeedType spt)
    {
        acc.x *= m_playerXDirection;
        if (spt == SpeedType.Impulse)
            m_Rigidbody.AddForce(acc, ForceMode.VelocityChange);
        else if (spt == SpeedType.SetSpeed)
            m_Rigidbody.velocity = acc;
        else if(spt == SpeedType.ConstantSpeed)
            m_Rigidbody.velocity = acc;
    }

    #endregion

    #region Inputs

    public void AssignDpad(ref Dpad dpad)
    {
        m_Dpad = dpad;
        m_Dpad.Subscribe(TriggerRight, Direction.Right);
        m_Dpad.Subscribe(TriggerLeft, Direction.Left);
        m_Dpad.Subscribe(TriggerUp, Direction.Up);
        m_Dpad.Subscribe(TriggerDown, Direction.Down);
    }

    void TriggerRight() { RegisterInput(Direction.Right); }
    void TriggerLeft() { RegisterInput(Direction.Left); }
    void TriggerUp() { RegisterInput(Direction.Up); if(m_IsHoldingSprint) m_Rigidbody.AddForce(new Vector3(0, 0, DashSpeed), ForceMode.VelocityChange); }
    void TriggerDown() { RegisterInput(Direction.Down); if (m_IsHoldingSprint) m_Rigidbody.AddForce(new Vector3(0, 0, -DashSpeed), ForceMode.VelocityChange); }

    public void TriggerJumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (!isControlLock)
            {
                m_IsHoldingJump = true;
                if (m_GroundChunks.Count > 0)
                {
                    m_Rigidbody.velocity += new Vector3(0, -m_Rigidbody.velocity.y, 0);
                    m_Rigidbody.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
                }
                // Fix raycast code please :pleading face
                else if (Physics.Raycast(transform.position, new Vector3(m_playerXDirection, 0, 0), out RaycastHit hit, 1f, LayerMask.GetMask("Terrain")))
                {
                    Debug.Log("walljump");
                    m_Rigidbody.velocity += new Vector3(0, -m_Rigidbody.velocity.y, 0);
                    Vector3 RayNormal = hit.normal * WalljumpImpulse + new Vector3(0, JumpForce, 0);
                    m_Rigidbody.AddForce(RayNormal, ForceMode.Impulse);
                }
            }
        }
        if (ctx.canceled)
        {
            m_IsHoldingJump = false;
        }
    }

    public void TriggerSprintInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            m_IsHoldingSprint = true;
        }
        if (ctx.canceled)
        {
            m_IsHoldingSprint = false;
        }
    }

    private void RegisterInput(Direction inputName)
    {
        m_inputRegistry.Add(new RegisteredInput(inputName, Timer));
    }

    public void ExecuteInput(string inputName)
    {
        // Execute attack sequence
        if (m_attackStatus == AttackStatus.Sequence || m_attackStatus == AttackStatus.ForcedSequence)
        {
            _attackButtonHeld = true;
            m_attackStatus = AttackStatus.Charge;
            m_currentAttack = m_nextAttack;
            m_nextAttack = null;
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = ExecuteAttack();
            StartCoroutine(_attackCoroutine);
            return;
        }
        // Execute isolated attack
        if (m_attackStatus == AttackStatus.None)
        {
            _attackButtonHeld = true;
            m_attackStatus = AttackStatus.Charge;
            m_attackStrengthType = EnumAttack(inputName);
            AttackCommand();
            _attackCoroutine = ExecuteAttack();
            StartCoroutine(_attackCoroutine);
        }
    }

    public void ReleaseInput(string inputName)
    {
        if (m_attackStrengthType == EnumAttack(inputName))
        {
            _attackButtonHeld = false;
        }
    }

    #endregion

    #region Timer
    void TimerTasks()
    {
        // Input expiry
        foreach (RegisteredInput item in m_inputRegistry.ToList())
        {
            if (Timer > item.Timestamp + inputLifespan)
            {
                m_inputRegistry.Remove(item);
            }
        }
    }

    bool TimerConditions()
    {
        if(m_inputRegistry.Count > 0)
        {
            return true;
        }
        if(m_attackStatus != AttackStatus.None)
        {
            return true;
        }
        return false;
    }

    #endregion

    #region Attacks

    IEnumerator ExecuteAttack()
    {
        m_attackAccumulatedTimestamp = 0;
        _attackTimer = 0;
        _attackCharge = 0;
        isControlLock = m_currentAttack.LockMovement;

        m_Animator.SetTrigger(m_currentAttack.AttackClassType);
        m_Animator.SetInteger("State", 0);

        CastAttack(m_currentAttack);
        RefreshAttackEffects();
        // Wait until attack execution conditions are met
        while (_attackButtonHeld && m_currentAttack.Type == AttackInputType.OnRelease)
        {
            if (_attackCharge < 1)
                _attackCharge += (1 / ChargeSpeed) * Time.deltaTime;
            yield return null;
        }
        if (m_currentAttack.Type != AttackInputType.OnRelease)
            _attackCharge = 1f;
        if (_attackCharge > 1)
            _attackCharge = 1f;
        // Linear transformation to attack charge
        _attackCharge = (_attackCharge * 0.5f) + 0.5f;

        while(m_attackStatus == AttackStatus.Windup && _attackTimer < m_currentAttack.Delay)
        {
            RefreshAttackEffects();
            _attackTimer += Time.deltaTime;
            yield return null;
        }

        m_attackStatus = AttackStatus.Release;
        m_attackAccumulatedTimestamp = _attackTimer;
        UpdateAttackStatus();

        // Wait until attack duration ends
        bool AttackConditionMet = false;
        while (m_attackStatus == AttackStatus.Release && !AttackConditionMet)
        {
            RefreshAttackEffects();
            _attackTimer += Time.deltaTime;

            if (m_currentAttack.DurationType == AttackDurationType.UntilDurationExpires) AttackConditionMet = _attackTimer > m_currentAttack.Duration + m_attackAccumulatedTimestamp;
            if (m_currentAttack.DurationType == AttackDurationType.UntilTouchingGround) AttackConditionMet = istouchingGround;
            if (m_currentAttack.Type == AttackInputType.OnHold) AttackConditionMet = !_attackButtonHeld;

            yield return null;
        }

        m_attackStatus = AttackStatus.Cooldown;
        m_attackAccumulatedTimestamp = _attackTimer;
        UpdateAttackStatus();
        
        // Wait until attack cooldown expires
        while ((m_attackStatus == AttackStatus.Cooldown || m_attackStatus == AttackStatus.Sequence) && _attackTimer < m_currentAttack.EndLag + m_attackAccumulatedTimestamp)
        {
            RefreshAttackEffects();
            _attackTimer += Time.deltaTime;

            yield return null;
        }

        if (m_attackStatus != AttackStatus.ForcedSequence)
        {
            m_attackStatus = AttackStatus.None;
            UpdateAttackStatus();
        }
    }

    void UpdateAttackStatus()
    {
        if(m_attackStatus == AttackStatus.Release)
        {
            // Update AttackHoldables state
            foreach (GameObject item in m_attackHeldObjects)
            {
                if (item != null)
                    item.GetComponent<IAttackHoldable>()?.ReleaseAttack();
            }

            // Prepare attack
            CastHitbox(m_climaxHitbox);
            m_Animator.SetInteger("State", 5);
        }
        if(m_attackStatus == AttackStatus.None)
        {
            // Update AttackHoldables state
            foreach (GameObject item in m_attackHeldObjects)
            {
                if (item != null)
                    item.GetComponent<IAttackHoldable>()?.EndCooldown();
            }

            // Attack ended. Clear all
            m_attackHeldObjects.Clear();
            m_nextAttack = null;
            isControlLock = false;
            m_attackStrengthType = AttackStrengthType.None;
            m_Animator.SetInteger("State", 10);
        }
        if(m_attackStatus == AttackStatus.Cooldown)
        {
            // Update AttackHoldables state
            foreach (GameObject item in m_attackHeldObjects)
            {
                if (item != null)
                    item.GetComponent<IAttackHoldable>()?.EndAttack();
            }

            // Check if there is an attack sequence
            if (m_currentAttack.Next != null)
            {
                m_nextAttack = m_currentAttack.Next;
                if (m_currentAttack.SequenceType == AttackSequenceType.Optional)
                    m_attackStatus = AttackStatus.Sequence;
                else
                    m_attackStatus = AttackStatus.ForcedSequence;
            }
        }
    }

    void RefreshAttackEffects()
    {
        foreach (CastableObject item in m_pendingEffects.ToList())
        {
            if (_attackTimer > item.delay || item.delay < 0)
            {
                CastGameObject(item);
                m_pendingEffects.Remove(item);
            }
        }
    }

    void AttackCommand()
    {
        Direction[] CommandChain = { Direction.None, Direction.None, Direction.None };
        for(int i =  0; i < CommandChain.Length && i < m_inputRegistry.Count; i++)
        {
            if(m_inputRegistry.Count < 3)
                CommandChain[i + (CommandChain.Length - m_inputRegistry.Count)] = m_inputRegistry[^((m_inputRegistry.Count) - (i))].Direction;
            else
                CommandChain[i] = m_inputRegistry[^((CommandChain.Length) - (i))].Direction;
        }

        ActionCommand Command = new ActionCommand(CommandChain);

        Attack current;
        if (istouchingGround)
        {
            if (m_attackStrengthType == AttackStrengthType.StrongAttack)
            {
                /* if (Command.Last(2) == "/up/up" && m_Dpad.Up)
                    current = attackCollection.heavyUp;
                else if (Command.Last(2) == "/down/down" && m_Dpad.Down)
                    current = attackCollection.heavyDown;
                else */
                if (Command.All() == "/back/back/forth" && (m_Dpad.Right || m_Dpad.Left))
                    current = attackCollection.heavySlow;
                else if (Command.Last(2) == "/forth/forth" && (m_Dpad.Right || m_Dpad.Left))
                    current = attackCollection.heavyDash;
                else
                    current = attackCollection.heavyNeutral;
            }
            else
            {
                /* if (Command.Last(2) == "/up/up" && m_Dpad.Up)
                    current = attackCollection.lightUp;
                else if (Command.Last(2) == "/down/down" && m_Dpad.Down)
                    current = attackCollection.lightDown;
                else */
                if (Command.Last(2) == "/forth/forth" && (m_Dpad.Right || m_Dpad.Left))
                    current = attackCollection.lightDash;
                else
                    current = attackCollection.lightNeutral;
            }
                
        }
        else
        {
            if (m_attackStrengthType == AttackStrengthType.StrongAttack)
                current = attackCollection.heavyAerial;
            else
                current = attackCollection.lightAerial;
        }

        m_currentAttack = current;
    }

    void CastAttack(Attack atk)
    {
        m_attackStatus = AttackStatus.Windup;
        m_climaxHitbox = atk.Hitbox;
        isControlLock = atk.LockMovement;
        foreach (CastableObject item in atk.Effects)
        {
            m_pendingEffects.Add(item.WithDelay(0));
        }
        
    }

    void CastHitbox(CastableHitbox hitbox)
    {
        ApplyImpulse(m_currentAttack.ImpulseVector, m_currentAttack.SpeedType);
        GameObject HitboxObj = Instantiate(hitbox.hitbox);

        AttackMessage newAtk = new AttackMessage();
        newAtk.Damage = (int)(hitbox.dmg * _attackCharge);
        newAtk.Knockback = hitbox.knockback * _attackCharge;
        newAtk.ForceDirection = new Vector2(hitbox.forceDirection.x * m_playerXDirection, hitbox.forceDirection.y);
        newAtk.AttackingTeam = Team;

        // Fix rotations not being applied depending on player orientation
        HitboxObj.GetComponent<IHitbox>()?.SetTransform(transform.position + new Vector3(hitbox.offset.x * m_playerXDirection, hitbox.offset.y, hitbox.offset.z), hitbox.scale, transform.rotation * Quaternion.Euler(hitbox.rotation));
        HitboxObj.GetComponent<IHitbox>()?.SetAttackMessage(newAtk);

        HitboxObj.transform.parent = transform;
        m_attackHeldObjects.Add(HitboxObj);
    }

    void CastGameObject(CastableObject castObj)
    {
        GameObject EffectObj = Instantiate(castObj.obj, transform.position, Quaternion.identity);

        EffectObj.GetComponent<IAttackHoldable>()?.SetTarget(gameObject);

        if (m_playerXDirection < 0)
            EffectObj.transform.Rotate(new Vector3(0, 180, 0));

        Vector3 newOffset = castObj.offset;
        newOffset.x *= m_playerXDirection;
        EffectObj.GetComponent<IAttackHoldable>()?.SetOffset(newOffset);

        EffectObj.transform.position += newOffset;
        EffectObj.transform.Rotate(castObj.rotation);
        EffectObj.transform.localScale = castObj.scale;
        m_attackHeldObjects.Add(EffectObj);
    }

    AttackStrengthType EnumAttack(string name)
    {
        switch (name)
        {
            case "Strong Attack":
                return AttackStrengthType.StrongAttack;
            case "Light Attack":
                return AttackStrengthType.LightAttack;
            default:
                return AttackStrengthType.None;
        }
    }

    #endregion

    #region Network

    public NetworkPlayerData GetPlayerState()
    {
        NetworkPlayerData newData = new();

        newData.Position = transform.position;
        newData.HP = Hp;
        newData.Team = Team;

        return newData;
    }

    public void SetNetworkId(ulong id)
    {
        _netID = id;
    }

    public void TakeDamage(AttackMessage attackMessage)
    {
        if (Team != attackMessage.AttackingTeam)
        {
            Hp -= attackMessage.Damage;
            m_Rigidbody.AddForce(attackMessage.ForceDirection * attackMessage.Knockback, ForceMode.VelocityChange);
        }
    }

    #endregion
}
