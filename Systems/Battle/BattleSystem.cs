
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Models;


public class BattleSystem : MonoBehaviour
{
    // Struktura do obsługi wyboru ruchów przez gracza
    public class MoveSelectionState
    {
        public int selectedZawomonIdx = 0;
        public int selectedSpellIdx = 0;
        public bool isConfirmed = false;
        public bool isVisible = true;
    }

    // Przykładowa logika wyboru ruchów dla dwóch graczy lokalnie (do podpięcia pod UI lub InputManager)
    public MoveSelectionState[] moveSelections = new MoveSelectionState[2] { new MoveSelectionState(), new MoveSelectionState() };

    // Wywołuj w Update lub przez UI
    public void HandleLocalInput(List<BattleParticipant> team, int playerIdx)
    {
        var sel = moveSelections[playerIdx];
        var zawomons = team;
        if (zawomons.Count == 0) return;

        // Przykład: obsługa klawiszy dla gracza 1 (WASDQE) i gracza 2 (IJKLUO)
        // (W/S lub I/K) - zmiana zawomona
        // (A/D lub J/L) - zmiana spella
        // (E/U) - zatwierdź
        // (Q/O) - tryb widoczny

        // To jest tylko szkielet, podłącz pod swój system inputów lub UI!
        // Poniżej pseudokod:
        // if (Input.GetKeyDown(KeyCode.W)) { ... }
        // if (Input.GetKeyDown(KeyCode.A)) { ... }
        // ...

        // Przykład zmiany wybranego zawomona
        // sel.selectedZawomonIdx = (sel.selectedZawomonIdx + 1 + zawomons.Count) % zawomons.Count;
        // Przykład zmiany wybranego spella
        // var spells = zawomons[sel.selectedZawomonIdx].zawomon.Spells;
        // sel.selectedSpellIdx = (sel.selectedSpellIdx + 1 + spells.Count) % spells.Count;
        // Przykład zatwierdzenia
        // sel.isConfirmed = true;
        // Przykład trybu widoczności
        // sel.isVisible = !sel.isVisible;

        // Po zatwierdzeniu wyboru:
        // zawomons[sel.selectedZawomonIdx].selectedSpell = spells[sel.selectedSpellIdx];
        // zawomons[sel.selectedZawomonIdx].isVisible = sel.isVisible;
    }

    public BattleState state;


    public enum BattleMode { Local, Online }

    public class BattleParticipant
    {
        public Zawomon zawomon;
        public Spell selectedSpell;
        public bool isVisible = true; // tryb "widoczny"
        public int currentHP;
        public int initiativeBonus = 0;
        // ... inne statusy
    }

    public class BattleState
    {
        public List<BattleParticipant> teamA = new();
        public List<BattleParticipant> teamB = new();
        public BattleMode mode;
        public int currentTurn = 0;
        // ... inne dane stanu walki
    }

        public void StartBattle(List<Zawomon> teamA, List<Zawomon> teamB, BattleMode mode)
        {
            state = new BattleState
            {
                teamA = teamA.Select(z => new BattleParticipant { zawomon = z, currentHP = z.MaxHP }).ToList(),
                teamB = teamB.Select(z => new BattleParticipant { zawomon = z, currentHP = z.MaxHP }).ToList(),
                mode = mode
            };
            // ... inicjalizacja walki
        }
    // ...existing code...

    public void SelectMove(BattleParticipant participant, Spell spell)
    {
        participant.selectedSpell = spell;
    }

    public void ToggleVisibility(BattleParticipant participant)
    {
        if (state.mode == BattleMode.Local)
            participant.isVisible = !participant.isVisible;
    }

    public void NextTurn()
    {
        // 1. Zbierz wszystkich uczestników
        var all = state.teamA.Concat(state.teamB).ToList();
        // 2. Posortuj po inicjatywie i levelu
        all = all.OrderByDescending(p => p.zawomon.Initiative + p.initiativeBonus)
                 .ThenByDescending(p => p.zawomon.Level)
                 .ToList();
        // 3. Wykonaj ruchy
        foreach (var p in all)
        {
            if (p.currentHP > 0 && p.selectedSpell != null)
                ResolveSpell(p, p.selectedSpell);
        }
        // 4. Sprawdź zwycięzcę
        CheckWinner();
        state.currentTurn++;
    }

    private void ResolveSpell(BattleParticipant caster, Spell spell)
    {
        List<BattleParticipant> targets = new();
        // Ustal drużyny
        var teamA = state.teamA;
        var teamB = state.teamB;
        bool isCasterA = teamA.Contains(caster);
        var ownTeam = isCasterA ? teamA : teamB;
        var enemyTeam = isCasterA ? teamB : teamA;

        // Ustal targety
        switch (spell.TargetType)
        {
            case Models.SpellTargetType.Enemy:
                // Najpierw pierwszy żywy przeciwnik
                var target = enemyTeam.FirstOrDefault(p => p.currentHP > 0);
                if (target != null) targets.Add(target);
                break;
            case Models.SpellTargetType.AllEnemies:
                targets.AddRange(enemyTeam.Where(p => p.currentHP > 0));
                break;
            case Models.SpellTargetType.Ally:
                var ally = ownTeam.FirstOrDefault(p => p.currentHP > 0 && p != caster);
                if (ally != null) targets.Add(ally);
                break;
            case Models.SpellTargetType.AllAllies:
                targets.AddRange(ownTeam.Where(p => p.currentHP > 0 && p != caster));
                break;
            case Models.SpellTargetType.Self:
                targets.Add(caster);
                break;
        }

        // Efekty
        foreach (var t in targets)
        {
            switch (spell.EffectType)
            {
                case Models.SpellEffectType.Damage:
                    t.currentHP -= spell.EffectValue;
                    Debug.Log($"{caster.zawomon.Name} zadaje {spell.EffectValue} dmg {t.zawomon.Name}");
                    if (t.currentHP < 0) t.currentHP = 0;
                    break;
                case Models.SpellEffectType.Heal:
                    t.currentHP += spell.EffectValue;
                    if (t.currentHP > t.zawomon.MaxHP) t.currentHP = t.zawomon.MaxHP;
                    Debug.Log($"{caster.zawomon.Name} leczy {t.zawomon.Name} o {spell.EffectValue}");
                    break;
                case Models.SpellEffectType.BuffInitiative:
                    t.initiativeBonus += spell.EffectValue;
                    Debug.Log($"{caster.zawomon.Name} daje {spell.EffectValue} inicjatywy {t.zawomon.Name}");
                    break;
                case Models.SpellEffectType.BuffDamage:
                    t.zawomon.Damage += spell.EffectValue;
                    Debug.Log($"{caster.zawomon.Name} daje {spell.EffectValue} dmg {t.zawomon.Name}");
                    break;
            }
        }
    }

    private void CheckWinner()
    {
        bool teamADead = state.teamA.All(p => p.currentHP <= 0);
        bool teamBDead = state.teamB.All(p => p.currentHP <= 0);
        if (teamADead || teamBDead)
        {
            // ... obsługa końca walki
        }
    }
}