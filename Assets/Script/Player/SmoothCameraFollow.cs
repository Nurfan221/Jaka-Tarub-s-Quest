using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour // Follow plyaer with damping
{
    public static SmoothCameraFollow Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);

        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public Transform target;
    public Vector3 offset = new(0, 0, -10);
    public float damping;
    public ParticleSystem particleHujan;

    Vector3 velocity = Vector3.zero;

    private void Start()
    {
        particleHujan = gameObject.GetComponentInChildren<ParticleSystem>();
    }

    private void LateUpdate()
    {
        Vector3 movePos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, movePos, ref velocity, damping);
    }

    public void EnterHouse(bool inHouse)
    {
        Debug.Log("Hujan Masuk rumah: " + inHouse);
        if (TimeManager.Instance.isRain)
        {
            if (inHouse)
            {
                particleHujan.Stop();
            }
            else
            {
                particleHujan.Play();
            }
        }
        else
        {
            Debug.Log("Tidak hujan, pastikan hujan mati");
            particleHujan.Stop();
        }
    }
}
