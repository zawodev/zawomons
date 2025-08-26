using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Systems;
using Models;
using TMPro;

namespace UI {
    public class GlobalTopBarPanel : MonoBehaviour
    {
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI woodText;
        public TextMeshProUGUI stoneText;
        public TextMeshProUGUI gemsText;

        private void OnEnable()
        {
            // Sprawdź czy GameManager i dane są dostępne
            if (GameManager.Instance != null)
            {
                // Subskrybuj eventy
                GameManager.Instance.OnPlayerDataReady += OnPlayerDataReady;
                GameManager.Instance.OnPlayerResourcesUpdated += OnResourcesUpdated;
                GameManager.Instance.OnPlayerGoldUpdated += OnGoldUpdated;
                GameManager.Instance.OnPlayerWoodUpdated += OnWoodUpdated;
                GameManager.Instance.OnPlayerStoneUpdated += OnStoneUpdated;
                GameManager.Instance.OnPlayerGemsUpdated += OnGemsUpdated;

                // Jeśli dane są już załadowane, odśwież od razu
                if (GameManager.Instance.IsPlayerDataLoaded())
                {
                    RefreshAll();
                }
            }
        }

        private void OnDisable()
        {
            // Odsubskrybuj eventy jeśli GameManager nadal istnieje
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDataReady -= OnPlayerDataReady;
                GameManager.Instance.OnPlayerResourcesUpdated -= OnResourcesUpdated;
                GameManager.Instance.OnPlayerGoldUpdated -= OnGoldUpdated;
                GameManager.Instance.OnPlayerWoodUpdated -= OnWoodUpdated;
                GameManager.Instance.OnPlayerStoneUpdated -= OnStoneUpdated;
                GameManager.Instance.OnPlayerGemsUpdated -= OnGemsUpdated;
            }
        }

        private void RefreshAll()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPlayerDataLoaded())
            {
                var playerData = GameManager.Instance.GetPlayerData();
                SetTexts(playerData);
            }
            else
            {
                // Ustaw domyślne wartości jeśli dane nie są załadowane
                SetDefaultTexts();
            }
        }

        private void SetTexts(PlayerData playerData)
        {
            if (playerData != null)
            {
                goldText.text = playerData.gold.ToString();
                woodText.text = playerData.wood.ToString();
                stoneText.text = playerData.stone.ToString();
                gemsText.text = playerData.gems.ToString();
            }
        }

        private void SetDefaultTexts()
        {
            goldText.text = "---";
            woodText.text = "---";
            stoneText.text = "---";
            gemsText.text = "---";
        }

        // Event handlers
        private void OnPlayerDataReady()
        {
            RefreshAll();
        }

        private void OnResourcesUpdated(PlayerData playerData) => SetTexts(playerData);
        private void OnGoldUpdated(int newGold) => goldText.text = newGold.ToString();
        private void OnWoodUpdated(int newWood) => woodText.text = newWood.ToString();
        private void OnStoneUpdated(int newStone) => stoneText.text = newStone.ToString();
        private void OnGemsUpdated(int newGems) => gemsText.text = newGems.ToString();
    }
}
