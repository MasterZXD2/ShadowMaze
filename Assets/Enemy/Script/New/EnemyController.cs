using UnityEngine;

public class EnemyController : MonoBehaviour
{
    #region Class Veriables
    [SerializeField] private EnemyBrain brain;
    public bool IsStop { get; private set; }
    #endregion

    #region Normal Function
    protected virtual void Awake()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.Add(gameObject);
    }

    protected virtual void Start()
    { }

    private void OnCollisionEnter(Collision collision)
    { }

    protected virtual void Update()
    { }

    protected virtual void OnDrawGizmos()
    { }
    #endregion

    #region Control Functions

    public void Die()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.Remove(gameObject);
        Destroy(gameObject);
    }
    public void Pause()
    {
        IsStop = true;
        brain.Pause();
    }
    public void Resume()
    {
        IsStop = false;
        brain.Resume();
    }
    #endregion
}
