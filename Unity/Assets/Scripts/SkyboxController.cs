using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [SerializeField, Range(0f, 5f)] private float skyboxSpeed = 0.1f;

    private void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyboxSpeed);
    }
}
