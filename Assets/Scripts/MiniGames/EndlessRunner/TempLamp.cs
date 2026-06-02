using UnityEngine;

public class TempLamp : MonoBehaviour
{
    public Material matOn;
    public Material matOff;
    public float interval = 0.5f;
    public GameObject toggleObject;

    private Renderer _renderer;
    private float _timer;
    private bool _isOn = true;

    void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _renderer.material = matOn;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= interval)
        {
            _timer = 0f;
            _isOn = !_isOn;
            _renderer.material = _isOn ? matOn : matOff;
            if (toggleObject != null)
                toggleObject.SetActive(!_isOn);
        }
    }
}
