using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public enum BattleMode { Local, Online }
    
    public enum BattlePhase { Selection, Combat, Finished }
    
    [System.Serializable]
    public class BattleState
    {
        public List<BattleParticipant> teamA = new();
        public List<BattleParticipant> teamB = new();
        public BattleMode mode = BattleMode.Local;
        public BattlePhase phase = BattlePhase.Selection;
        public int currentTurn = 0;
        public string winner = "";
        
        public bool IsTeamAReady => teamA.TrueForAll(p => p.hasConfirmedMove || !p.IsAlive);
        public bool IsTeamBReady => teamB.TrueForAll(p => p.hasConfirmedMove || !p.IsAlive);
        public bool AreBothTeamsReady => IsTeamAReady && IsTeamBReady;
        
        public bool IsTeamAAlive => teamA.Exists(p => p.IsAlive);
        public bool IsTeamBAlive => teamB.Exists(p => p.IsAlive);
    }
}
