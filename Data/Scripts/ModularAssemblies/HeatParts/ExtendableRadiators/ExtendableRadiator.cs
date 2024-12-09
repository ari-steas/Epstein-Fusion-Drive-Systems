using System;
using System.Collections.Generic;
using Epstein_Fusion_DS.HudHelpers;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Epstein_Fusion_DS.HeatParts.ExtendableRadiators
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "RadiatorBase")]
    internal class ExtendableRadiator : MyGameLogicComponent
    {
        public static readonly string[] ValidPanelSubtypes =
        {
            "RadiatorPanel",
        };


        public IMyCubeBlock Block;
        internal StoredRadiator[] StoredRadiators = Array.Empty<StoredRadiator>();
        internal RadiatorAnimation Animation;

        private bool _isExtended = true;
        public bool IsExtended
        {
            get
            {
                return _isExtended;
            }
            set
            {
                if (Animation.IsActive)
                    return;

                if (value)
                    ExtendPanels();
                else
                    RetractPanels();
                _isExtended = value;
            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            Block = (IMyCubeBlock)Entity;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            RadiatorControls.DoOnce();
            base.UpdateOnceBeforeFrame();

            if (Block?.CubeGrid?.Physics == null)
                return;

            Animation = new RadiatorAnimation(this);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            //NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();

            // This is stupid, but prevents the mod profiler cost from being incurred every tick per block when inactive
            if (Animation.IsActive)
                Animation.UpdateTick();
        }

        public void ExtendPanels()
        {
            if (_isExtended)
                return;

            Vector3I nextPosition = Block.Position;

            // TODO move this to clientside
            for (int i = 0; i < StoredRadiators.Length; i++)
            {
                nextPosition += (Vector3I)(Block.LocalMatrix.Up * (i + 1));

                if (Block.CubeGrid.CubeExists(nextPosition))
                {
                    MyAPIGateway.Utilities.ShowNotification("Block already exists at position!");
                    DebugDraw.AddGridPoint(nextPosition, Block.CubeGrid, Color.Red, 4);
                    _isExtended = false;
                    return;
                }
            }

            foreach (var block in StoredRadiators)
                Block.CubeGrid.AddBlock(block.ObjectBuilder, true);

            Animation.StartExtension();

            StoredRadiators = Array.Empty<StoredRadiator>();
        }

        public void RetractPanels()
        {
            if (!_isExtended)
                return;

            IMyCubeBlock nextBlock;
            List<StoredRadiator> builders = new List<StoredRadiator>();
            int idx = 1;

            while (GetNextPanel(idx, out nextBlock))
            {
                var builder = nextBlock.GetObjectBuilderCubeBlock(true);

                builder.BlockOrientation = nextBlock.Orientation;

                Matrix matrix;
                builders.Add(new StoredRadiator(builder, nextBlock.LocalMatrix, nextBlock.CalculateCurrentModel(out matrix)));

                nextBlock.CubeGrid.RemoveBlock(nextBlock.SlimBlock, true);
                idx++;
            }

            StoredRadiators = builders.ToArray();

            Animation.StartRetraction();
        }

        internal bool GetNextPanel(int idx, out IMyCubeBlock next)
        {
            IMySlimBlock block = Block.CubeGrid.GetCubeBlock((Vector3I)(Block.Position + Block.LocalMatrix.Up * idx));
            if (block == null || !ValidPanelSubtypes.Contains(block.BlockDefinition.Id.SubtypeName))
            {
                next = null;
                return false;
            }

            next = block.FatBlock;
            return true;
        }

        internal struct StoredRadiator
        {
            public MyObjectBuilder_CubeBlock ObjectBuilder;
            public Matrix LocalMatrix;
            public string Model;

            public StoredRadiator(MyObjectBuilder_CubeBlock objectBuilder, Matrix localMatrix, string model)
            {
                ObjectBuilder = objectBuilder;
                LocalMatrix = localMatrix;
                Model = model;
            }
        }
    }
}
