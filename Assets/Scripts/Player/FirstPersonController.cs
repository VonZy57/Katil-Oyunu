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
    public float headBobAmplitude = 0.05f;
    public float headBobHorizontalAmp = 0.05f;
    private float headBobTimer = 0f;
    private Vector3 initialCameraPosition;

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
            HandleHeadBob();
        }
    }

    private void HandleRotation()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

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
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

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

        controller.Move(move * speed * Time.deltaTime);

        // Yerçekimi
        velocity.y += gravity * Time.deltaTime;
        if (controller.enabled)
            controller.Move(velocity * Time.deltaTime);
        else
            return; // Controller kapalýysa hareket etmeye çalýþma

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    private void HandleHeadBob()
    {
        if (controller.isGrounded && moveInput.magnitude > 0)
        {
            headBobTimer += Time.deltaTime * (isRunning ? runSpeed : walkSpeed) * headBobFrequency;
            float bobOffsetY = Mathf.Sin(headBobTimer) * headBobAmplitude;
            float bobOffsetX = Mathf.Cos(headBobTimer / 2) * headBobHorizontalAmp;
            cameraTransform.localPosition = initialCameraPosition + new Vector3(bobOffsetX, bobOffsetY, 0);
        }
        else
        {
            headBobTimer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, initialCameraPosition, Time.deltaTime * 5f);
        }
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