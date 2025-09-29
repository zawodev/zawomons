using UnityEngine;
using UnityEngine.UI;

using Systems.Creatures.UI;
using UI.Social;

namespace UI.Global
{
    public class GlobalWheelPanel : MonoBehaviour
    {
        public Button buttonA;
        public Button buttonB;
        public Button buttonC;
        public Button buttonD;
        public Button buttonE;
        public Button buttonF;

        [Header("Panel References")]
        public CreatureCollectionPanel creatureCollectionPanel;
        public CreatureDetailPanel creatureDetailPanel;
        public FriendsPanel friendsPanel;

        private void OnEnable()
        {
            // Initialize buttons or set up event listeners if needed
            buttonA.onClick.AddListener(OnButtonAClicked);
            buttonB.onClick.AddListener(OnButtonBClicked);
            buttonC.onClick.AddListener(OnButtonCClicked);
            buttonD.onClick.AddListener(OnButtonDClicked);
            buttonE.onClick.AddListener(OnButtonEClicked);
            buttonF.onClick.AddListener(OnButtonFClicked);
        }

        private void OnDisable()
        {
            // Clean up event listeners
            buttonA.onClick.RemoveListener(OnButtonAClicked);
            buttonB.onClick.RemoveListener(OnButtonBClicked);
            buttonC.onClick.RemoveListener(OnButtonCClicked);
            buttonD.onClick.RemoveListener(OnButtonDClicked);
            buttonE.onClick.RemoveListener(OnButtonEClicked);
            buttonF.onClick.RemoveListener(OnButtonFClicked);
        }

        private void OnButtonAClicked()
        {
            Debug.Log("Button A clicked - Opening Friends Panel");
            if (friendsPanel != null)
            {
                if (!friendsPanel.gameObject.activeSelf)
                    friendsPanel.ShowPanel();
                else
                    friendsPanel.HidePanel();
            }
            else
            {
                Debug.LogWarning("FriendsPanel reference is not set in GlobalWheelPanel!");
            }
        }

        private void OnButtonBClicked()
        {
            Debug.Log("Button B clicked - Opening Creatures Collection");
            if (creatureCollectionPanel != null)
            {
                if (!creatureCollectionPanel.gameObject.activeSelf)
                    creatureCollectionPanel.ShowPanel();
                else
                {
                    creatureCollectionPanel.HidePanel();
                    // Also hide detail panel when closing collection panel
                    if (creatureDetailPanel != null)
                    {
                        creatureDetailPanel.HidePanel();
                    }
                }
            }
            else
            {
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
        private void OnButtonFClicked()
        {
            Debug.Log("Button F clicked");
            // Add functionality for Button F
        }
    }
}
