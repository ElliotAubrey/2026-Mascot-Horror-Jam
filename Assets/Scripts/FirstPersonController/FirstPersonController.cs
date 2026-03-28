using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove;
    public bool CanLook;
    bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded && !IsSliding;
    bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;
  
    [Header("Functional Options")]
    [SerializeField] bool canSprint = true;
    [SerializeField] bool canJump = true;
    [SerializeField] bool canCrouch = true;
    [SerializeField] bool canInteract = true;
    [SerializeField] bool canUseHeadbob = true;
    [SerializeField] bool willSlideOnSlopes = true;
    [SerializeField] bool canZoom = true;
    [SerializeField] bool useFootsteps = true;

    [Header("Controls")]
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] KeyCode interactKey = KeyCode.F;

    [Header("Movement Parameters")]
    [SerializeField] float walkSpeed = 3;
    [SerializeField] float sprintSpeed = 6;
    [SerializeField] float crouchSpeed = 1.5f;
    [SerializeField] float slopeSpeed = 8f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] float lookSpeedX = 2;
    [SerializeField, Range(1, 10)] float lookSpeedY = 2;
    [SerializeField, Range(1, 180)] float upperLookLimit = 90;
    [SerializeField, Range(1, 180)] float lowerLookLimit = 90;

    [Header("Jumping Parameters")]
    [SerializeField] float gravity = 30f;
    [SerializeField] float jumpForce = 8f;

    [Header("Crouch Parameters")]
    [SerializeField] float crouchingHeight = 0.5f;
    [SerializeField] float standingHeight = 2f;
    [SerializeField] float timeToCrouch = 0.25f;
    [SerializeField] Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] Vector3 standingCenter = new Vector3(0, 0, 0);
    bool isCrouching;
    bool duringCrouchAnimation;

    [Header("Interact Parameters")]
    [SerializeField] float range = 2f;
    [SerializeField] TextMeshProUGUI prompt;

    [Header("Headbob Parameters")]
    [SerializeField] float walkBobSpeed = 14;
    [SerializeField] float walkBobAmount = 0.05f;
    [SerializeField] float sprintBobSpeed = 18;
    [SerializeField] float sprintBobAmount = 0.1f;
    [SerializeField] float crouchBobSpeed = 8;
    [SerializeField] float crouchBobAmount = 0.025f;
    float defaultYPos;
    float timer;

    [Header("Zoom Parameters")]
    [SerializeField] float timeToZoom = 0.3f;
    [SerializeField] float zoomFOV = 30f;
    float defaultFOV;
    Coroutine zoomRoutine;

    [Header("Footstep Parameters")]
    [SerializeField] float baseStepSpeed = 0.5f;
    [SerializeField] float crouchStepMultiplier = 1.5f;
    [SerializeField] float sprintStepMultiplier = 0.6f;
    [SerializeField] AudioSource footStepAudioSource = default;
    [SerializeField] AudioClip[] woodClips = default;
    [SerializeField] AudioClip[] grassClips = default;
    [SerializeField] AudioClip[] metalClips = default;
    float footstepTimer = 0;

    float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : IsSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

    // SLIDER PARAMETERS

    Vector3 hitPointNomral;
    bool IsSliding
    {
        get
        {
            if(characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNomral = slopeHit.normal;
                return Vector3.Angle(hitPointNomral, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    Camera playerCamera;
    CharacterController characterController;

    Vector3 moveDirection;
    Vector2 currentInput;

    float xRotation;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if(CanMove) HandleMovementInput();
        if(CanLook) HandleMouseLook();
        if (canJump) HandleJump();
        if (canCrouch) HandleCrouch();
        if (canInteract) HandleInteract();
        if (canUseHeadbob) HandleHeadbob();
        if (canZoom) HandleZoom();
        if (useFootsteps) HandleFootsteps();
            
        ApplyMovement();

        moveDirection = new Vector3(0, moveDirection.y, 0);
        currentInput = Vector3.zero;
    }

    void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    void HandleMouseLook()
    {
        xRotation -= Input.GetAxis("Mouse Y") * lookSpeedY;
        xRotation = Mathf.Clamp(xRotation, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    void HandleJump()
    {
        if (ShouldJump) moveDirection.y = jumpForce;
    }

    void HandleCrouch()
    {
        if (ShouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    void HandleInteract()
    {
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, range))
        {
            if(hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                prompt.text = interactable.GetPrompt(interactKey);
                if(Input.GetKeyDown(interactKey))
                {
                    interactable.Interact(transform);
                }
            }
            else
            {
                prompt.text = string.Empty;
            }
        }
        else
        {
            prompt.text = string.Empty;
        }
    }

    void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;

        if(Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    void HandleZoom()
    {
        if(Input.GetKeyDown(zoomKey))
        {
            if(zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(true));
        }

        if (Input.GetKeyUp(zoomKey))
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(false));
        }
    }

    void HandleFootsteps()
    {
        if (!characterController.isGrounded) return;
        if (currentInput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;

        if(footstepTimer <= 0)
        {
            if (Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {
                    case "Footsteps/GRASS":
                        footStepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                        break;

                    case "Footsteps/METAL":
                        footStepAudioSource.PlayOneShot(metalClips[Random.Range(0, metalClips.Length - 1)]);
                        break;

                    case "Footsteps/WOOD":
                        footStepAudioSource.PlayOneShot(woodClips[Random.Range(0, woodClips.Length - 1)]);
                        break;

                    default:
                        footStepAudioSource.PlayOneShot(woodClips[Random.Range(0, woodClips.Length - 1)]);
                        break;
                }
            }

            footstepTimer = GetCurrentOffset;
        }
    }

    void ApplyMovement()
    {
        if(!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if(willSlideOnSlopes && IsSliding)
        {
            moveDirection += new Vector3(hitPointNomral.x, -hitPointNomral.y, hitPointNomral.z) * slopeSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    public float GetMoveSpeed()
    {
        return moveDirection.x + moveDirection.z;
    }

    IEnumerator CrouchStand()
    {
        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            yield break;
        }

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchingHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    IEnumerator ToggleZoom(bool isEnter)
    {
        float targetFOV = isEnter ? zoomFOV : defaultFOV;
        float startFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;

        while(timeElapsed < timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, timeElapsed / timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = targetFOV;
        zoomRoutine = null;
    }

}