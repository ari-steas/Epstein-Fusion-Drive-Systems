using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace Epstein_Fusion_DS.FusionParts.FusionThruster
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false, 
        "ARYLNX_Epstein_Drive",
        "ARYLNX_Mega_Epstein_Drive",
        "ARYLNX_MUNR_Epstein_Drive",
        "ARYLNX_ROCI_Epstein_Drive",
        "ARYLYNX_SILVERSMITH_DRIVE",
        "ARYLNX_RAIDER_Epstein_Drive",
        "ARYLNX_QUADRA_Epstein_Drive",
        "ARYLNX_SCIRCOCCO_Epstein_Drive",
        "ARYLNX_DRUMMER_Epstein_Drive",
        "ARYLNX_PNDR_Epstein_Drive"
        )]
    public class FusionThrusterLogic : FusionPart<IMyThrust>
    {
        private int _bufferBlockCount = 1;
        private float _bufferThrustOutput;


        internal override string[] BlockSubtypes => new[]
        {
            "ARYLNX_Epstein_Drive",
            "ARYLNX_Mega_Epstein_Drive",
            "ARYLNX_MUNR_Epstein_Drive",
            "ARYLNX_ROCI_Epstein_Drive",
            "ARYLYNX_SILVERSMITH_DRIVE",
            "ARYLNX_RAIDER_Epstein_Drive",
            "ARYLNX_QUADRA_Epstein_Drive",
            "ARYLNX_SCIRCOCCO_Epstein_Drive",
            "ARYLNX_DRUMMER_Epstein_Drive",
            "ARYLNX_PNDR_Epstein_Drive"
        };

        internal override string ReadableName => "Thruster";

        internal Dictionary<string, float> EfficiencyModifiers = new Dictionary<string, float>
        {
            ["ARYLNX_Epstein_Drive"] = 1.0f,
            ["ARYLNX_Mega_Epstein_Drive"] = 1.0f,
            ["ARYLNX_MUNR_Epstein_Drive"] = 1.0f,
            ["ARYLNX_ROCI_Epstein_Drive"] = 1.0f,
            ["ARYLYNX_SILVERSMITH_DRIVE"] = 1.0f,
            ["ARYLNX_RAIDER_Epstein_Drive"] = 1.0f,
            ["ARYLNX_QUADRA_Epstein_Drive"] = 1.0f,
            ["ARYLNX_SCIRCOCCO_Epstein_Drive"] = 1.0f,
            ["ARYLNX_DRUMMER_Epstein_Drive"] = 1.0f,
            ["ARYLNX_PNDR_Epstein_Drive"] = 1.0f,
        };

        public override void UpdatePower(float powerGeneration, float newtonsPerFusionPower, int numberThrusters)
        {
            BufferPowerGeneration = powerGeneration;
            _bufferBlockCount = numberThrusters;

            var thrustOutput = Block.CurrentThrust;
            var efficiencyMultiplier = EfficiencyModifiers[Block.BlockDefinition.SubtypeId];

            // Power generation consumed (per second)
            var powerConsumption = thrustOutput * efficiencyMultiplier;


            // Power generated (per second)
            //var thrustOutput = efficiencyMultiplier * powerConsumption * newtonsPerFusionPower;
            _bufferThrustOutput = thrustOutput;
            MaxPowerConsumption = powerConsumption / 60;

            InfoText.Clear();
            InfoText.AppendLine(
                $"\nOutput: {Math.Round(thrustOutput, 1)}/{Math.Round(powerGeneration * 60 * newtonsPerFusionPower, 1)}");
            InfoText.AppendLine($"Input: {Math.Round(powerConsumption, 1)}/{Math.Round(powerGeneration * 60, 1)}");
            InfoText.AppendLine($"Efficiency: {Math.Round(efficiencyMultiplier * 100)}%");

            // Convert back into power per tick
            if (!IsShutdown)
                SyncMultipliers.ThrusterOutput(Block, Block.MaxThrust / Block.ThrustMultiplier);
        }

        public void SetPowerBoost(bool value)
        {
            if (OverrideEnabled.Value == value)
                return;

            OverrideEnabled.Value = value;
            UpdatePower(BufferPowerGeneration, SFusionSystem.NewtonsPerFusionPower, _bufferBlockCount);
        }

        #region Base Methods

        public override void Init(MyObjectBuilder_EntityBase definition)
        {
            base.Init(definition);
            Block = (IMyThrust)Entity;
            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            // Trigger power update is only needed when OverrideEnabled is false
            PowerUsageSync.ValueChanged += value =>
            {
                if (!OverrideEnabled.Value)
                    UpdatePower(BufferPowerGeneration, SFusionSystem.NewtonsPerFusionPower, _bufferBlockCount);
            };

            // Trigger power update is only needed when OverrideEnabled is true
            OverridePowerUsageSync.ValueChanged += value =>
            {
                if (OverrideEnabled.Value)
                    UpdatePower(BufferPowerGeneration, SFusionSystem.NewtonsPerFusionPower, _bufferBlockCount);
            };

            // Trigger power update if boostEnabled is changed
            OverrideEnabled.ValueChanged += value =>
                UpdatePower(BufferPowerGeneration, SFusionSystem.NewtonsPerFusionPower, _bufferBlockCount);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            var storagePct = MemberSystem?.PowerStored / MemberSystem?.MaxPowerStored ?? 0;

            if (storagePct <= 0.05f)
            {
                if (Block.ThrustMultiplier <= 0.01)
                    return;
                SyncMultipliers.ThrusterOutput(Block, 0);
                PowerConsumption = 0;
                LastShutdown = DateTime.Now.Ticks + 4 * TimeSpan.TicksPerSecond;
                IsShutdown = true;
                return;
            }

            // If boost is unsustainable, disable it.
            // If power draw exceeds power available, disable self until available.
            if ((OverrideEnabled.Value && MemberSystem?.PowerStored <= MemberSystem?.PowerConsumption * 30) ||
                !Block.IsWorking)
            {
                SetPowerBoost(false);
                PowerConsumption = 0;
                SyncMultipliers.ThrusterOutput(Block, 0);
            }
            else if (storagePct > 0.1f && DateTime.Now.Ticks > LastShutdown)
            {
                SyncMultipliers.ThrusterOutput(Block, _bufferThrustOutput);
                PowerConsumption = MaxPowerConsumption * (Block.CurrentThrustPercentage / 100f);
                IsShutdown = false;
            }
        }

        #endregion
    }
}