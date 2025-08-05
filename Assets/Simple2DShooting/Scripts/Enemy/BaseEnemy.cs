using System.Collections;
using System.Collections.Generic;
using GiroFrame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public interface IEnemy
{
    void AI();
    //这里的最佳方案应该是把伤害数据封装为一个类/结构体，但是项目没那么大就随便搞搞
    void TakeDamage(float damage,float knockbackPower, Direction knockbackDirection);
    //这里同上，按照需要设置参数，但是没有需要
    float GetAttackDamage();
}

public class BaseEnemy : MonoBehaviour, IEnemy
{
    [Header("敌人属性设置")]
    public float moveSpeed = 2f;
    public float maxHealth = 1;
    public float activationRange = 2.5f; // 玩家进入此范围内敌人开始行动
    public float damage = 1f;
    public Direction defaultDirection = Direction.Left;
    public LayerMask wallLayerMask ; // 墙壁层级

    public Color hurtFlashColor;
    public float hurtFlashTime;
    [Tooltip("影响击退")] public float weight;
    public float deathKnockbackForce;
    public float deathKnockbackAngle;
    [Header("Wall Check")]
    public Transform wallCheckPoint;
    public float wallCheckDistance = 0.5f;

    [Header("音效")] public AudioClip deathAudio;
    
    [Header("组件")]
    public SpriteRenderer spriteRenderer;

    public Collider2D hurtCollider;
    public Animator animator;
    
    // 私有变量
    private float currentHealth;
    private bool facingRight;
    private bool isActive = false;
    private Rigidbody2D rb;
    private Transform player;
    private float flashCountdown;
    private Color originalColor;
    private Material flashMaterial;
    private bool isDead;
    
    private static readonly int FlashFactor = Shader.PropertyToID("_FlashFactor");
    private static readonly int FlashColor = Shader.PropertyToID("_FlashColor");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        originalColor = spriteRenderer.color;
        facingRight = defaultDirection == Direction.Right ? true : false;
        
        // 寻找玩家对象
        GameObject playerObj = PlayerContext.PlayerInstance;
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
 
        if (wallCheckPoint == null)
        {
            GameObject wallCheck = new GameObject("WallCheck");
            wallCheck.transform.SetParent(transform);
            wallCheck.transform.localPosition = new Vector3(facingRight ? 0.5f : -0.5f, 0, 0);
            wallCheckPoint = wallCheck.transform;
        }
        
        flashMaterial = spriteRenderer.material;
        /*
        if (flashMaterial.name != "Flash (Instance)")
            flashMaterial = null;
        */
        if(flashMaterial)
            flashMaterial.SetColor(FlashColor,hurtFlashColor);

        if (weight <= 0)
            weight = 0.0001f;
    }
   
    void Update()
    {
        
        AI();
        HandleHurt();
    }

    public void AI()
    {
        if(isDead)
            return;
        // 检查玩家是否在激活范围内
        CheckPlayerInRange();
        
        // 只有在激活状态下才执行AI逻辑
        if (!isActive) return;
        
        // 检查是否需要转向
        if (ShouldTurn())
        {
            Turn();
        }
        
        // 移动
        Move();
    }
    
    private void CheckPlayerInRange()
    {
        if (!player) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 如果玩家进入范围内，激活敌人
        if (distanceToPlayer <= activationRange)
        {
            isActive = true;
        }
        
    }
    
    private bool ShouldTurn()
    {
        // 检查前方是否有墙壁
        bool hitWall = Physics2D.Raycast(wallCheckPoint.position, 
            facingRight ? Vector2.right : Vector2.left, 
            wallCheckDistance, wallLayerMask);
        
        /*// 检查前方是否还有地面（防止掉下平台）
        bool hasGround = Physics2D.Raycast(groundCheckPoint.position, 
            Vector2.down, groundCheckDistance, groundLayerMask);
            */
        
        return hitWall /*|| !hasGround*/;
    }
    
    private void Turn()
    {
        facingRight = !facingRight;
        
        // 翻转精灵
        if (spriteRenderer)
        {
            spriteRenderer.flipX = facingRight;
        }
        
        // 更新墙壁检查点位置
        Vector3 wallCheckPos = wallCheckPoint.localPosition;
        wallCheckPos.x = facingRight ? Mathf.Abs(wallCheckPos.x) : -Mathf.Abs(wallCheckPos.x);
        wallCheckPoint.localPosition = wallCheckPos;
    }
    
    private void Move()
    {
        Vector2 movement = new Vector2(facingRight ? moveSpeed : -moveSpeed, rb.velocity.y);
        rb.velocity = movement;
    }
    
    public void TakeDamage(float damage, float knockbackPower, Direction knockbackDirection)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
        Flash();
        Knockback(knockbackPower,knockbackDirection);
    }

    public float GetAttackDamage()
    {
        return damage;
    }



    private void Die()
    {
        if (isDead) return;
        isDead = true;
        hurtCollider.includeLayers = 0;
        hurtCollider.excludeLayers = LayerMask.NameToLayer("Everything");
        Vector2 knockbackDirection = CalculateDeathKnockbackDirection();
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0.5f;
        rb.AddForce(knockbackDirection * 0.5f * deathKnockbackForce, ForceMode2D.Impulse);
        BounceComponent2D bounce = GetComponent<BounceComponent2D>();
        if (bounce)
        {
            bounce.SetBounceParameters(0f, 0f, 1);
            bounce.StartBouncing();
        }
        if(deathAudio)
            AudioManager.Instance.PlayOnShot(deathAudio, this);
        // 播放动画
        if (animator)
        {
            animator.SetTrigger("Death");
        }
    }
    private Vector2 CalculateDeathKnockbackDirection()
    {
        float angleRad = Mathf.Clamp(deathKnockbackAngle,0,90) * Mathf.Deg2Rad;
        float sign = facingRight ? -1 : 1;
        return new Vector2(sign* Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

 
    private void HandleHurt()
    {
        if (flashCountdown > 0)
        {
            flashCountdown -= Time.deltaTime;
        }
        else
        {
            spriteRenderer.color = originalColor;
            flashMaterial.SetFloat(FlashFactor, 0);
        }
    }
    private void Flash()
    {
        flashCountdown = hurtFlashTime;
        flashMaterial.SetFloat(FlashFactor, 1);
    }

    private void Knockback(float knockbackPower,Direction knockbackDirection)
    {
        if(knockbackDirection == Direction.Middle)
            return;
        float sign = knockbackDirection == Direction.Right ? 1 : -1;
        transform.Translate(new Vector3(sign * knockbackPower * Time.deltaTime / weight, 0,0), Space.World);
    }
    
    // 可视化调试
    private void OnDrawGizmosSelected()
    {
        // 绘制激活范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);
        
        // 绘制墙壁检测
        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 direction = facingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawRay(wallCheckPoint.position, direction * wallCheckDistance);
        }
      
    }
}