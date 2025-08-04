using UnityEngine;

public class GunDelay : MonoBehaviour
{
    [Header("延迟参数")]
    public float delaySeconds = 0.1f;      // 基础延迟时间（秒）
    public float jumpDelayMultiplier = 1.5f; // 跳跃时延迟倍率
    public Vector2 maxOffset = new Vector2(0.5f, 0.3f); // 最大水平/垂直偏移

    private Transform _player;
    private Vector3[] _positionBuffer;
    private int _bufferSize;
    private int _writeIndex = 0;
    private int _readIndex = 0;

    private void Start()
    {
        _player = PlayerContext.PlayerInstance.transform;

        // 计算缓冲区大小（基于FixedUpdate频率）
        int delayFrames = Mathf.CeilToInt(delaySeconds / Time.fixedDeltaTime);
        _bufferSize = delayFrames + 1;
        _positionBuffer = new Vector3[_bufferSize];

        // 初始化缓冲区
        for (int i = 0; i < _bufferSize; i++)
        {
            _positionBuffer[i] = _player.position;
        }
    }

    private void FixedUpdate()
    {
        // 记录当前帧玩家位置
        _positionBuffer[_writeIndex] = _player.position;

        // 计算动态延迟（跳跃时更长）
        int delayFrames = Mathf.FloorToInt(delaySeconds / Time.fixedDeltaTime);

        // 计算读取索引
        _readIndex = (_writeIndex - delayFrames + _bufferSize) % _bufferSize;

        // 移动写入指针
        _writeIndex = (_writeIndex + 1) % _bufferSize;
    }

    private void Update()
    {
        // 获取延迟后的世界坐标位置
        Vector3 delayedWorldPos = _positionBuffer[_readIndex];
        float sign = PlayerContext.Forward == Direction.Right ? 1 : -1;

        // 转换为相对于玩家的局部坐标偏移
        Vector3 localOffset = _player.InverseTransformPoint(delayedWorldPos);

        // 应用二维偏移限制
        localOffset.x = Mathf.Clamp(localOffset.x, -maxOffset.x, maxOffset.x);
        localOffset.y = Mathf.Clamp(localOffset.y, -maxOffset.y, maxOffset.y);
        
        // 更新枪械局部位置
        PlayerContext.NextGunLocalPosition += localOffset;
    }

}