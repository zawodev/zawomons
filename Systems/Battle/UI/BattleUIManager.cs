using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Models;
using Systems;

public class BattleUIManager : MonoBehaviour
{
    // Listy wybranych zawomonów do drużyn (UI, nie test!)
    public List<Zawomon> teamA = new();
    public List<Zawomon> teamB = new();
    public Transform zawomonGridParent; // Panel z GridLayoutGroup
    public GameObject zawomonButtonPrefab; // Prefab z Button + TextMeshPro
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI moveText;

    public List<GameObject> zawomonButtons = new();
    public int gridCols = 4; // liczba kolumn w gridzie (możesz zmienić pod swój UI)
    public int redCursor = 0;
    public int blueCursor = 0;
    public bool redReady = false;
    public bool blueReady = false;

    private int zawCount = 0;
    public int maxTeamSize = 3;

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDataReady += OnPlayerDataReadyHandler;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerDataReady -= OnPlayerDataReadyHandler;
    }

    private void OnPlayerDataReadyHandler()
    {
        GenerateZawomonGrid();
        infoText.text = "Wybierz zawomony do drużyn: LPM - czerwona, PPM - niebieska. ENTER - start walki.";
    }

    public void GenerateZawomonGrid()
    {
        // Wyczyść stare przyciski
        foreach (var btn in zawomonButtons)
            Destroy(btn);
        zawomonButtons.Clear();

        var zawomons = GameManager.Instance.PlayerData.Zawomons;
        zawCount = zawomons.Count;
        for (int i = 0; i < zawCount; i++)
        {
            int idx = i;
            var zaw = zawomons[i];
            var btnObj = Instantiate(zawomonButtonPrefab, zawomonGridParent);
            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = zaw.Name + " (" + zaw.MainClass + ")";
            // Dodaj obsługę LPM i PPM przez EventTrigger
            var trigger = btnObj.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btnObj.AddComponent<EventTrigger>();
            trigger.triggers.Clear();
            // LPM - czerwony
            var entryL = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entryL.callback.AddListener((data) =>
            {
                var ev = (PointerEventData)data;
                if (ev.button == PointerEventData.InputButton.Left)
                {
                    redCursor = idx;
                    // TODO: Dodaj logikę wyboru do teamA
                    UpdateGridHighlights();
                }
                else if (ev.button == PointerEventData.InputButton.Right)
                {
                    blueCursor = idx;
                    // TODO: Dodaj logikę wyboru do teamB
                    UpdateGridHighlights();
                }
            });
            trigger.triggers.Add(entryL);
            zawomonButtons.Add(btnObj);
        }
        UpdateGridHighlights();
        UpdateGridHighlights();
    }

    public void UpdateGridHighlights()
    {
        for (int i = 0; i < zawomonButtons.Count; i++)
        {
            var img = zawomonButtons[i].GetComponent<Image>();
            var zaw = GameManager.Instance.PlayerData.Zawomons[i];
            if (teamA.Contains(zaw))
                img.color = Color.red;
            else if (teamB.Contains(zaw))
                img.color = Color.blue;
            else
                img.color = Color.white;
            // Obwódki
            var outline = zawomonButtons[i].GetComponent<Outline>();
            if (outline != null)
            {
                if (i == redCursor && i == blueCursor)
                {
                    outline.enabled = true;
                    outline.effectColor = new Color(0.5f, 0f, 0.5f, 1f); // fioletowy jako mix czerwony+niebieski
                }
                else if (i == redCursor)
                {
                    outline.enabled = true;
                    outline.effectColor = Color.red;
                }
                else if (i == blueCursor)
                {
                    outline.enabled = true;
                    outline.effectColor = Color.blue;
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
        var zaw = GameManager.Instance.PlayerData.Zawomons[idx];
        if (toTeamA)
        {
            if (teamA.Contains(zaw))
                teamA.Remove(zaw); // odznacz
            else if (!teamB.Contains(zaw) && teamA.Count < maxTeamSize)
                teamA.Add(zaw);
        }
        else
        {
            if (teamB.Contains(zaw))
                teamB.Remove(zaw); // odznacz
            else if (!teamA.Contains(zaw) && teamB.Count < maxTeamSize)
                teamB.Add(zaw);
        }
    }

    public void UpdateSelectionInfo()
    {
        infoText.text = $"Czerwoni: {string.Join(", ", teamA.ConvertAll(z => z.Name))}\nNiebiescy: {string.Join(", ", teamB.ConvertAll(z => z.Name))}";
        if (redReady && blueReady)
            infoText.text += "\nObaj gracze gotowi! Możesz rozpocząć walkę.";
        else if (redReady)
            infoText.text += "\nCzerwony gracz gotowy.";
        else if (blueReady)
            infoText.text += "\nNiebieski gracz gotowy.";
    }

    public void UpdateMoveUI(int player, int zawomonIdx)
    {
        var team = player == 0 ? teamA : teamB;
        if (team.Count == 0) { moveText.text = ""; return; }
        int count = team.Count;
        // Wyświetl 3 zawomony: poprzedni, wybrany, następny (zawijanie)
        int prev = (zawomonIdx - 1 + count) % count;
        int next = (zawomonIdx + 1) % count;
        string zawList = $"<color=grey>{team[prev].Name}</color>\n<color=yellow>{team[zawomonIdx].Name}</color>\n<color=grey>{team[next].Name}</color>";
        // Spelle dla wybranego
        var spells = team[zawomonIdx].Spells;
        int spellIdx = 0;
        if (spells.Count == 0) { moveText.text = zawList + "\nBrak spellów"; return; }
        int spellPrev = (spellIdx - 1 + spells.Count) % spells.Count;
        int spellNext = (spellIdx + 1) % spells.Count;
        string spellList = $"<color=grey>{spells[spellPrev].Name}</color> | <color=yellow>{spells[spellIdx].Name}</color> | <color=grey>{spells[spellNext].Name}</color>";
        moveText.text = zawList + "\nSpell: " + spellList;
    }

    public void Update()
    {
        HandleGridInput();
    }

    public void HandleGridInput()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null) return;
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        // Czerwony gracz (WASD, Q - zatwierdź, E - gotowość)
        if (!redReady)
        {
            int prevRed = redCursor;
            if (keyboard.wKey.wasPressedThisFrame) redCursor = WrapCursor(redCursor, -gridCols);
            if (keyboard.sKey.wasPressedThisFrame) redCursor = WrapCursor(redCursor, gridCols);
            if (keyboard.aKey.wasPressedThisFrame) redCursor = WrapCursor(redCursor, -1);
            if (keyboard.dKey.wasPressedThisFrame) redCursor = WrapCursor(redCursor, 1);
            if (keyboard.qKey.wasPressedThisFrame)
            {
                ToggleZawomon(redCursor, true); // tylko teamA
                UpdateSelectionInfo();
                UpdateGridHighlights();
            }
            if (keyboard.eKey.wasPressedThisFrame) redReady = true;
            if (prevRed != redCursor) UpdateGridHighlights();
        }
        // Niebieski gracz (IJKL, U - zatwierdź, O - gotowość)
        if (!blueReady)
        {
            int prevBlue = blueCursor;
            if (keyboard.iKey.wasPressedThisFrame) blueCursor = WrapCursor(blueCursor, -gridCols);
            if (keyboard.kKey.wasPressedThisFrame) blueCursor = WrapCursor(blueCursor, gridCols);
            if (keyboard.jKey.wasPressedThisFrame) blueCursor = WrapCursor(blueCursor, -1);
            if (keyboard.lKey.wasPressedThisFrame) blueCursor = WrapCursor(blueCursor, 1);
            if (keyboard.uKey.wasPressedThisFrame)
            {
                ToggleZawomon(blueCursor, false); // tylko teamB
                UpdateSelectionInfo();
                UpdateGridHighlights();
            }
            if (keyboard.oKey.wasPressedThisFrame) blueReady = true;
            if (prevBlue != blueCursor) UpdateGridHighlights();
        }
        // Jeśli obaj gotowi, tutaj możesz wywołać własną logikę rozpoczęcia walki
        if (redReady && blueReady)
        {
            Debug.Log("Obaj gracze gotowi! Możesz rozpocząć walkę.");
            //StartBattleIfReady();
        }
    }

    // Zawijanie kursora w gridzie (pion/poziom, z uwzględnieniem niepełnych wierszy)
    private int WrapCursor(int current, int delta)
    {
        int total = zawCount;
        int cols = gridCols;
        int rows = (total + cols - 1) / cols;
        int curRow = current / cols;
        int curCol = current % cols;
        int newRow = curRow;
        int newCol = curCol;
        if (delta == 1 || delta == -1) // poziomo
        {
            newCol = (curCol + delta + cols) % cols;
            // Jeśli w nowej kolumnie nie ma kafelka, zawijaj do pierwszego dostępnego w tym wierszu
            int flat = curRow * cols + newCol;
            if (flat >= total)
            {
                // Szukaj od lewej/prawej do pierwszego dostępnego w tym wierszu
                if (delta == 1) newCol = 0;
                else newCol = (total - 1) % cols;
            }
        }
        else if (delta == cols || delta == -cols) // pionowo
        {
            newRow = (curRow + (delta / cols) + rows) % rows;
            int flat = newRow * cols + curCol;
            if (flat >= total)
            {
                // Jeśli w nowym wierszu nie ma kafelka w tej kolumnie, zawijaj do ostatniego w tym wierszu
                flat = newRow * cols + (cols - 1);
                if (flat >= total) flat = total - 1;
                return flat;
            }
        }
        int result = newRow * cols + newCol;
        if (result >= total) result = total - 1;
        return result;
    }
}
