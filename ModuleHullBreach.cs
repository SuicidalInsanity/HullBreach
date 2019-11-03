using System;
using UnityEngine;

namespace HullBreach
{
    public class ModuleHullBreach : PartModule
    {
        static ModuleHullBreach instance;
        public static ModuleHullBreach Instance => instance;

        #region KSP Fields

        float _hp = 0;
        float maxDamage = 0;

        public bool isHullBreached;
        public string DamageState = "None"; //None, Minor, Serious, Fatal

        [KSPField(isPersistant = false)]
        public double MinorFlooding = .3;

        [KSPField(isPersistant = false)]
        public double SeriousFlooding = .7;

        [KSPField(isPersistant = false)]
        public double FatalFlooding = 1;

        [KSPField(isPersistant = false)]
        public double MinorDmg = 0.90;

        [KSPField(isPersistant = false)]
        public double SeriousDmg = 0.6;

        [KSPField(isPersistant = false)]
        public double FatalDmg = 0.25;

        [KSPField(isPersistant = true)]
        public bool hydroExplosive = false;

        [KSPField(isPersistant = true)]
        public bool hull = false;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Crush Depth", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 600f, stepIncrement = 1f)]
        public float crushDepth = 200f;

        [KSPField(isPersistant = true)]
        public bool DepthCharge = false;

        #region Debug Fields

        [KSPField(isPersistant = true)]
        public bool partDebug = true;

        //[KSPField(guiActive = true, isPersistant = false, guiName = "Submerged Portion")]
        //public double sumergedPortion;

        //[KSPField(guiActive = true, isPersistant = false, guiName = "Current Situation")]
        //public string vesselSituation;

        //[KSPField(guiActive = true, isPersistant = false, guiName = "Heat Level")]
        //public double pctHeat = 0;

        [KSPField(guiActive = true, isPersistant = false, guiName = "Current Depth")]
        public double currentDepth = 0;

        [KSPField(guiActive = true, isPersistant = false, guiName = "Vessel Mass")]
        public double VesselMass;

        #endregion DebugFields

        [UI_FloatRange(minValue = 1, maxValue = 100, stepIncrement = 1)]
        [KSPField(guiActive = true, guiActiveEditor = true, isPersistant = true, guiName = "Flow Rate")]
        public float flowMultiplier = 1;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Test Breach")]
        public static Boolean forceHullBreach;

        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Test Breach")]


        #endregion KSPFields

        public void ToggleHullBreach()
        {
            if (!vessel.isActiveVessel) { return; }

            if (isHullBreached)
            {
                isHullBreached = false;
                forceHullBreach = false;
                DamageState = "None";
                //FixedUpdate();
            }
            else
            {
                isHullBreached = true;
                forceHullBreach = true;
                DamageState = "Serious";
                //FixedUpdate();
            }
        }

        #region GameEvents

        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor && vessel != null)
            {
                part.force_activate();
                instance = this;
                if (part.FindModulesImplementing<ModuleHullBreach>().Count != 0)
                    config.Instance.vesselHullBreach = this;
            }

            if (!crushable)
            {
                Fields["crushDepth"].guiActive = false;
                Fields["crushDepth"].guiActiveEditor = false;
            }
            else
            {
                Fields["flowMultiplier"].guiActive = false;
                Fields["flowMultiplier"].guiActiveEditor = false;
            }
        }

        public void CheckCatastrophicBreach(PartJoint partJoint, float breakForce)
        {
            if (vessel.situation != Vessel.Situations.SPLASHED) return;
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            try
            {
                if (part == null ||
                    !part.Modules.Contains<ModuleHullBreach>() ||
                    !part.Modules.Contains("HitpointTracker") ||
                    part.Modules.Contains<ModuleHBIgnore>()
                    )
                    return;
            }
            catch (Exception)
            { }

            part.rigidAttachment = true;

            if (vessel.situation != Vessel.Situations.SPLASHED) return;

            if (part.WaterContact & ShipIsDamaged() & isHullBreached & hull)
            {
                if (DamageState == "Minor")
                {
                    vessel.IgnoreGForces(240);
                    part.RequestResource("SeaWater", (0 - (MinorFlooding * (0.1 + part.submergedPortion) * flowMultiplier)));
                    if (this.vessel == FlightGlobals.ActiveVessel)
                    {
                        ScreenMessages.PostScreenMessage("Warning: Minor Hull Breach", 1.0f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                else
                {
                    if (DamageState == "Serious")
                    {
                        vessel.IgnoreGForces(240);
                        part.RequestResource("SeaWater", (0 - (SeriousFlooding * (0.1 + part.submergedPortion) * flowMultiplier)));
                        if (this.vessel == FlightGlobals.ActiveVessel)
                        {
                            ScreenMessages.PostScreenMessage("Warning: Serious Hull Breach!", 1.0f, ScreenMessageStyle.UPPER_CENTER);
                        }
                    }
                    else
                    {
                        if (DamageState == "Fatal")
                        {
                            vessel.IgnoreGForces(240);
                            part.RequestResource("SeaWater", (0 - (FatalFlooding * (0.1 + part.submergedPortion) * flowMultiplier)));
                            if (this.vessel == FlightGlobals.ActiveVessel)
                            {
                                ScreenMessages.PostScreenMessage("Warning: FATAL HULL BREACH!!", 1.0f, ScreenMessageStyle.UPPER_CENTER);
                            }
                        }
                    }
                }
            }

            //If part underwater add damage at a greater rate based on depth to simulate pressure
            //sumergedPortion = Math.Round(this.part.submergedPortion, 4);

            if (part.submergedPortion == 1.00 & hydroExplosive)
            {
                part.temperature += (0.1 * part.depth);
            }
            else if (crushable && part.submergedPortion == 1.00 && !part.localRoot.name.StartsWith("Sub"))
            {

                if (config.ecDrain)
                    part.RequestResource("ElectricCharge", 1000d); //kill EC if sumberged

                //if (crushable) part.buoyancy = -1.0f; // trying to kill floaty bits that never sink 

                if (warnTimer > 0f) warnTimer -= Time.deltaTime;
                if (part.depth > warnDepth && oldVesselDepth > warnDepth && warnTimer <= 0)
                {
                    if (FlightGlobals.ActiveVessel)
                    {
                        if (DepthCharge == false)
                        {
                            ScreenMessages.PostScreenMessage(
                            "Warning! Vessel will be crushed at " + (crushDepth) + "m depth!", 3,
                            ScreenMessageStyle.LOWER_CENTER);
                        }
                        else
                        {
                            ScreenMessages.PostScreenMessage(
                            (crushDepth) + "m Charge Deployed!", 3,
                            ScreenMessageStyle.LOWER_CENTER);
                        }
                    }
                    warnTimer = 5;
                }
                oldVesselDepth = part.depth;
                CrushingDepth();
            }
        }

        public void LateUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;
            try
            {
                if (part == null ||
                    !part.Modules.Contains("ModuleHullBreach") ||
                    !part.Modules.Contains("HitpointTracker")
                    )
                    return;
            }
            catch (Exception)
            { }
            currentDepth = Math.Round(part.depth, 2);
            VesselMass = Math.Round(vessel.totalMass);
        }

        public void OnDestroy()
        {
            instance = null;
        }

        #endregion

        #region HullBreach Events

        public bool ShipIsDamaged()
        {
            if (forceHullBreach == true) // if testing a breach then this code runs
            {
                isHullBreached = true;
                DamageState = "Serious";
                return true;
            }
            else // this code runs if not testing a breach
            {
                maxDamage = part.MaxDamage();
                _hp = part.Damage();
                float dmg_pct = _hp / maxDamage;
                if (dmg_pct <= MinorDmg) // if hp left is below minimum breach damage
                {
                    isHullBreached = true;
                    if (dmg_pct <= FatalDmg) // if percentage of hp left is less than or equal to Fatal
                    {
                        DamageState = "Fatal";
                    }
                    else
                    {
                        if (dmg_pct <= SeriousDmg) // if percentage of hp left is lower than Serious
                        {
                            DamageState = "Serious";
                        }
                        else
                        {
                            DamageState = "Minor";
                        }
                    }

                    return true; // regardless of ammount of damage, the hull has a hole in it
                }
                else // if hp left is above minimum breach damage ... this should reset the system if testing a breach and the test is cancelled
                {
                    isHullBreached = false;
                    DamageState = "None";
                    return false;
                }
            }
        }

        #region Parts that do not take on water crushed by going below a certain depth

        [KSPField(isPersistant = true)] public bool crushable = false;

        public double warnTimer = 0;
        public double warnDepth = 100;
        public double oldVesselDepth;

        private void CrushingDepth()
        {
            //Nothing crushed unless : Vessel is under water, part is crushable,part is fully submerged, part is not a hull and part is not hydroexplosive
            // Any of these true do not crush
            if (!crushable || hull || hydroExplosive || part.submergedPortion != 1.00 || TrueAlt() > 0.01) return;

            if (crushable & part.depth > crushDepth & (TrueAlt() * -1) > crushDepth)
            {
                part.explode();
            }
        }

        public double TrueAlt()
        {
            Vector3 pos = vessel.GetWorldPos3D();
            double ASL = FlightGlobals.getAltitudeAtPos(pos);
            if (vessel.mainBody.pqsController == null)
            {
                return ASL;
            }
            double terrainAlt = vessel.pqsAltitude;
            if (vessel.mainBody.ocean && terrainAlt <= 0)
            {
                return ASL;
            } //Checks for oceans
            return ASL - terrainAlt;
        }

        #endregion

        #endregion
    }
}