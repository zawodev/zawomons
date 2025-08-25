using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Models;
using Systems;

namespace Systems.Battle.UI
{
    public class TeamSelectionUI : MonoBehaviour
    {
        [Header("UI Components")]
        public Transform zawomonGridParent;
        public GameObject zawomonButtonPrefab;
        public TextMeshProUGUI infoText;
        public Button startBattleButton;
        
        [Header("Battle Settings")]
        public int maxTeamSize = 3;
        public int gridCols = 4;
        
        // Teams
        private List<Creature> teamA = new();
        private List<Creature> teamB = new();
        
        // UI State
        private List<GameObject> zawomonButtons = new();
        private int redCursor = 0;
        private int blueCursor = 0;
        private bool redReady = false;
        private bool blueReady = false;
        private int totalZawomons = 0;
        
        // Events
        public System.Action<List<Creature>, List<Creature>> OnTeamsSelected;

        void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.PlayerData != null)
                {
                    GenerateZawomonGrid();
                    UpdateInfoText();
                }
                else
                {
                    GameManager.Instance.OnPlayerDataReady += OnPlayerDataReadyHandler;
                }
            }
        }
        
        void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerDataReady -= OnPlayerDataReadyHandler;
        }
        
        void Start()
        {
            if (startBattleButton != null)
                startBattleButton.onClick.AddListener(StartBattle);
        }
        
        private void OnPlayerDataReadyHandler()
        {
            GenerateZawomonGrid();
            UpdateInfoText();
        }
        
        public void GenerateZawomonGrid()
        {
            // Clear old buttons
            foreach (var btn in zawomonButtons)
                if (btn != null) Destroy(btn);
            zawomonButtons.Clear();
            
            var zawomons = GameManager.Instance.PlayerData.creatures;
            totalZawomons = zawomons.Count;
            
            for (int i = 0; i < totalZawomons; i++)
            {
                int idx = i;
                var zaw = zawomons[i];
                var btnObj = Instantiate(zawomonButtonPrefab, zawomonGridParent);
                var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                txt.text = $"{zaw.name} ({zaw.mainElement})";
                
                // Add click handlers
                var trigger = btnObj.GetComponent<EventTrigger>();
                if (trigger == null) trigger = btnObj.AddComponent<EventTrigger>();
                trigger.triggers.Clear();
                
                var entryClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                entryClick.callback.AddListener((data) =>
                {
                    var ev = (PointerEventData)data;
                    if (ev.button == PointerEventData.InputButton.Left)
                        ToggleZawomon(idx, true); // Team A (Red)
                    else if (ev.button == PointerEventData.InputButton.Right)
                        ToggleZawomon(idx, false); // Team B (Blue)
                });
                trigger.triggers.Add(entryClick);
                
                zawomonButtons.Add(btnObj);
            }
            
            UpdateGridHighlights();
        }
        
        public void UpdateGridHighlights()
        {
            var zawomons = GameManager.Instance.PlayerData.creatures;
            
            for (int i = 0; i < zawomonButtons.Count && i < zawomons.Count; i++)
            {
                var zaw = zawomons[i];
                var img = zawomonButtons[i].GetComponent<Image>();
                var outline = zawomonButtons[i].GetComponent<Outline>();
                
                // Set background color based on team
                if (teamA.Contains(zaw))
                    img.color = Color.red;
                else if (teamB.Contains(zaw))
                    img.color = Color.blue;
                else
                    img.color = Color.white;
                
                // Set outline for cursors
                if (outline != null)
                {
                    if (i == redCursor && i == blueCursor)
                    {
                        outline.enabled = true;
                        outline.effectColor = Color.magenta;
                        outline.effectDistance = new Vector2(3, 3);
                    }
                    else if (i == redCursor)
                    {
                        outline.enabled = true;
                        outline.effectColor = Color.red;
                        outline.effectDistance = new Vector2(2, 2);
                    }
                    else if (i == blueCursor)
                    {
                        outline.enabled = true;
                        outline.effectColor = Color.blue;
                        outline.effectDistance = new Vector2(2, 2);
                    }
                    else
                    {
                        outline.enabled = false;
                    }
                }
            }
        }
        
        public void ToggleZawomon(int idx, bool toTeamA)
        {
            if (idx >= GameManager.Instance.PlayerData.creatures.Count) return;
            
            var zaw = GameManager.Instance.PlayerData.creatures[idx];
            
            if (toTeamA)
            {
                if (teamA.Contains(zaw))
                    teamA.Remove(zaw);
                else if (!teamB.Contains(zaw) && teamA.Count < maxTeamSize)
                    teamA.Add(zaw);
            }
            else
            {
                if (teamB.Contains(zaw))
                    teamB.Remove(zaw);
                else if (!teamA.Contains(zaw) && teamB.Count < maxTeamSize)
                    teamB.Add(zaw);
            }
            
            UpdateInfoText();
            UpdateGridHighlights();
        }
        
        public void UpdateInfoText()
        {
            string teamANames = string.Join(", ", teamA.ConvertAll(z => z.name));
            string teamBNames = string.Join(", ", teamB.ConvertAll(z => z.name));
            
            infoText.text = $"Team A (Red): {teamANames}\nTeam B (Blue): {teamBNames}\n\n";
            
            if (!redReady && !blueReady)
                infoText.text += "Red: WASD to move, Q to select, E for ready\nBlue: IJKL to move, U to select, O for ready";
            else if (redReady && !blueReady)
                infoText.text += "Red team ready! Waiting for Blue team...";
            else if (!redReady && blueReady)
                infoText.text += "Blue team ready! Waiting for Red team...";
            else if (redReady && blueReady)
                infoText.text += "Both teams ready! Click Start Battle or press ENTER";
        }
        
        void Update()
        {
            HandleGridInput();
            
            // Start battle with Enter when both teams ready
            if (redReady && blueReady && Input.GetKeyDown(KeyCode.Return))
                StartBattle();
        }
        
        void HandleGridInput()
        {
            if (UnityEngine.InputSystem.Keyboard.current == null) return;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            
            // Red player (WASD, Q - select, E - ready)
            if (!redReady)
            {
                int prevRed = redCursor;
                if (keyboard.wKey.wasPressedThisFrame) redCursor = WrapCursor(redCursor, -gridCols);
                if (keyboard.sKey.wasPressedThisFrame) redCursor = WrapCursor(redCursor, gridCols);
                if (keyboard.aKey.wasPressedThisFrame) redCursor = WrapCursor(redCursor, -1);
                if (keyboard.dKey.wasPressedThisFrame) redCursor = WrapCursor(redCursor, 1);
                
                if (keyboard.qKey.wasPressedThisFrame)
                {
                    ToggleZawomon(redCursor, true);
                }
                
                if (keyboard.eKey.wasPressedThisFrame && teamA.Count > 0)
                {
                    redReady = true;
                    UpdateInfoText();
                }
                
                if (prevRed != redCursor) UpdateGridHighlights();
            }
            
            // Blue player (IJKL, U - select, O - ready)
            if (!blueReady)
            {
                int prevBlue = blueCursor;
                if (keyboard.iKey.wasPressedThisFrame) blueCursor = WrapCursor(blueCursor, -gridCols);
                if (keyboard.kKey.wasPressedThisFrame) blueCursor = WrapCursor(blueCursor, gridCols);
                if (keyboard.jKey.wasPressedThisFrame) blueCursor = WrapCursor(blueCursor, -1);
                if (keyboard.lKey.wasPressedThisFrame) blueCursor = WrapCursor(blueCursor, 1);
                
                if (keyboard.uKey.wasPressedThisFrame)
                {
                    ToggleZawomon(blueCursor, false);
                }
                
                if (keyboard.oKey.wasPressedThisFrame && teamB.Count > 0)
                {
                    blueReady = true;
                    UpdateInfoText();
                }
                
                if (prevBlue != blueCursor) UpdateGridHighlights();
            }
        }
        
        private int WrapCursor(int current, int delta)
        {
            int total = totalZawomons;
            if (total == 0) return 0;
            
            int cols = gridCols;
            int rows = (total + cols - 1) / cols;
            int curRow = current / cols;
            int curCol = current % cols;
            
            if (delta == 1 || delta == -1) // Horizontal movement
            {
                int newCol = (curCol + delta + cols) % cols;
                int flat = curRow * cols + newCol;
                
                if (flat >= total)
                {
                    if (delta == 1) newCol = 0;
                    else newCol = (total - 1) % cols;
                }
                
                return curRow * cols + newCol;
            }
            else if (delta == cols || delta == -cols) // Vertical movement
            {
                int newRow = (curRow + (delta / cols) + rows) % rows;
                int flat = newRow * cols + curCol;
                
                if (flat >= total)
                {
                    flat = newRow * cols + (cols - 1);
                    if (flat >= total) flat = total - 1;
                    return flat;
                }
                
                return flat;
            }
            
            return current;
        }
        
        public void StartBattle()
        {
            if (!redReady || !blueReady || teamA.Count == 0 || teamB.Count == 0)
            {
                Debug.LogWarning("Cannot start battle: teams not ready or empty");
                return;
            }
            
            OnTeamsSelected?.Invoke(teamA, teamB);
        }
        
        public void ResetSelection()
        {
            teamA.Clear();
            teamB.Clear();
            redReady = false;
            blueReady = false;
            redCursor = 0;
            blueCursor = 0;
            UpdateInfoText();
            UpdateGridHighlights();
        }
    }
}
