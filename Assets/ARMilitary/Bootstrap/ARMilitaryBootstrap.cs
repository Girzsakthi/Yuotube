using ARMilitary.AR;
using ARMilitary.Instructor;
using ARMilitary.Player;
using ARMilitary.Shared;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace ARMilitary.Bootstrap
{
    /// <summary>
    /// Drop this component onto a single empty GameObject in a new Unity scene.
    /// It builds the entire AR Military Trainer at runtime — no manual scene setup required.
    /// </summary>
    [AddComponentMenu("ARMilitary/Bootstrap")]
    [DisallowMultipleComponent]
    public class ARMilitaryBootstrap : MonoBehaviour
    {
        private Canvas         _uiCanvas;
        private ARSession      _arSession;
        private Camera         _arCamera;
        private ARPlaneManager _planeManager;
        private ARRaycastManager _raycastManager;
        private ARAnchorManager  _anchorManager;

        private ModeSelectUI _modeSelectUI;
        private AppMode      _mode = AppMode.None;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            BuildSystems();
            BuildARSession();
            BuildCamera();
            BuildCanvas();
            ShowModeSelect();
        }

        // ── Singletons / shared systems ──────────────────────────────────────
        private void BuildSystems()
        {
            var systemsGO = new GameObject("ARMilitary_Systems");
            DontDestroyOnLoad(systemsGO);

            systemsGO.AddComponent<MainThreadDispatcher>();
            systemsGO.AddComponent<NetworkManager>();
        }

        // ── AR Foundation session ────────────────────────────────────────────
        private void BuildARSession()
        {
            var sessionGO = new GameObject("AR Session");
            _arSession = sessionGO.AddComponent<ARSession>();
            sessionGO.AddComponent<ARInputManager>();

            var sessionCtrl = sessionGO.AddComponent<ARSessionController>();

            var originGO = new GameObject("XR Origin");
            var origin = originGO.AddComponent<Unity.XR.CoreUtils.XROrigin>();

            // Camera under origin
            var camOffset = new GameObject("Camera Offset");
            camOffset.transform.SetParent(originGO.transform, false);

            var camGO = new GameObject("AR Camera");
            camGO.transform.SetParent(camOffset.transform, false);
            _arCamera = camGO.AddComponent<Camera>();
            _arCamera.clearFlags      = CameraClearFlags.Color;
            _arCamera.backgroundColor = Color.black;
            _arCamera.nearClipPlane   = 0.1f;
            _arCamera.farClipPlane    = 100f;
            _arCamera.tag             = "MainCamera";

            camGO.AddComponent<ARCameraManager>();
            camGO.AddComponent<ARCameraBackground>();

            origin.Camera = _arCamera;
            origin.CameraFloorOffsetObject = camOffset;

            _planeManager    = originGO.AddComponent<ARPlaneManager>();
            _raycastManager  = originGO.AddComponent<ARRaycastManager>();
            _anchorManager   = originGO.AddComponent<ARAnchorManager>();

            // Plane visualizer (optional debug mesh)
            _planeManager.planePrefab = CreatePlanePrefab();

            // Initialize AR session controller
            sessionCtrl.Initialize(_arSession);

            var anchorMgrComp = originGO.AddComponent<PlaneAnchorManager>();
            anchorMgrComp.Initialize(_raycastManager, _anchorManager, _arCamera);
        }

        // ── UI Canvas (Screen Space Overlay) ─────────────────────────────────
        private void BuildCanvas()
        {
            var canvasGO = new GameObject("UI Canvas");
            _uiCanvas = canvasGO.AddComponent<Canvas>();
            _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _uiCanvas.sortingOrder = 10;

            canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        private void BuildCamera()
        {
            // AR camera already built inside BuildARSession; ensure EventSystem exists
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── Mode select screen ───────────────────────────────────────────────
        private void ShowModeSelect()
        {
            _modeSelectUI = gameObject.AddComponent<ModeSelectUI>();
            _modeSelectUI.OnModeSelected += ActivateMode;
            _modeSelectUI.Build(_uiCanvas);
        }

        public void ActivateMode(AppMode mode)
        {
            if (_mode != AppMode.None) return;
            _mode = mode;

            // Clear mode select UI
            foreach (Transform child in _uiCanvas.transform)
                Destroy(child.gameObject);

            switch (mode)
            {
                case AppMode.Instructor: ActivateInstructor(); break;
                case AppMode.Player:     ActivatePlayer();     break;
            }
        }

        private void ActivateInstructor()
        {
            // Instructor doesn't need AR session
            _planeManager.enabled   = false;
            _raycastManager.enabled = false;
            _arCamera.clearFlags    = CameraClearFlags.SolidColor;
            _arCamera.backgroundColor = new Color(0.05f, 0.08f, 0.05f);

            var instructorGO = new GameObject("InstructorSystem");
            instructorGO.AddComponent<InstructorController>();
            instructorGO.AddComponent<InstructorUIManager>().BuildUI(_uiCanvas);
        }

        private void ActivatePlayer()
        {
            // Enable AR
            _planeManager.enabled   = true;
            _raycastManager.enabled = true;

            // HUD
            var hudGO = new GameObject("HUD");
            var hud = hudGO.AddComponent<MilitaryHUD>();
            hud.Build(_uiCanvas.transform);

            // Player controller
            var playerGO = new GameObject("PlayerSystem");
            playerGO.AddComponent<PlayerController>();
        }

        // ── Plane prefab (semi-transparent grid overlay) ─────────────────────
        private static GameObject CreatePlanePrefab()
        {
            var go = new GameObject("DetectedPlane");
            var meshFilter   = go.AddComponent<MeshFilter>();
            var meshRenderer = go.AddComponent<MeshRenderer>();

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.2f, 1f, 0.2f, 0.15f);
            meshRenderer.material = mat;

            go.AddComponent<ARPlaneMeshVisualizer>();
            go.SetActive(false);
            return go;
        }
    }
}
