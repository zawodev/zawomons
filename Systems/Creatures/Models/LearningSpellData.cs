using System.Collections.Generic;
using UnityEngine;
using System;

namespace Systems.Creatures.Models {
    [System.Serializable]
    public class LearningSpellData {
        public string spellName;
        public double startTimeUtc;
        public float learnTimeSeconds;
    }
}
