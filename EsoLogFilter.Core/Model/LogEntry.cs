namespace EsoLogFilter.Core.Model
{
    using System;
    using EsoLogFilter.Core.Model.Objects;

    public class LogEntry
    {
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
                    return this.LineArray[2];
                case LineTypes.UnitRemoved:
                    return this.LineArray[2];
                case LineTypes.UnitChanged:
                    return this.LineArray[2];
                case LineTypes.PlayerInfo:
                    return this.LineArray[2];
                default:
                    throw new Exception($"Line type '{this.LineType}' ha no known id");
            }
        }

        public UnitTypes GetUnitType()
        {
            if (this.LineType != LineTypes.UnitAdded)
            {
                throw new Exception($"SetUnitType not allowed for line type {this.LineType}");
            }

            var unitTypeString = this.LineArray[3];

            switch (unitTypeString)
            {
                case Constants.UnitTypes.Player:
                    return UnitTypes.Player;
                case Constants.UnitTypes.Monster:
                    var monsterTypeString = this.LineArray[this.arrayLength - 2];

                    switch (monsterTypeString)
                    {
                        case Constants.MonsterTypes.Hostile:
                            return UnitTypes.MonsterHostile;
                        case Constants.MonsterTypes.NpcAlly:
                            return UnitTypes.MonsterNpcAlly;
                        case Constants.MonsterTypes.Friendly:
                            return UnitTypes.MonsterFriendly;
                        case Constants.MonsterTypes.Neutral:
                            return UnitTypes.MonsterNeutral;
                        default:
                            throw new System.Exception($"Monster type '{monsterTypeString}' unknown!");
                    }
                case Constants.UnitTypes.Object:
                    return UnitTypes.Object;
                case Constants.UnitTypes.SiegeWeapon:
                    return UnitTypes.SiegeWeapon;
                default:
                    throw new System.Exception($"Unit type '{unitTypeString}' unknown!");
            }
        }

        public string GetSourceIdString()
        {
            switch (this.LineType)
            {
                case LineTypes.BeginCast:
                    return this.LineArray[6];
                case LineTypes.EndCast:
                    return this.LineArray[4];
                default:
                    throw new Exception($"Line type '{this.LineType}' does not have a known sourceId");
            }
        }

        public string GetTargetIdString()
        {
            switch (this.LineType)
            {
                case LineTypes.EffectChanged:
                    return this.LineArray[6];
                case LineTypes.CombatEvent:
                    return this.LineArray[9];
                // Checked
                case LineTypes.HealthRegen:
                    return this.LineArray[3];
                default:
                    throw new Exception($"Line type '{this.LineType}' does not have a known targetId");
            }
        }

        private void SetLineType()
        {
            var lineTypeSting = this.LineArray[1];

            switch (lineTypeSting)
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
                default:
                    throw new System.Exception($"Line type '{lineTypeSting}' unknown!");
            }
        }
    }
}
