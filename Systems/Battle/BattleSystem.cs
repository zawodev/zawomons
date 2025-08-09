using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Models;

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

public class BattleSystem : MonoBehaviour
{
    public BattleState state;

    public void StartBattle(List<Zawomon> teamA, List<Zawomon> teamB, BattleMode mode)
    {
        state = new BattleState
        {
            teamA = teamA.Select(z => new BattleParticipant { zawomon = z, currentHP = z.HP }).ToList(),
            teamB = teamB.Select(z => new BattleParticipant { zawomon = z, currentHP = z.HP }).ToList(),
            mode = mode
        };
        // ... inicjalizacja walki
    }

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
        // Przykład: spell typu "Zadaj 2 dmg wszystkim ognistym"
        // spell.TargetType, spell.Damage, spell.EffectType, itp.
        // ... logika efektów
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