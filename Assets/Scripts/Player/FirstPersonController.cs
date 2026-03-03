using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float starisSpeed = 1.5f;
    public float gravity = -9.81f;

    [Header("Kamera Ayarlarý")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;

    [Header("Oturma Kýsýtlamalarý")]
    public float sittingLookLimit = 45f; // Saða sola yukarý aþaðý limit

    private PlayerControls controls;
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;

    // Bakýþ açýlarý
    private float xRotation = 0f; // Yukarý/Aþaðý (Pitch)
    private float yRotation = 0f; // Saða/Sola (Yaw) - Sadece otururken kullanýlýr

    private Vector3 velocity;
    private bool isRunning;

    // Oturma durumu kontrolü
    public bool IsSitting { get; private set; } = false;

    [Header("Head Bob Ayarlarý")]
    public float headBobFrequency = 1.5f;
    public float headBobVerticalAmp = 0.05f;
    public float headBobHorizontalAmp = 0.05f;
    private float headBobTimer = 0f;
    private Vector3 initialCameraPosition;

    [Header("Head Tilt Ayarlarý")]
    public float tiltAngle = 1f;
    public float tiltSpeed = 5f;
    private float currentTilt = 0f;

    [Header("Breathing Ayarlarý")]
    public float breathFrequency = 1.0f;
    public float breathAmpX = 0.01f;
    public float breathAmpY = 0.01f;
    private float breathTimer = 0f;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Sprint.performed += ctx => isRunning = true;
        controls.Player.Sprint.canceled += ctx => isRunning = false;

        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform != null)
            initialCameraPosition = cameraTransform.localPosition;
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();


    private void Update()
    {
        HandleRotation();

        // Eðer oturmuyorsak hareket et ve yerçekimi uygula
        if (!IsSitting)
        {
            HandleMovement();
            HandleCameraMotions();
        }
    }

    private void HandleRotation()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        //Kamera tilt için - a ve d tuþlarýna basýldýðýnda hareket kamera belli belirsiz saða ve sola eðilecek.
        float targetTilt = IsSitting ? 0f : -moveInput.x * tiltAngle; 
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        if (IsSitting)
        {
            // --- OTURMA MODU ROTASYONU ---

            // Yukarý Aþaðý (Pitch) - Limitli
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -sittingLookLimit, sittingLookLimit);

            // Saða Sola (Yaw) - Limitli (Normalde gövde dönerdi, þimdi sadece kafa)
            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, -sittingLookLimit, sittingLookLimit);

            // Hem X hem Y rotasyonunu kameraya uygula (Gövde sabit kalýr)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
        else
        {
            // --- NORMAL MOD ROTASYONU ---

            // Yukarý Aþaðý (Pitch) - 90 Derece Limit
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);

            // Saða Sola (Yaw) - Gövdeyi döndür
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    private void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        float speed;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 3f))
        {
            if (hit.collider.CompareTag("Stairs"))
            {
                speed = starisSpeed;
            }
            else
            {
                speed = isRunning ? runSpeed : walkSpeed;
            }
        }
        else
        {
            speed = isRunning ? runSpeed : walkSpeed;
        }

        // 1. Yerçekimi Hesaplamasý (Henüz hareket ettirme)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        // 2. Ýki hareketi birleþtir (Yatay hýz + Dikey yerçekimi)
        // move vektöründe Y her zaman 0'dýr, velocity vektöründe ise X ve Z 0'dýr.
        Vector3 finalMovement = (move * speed) + velocity;

        // 3. Tek seferde hareket ettir
        if (controller.enabled)
        {
            controller.Move(finalMovement * Time.deltaTime);
        }
    }


    private void HandleCameraMotions() //BREATHING VE HEADBOB HAREKETLERÝ
    {
        Vector3 targetOffset = Vector3.zero;
        float currentRealSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            if (currentRealSpeed > 0.1f)
            {
                headBobTimer += Time.deltaTime * currentRealSpeed * headBobFrequency;

                float bobOffsetY = Mathf.Sin(headBobTimer) * headBobVerticalAmp;
                float bobOffsetX = Mathf.Cos(headBobTimer / 2f) * headBobHorizontalAmp;

                targetOffset = new Vector3(bobOffsetX, bobOffsetY, 0f);
            }
            else
            {
                breathTimer += Time.deltaTime * breathFrequency;

                float breathOffsetY = Mathf.Sin(breathTimer) * breathAmpY;
                float breathOffsetX = Mathf.Cos(breathTimer / 2f) * breathAmpX;

                targetOffset = new Vector3(breathOffsetX, breathOffsetY, 0f);

                headBobTimer = 0f;
            }
        }
        Vector3 targetPosition = initialCameraPosition + targetOffset;
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetPosition, Time.deltaTime * 10f);
    }


    // Dýþarýdan (Sitting scriptinden) çaðrýlacak fonksiyon
    public void SetSittingState(bool state)
    {
        IsSitting = state;

        if (state)
        {
            // Oturmaya baþladýðýmýzda kafa rotasyonlarýný sýfýrla (öne bak)
            xRotation = 0f;
            yRotation = 0f;
            velocity = Vector3.zero; // Kaymayý engelle
        }
        else
        {
            // Kalktýðýmýzda kamera açýsýný tekrar düzelt
            yRotation = 0f;
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    
}