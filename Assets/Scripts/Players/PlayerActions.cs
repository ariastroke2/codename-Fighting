using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    [Header("Character Attributes")]
    public float JumpForce = 0f;
    public float SprintSpeed = 1.5f;
    public float BaseSpeed = 1f;
    public float DashSpeed = 50f;
    public float WalljumpImpulse = 50f;
    public float inputLifespan = 2f;

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
    private AttackStrengthType m_attackStrengthType;
    private Attack m_currentAttack;
    private Attack m_nextAttack;
    private AttackStatus m_attackStatus;
    private float m_attackStartTimestamp;
    private readonly List<CastableObject> m_pendingEffects = new(); // Visual or technical effects to be casted
    private CastableHitbox m_climaxHitbox = new();     // Hitbox to be casted when attack reaches climax
    private List<GameObject> m_attackHeldObjects = new();    // Updates attack state for objects that persist, ex; lingering fire

    // Unity components
    private Rigidbody m_Rigidbody;
    private Animator m_Animator;
    private LayerMask m_TerrainMask;

    #region Unity MonoBehavior calls
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        m_Animator.runtimeAnimatorController = attackCollection.animatorController;

        m_TerrainMask = LayerMask.GetMask("Terrain");
        gravityScale = 9f;
        Timer = 0f;
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
        if (collision.gameObject.layer == 3)
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
            transform.localScale = new Vector3(2f * m_playerXDirection, transform.localScale.y, transform.localScale.z);
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
        m_Animator.SetFloat("Speed", HorizontalVelocity.magnitude);

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
                else if (Physics.Raycast(transform.position, new Vector3(m_playerXDirection, 0, 0), out RaycastHit hit, 1f, m_TerrainMask))
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
        if(m_attackStatus == AttackStatus.Sequence || m_attackStatus == AttackStatus.ForcedSequence)
        {
            m_attackStatus = AttackStatus.Charge;
            m_currentAttack = m_nextAttack;
            m_nextAttack = null;
            CastAttack(m_currentAttack);
            return;
        }
        if (m_attackStatus == AttackStatus.None && m_attackStrengthType == AttackStrengthType.None)
        {
            m_attackStrengthType = EnumAttack(inputName);
            AttackCommand();
            isControlLock = m_currentAttack.LockMovement;
            m_attackStatus = AttackStatus.Charge;
            if (m_currentAttack.Type != AttackInputType.OnRelease)
            {
                CastAttack(m_currentAttack);
            }
        }
    }

    public void ReleaseInput(string inputName)
    {
        if (m_attackStrengthType == EnumAttack(inputName))
        {
            m_attackStrengthType = AttackStrengthType.None;
            if (m_currentAttack.Type == AttackInputType.OnRelease)
            {
                CastAttack(m_currentAttack);
            }
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

        // Attack execution
        foreach (CastableObject item in m_pendingEffects.ToList())
        {
            if (Timer > item.delay)
            {
                CastGameObject(item);
                m_pendingEffects.Remove(item);
            }
        }
        if(m_attackStatus == AttackStatus.Windup && Timer > m_currentAttack.Delay + m_attackStartTimestamp)
        {
            CastHitbox(m_climaxHitbox);
            m_Animator.SetInteger("State", 5);
            m_attackStatus = AttackStatus.Release;
        }
        else if (m_attackStatus == AttackStatus.Release)
        {
            bool AttackConditionMet = false;
            if (m_currentAttack.DurationType == AttackDurationType.UntilDurationExpires) AttackConditionMet = Timer > m_currentAttack.Duration + m_attackStartTimestamp;
            if (m_currentAttack.DurationType == AttackDurationType.UntilTouchingGround) AttackConditionMet = istouchingGround;
            if( m_currentAttack.Type == AttackInputType.OnHold) AttackConditionMet = m_attackStrengthType == AttackStrengthType.None;
            if (AttackConditionMet)
            {
                foreach (GameObject item in m_attackHeldObjects)
                {
                    if(item != null)
                        item.GetComponent<IHoldable>()?.EndAttack();
                }
                if (m_currentAttack.Next != null)
                {
                    m_nextAttack = m_currentAttack.Next;
                    if (m_currentAttack.SequenceType == AttackSequenceType.Optional)
                        m_attackStatus = AttackStatus.Sequence;
                    else
                        m_attackStatus = AttackStatus.ForcedSequence;
                }
                else
                {
                    m_attackStatus = AttackStatus.Cooldown;
                }
            }
        }
        else if(m_attackStatus == AttackStatus.Cooldown || m_attackStatus == AttackStatus.Sequence && Timer > m_currentAttack.EndLag + m_attackStartTimestamp)
        {
            foreach (GameObject item in m_attackHeldObjects)
            {
                if (item != null)
                    item.GetComponent<IHoldable>()?.EndCooldown();
            }
            m_attackHeldObjects.Clear();
            m_nextAttack = null;
            isControlLock = false;
            m_attackStatus = AttackStatus.None;
            m_Animator.SetInteger("State", 10);
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
                if (Command.Last(2) == "/up/up" && m_Dpad.Up)
                    current = attackCollection.heavyUp;
                else if (Command.Last(2) == "/down/down" && m_Dpad.Down)
                    current = attackCollection.heavyDown;
                else if (Command.All() == "/back/back/forth" && (m_Dpad.Right || m_Dpad.Left))
                    current = attackCollection.heavySlow;
                else if (Command.Last(2) == "/forth/forth" && (m_Dpad.Right || m_Dpad.Left))
                    current = attackCollection.heavyDash;
                else
                    current = attackCollection.heavyNeutral;
            }
            else
            {
                if (Command.Last(2) == "/up/up" && m_Dpad.Up)
                    current = attackCollection.lightUp;
                else if (Command.Last(2) == "/down/down" && m_Dpad.Down)
                    current = attackCollection.lightDown;
                else if (Command.Last(2) == "/forth/forth" && (m_Dpad.Right || m_Dpad.Left))
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
        m_attackStartTimestamp = Timer;
        m_climaxHitbox = atk.Hitbox;
        isControlLock = atk.LockMovement;
        foreach (CastableObject item in atk.Effects)
        {
            m_pendingEffects.Add(item.WithDelay(Timer));
        }

        m_Animator.SetInteger("State", 0);
        m_Animator.SetTrigger(atk.AttackClassType);
    }

    void CastHitbox(CastableHitbox hitbox)
    {
        ApplyImpulse(m_currentAttack.ImpulseVector, m_currentAttack.SpeedType);
        GameObject HitboxObj = Instantiate(hitbox.hitbox);

        HitboxObj.GetComponent<IHitbox>()?.SetKnockback(hitbox.knockback);
        HitboxObj.GetComponent<IHitbox>()?.SetDamage(hitbox.dmg);
        HitboxObj.GetComponent<IHitbox>()?.SetForceDirection(new Vector2(hitbox.forceDirection.x * m_playerXDirection, hitbox.forceDirection.y));

        // Fix rotations not being applied depending on player orientation
        HitboxObj.GetComponent<IHitbox>()?.SetTransform(transform.position + new Vector3(hitbox.offset.x * m_playerXDirection, hitbox.offset.y, hitbox.offset.z), hitbox.scale, transform.rotation * Quaternion.Euler(hitbox.rotation));

        HitboxObj.transform.parent = transform;
        m_attackHeldObjects.Add(HitboxObj);
    }

    void CastGameObject(CastableObject castObj)
    {
        GameObject EffectObj = Instantiate(castObj.obj, transform.position + new Vector3(castObj.offset.x * m_playerXDirection, castObj.offset.y, castObj.offset.z), transform.rotation * Quaternion.Euler(castObj.rotation));
        if (Mathf.RoundToInt(m_playerXDirection) == -1)
            EffectObj.transform.Rotate(new Vector3(180, 0, 0));
        EffectObj.transform.localScale = castObj.scale;
        EffectObj.transform.parent = transform;
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

}
