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
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        IMyShipController cockpit = null;
        IMyLightingBlock lightingUpBlock = null, lightingDownBlock = null;

        List<IMyGyro> gyros = new List<IMyGyro>();
        List<IMyThrust> upThrusts = new List<IMyThrust>();
        List<IMyThrust> downThrusts = new List<IMyThrust>();
        List<IMyThrust> leftThrusts = new List<IMyThrust>();
        List<IMyThrust> rightThrusts = new List<IMyThrust>();
        List<IMyThrust> frontThrusts = new List<IMyThrust>();
        List<IMyThrust> backThrusts = new List<IMyThrust>();

        String lightingUp = "AutoUp";

        String lightingDown = "AutoDown";

        bool upWorking = false;
        bool downWorking = false;

        public Program()
        {
            // 构造函数，每次脚本运行时会被首先调用一次。用它来初始化脚本。
            //  构造函数是可选项，
            // 如不需要可以删除。
            // 
            // 建议这里设定RuntimeInfo.UpdateFrequency，
            // 这样脚本就不需要定时器方块也能自动运行了。
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            GridTerminalSystem.GetBlocksOfType(gyros);

            lightingUpBlock = (IMyLightingBlock)GridTerminalSystem.GetBlockWithName(lightingUp);

            lightingDownBlock = (IMyLightingBlock)GridTerminalSystem.GetBlockWithName(lightingDown);

            //获取当前玩家所在驾驶舱
            if (cockpit == null)
            {
                List<IMyShipController> cockpits = new List<IMyShipController>();
                GridTerminalSystem.GetBlocksOfType(cockpits);
                if (cockpits.Count < 1) return;
                cockpit = cockpits[0];

            }

            //获取所有引擎
            List<IMyThrust> thrusts = new List<IMyThrust>();

            GridTerminalSystem.GetBlocksOfType(thrusts);




            //确定引擎方向
            foreach (var thrust in thrusts)
            {
                //判断引擎喷口对于驾驶舱的相对方向
                switch (cockpit.WorldMatrix.GetClosestDirection(thrust.WorldMatrix.Backward))
                {

                    //设定推力
                    case Base6Directions.Direction.Up:
                        upThrusts.Add(thrust);
                        break;
                    case Base6Directions.Direction.Down:
                        downThrusts.Add(thrust);
                        break;
                    case Base6Directions.Direction.Left:
                        leftThrusts.Add(thrust);
                        break;
                    case Base6Directions.Direction.Right:
                        rightThrusts.Add(thrust);
                        break;
                    case Base6Directions.Direction.Forward:
                        frontThrusts.Add(thrust);
                        break;
                    case Base6Directions.Direction.Backward:
                        backThrusts.Add(thrust);
                        break;
                }
            }


        }

        public void Save()
        {
            // 当程序需要保存时，会发出提醒。使用这种方法可以将状态保存
            // 至内存或其他路径。此为备选项，
            // 如果不需要，可以删除。

        }

        public void Main(string argument, UpdateType updateSource)
        {
            // 脚本的主入口点，每次调用可编程模块运行操作的一个调用。
            // 该入口点本身是必需的。UpdateSource参数指明更新的来源。
            // 需要使用此方法，但上述参数如果不需要，可以删除。


            var naturalGravity =cockpit.GetNaturalGravity();

            Echo("当前重力："+naturalGravity.Normalize().ToString());
            Echo("AutoUp:" + (upWorking?"true":"false")+" "+ (lightingUpBlock.Enabled ? "true" : "false"));
            Echo("AutoDown:" + (downWorking?"true":"false")+" "+ (lightingDownBlock.Enabled ? "true" : "false"));

            if (lightingUpBlock.Enabled && lightingDownBlock.Enabled)
            {
                Echo("Not Both On");
                return;
            }


            if (lightingUpBlock.Enabled)
            {

                if(naturalGravity.IsZero())
                {
                    setPercent(upThrusts, 0);changeGyrosStatus(gyros, true);upWorking = false;
                }
                else
                {
                    setPercent(upThrusts, 100);
                    setPercent(downThrusts, 0);
                    changeGyrosStatus(gyros, false);
                    upWorking = true;
                }

            }
            else
            {
                if (upWorking)
                {
                    setPercent(upThrusts, 0);
                    changeGyrosStatus(gyros, true);
                    upWorking = false;
                }
            }



            if (lightingDownBlock.Enabled)
            {
                if (((float)naturalGravity.Normalize()) > 8) 
                { 
                    setPercent(downThrusts, 0);
                    changeGyrosStatus(gyros, true);
                    downWorking = false; 
                }
                else 
                { 
                    setPercent(downThrusts, 100); 
                    setPercent(upThrusts, 0); 
                    changeGyrosStatus(gyros, false);
                    downWorking = true; 
                }
            }
            else
            {
                if (downWorking)
                {
                    setPercent(downThrusts, 0);
                    changeGyrosStatus(gyros, true);
                    downWorking = false;
                }
            }


        }


        private void setPercent(List<IMyThrust> thrusts, int percent)
        {
            foreach (var thrust in thrusts)
            {
                thrust.ThrustOverridePercentage = percent;
                thrust.Enabled = true;
            }

        }


        private void changeGyrosStatus(List<IMyGyro> gyros, bool enabled)
        {
            foreach (var gyro in gyros)
            {
                gyro.Enabled = enabled;
            }
        }


    }
}
