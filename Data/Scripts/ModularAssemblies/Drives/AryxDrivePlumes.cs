using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace Epstein_Fusion_DS.Drives
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false)]
    public class AryxDrivePlume : MyGameLogicComponent
    {
        private MatrixD particle_matrix = MatrixD.Identity;
        private Vector3D particle_position = Vector3D.Zero;
        private MyEntity3DSoundEmitter emitter;
        private MySoundPair driveSound;
        private MyParticleEffect particle;
        private IMyThrust thrust;

        private bool thrusterFiring;
        private string particleToCreate;
        private float particleRadius;
        private float particleSize;
        private string driveSoundToUse;
        private bool isValidEDrive;
        private bool initialised;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            thrust = Entity as IMyThrust;
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

        }
        public override void UpdateOnceBeforeFrame()
        {
            if (thrust.CubeGrid.Physics != null)
            {
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
                if (initialised == false)
                {
                    string defaultDriveSound = "ArcARYLYN_edrive_standard_burn";

                    string typeComparison = thrust.BlockDefinition.SubtypeId.ToLower();
                    if (typeComparison.Contains("_drive") || typeComparison.Contains("xrcs")) //what no formal standardisation in a formerly-cooperative mod does to a mf
                    {
                        isValidEDrive = true;
                        //TODO: this is dumb.
                        if (typeComparison.Contains("arylnx_raider_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Raider";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylnx_quadra_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Avalon";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylnx_munr_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Munroe";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylnx_pndr_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Pandora";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylnx_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Morrigan";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylnx_roci_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Tachi";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylnx_drummer_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Drummer";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylynx_silversmith_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Silversmith";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylnx_scircocco_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Scirocco";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylnx_mega_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Kaminari";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("arylnx_rzb_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Razorback";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.Contains("aryxlnx_yacht_epstein_drive"))
                        {
                            particleToCreate = "Aryx_Driveplume_Yacht";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = defaultDriveSound;
                        }
                        else if (typeComparison.ToLower().Contains("xrcs")) //todo: differentiate lg/sg rcs plumes.
                        {
                            particleToCreate = "Aryx_Driveplume_RCS";
                            particleSize = 1f;
                            particleRadius = 1f;
                            driveSoundToUse = "";
                        }
                        else
                        {
                            isValidEDrive = false;
                        }
                    }
                    else
                    {
                        isValidEDrive = false;
                    }
                    initialised = true; //Stop this terribleness from being called again.
                }
            }
        }
        public override void UpdateAfterSimulation()
        {
            if (isValidEDrive)
            {
                if (!MyAPIGateway.Utilities.IsDedicated) //this needs fixed.
                {
                    if (thrust.CurrentThrust / thrust.MaxThrust >= 0.05f) thrusterFiring = true;
                    else thrusterFiring = false;

                    if (thrusterFiring && thrust.Enabled)
                    {
                        if (particle == null)
                        {
                            particle_matrix = thrust.WorldMatrix;
                            particle_position = particle_matrix.Translation;
                            MyParticlesManager.TryCreateParticleEffect(particleToCreate, ref particle_matrix, ref particle_position, uint.MaxValue, out particle);
                        }
                        else
                        {
                            particle.WorldMatrix = thrust.WorldMatrix;
                            particle.UserRadiusMultiplier = particleRadius;
                            particle.UserScale = particleSize;
                            particle.UserLifeMultiplier = thrust.CurrentThrust / thrust.MaxThrust;
                            particle.Play();
                        }
                        if (emitter == null || driveSound == null)
                        {
                            emitter = new MyEntity3DSoundEmitter((MyEntity)thrust);
                            driveSound = new MySoundPair(driveSoundToUse);
                        }
                        else { if (!emitter.IsPlaying) emitter.PlaySound(driveSound); }
                    }
                    else
                    {
                        if (particle != null) //this is a stupid fix
                        {
                            KillParticle();

                        }
                        if (emitter != null)
                        {
                            if (emitter.IsPlaying) emitter.StopSound(true, false);
                        }
                    }
                }
            }
        }
        public override void Close()
        {
            if (thrusterFiring && isValidEDrive)
            {
                if (particle != null) KillParticle();
                if (emitter != null)
                {
                    if (emitter.IsPlaying) emitter.StopSound(false, true);
                }
            }
        }
        private void KillParticle() //The block button ain't enough I want this mf dead
        {
            particle.StopLights();
            particle.StopEmitting();
            particle.Stop();
        }
    }
}