using System.Collections.Generic;
using System.Drawing;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Epstein_Fusion_DS.HeatParts.ExtendableRadiators
{
    internal class RadiatorAnimation
    {
        public readonly ExtendableRadiator Radiator;
        public bool IsActive = false;
        public bool IsExtending = false;

        private HashSet<MyEntity> _animationEntities = new HashSet<MyEntity>();

        public RadiatorAnimation(ExtendableRadiator radiator)
        {
            Radiator = radiator;
        }

        public void StartExtension()
        {
            IsActive = true;
            IsExtending = true;
        }

        public void StartRetraction()
        {
            IsActive = true;
            IsExtending = false;
        }

        private int _animationTick = 0;
        public void UpdateTick()
        {
            if (!IsActive)
                return;

            if (IsExtending)
            {
                // Extension animation

                Reset();
            }
            else
            {
                // Retraction animation

                if (_animationTick == 0)
                {
                    IMyCubeGrid grid = Radiator.Block.CubeGrid;

                    for (int i = 0; i < Radiator.StoredRadiators.Length; i++)
                    {
                        _animationEntities.Add(new AnimationPanel(@"Models\RadiatorPanel.mwm", Radiator.StoredRadiatorMatrices[i], (MyEntity) grid));
                    }
                }

                Reset();
                return;
            }

            _animationTick++;
        }

        private void Reset()
        {
            _animationTick = 0;
            IsActive = false;
        }

        private sealed class AnimationPanel : MyEntity
        {
            public AnimationPanel(string model, MatrixD localMatrix, MyEntity parent)
            {
                Init(null, model, parent, 1);
                if (string.IsNullOrEmpty(model))
                    Flags &= ~EntityFlags.Visible;
                Save = false;
                NeedsWorldMatrix = true;
                //Flags |= EntityFlags.Visible;
                //Flags |= EntityFlags.Near;
                //Flags |= EntityFlags.Sync;
                //Flags |= EntityFlags.NeedsDraw;
                WorldMatrix = parent.WorldMatrix * localMatrix;
                MyEntities.Add(this);

                HudHelpers.DebugDraw.AddGps("ENTITY", WorldMatrix.Translation, 10);
                MyAPIGateway.Utilities.ShowMessage("AnimationPanel", $"Inited new panel at {WorldMatrix.Translation:N0}");
            }
        }
    }
}
