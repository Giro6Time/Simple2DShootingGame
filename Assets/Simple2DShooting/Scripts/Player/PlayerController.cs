using System;
using System.Collections;
using System.Collections.Generic;
using GiroFrame;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    [Tooltip("影响击退")] public float weight = 1f;
    
    [Header("地面检测")]
    public Transform groundCheck;
    public float groundCheckBias = 0.2f;
    public LayerMask groundLayerMask = 1;
    [Header("踩头")]
    public float bounceForce = 8f;        // 踩敌人时的弹跳力度
    public float bounceThreshold = 0.7f;  // 踩踏判定的角度阈值（0-1之间，越接近1越严格）
    public float stepDamage = 1f;
    public float invincibleTime = 0.2f;
    [Header("死亡")] 
    public float deathKnockbackForce;
    public float deathKnockbackAngle;

    [Header("音效")] public AudioClip[] jumpAudio;
    public AudioClip deathAudio;
    
    [Header("可选设置")]
    public float coyoteTime = 0.1f; // 土狼时间：离开地面后仍可跳跃的时间
    public float jumpBufferTime = 0.1f; // 跳跃缓冲：提前按跳跃键的容错时间
    public float shootBufferTime = 0.01f; // 射击缓冲：提前按射击键的容错时间
    public float shootingScreenShakeIntensity = 0.01f;
    public float deathTimeScale = 0.3f;
    [Header("组件")] public SpriteRenderer sprite;
    public BounceComponent2D bounceComponent;
    public Animator animator;
    public IGun gun;
    public ParticleSystem jumpEffect;
    // 私有变量
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isGrounded;
    private bool wasGrounded;
    /// <summary>
    /// 跳跃冷却用于防止由于物理检测不精准导致的地面状态判断失败，短时间内可以多次跳跃的问题
    /// 这个值只需要设置一个不影响手感的值即可，因此外部不需要对他进行修改，仅当用户手速过快或者某些值设置的过于极端的时候才可能影响手感
    /// </summary>
    private float jumpCooldown = 0.1f; // 跳跃后的冷却时间
    private float jumpCooldownCounter;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float horizontalInput;
    private bool jumpInput;
    private float invisibleCounter;
    private float shootBufferCounter;
    private bool shootInput;
    private bool shot = false;
    private CameraController cameraController;

    void Start()
    {
        cameraController = LevelContext.CameraGO.GetComponent<CameraController>();

        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // 如果没有设置groundCheck，自动创建一个
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -boxCollider.size.y / 2 - 0.1f, 0);
            groundCheck = groundCheckObj.transform;
        }

        if (gun)
        {
            gun.Init();
        }
    }
    
    void Update()
    {
        if (PlayerContext.IsDead) return;
        invisibleCounter -= Time.deltaTime;
        // 获取输入
        GetInput();
        
        // 地面检测
        CheckGrounded();
        
        // 处理跳跃
        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleJump();
        // 处理射击
        HandleShootBuffer();
        HandleShoot();
    }
    
    void FixedUpdate()
    {
        if (PlayerContext.IsDead) return;

        // 水平移动
        HandleMovement();
        HandleLastShot();
    }
    
    void GetInput()
    {
        // WASD移动输入
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A))
            horizontalInput -= 1f;
        if (Input.GetKey(KeyCode.D))
            horizontalInput += 1f;
 
        // 跳跃输入 (W键或空格键)
        jumpInput = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space);

        shootInput = Input.GetKey(KeyCode.K);
    }
    
    
    void CheckGrounded()
    {
        wasGrounded = isGrounded;
        
        // 使用多点检测提高准确性
        Vector2 boxCenter = (Vector2)transform.position + boxCollider.offset;
        Vector2 boxSize = boxCollider.size * transform.localScale;
        
        // 在角色底部的多个点进行检测
        float leftX = boxCenter.x - boxSize.x  / 2;
        float rightX = boxCenter.x + boxSize.x  / 2;
        float centerX = boxCenter.x;
        
        bool leftHit = Physics2D.Raycast(new Vector2(leftX, boxCenter.y), Vector2.down, 
            boxSize.y / 2 + groundCheckBias, groundLayerMask);
        bool centerHit = Physics2D.Raycast(new Vector2(centerX, boxCenter.y), Vector2.down, 
            boxSize.y / 2 + groundCheckBias, groundLayerMask);
        bool rightHit = Physics2D.Raycast(new Vector2(rightX, boxCenter.y), Vector2.down, 
            boxSize.y / 2 + groundCheckBias, groundLayerMask);
            
        isGrounded = leftHit || centerHit || rightHit;
    }
    
    void HandleCoyoteTime()
    {
        //由于isGrounded检测不准确，经常导致跳跃后的下n帧立刻又重置了土狼时间的计时器，导致短时间内可以再跳一次
        if (isGrounded && jumpCooldownCounter <= 0)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }
    
    void HandleJumpBuffer()
    {
        if (jumpInput)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    void HandleJump()
    {
        // 当有跳跃输入且在土狼时间内时执行跳跃
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && jumpCooldownCounter <= 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            
            // 重置计数器
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            jumpCooldownCounter = jumpCooldown;
            
            if(jumpEffect)
                jumpEffect.Play();
            int ind = Random.Range(0, jumpAudio.Length);
            AudioManager.Instance.PlayOnShot(jumpAudio[ind], this);
        }
        
        // 可变跳跃高度：松开跳跃键时减少上升速度
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }
        
        if (jumpCooldownCounter > 0)
        {
            jumpCooldownCounter -= Time.deltaTime;
        }
    }

    void HandleShootBuffer()
    {
        if (shootInput)
        {
            shootBufferCounter = shootBufferTime;
        }
        else
        {
            shootBufferCounter -= Time.deltaTime;
        }
    }

    void HandleShoot()
    {
        if (shootBufferCounter > 0f)
        {
            if (gun.Shoot())
            {
                shot = true;
                shootBufferCounter = 0;
                cameraController.Shake(0.1f, shootingScreenShakeIntensity);
            }
        }
    }

    void HandleLastShot()
    {
        if (!shot)
            return;
        KnockBack();
        shot = false;
    }

    void KnockBack()
    {
        float sign = PlayerContext.Forward == Direction.Right ? 1 : -1;
        rb.velocity = rb.velocity - new Vector2(sign * gun.playerKnockBack * Time.deltaTime / weight, 0);
    }
    void HandleMovement()
    {
        // 水平移动
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        if(shootInput)
            return;
        var eulerAngles = transform.eulerAngles;
        if (horizontalInput > 0)
        {
            eulerAngles.y = 0;
            transform.eulerAngles = eulerAngles;
            PlayerContext.Forward = Direction.Right;
        }
        else if (horizontalInput < 0)
        {
            eulerAngles.y = 180;
            transform.eulerAngles = eulerAngles;
            PlayerContext.Forward = Direction.Left;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (PlayerContext.IsDead)
            return;
        if (collision.gameObject.CompareTag("Enemy"))
        {
            IEnemy enemy = collision.gameObject.GetComponent<IEnemy>();
            // 判断碰撞是否为踩踏
            if (IsStompingEnemy(collision))
            {
                Bounce();
                invisibleCounter = invincibleTime;
                enemy?.TakeDamage(stepDamage, 0, Direction.Middle);
            }
            else if(invisibleCounter < 0)
            {
                TakeDamage(enemy.GetAttackDamage());
            }
        }
    }

    private void TakeDamage(float damage)
    {
        PlayerContext.Hp -= damage;
        //由于玩家其实碰到一下就死了，所以先不搞多复杂的效果了，指确保基本的逻辑通顺
        if (PlayerContext.Hp <= 0)
        {
            Debug.Log("PlayerDie");
            Die();
        }
    }

    private void Die()
    {
        if (PlayerContext.IsDead) return;
        PlayerContext.IsDead = true;
        
        Vector2 knockbackDirection = CalculateKnockbackDirection();
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection * deathKnockbackForce, ForceMode2D.Impulse);
        
        // 开始死亡弹跳序列
        if (bounceComponent)
        {
            bounceComponent.StartBouncing(deathKnockbackForce * 0.5f); // 弹跳力度为击飞力度的一半
        }

        if (deathAudio)
        {
            AudioManager.Instance.PlayOnShot(deathAudio, this);
            AudioManager.Instance.IsPause = true;
        }

        Time.timeScale = deathTimeScale;
        
        // 播放动画
        if (animator)
        {
            animator.SetTrigger("Death");
        }
    }
    
    private Vector2 CalculateKnockbackDirection()
    {
        float angleRad = Mathf.Clamp(deathKnockbackAngle,0,90) * Mathf.Deg2Rad;
        
        float sign = PlayerContext.Forward == Direction.Right ? -1 : 1;
        return new Vector2(sign* Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
    /// <summary>
    /// 判断是否为踩踏敌人（从上方接触）
    /// </summary>
    private bool IsStompingEnemy(Collision2D collision)
    {
        // 基于碰撞法向量判断
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 如果碰撞点的法向量主要朝向上方，说明玩家在敌人上方
            if (contact.normal.y > bounceThreshold)
            {
                return true;
            }
        }
        return false;
    }
    
    
    /// <summary>
    /// 踩敌人时的弹跳效果
    /// </summary>
    private void Bounce()
    {
        // 重置垂直速度并施加向上的力
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        
        //AudioManager.Instance.PlayOnShot("Bounce");
    }
    
    // 在Scene视图中绘制地面检测范围
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            var pos = transform.position;
            pos.y -= (boxCollider.size.y * transform.localScale.y / 2 + groundCheckBias);
            var from = pos;
            from.x -= boxCollider.size.x * transform.localScale.x / 2;
            var to = pos;
            to.x += boxCollider.size.x * transform.localScale.x / 2;

            Gizmos.DrawLine(from, to);
        }
    }
}
