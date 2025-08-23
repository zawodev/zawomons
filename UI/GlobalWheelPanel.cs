using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GlobalWheelPanel : MonoBehaviour
    {
        public Button buttonA;
        public Button buttonB;
        public Button buttonC;
        public Button buttonD;
        public Button buttonE;
        
        [Header("Panel References")]
        public CreatureCollectionPanel creatureCollectionPanel;

        private void OnEnable()
        {
        // Initialize buttons or set up event listeners if needed
        buttonA.onClick.AddListener(OnButtonAClicked);
        buttonB.onClick.AddListener(OnButtonBClicked);
        buttonC.onClick.AddListener(OnButtonCClicked);
        buttonD.onClick.AddListener(OnButtonDClicked);
        buttonE.onClick.AddListener(OnButtonEClicked);
        }

        private void OnDisable()
        {
            // Clean up event listeners
            buttonA.onClick.RemoveListener(OnButtonAClicked);
            buttonB.onClick.RemoveListener(OnButtonBClicked);
            buttonC.onClick.RemoveListener(OnButtonCClicked);
            buttonD.onClick.RemoveListener(OnButtonDClicked);
            buttonE.onClick.RemoveListener(OnButtonEClicked);
        }

        private void OnButtonAClicked()
        {
            Debug.Log("Button A clicked");
            // Add functionality for Button A
        }

        private void OnButtonBClicked()
        {
            Debug.Log("Button B clicked - Opening Creatures Collection");
            if (creatureCollectionPanel != null) {
                creatureCollectionPanel.ShowPanel();
            } else {
                Debug.LogWarning("CreatureCollectionPanel reference is not set in GlobalWheelPanel!");
            }
        }
        
        private void OnButtonCClicked()
        {
            Debug.Log("Button C clicked");
            // Add functionality for Button C
        }
        
        private void OnButtonDClicked()
        {
            Debug.Log("Button D clicked");
            // Add functionality for Button D
        }
        
        private void OnButtonEClicked()
        {
            Debug.Log("Button E clicked");
            // Add functionality for Button E
        }
    }
}
