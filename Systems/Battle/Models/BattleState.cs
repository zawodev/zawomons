using System.Collections.Generic;
using UnityEngine;

namespace Systems.Battle.Models {
    public enum BattleMode { Local, Online }
    
    public enum BattleType { 
        FriendlyMatch,  // Sparring - no consequences, no stats change
        RankedBattle    // Full battle - HP loss, fatigue, exp gain
    }
    
    public enum BattlePhase { Selection, Combat, Finished }
    
    [System.Serializable]
    public class BattleState {
        public List<BattleParticipant> teamA = new();
        public List<BattleParticipant> teamB = new();
        public BattleMode mode = BattleMode.Local;
        public BattleType battleType = BattleType.FriendlyMatch;
        public BattlePhase phase = BattlePhase.Selection;
        public int currentTurn = 0;
        public string winner = "";
        
        // Manual ready system - hold E/O
        public bool teamAManualReady = false;
        public bool teamBManualReady = false;
        
        public bool IsTeamAReady => teamA.TrueForAll(p => p.hasConfirmedMove || !p.IsAlive);
        public bool IsTeamBReady => teamB.TrueForAll(p => p.hasConfirmedMove || !p.IsAlive);
        public bool AreBothTeamsReady => teamAManualReady && teamBManualReady && IsTeamAReady && IsTeamBReady;
        
        public bool IsTeamAAlive => teamA.Exists(p => p.IsAlive);
        public bool IsTeamBAlive => teamB.Exists(p => p.IsAlive);
    }
}
