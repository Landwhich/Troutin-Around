using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.EventSystems;

namespace MultiUser
{
    public class MultiUserPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public CharacterController controller;
        public float interactDistance = 10f;
        public float speed = 12.0f;
        public GameObject voxel;
        public Vector3 CurrentMove;
        private Transform cameraTransform;
        public GameObject placementPreviewPrefab; 
        private GameObject placementPreviewInstance;
        public static bool IsChatFocused = false;

        public override void OnNetworkSpawn()
        {
            Position.OnValueChanged += OnStateChanged;

            if (IsOwner)
            {
                cameraTransform = Camera.main.transform;
                controller.enabled = true;
                var vcam = GetComponentInChildren<CinemachineCamera>();
                if (vcam != null) vcam.Priority = 100;

                if (IsHost && placementPreviewPrefab != null)
                {
                    placementPreviewInstance = Instantiate(placementPreviewPrefab);
                    placementPreviewInstance.SetActive(false);
                }
            }
            else
            {
                var vcam = GetComponentInChildren<CinemachineCamera>();
                if (vcam != null) vcam.gameObject.SetActive(false);
                controller.enabled = false;
            }
        }

        void Start()
        {
            if (IsOwner) LockCursor();
        }

        public static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            if (!IsOwner) return;
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                if(Cursor.visible)
                UnlockCursor();
            }
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                UnlockCursor();
                return;
            }

            if (IsChatFocused)
            {
                UnlockCursor();
                CurrentMove = Vector3.zero;
                return;
            }
            else
            {
                if (!Keyboard.current.enterKey.isPressed &&
                    !Keyboard.current.numpadEnterKey.isPressed &&
                    Cursor.lockState != CursorLockMode.Locked)
                {
                    LockCursor();
                }
            }

            Vector2 moveInput = Vector2.zero; float y = 0;
            if (Keyboard.current != null) {
                moveInput = new Vector2 (
                    (Keyboard.current.aKey.isPressed ? -1 : 0) + (Keyboard.current.dKey.isPressed ?  1 : 0),
                    (Keyboard.current.wKey.isPressed ?  1 : 0) + (Keyboard.current.sKey.isPressed ? -1 : 0)
                );
                y = Keyboard.current.spaceKey.isPressed ? 1 : 0;
                if (Keyboard.current.shiftKey.isPressed) {y *= -1;}
            }
            Vector3 move = cameraTransform.right * moveInput.x + cameraTransform.forward * moveInput.y;
            move.y = y;
            CurrentMove = move * speed;

            if (!IsHost) return;

            Ray r = new Ray(
                cameraTransform.position + cameraTransform.forward * 0.5f,
                cameraTransform.forward
            );

            if (Physics.Raycast(r, out RaycastHit hit, interactDistance))
            {
                Vector3Int gridPos = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);

                Vector3 worldPos = GridToWorld(gridPos);

                if (placementPreviewInstance != null)
                {
                    placementPreviewInstance.SetActive(true);
                    placementPreviewInstance.transform.position = worldPos;
                }

                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    PlaceBlock(gridPos);
                }
                if (Mouse.current.leftButton.wasPressedThisFrame && hit.collider.gameObject.CompareTag("Voxel"))
                {
                    DestroyBlock(gridPos, hit.collider.gameObject);
                }
            }
            else
            {
                if (placementPreviewInstance != null)
                    placementPreviewInstance.SetActive(false);
            }
        }

        void FixedUpdate() {
            if (!IsOwner) return;
            if (transform.position.y < 1) 
                {CurrentMove.y = Mathf.Clamp(CurrentMove.y, 0, 100);}
            controller.Move(CurrentMove * Time.fixedDeltaTime);
            Position.Value = transform.position; 
        }

        public override void OnNetworkDespawn()
        {
            Position.OnValueChanged -= OnStateChanged;
        }

        void OnStateChanged(Vector3 previous, Vector3 current)
        {
            if (!IsOwner) transform.position = current;
        }

        public void PlaceBlock(Vector3Int gridPos)
        {
            VoxelGrid.Instance.PlaceVoxelServerRpc(gridPos);
        }

        public void DestroyBlock(Vector3Int gridPos, GameObject voxel)
        {
            Destroy(voxel);
            VoxelGrid.Instance.DestroyVoxelServerRpc(gridPos);
        }

        Vector3 GridToWorld(Vector3Int gridPos)
        {
            return new Vector3(
                gridPos.x + 0.5f,
                gridPos.y + 0.5f,
                gridPos.z + 0.5f
            );
        }
    }
}