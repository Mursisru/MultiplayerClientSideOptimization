using System.Text;

using NOLoader.API;

using NuclearOption.Networking;

using UnityEngine;



namespace NOLoader.MultiplayerClientSideOptimization

{

    internal static class MpPhysicsProfiler

    {

        internal static void ReportIfDue(float dt)

        {

            if (!MpConfig.ProfilerEnabled || !MpSessionState.Active)

                return;



            MpSessionState.ReportAccumulator += dt;

            if (MpSessionState.ReportAccumulator < MpConfig.ReportIntervalS)

                return;



            MpSessionState.ReportAccumulator = 0f;

            LogSnapshot();

        }



        private static void LogSnapshot()

        {

            int remoteAircraft = 0;

            int localAircraft = 0;

            int remoteMissiles = 0;



            foreach (Unit unit in UnitRegistry.allUnits)

            {

                if (unit == null)

                    continue;



                if (unit is Aircraft)

                {

                    if (unit.LocalSim)

                        localAircraft++;

                    else

                        remoteAircraft++;

                }

                else if (unit is Missile missile && !missile.LocalSim)

                {

                    remoteMissiles++;

                }

            }



            int players = NetworkManagerNuclearOption.i?.GamePlayers.Count ?? 0;



            float fixedMs = 0f;

            float lateMs = 0f;

            float sendMs = 0f;

            try

            {

                fixedMs = NuclearOption.Debugging.PlayerLoopPerformanceTracker.FixedUpdateTime;

                lateMs = NuclearOption.Debugging.PlayerLoopPerformanceTracker.LateUpdateTime;

                sendMs = NuclearOption.Debugging.PlayerLoopPerformanceTracker.SendTime;

            }

            catch

            {

                // optional debug API

            }



            var sb = new StringBuilder(512);

            sb.Append("[MpOpt] v0.6.3 units=").Append(UnitRegistry.allUnits.Count)

                .Append(" localAc=").Append(localAircraft)

                .Append(" remoteAc=").Append(remoteAircraft)

                .Append(" remoteMs=").Append(remoteMissiles)

                .Append(" players=").Append(players)

                .Append(" fixedMs=").Append(fixedMs.ToString("F2"))

                .Append(" lateMs=").Append(lateMs.ToString("F2"))

                .Append(" sendMs=").Append(sendMs.ToString("F2"))

                .Append(" opticalM=").Append(MpStats.LastOpticalRangeM.ToString("F0"))

                .Append(" memMb=").Append(MpMemoryBudget.GetMonoUsedMb())

                .Append(" reservedMb=").Append(MpMemoryBudget.ReservedMb)

                .Append(" skip acFU=").Append(MpStats.AircraftFixedUpdateSkipped)

                .Append(" missThrust=").Append(MpStats.MissileMotorThrustSkipped)

                .Append(" missFU=").Append(MpStats.MissileFixedUpdateSkipped)

                .Append(" visual=").Append(MpStats.VisualUpdateSkipped)

                .Append(" removeOld=").Append(MpStats.SnapshotRemoveOldSkipped)

                .Append(" batcher=").Append(MpStats.BatcherVisualIterSkipped)
                .Append(" frozen=").Append(MpDeepFreezeManager.FrozenCount)
                .Append(" freezeApply=").Append(MpStats.DeepFreezeApply)
                .Append(" deopt=").Append(MpStats.DeepFreezeDeopt)
                .Append(" visualDeep=").Append(MpStats.VisualDeepSkipped)
                .Append(" cosmeticBeh=").Append(MpStats.CosmeticBehavioursDisabled)
                .Append(" cosmeticLights=").Append(MpStats.CosmeticLightsDisabled)
                .Append(" visualFull=").Append(MpStats.VisualFullZoneSkipped)
                .Append(" compFull=").Append(MpStats.ComponentFullZoneSkipped)

                .Append(" rb=").Append(MpStats.RbApplySkipped)

                .Append(" comp=").Append(MpStats.RemoteComponentSkipped)

                .Append(" weapon=").Append(MpStats.WeaponShellSkipped)

                .Append(" vapor=").Append(MpStats.VaporSkipped);



            LoaderLog.Write(sb.ToString());

        }

    }

}


