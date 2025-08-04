using GiroFrame;

public class GameRoot :  SingletonMono<GameRoot>
{
    /// <summary><see cref="Awake"/>
    /// </summary>
    protected override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
        DontDestroyOnLoad(gameObject);

        InitManager();
    }

    ///<summary>初始化所有管理器
    /// </summary>
    private void InitManager()
    {
        ManagerBase[] managers = GetComponentsInChildren<ManagerBase>();
        foreach (ManagerBase manager in managers)
        {
            manager.Init();
        }
    }

}

