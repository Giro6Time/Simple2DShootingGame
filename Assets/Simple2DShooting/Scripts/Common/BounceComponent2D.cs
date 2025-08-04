
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 通用弹跳组件 - 可用于死亡弹跳、踩敌人弹跳等场景
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BounceComponent2D : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("Bounce Force 有可能会被代码覆盖")]
    [SerializeField] private float bounceForce = 8f;                    // 初始弹跳力度
    [SerializeField] private float bounceReduction = 0.6f;              // 弹跳力度衰减系数
    [SerializeField] private int maxBounces = 3;                        // 最大弹跳次数
    [SerializeField] private float minBounceVelocity = 2f;              // 最小弹跳速度
    [SerializeField] private float horizontalDamping = 0.8f;            // 水平速度衰减
    [SerializeField] private LayerMask bounceLayerMask = -1;            // 可弹跳的图层
    [SerializeField] private float groundCheckThreshold = 0.7f;         // 地面检测阈值
    
    [Header("Events")]
    public UnityEvent OnBounceStart;                                    // 开始弹跳时触发
    public UnityEvent<int> OnBounce;                                    // 每次弹跳时触发（传递当前弹跳次数）
    public UnityEvent OnBounceEnd;                                      // 弹跳结束时触发
    
    // 私有变量
    private Rigidbody2D rb;
    private bool isBouncing = false;
    private int currentBounces = 0;
    private float originalBounceForce;
    
    // 属性
    public bool IsBouncing => isBouncing;
    public int CurrentBounces => currentBounces;
    public int MaxBounces => maxBounces;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalBounceForce = bounceForce;
        
        if (rb == null)
        {
            Debug.LogError($"BounceComponent requires a Rigidbody2D component on {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 开始弹跳序列
    /// </summary>
    /// <param name="initialForce">初始弹跳力度（可选，使用默认值如果不指定）</param>
    public void StartBouncing(float? initialForce = null)
    {
        if (isBouncing) return;
        
        isBouncing = true;
        currentBounces = 0;
        
        if (initialForce.HasValue)
        {
            bounceForce = initialForce.Value;
            originalBounceForce = initialForce.Value;
        }
        
        OnBounceStart?.Invoke();
    }
    
    /// <summary>
    /// 停止弹跳序列
    /// </summary>
    public void StopBouncing()
    {
        if (!isBouncing) return;
        
        isBouncing = false;
        currentBounces = 0;
        bounceForce = originalBounceForce;
        rb.velocity = new Vector2(rb.velocity.x * horizontalDamping, 0);

        OnBounceEnd?.Invoke();
    }
    
    /// <summary>
    /// 执行一次弹跳
    /// </summary>
    /// <param name="force">弹跳力度（可选）</param>
    public void PerformBounce(float? force = null)
    {
        if (!rb) return;
        
        float actualForce = force ?? bounceForce;
        
        // 应用弹跳力
        rb.velocity = new Vector2(rb.velocity.x * horizontalDamping, 0);
        rb.AddForce(Vector2.up * actualForce, ForceMode2D.Impulse);
        
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isBouncing) return;
        
        // 检查是否碰撞到可弹跳的层
        if (!IsInLayerMask(collision.gameObject.layer, bounceLayerMask)) return;
        
        // 检查是否是从上方碰撞
        if (!IsGroundCollision(collision)) return;
        
        HandleBounce();
    }
    
    private void HandleBounce()
    {
        currentBounces++;
        
        // 计算当前弹跳力度
        float currentBounceForce = originalBounceForce * Mathf.Pow(bounceReduction, currentBounces);
        
        // 检查是否应该停止弹跳
        if (currentBounces >= maxBounces || currentBounceForce < minBounceVelocity)
        {
            StopBouncing();
            return;
        }
        
        // 执行弹跳
        PerformBounce(currentBounceForce);
        
        // 触发事件
        OnBounce?.Invoke(currentBounces);
        
    }
    
    private bool IsGroundCollision(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > groundCheckThreshold)
            {
                return true;
            }
        }
        return false;
    }
    
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }
    
    /// <summary>
    /// 重置弹跳参数到初始状态
    /// </summary>
    public void ResetBounce()
    {
        StopBouncing();
        bounceForce = originalBounceForce;
    }
    
    /// <summary>
    /// 设置弹跳参数
    /// </summary>
    public void SetBounceParameters(float force, float reduction, int maxCount)
    {
        bounceForce = force;
        originalBounceForce = force;
        bounceReduction = reduction;
        maxBounces = maxCount;
    }
}
