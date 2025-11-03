// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0

using System;
using Xunit;
using OpenVDB.Utils;

namespace OpenVDB.Tests.UtilsTests
{
    public class NodeMasksTests
    {
        [Fact]
        public void NodeMask_Constructor_ShouldInitializeAllBitsOff()
        {
            var mask = new NodeMask(8);
            Assert.False(mask.IsOn(0));
            Assert.False(mask.IsOn(7));
        }

        [Fact]
        public void NodeMask_SetOn_ShouldTurnBitOn()
        {
            var mask = new NodeMask(8);
            mask.SetOn(3);
            Assert.True(mask.IsOn(3));
            Assert.False(mask.IsOn(2));
            Assert.False(mask.IsOn(4));
        }

        [Fact]
        public void NodeMask_SetOff_ShouldTurnBitOff()
        {
            var mask = new NodeMask(8);
            mask.SetOn(3);
            Assert.True(mask.IsOn(3));
            
            mask.SetOff(3);
            Assert.False(mask.IsOn(3));
        }

        [Fact]
        public void NodeMask_CountOn_ShouldReturnNumberOfOnBits()
        {
            var mask = new NodeMask(8);
            Assert.Equal(0, mask.CountOn());
            
            mask.SetOn(0);
            mask.SetOn(3);
            mask.SetOn(7);
            Assert.Equal(3, mask.CountOn());
        }

        [Fact]
        public void NodeMask_IsOff_ShouldBeOppositeOfIsOn()
        {
            var mask = new NodeMask(8);
            Assert.True(mask.IsOff(0));
            
            mask.SetOn(0);
            Assert.False(mask.IsOff(0));
        }
    }

    public class CpuTimerTests
    {
        [Fact]
        public void CpuTimer_Start_ShouldSetStartTime()
        {
            var timer = new CpuTimer();
            timer.Start();
            
            Assert.True(timer.IsRunning);
        }

        [Fact]
        public void CpuTimer_Stop_ShouldCalculateElapsedTime()
        {
            var timer = new CpuTimer();
            timer.Start();
            System.Threading.Thread.Sleep(10);
            timer.Stop();
            
            Assert.False(timer.IsRunning);
            Assert.True(timer.ElapsedMilliseconds >= 10);
        }

        [Fact]
        public void CpuTimer_Reset_ShouldClearElapsedTime()
        {
            var timer = new CpuTimer();
            timer.Start();
            System.Threading.Thread.Sleep(10);
            timer.Stop();
            
            var elapsed = timer.ElapsedMilliseconds;
            Assert.True(elapsed > 0);
            
            timer.Reset();
            Assert.Equal(0, timer.ElapsedMilliseconds);
        }

        [Fact]
        public void CpuTimer_Restart_ShouldResetAndStart()
        {
            var timer = new CpuTimer();
            timer.Start();
            System.Threading.Thread.Sleep(10);
            timer.Stop();
            
            timer.Restart();
            Assert.True(timer.IsRunning);
        }
    }

    public class NullInterrupterTests
    {
        [Fact]
        public void NullInterrupter_Start_ShouldNotThrow()
        {
            var interrupter = new NullInterrupter();
            interrupter.Start();
        }

        [Fact]
        public void NullInterrupter_End_ShouldNotThrow()
        {
            var interrupter = new NullInterrupter();
            interrupter.End();
        }

        [Fact]
        public void NullInterrupter_WasInterrupted_ShouldAlwaysReturnFalse()
        {
            var interrupter = new NullInterrupter();
            Assert.False(interrupter.WasInterrupted(0));
            Assert.False(interrupter.WasInterrupted(100));
        }
    }

    public class NameTests
    {
        [Fact]
        public void Name_Constructor_ShouldSetValue()
        {
            var name = new Name("test");
            Assert.Equal("test", name.Value);
        }

        [Fact]
        public void Name_Equality_ShouldWork()
        {
            var name1 = new Name("test");
            var name2 = new Name("test");
            var name3 = new Name("other");
            
            Assert.Equal(name1, name2);
            Assert.NotEqual(name1, name3);
        }

        [Fact]
        public void Name_ImplicitConversion_ToString_ShouldWork()
        {
            var name = new Name("test");
            string str = name;
            Assert.Equal("test", str);
        }

        [Fact]
        public void Name_ExplicitConversion_FromString_ShouldWork()
        {
            var name = (Name)"test";
            Assert.Equal("test", name.Value);
        }
    }

    public class LoggingTests
    {
        [Fact]
        public void Logging_SetLevel_ShouldChangeLevel()
        {
            global::Logging.SetLevel(global::VDBUtils.Logging.Level.Info);
            Assert.Equal(global::Logging.Level.Info, global::VDBUtils.Logging.GetLevel());
            
            global::Logging.SetLevel(global::VDBUtils.Logging.Level.Warn);
            Assert.Equal(global::Logging.Level.Warn, global::VDBUtils.Logging.GetLevel());
        }

        [Fact]
        public void Logging_Log_ShouldNotThrow()
        {
            global::Logging.SetLevel(global::VDBUtils.Logging.Level.Debug);
            global::Logging.Log(global::VDBUtils.Logging.Level.Debug, "Debug message");
            global::Logging.Log(global::VDBUtils.Logging.Level.Info, "Info message");
            global::Logging.Log(global::VDBUtils.Logging.Level.Warn, "Warning message");
            global::Logging.Log(global::VDBUtils.Logging.Level.Error, "Error message");
        }

        [Fact]
        public void Logging_Info_ShouldNotThrow()
        {
            global::Logging.Info("Test info message");
        }

        [Fact]
        public void Logging_Warn_ShouldNotThrow()
        {
            global::Logging.Warn("Test warning message");
        }

        [Fact]
        public void Logging_Error_ShouldNotThrow()
        {
            global::Logging.Error("Test error message");
        }
    }
}
