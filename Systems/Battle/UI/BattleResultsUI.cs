using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Systems.Battle.UI
{
    public class BattleResultsUI : MonoBehaviour
    {
        [Header("Panel References")]
        public GameObject panelRoot;
        public Button rematchButton;
        public Button exitBattleButton;
        
        [Header("Results Display")]
        public TMP_Text winnerText;
        public TMP_Text battleSummaryText;
        
        [Header("Team Results")]
        public Transform teamAResultsParent;
        public Transform teamBResultsParent;
        public GameObject creatureResultPrefab;
        
        // Events
        public System.Action OnRematchClicked;
        public System.Action OnExitBattleClicked;
        
        private BattleState lastBattleState;
        
        private void Awake()
        {
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // Setup button events
            if (rematchButton != null)
            {
                rematchButton.onClick.AddListener(() => OnRematchClicked?.Invoke());
            }
            
            if (exitBattleButton != null)
            {
                exitBattleButton.onClick.AddListener(() => OnExitBattleClicked?.Invoke());
            }
            
            // Initially hidden
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
        
        public void ShowResults(BattleState battleState)
        {
            if (battleState == null)
            {
                Debug.LogWarning("Cannot show battle results with null battle state!");
                return;
            }
            
            lastBattleState = battleState;
            
            // Show panel
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            
            // Display winner
            DisplayWinner(battleState);
            
            // Display battle summary
            DisplayBattleSummary(battleState);
            
            // Display team results
            DisplayTeamResults(battleState);
            
            Debug.Log("Battle results panel shown");
        }
        
        public void HideResults()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
        
        private void DisplayWinner(BattleState battleState)
        {
            if (winnerText != null)
            {
                string winnerMessage = "";
                
                switch (battleState.winner)
                {
                    case "Team A":
                        winnerMessage = "Team A Wins!";
                        break;
                    case "Team B":
                        winnerMessage = "Team B Wins!";
                        break;
                    case "Draw":
                        winnerMessage = "Draw!";
                        break;
                    default:
                        winnerMessage = "Battle Finished";
                        break;
                }
                
                winnerText.text = winnerMessage;
            }
        }
        
        private void DisplayBattleSummary(BattleState battleState)
        {
            if (battleSummaryText != null)
            {
                string summary = $"Battle lasted {battleState.currentTurn + 1} turn(s)\n";
                
                // Count alive creatures
                int teamAAlive = battleState.teamA.Count(p => p.IsAlive);
                int teamBAlive = battleState.teamB.Count(p => p.IsAlive);
                
                summary += $"Team A: {teamAAlive}/{battleState.teamA.Count} creatures alive\n";
                summary += $"Team B: {teamBAlive}/{battleState.teamB.Count} creatures alive";
                
                battleSummaryText.text = summary;
            }
        }
        
        private void DisplayTeamResults(BattleState battleState)
        {
            // Clear previous results
            ClearTeamResults();
            
            // Display Team A results
            if (teamAResultsParent != null)
            {
                DisplayTeamCreatures(battleState.teamA, teamAResultsParent, "Team A");
            }
            
            // Display Team B results
            if (teamBResultsParent != null)
            {
                DisplayTeamCreatures(battleState.teamB, teamBResultsParent, "Team B");
            }
        }
        
        private void DisplayTeamCreatures(List<BattleParticipant> team, Transform parent, string teamName)
        {
            if (creatureResultPrefab == null)
            {
                Debug.LogWarning("CreatureResultPrefab is not assigned!");
                return;
            }
            
            foreach (var participant in team)
            {
                GameObject resultItem = Instantiate(creatureResultPrefab, parent);
                
                // Get UI components from prefab
                var nameText = resultItem.transform.Find("CreatureName")?.GetComponent<TMP_Text>();
                var statusText = resultItem.transform.Find("Status")?.GetComponent<TMP_Text>();
                var hpBar = resultItem.transform.Find("HPBar")?.GetComponent<Slider>();
                var hpText = resultItem.transform.Find("HPText")?.GetComponent<TMP_Text>();
                var creatureImage = resultItem.transform.Find("CreatureImage")?.GetComponent<Image>();
                
                // Set creature name
                if (nameText != null)
                {
                    nameText.text = participant.creature.name;
                }
                
                // Set status (alive/defeated)
                if (statusText != null)
                {
                    statusText.text = participant.IsAlive ? "Alive" : "Defeated";
                    statusText.color = participant.IsAlive ? Color.green : Color.red;
                }
                
                // Set HP bar
                if (hpBar != null)
                {
                    hpBar.maxValue = participant.creature.maxHP;
                    hpBar.value = participant.currentHP;
                }
                
                // Set HP text
                if (hpText != null)
                {
                    hpText.text = $"{participant.currentHP}/{participant.creature.maxHP}";
                }
                
                // Set creature image (placeholder for now)
                if (creatureImage != null)
                {
                    // TODO: Load actual creature sprite when sprites are implemented
                    // For now just set a color based on creature element
                    creatureImage.color = participant.creature.color;
                }
            }
        }
        
        private void ClearTeamResults()
        {
            if (teamAResultsParent != null)
            {
                foreach (Transform child in teamAResultsParent)
                {
                    Destroy(child.gameObject);
                }
            }
            
            if (teamBResultsParent != null)
            {
                foreach (Transform child in teamBResultsParent)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        private Color GetElementColor(CreatureElement element)
        {
            switch (element)
            {
                case CreatureElement.Fire: return Color.red;
                case CreatureElement.Water: return Color.blue;
                case CreatureElement.Ice: return Color.cyan;
                case CreatureElement.Stone: return Color.gray;
                case CreatureElement.Nature: return Color.green;
                case CreatureElement.Magic: return Color.magenta;
                case CreatureElement.DarkMagic: return Color.black;
                default: return Color.white;
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event listeners
            if (rematchButton != null)
            {
                rematchButton.onClick.RemoveAllListeners();
            }
            
            if (exitBattleButton != null)
            {
                exitBattleButton.onClick.RemoveAllListeners();
            }
        }
    }
}
