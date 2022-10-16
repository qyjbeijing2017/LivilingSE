using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class WaitForSecond : Coroutine
        {
            double second = 0;
            double time = 0;
            Program program = null;
            public WaitForSecond(Program program, double second) : base(program)
            {
                this.program = program;
                this.second = second * 1000.0f;
            }

            public override IEnumerator GetEnumerator()
            {
                while (time < second)
                {
                    yield return null;
                    time += program.deltaTime;
                }
            }
        }
    }
}
