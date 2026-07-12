namespace EsoLogFilter.Core.Model
{
    using System;
    using EsoLogFilter.Core.Model.Objects;

    public class LogEntry
    {
        // Number of CSV tokens in an embedded unit-state block:
        // unitId, health/max, magicka/max, stamina/max, ultimate/max, werewolf/max, shield, posX, posY, heading
        private const int UnitStateTokenCount = 10;

        // A '*' target block means "target is the same unit as the source".
        private const string SelfTarget = "*";

        // Unit id 0 means "no unit" (e.g. the target is dead or absent).
        private const string NoUnitId = "0";

        private readonly int arrayLength;

        public LogEntry(string line)
        {
            this.Line = line;
            this.LineArray = line.Split(',');
            this.arrayLength = this.LineArray.Length;

            this.SetLineType();
        }

        public string Line { get; private set; }

        public string[] LineArray { get; private set; }

        public LineTypes LineType { get; private set; }

        public string GetIdString()
        {
            switch (this.LineType)
            {
                case LineTypes.UnitAdded:
                case LineTypes.UnitRemoved:
                case LineTypes.UnitChanged:
                case LineTypes.PlayerInfo:
                    return this.LineArray[2];
                case LineTypes.HealthRegen:
                    return this.LineArray[3];
                default:
                    throw new Exception($"Line type '{this.LineType}' has no known id");
            }
        }

        public UnitTypes GetUnitType()
        {
            if (this.LineType != LineTypes.UnitAdded)
            {
                throw new Exception($"GetUnitType not allowed for line type {this.LineType}");
            }

            var unitTypeString = this.LineArray[3];

            switch (unitTypeString)
            {
                case Constants.UnitTypes.Player:
                    return UnitTypes.Player;
                case Constants.UnitTypes.Monster:
                    // The name fields may contain commas, so the reaction and ownerUnitId
                    // are read relative to the line end.
                    var monsterTypeString = this.LineArray[this.arrayLength - 2];

                    switch (monsterTypeString)
                    {
                        case Constants.MonsterTypes.Hostile:
                            // A hostile monster owned by a unit is an enemy player's pet;
                            // guards and other NPCs have ownerUnitId 0.
                            var ownerUnitId = this.LineArray[this.arrayLength - 3];

                            return ownerUnitId == NoUnitId ? UnitTypes.MonsterHostile : UnitTypes.MonsterNpcEnemy;
                        case Constants.MonsterTypes.NpcAlly:
                            return UnitTypes.MonsterNpcAlly;
                        case Constants.MonsterTypes.Friendly:
                            return UnitTypes.MonsterFriendly;
                        case Constants.MonsterTypes.Neutral:
                            return UnitTypes.MonsterNeutral;
                        default:
                            return UnitTypes.Unknown;
                    }
                case Constants.UnitTypes.Object:
                    return UnitTypes.Object;
                case Constants.UnitTypes.SiegeWeapon:
                    return UnitTypes.SiegeWeapon;
                default:
                    return UnitTypes.Unknown;
            }
        }

        public string GetSourceIdString()
        {
            switch (this.LineType)
            {
                case LineTypes.BeginCast:
                case LineTypes.EffectChanged:
                    return this.LineArray[6];
                case LineTypes.CombatEvent:
                    return this.LineArray[9];
                default:
                    throw new Exception($"Line type '{this.LineType}' does not have a known sourceId");
            }
        }

        public string GetTargetIdString()
        {
            // The target unit-state block follows the source block and collapses to a
            // single '*' token when the target is the source unit itself.
            switch (this.LineType)
            {
                case LineTypes.BeginCast:
                case LineTypes.EffectChanged:
                    return this.GetTargetToken(6 + UnitStateTokenCount);
                case LineTypes.CombatEvent:
                    return this.GetTargetToken(9 + UnitStateTokenCount);
                default:
                    throw new Exception($"Line type '{this.LineType}' does not have a known targetId");
            }
        }

        public string GetEffectiveTargetIdString()
        {
            var targetId = this.GetTargetIdString();

            if (targetId == SelfTarget || targetId == NoUnitId)
            {
                return this.GetSourceIdString();
            }

            return targetId;
        }

        public string GetCombatResult()
        {
            if (this.LineType != LineTypes.CombatEvent)
            {
                throw new Exception($"GetCombatResult not allowed for line type {this.LineType}");
            }

            return this.LineArray[2];
        }

        public long GetHitValue()
        {
            if (this.LineType != LineTypes.CombatEvent)
            {
                throw new Exception($"GetHitValue not allowed for line type {this.LineType}");
            }

            // Malformed values count as 0 instead of aborting the run (fail-open).
            return long.TryParse(this.LineArray[5], out var hitValue) ? hitValue : 0;
        }

        public string GetOwnerUnitIdString()
        {
            // The name fields may contain commas, so the ownerUnitId is read
            // relative to the line end (reaction is at len-2, owner at len-3).
            switch (this.LineType)
            {
                case LineTypes.UnitAdded:
                case LineTypes.UnitChanged:
                    return this.LineArray[this.arrayLength - 3];
                default:
                    throw new Exception($"Line type '{this.LineType}' does not have an ownerUnitId");
            }
        }

        public string GetUnitName()
        {
            return this.GetQuotedField(0);
        }

        public string GetUnitDisplayName()
        {
            return this.GetQuotedField(1);
        }

        public string GetCastTrackId()
        {
            switch (this.LineType)
            {
                case LineTypes.BeginCast:
                    return this.LineArray[4];
                case LineTypes.EndCast:
                    return this.LineArray[3];
                default:
                    throw new Exception($"Line type '{this.LineType}' does not have a castTrackId");
            }
        }

        // UNIT_ADDED/UNIT_CHANGED carry exactly two quoted fields: name, then
        // displayName. Names may contain commas but never double quotes, so the
        // fields are extracted between quote pairs instead of by CSV index.
        private string GetQuotedField(int fieldIndex)
        {
            if (this.LineType != LineTypes.UnitAdded && this.LineType != LineTypes.UnitChanged)
            {
                throw new Exception($"Line type '{this.LineType}' has no quoted name fields");
            }

            var searchFrom = 0;

            for (var i = 0; i <= fieldIndex; i++)
            {
                var start = this.Line.IndexOf('"', searchFrom);
                if (start < 0)
                {
                    return string.Empty;
                }

                var end = this.Line.IndexOf('"', start + 1);
                if (end < 0)
                {
                    return string.Empty;
                }

                if (i == fieldIndex)
                {
                    return this.Line.Substring(start + 1, end - start - 1);
                }

                searchFrom = end + 1;
            }

            return string.Empty;
        }

        private string GetTargetToken(int index)
        {
            if (index >= this.arrayLength)
            {
                return SelfTarget;
            }

            return this.LineArray[index];
        }

        private void SetLineType()
        {
            var lineTypeString = this.LineArray[1];

            switch (lineTypeString)
            {
                case Constants.LineTypes.BeginLog:
                    this.LineType = LineTypes.BeginLog;
                    break;
                case Constants.LineTypes.ZoneChanged:
                    this.LineType = LineTypes.ZoneChanged;
                    break;
                case Constants.LineTypes.UnitAdded:
                    this.LineType = LineTypes.UnitAdded;
                    break;
                case Constants.LineTypes.UnitRemoved:
                    this.LineType = LineTypes.UnitRemoved;
                    break;
                case Constants.LineTypes.UnitChanged:
                    this.LineType = LineTypes.UnitChanged;
                    break;
                case Constants.LineTypes.BeginCombat:
                    this.LineType = LineTypes.BeginCombat;
                    break;
                case Constants.LineTypes.EndCombat:
                    this.LineType = LineTypes.EndCombat;
                    break;
                case Constants.LineTypes.AbilityInfo:
                    this.LineType = LineTypes.AbilityInfo;
                    break;
                case Constants.LineTypes.EffectInfo:
                    this.LineType = LineTypes.EffectInfo;
                    break;
                case Constants.LineTypes.PlayerInfo:
                    this.LineType = LineTypes.PlayerInfo;
                    break;
                case Constants.LineTypes.MapChanged:
                    this.LineType = LineTypes.MapChanged;
                    break;
                case Constants.LineTypes.BeginCast:
                    this.LineType = LineTypes.BeginCast;
                    break;
                case Constants.LineTypes.EndCast:
                    this.LineType = LineTypes.EndCast;
                    break;
                case Constants.LineTypes.EffectChanged:
                    this.LineType = LineTypes.EffectChanged;
                    break;
                case Constants.LineTypes.CombatEvent:
                    this.LineType = LineTypes.CombatEvent;
                    break;
                case Constants.LineTypes.HealthRegen:
                    this.LineType = LineTypes.HealthRegen;
                    break;
                case Constants.LineTypes.EndLog:
                    this.LineType = LineTypes.EndLog;
                    break;
                case Constants.LineTypes.TrialInit:
                    this.LineType = LineTypes.TrialInit;
                    break;
                case Constants.LineTypes.BeginTrial:
                    this.LineType = LineTypes.BeginTrial;
                    break;
                case Constants.LineTypes.EndTrial:
                    this.LineType = LineTypes.EndTrial;
                    break;
                default:
                    // Record types this app does not know yet (e.g. added by a game update)
                    // are passed through unchanged instead of aborting the whole filter run.
                    this.LineType = LineTypes.Unknown;
                    break;
            }
        }
    }
}
