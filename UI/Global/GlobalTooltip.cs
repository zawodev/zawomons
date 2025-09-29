using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

namespace UI.Global {
    public class GlobalTooltip : MonoBehaviour {
        public static GlobalTooltip Instance { get; private set; }
        public TextMeshProUGUI tooltipText;
        public RectTransform tooltipRect;
        
        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Wyłącz raycast blocking dla tooltipa
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            
            HideTooltip();
        }

        void Update() {
            if (tooltipText.gameObject.activeInHierarchy) {
                // Podążaj za kursorem używając nowego Input System
                Vector2 mousePos = Mouse.current.position.ReadValue();
                
                // Konwertuj pozycję myszy na pozycję w Canvas space
                Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
                if (canvas != null) {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        mousePos,
                        canvas.worldCamera,
                        out Vector2 localPoint);
                    
                    tooltipRect.localPosition = localPoint + new Vector2(15, 15);
                }
            }
        }

        public void ShowTooltip(string text) {
            tooltipText.text = text;
            tooltipText.gameObject.SetActive(true);
        }

        public void HideTooltip() {
            tooltipText.gameObject.SetActive(false);
        }
    }
} 