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
    partial class Program : MyGridProgram
    {
        const double TimeStep = 1.0 / 6.0;
        PID _pid;
        IMyShipController Cockpit = null;
        List<IMyGyro> gyros = new List<IMyGyro>();

        public Program()
        {
            _pid = new PID(2.0, 0.0, 0.0, TimeStep);
            List<IMyShipController> cockpits = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType(cockpits);

            foreach (var cockpit in cockpits)
            {
                if (cockpit.CanControlShip)
                {
                    Cockpit = cockpit;
                }
            }

            if (Cockpit == null)
            {
                Echo("Can not found the ship controller");
            }

            GridTerminalSystem.GetBlocksOfType(gyros);

            if (gyros.Count <= 0)
            {
                Echo("Can not found the ship gyros");
            }
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                //// Program start
                //if (!isCoroutinesRunning)
                //{
                //    // TODO: Start

                //}
                //// Update
                //// TODO: update

                //// update Coroutines
                //RunCoroutines(argument, updateSource);

                if ((updateSource & (UpdateType.Terminal | UpdateType.Trigger)) != 0)
                {
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    foreach (var gyro in gyros)
                    {
                        gyro.GyroOverride = false;
                    }
                }

                if (Cockpit != null && gyros.Count > 0 && (updateSource & UpdateType.Update10) != 0)
                {
                    if(Cockpit.RollIndicator >= 0.03 || Cockpit.RotationIndicator.Length() >= 0.03)
                    {
                        Runtime.UpdateFrequency = UpdateFrequency.None;
                        foreach (var gyro in gyros)
                        {
                            gyro.GyroOverride = false;
                        }
                        return;
                    }

                    var gravityd = Cockpit.GetNaturalGravity();

                    // 转到飞船坐标
                    var world2Ship = MatrixD.CreateLookAt(new Vector3(), Cockpit.WorldMatrix.Forward, Cockpit.WorldMatrix.Up);

                    Vector3D gravityShip = Vector3D.TransformNormal(gravityd, world2Ship);


                    
                    // 获得目标向量
                    var target = -new Vector3(gravityShip.X, gravityShip.Y, gravityShip.Z);
                    var up = new Vector3(0.0, 1.0, 0.0);
                    // 获得旋转矩阵
                    var rotationMat = new Matrix3x3();
                    Matrix3x3.CreateRotationFromTwoVectors(ref up, ref target, out rotationMat);
                    // 获得欧拉角变化值
                    var eular = new Vector3();
                    Matrix3x3.GetEulerAnglesXYZ(ref rotationMat, out eular);

                    // 获得需要旋转的角度, 并且重置方向
                    float error = eular.Normalize();

                    float speed = (float)_pid.Control(error);

                    if (speed <= 0.005)
                    {
                        Runtime.UpdateFrequency = UpdateFrequency.None;
                        foreach (var gyro in gyros)
                        {
                            gyro.GyroOverride = false;
                        }
                        return;
                    }

                    var currentEular = new Vector3(eular.X * speed, eular.Y * speed, eular.Z * speed);

                    foreach (var gyro in gyros)
                    {
                        gyro.GyroOverride = true;
                        gyro.Pitch = currentEular.X;
                        gyro.Yaw = currentEular.Y;
                        gyro.Roll = currentEular.Z;

                    }
                }

            }
            catch (Exception e)
            {
                Runtime.UpdateFrequency = UpdateFrequency.None;
                Echo(e.Message);
                Echo(e.StackTrace);
            }
        }
    }
}
