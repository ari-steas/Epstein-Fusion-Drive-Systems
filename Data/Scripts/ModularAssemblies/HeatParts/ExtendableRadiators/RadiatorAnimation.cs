using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Epstein_Fusion_DS.HudHelpers;
using Sandbox.Engine.Physics;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;
using CollisionLayers = Sandbox.Engine.Physics.MyPhysics.CollisionLayers;
using Color = VRageMath.Color;

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

            _animationTick++;

            if (IsExtending)
            {
                // Extension animation

                Reset();
            }
            else
            {
                // Retraction animation

                if (_animationTick == 1)
                {
                    MyEntity parentEntity = (MyEntity) Radiator.Block.CubeGrid;
                    Matrix localMatrixOffset = Matrix.Identity;

                    for (int i = 0; i < Radiator.StoredRadiators.Length; i++)
                    {
                        _animationEntities.Add(new AnimationPanel(Radiator.StoredRadiators[i].Model, Radiator.StoredRadiators[i].LocalMatrix * localMatrixOffset, parentEntity));
                        parentEntity = _animationEntities.Last();
                        localMatrixOffset = Matrix.Invert(Radiator.StoredRadiators[i].LocalMatrix);
                    }
                }

                if (_animationTick <= 120)
                {
                    int idx = 0;
                    foreach (var entity in _animationEntities)
                    {
                        Vector3D rotationPoint = new Vector3D(-0.5, 1.25, 0);

                        //Matrix newMatrix = entity.PositionComp.LocalMatrixRef * Matrix.CreateFromAxisAngle(entity.PositionComp.LocalMatrixRef.Backward, 0.0098175f);
                        //newMatrix.Translation = entity.PositionComp.LocalMatrixRef.Translation;
                        Matrix newMatrix = Utils.RotateMatrixAroundPoint(entity.PositionComp.LocalMatrixRef,
                            Vector3D.Transform(rotationPoint, entity.PositionComp.LocalMatrixRef), entity.PositionComp.LocalMatrixRef.Backward, idx == 0 ? 0.0098175 : 0.0098175*2);
                        DebugDraw.AddPoint(Vector3D.Transform(rotationPoint, entity.WorldMatrix), Color.Blue, 0);
                        entity.PositionComp.SetLocalMatrix(ref newMatrix);

                        idx++;
                    }

                    return;
                }

                Reset();
                return;
            }
        }

        private void Reset()
        {
            _animationTick = 0;
            IsActive = false;

            foreach (var entity in _animationEntities)
                entity.Close();
            _animationEntities.Clear();
        }

        private sealed class AnimationPanel : MyEntity
        {
            public AnimationPanel(string model, Matrix localMatrix, MyEntity parent)
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

                PositionComp.SetLocalMatrix(ref localMatrix);
                MyEntities.Add(this);
            }
        }
    }
}
