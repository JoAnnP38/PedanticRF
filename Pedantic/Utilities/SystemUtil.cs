// <copyright file="Util.cs" company="JoAnn D. Peeler">
// Copyright (c) JoAnn D. Peeler. All rights reserved.
//
// Licensed under the MIT license. See LICENSE file in the project root for full
// license information.
// </copyright>

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pedantic.Utilities
{
    public static class SystemUtil
    {
        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        public static int EmptyWorkingSet()
        {
            return EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }
    }
}
