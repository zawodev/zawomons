using System;
using Systems.Battle.Models;
using Systems.Creatures.Models;

namespace Systems.Battle.Network
{
    public interface IBattleNetworkHandler
    {
        // Events
        event Action<BattleNetworkMessage> OnMessageReceived;
        event Action OnConnected;
        event Action OnDisconnected;
        
        // Connection
        bool IsConnected { get; }
        void Connect();
        void Disconnect();
        
        // Battle Flow
        void SendBattleStart(BattleStartRequest request);
        void SendMoveSelection(MoveSelectionRequest request);
        void SendTurnComplete(TurnCompleteRequest request);
        
        // Mock for offline mode
        void EnableOfflineMode(bool enabled);
    }
    
    // Message Types
    [Serializable]
    public class BattleNetworkMessage
    {
        public string messageType;
        public string payload;
        public long timestamp;
    }
    
    [Serializable]
    public class BattleStartRequest
    {
        public string battleId;
        public BattleMode mode;
        public BattleType battleType;
        public Creature[] teamA;
        public Creature[] teamB;
        public bool isPlayerTeamA; // true if local player controls team A
    }
    
    [Serializable]
    public class MoveSelectionRequest
    {
        public string battleId;
        public string creatureId;
        public string spellId;
        public string[] targetIds; // can be multiple for AoE
        public int turnNumber;
    }
    
    [Serializable]
    public class TurnCompleteRequest
    {
        public string battleId;
        public int turnNumber;
        public TurnResult[] results;
    }
    
    [Serializable]
    public class TurnResult
    {
        public string casterId;
        public string spellId;
        public string[] targetIds;
        public int[] damageDealt;
        public int[] healingDone;
        public CreatureStatusUpdate[] statusUpdates;
    }
    
    [Serializable]
    public class CreatureStatusUpdate
    {
        public string creatureId;
        public int currentHP;
        public bool isAlive;
        public int fatigue; // for ranked battles
    }
}