// <copyright file="CpuStats.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

namespace Pedantic.Utilities
{
    using System.Diagnostics;

    public struct CpuStats
    {
        public CpuStats()
        {
            startTime = DateTime.Now;
            startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        }

        public void Reset()
        {
            startTime = DateTime.Now;
            startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
        }

        public int CpuLoad
        {
            get
            {
                TimeSpan totalUsage = Process.GetCurrentProcess().TotalProcessorTime - startCpuUsage;
                TimeSpan totalTime = DateTime.Now - startTime;
                int cpuLoad = (int)((totalUsage.TotalMilliseconds * 1000) / (totalTime.TotalMilliseconds));
                return cpuLoad;
            }
        }

        private DateTime startTime;
        private TimeSpan startCpuUsage;
    }
}
