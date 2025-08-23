using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Systems;
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
            RefreshAll();
            GameAPI.OnPlayerResourcesUpdated += OnResourcesUpdated;
            GameAPI.OnPlayerGoldUpdated += OnGoldUpdated;
            GameAPI.OnPlayerWoodUpdated += OnWoodUpdated;
            GameAPI.OnPlayerStoneUpdated += OnStoneUpdated;
            GameAPI.OnPlayerGemsUpdated += OnGemsUpdated;
        }

        private void OnDisable()
        {
            GameAPI.OnPlayerResourcesUpdated -= OnResourcesUpdated;
            GameAPI.OnPlayerGoldUpdated -= OnGoldUpdated;
            GameAPI.OnPlayerWoodUpdated -= OnWoodUpdated;
            GameAPI.OnPlayerStoneUpdated -= OnStoneUpdated;
            GameAPI.OnPlayerGemsUpdated -= OnGemsUpdated;
        }

        private async void RefreshAll()
        {
            var res = await GameAPI.GetPlayerResourcesAsync();
            SetTexts(res);
        }

        private void SetTexts(GameAPI.PlayerResources res)
        {
            goldText.text = res.gold.ToString();
            woodText.text = res.wood.ToString();
            stoneText.text = res.stone.ToString();
            gemsText.text = res.gems.ToString();
        }

        private void OnResourcesUpdated(GameAPI.PlayerResources res) => SetTexts(res);
        private void OnGoldUpdated(int newGold) => goldText.text = newGold.ToString();
        private void OnWoodUpdated(int newWood) => woodText.text = newWood.ToString();
        private void OnStoneUpdated(int newStone) => stoneText.text = newStone.ToString();
        private void OnGemsUpdated(int newGems) => gemsText.text = newGems.ToString();
    }
}
