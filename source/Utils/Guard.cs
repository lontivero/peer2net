//
// - Guard.cs
// 
// Author:
//     Lucas Ontivero <lucasontivero@gmail.com>
// 
// Copyright 2013 Lucas E. Ontivero
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

// <summary></summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Open.P2P.Utils
{
    static class Guard
    {
        [DebuggerStepThrough]
        public static void NotNull(object o, string paramName)
        {
            if(o == null) throw new ArgumentNullException(paramName);
        }

        [DebuggerStepThrough]
        public static void NotEmpty(string str, string paramName)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(paramName);
        }

        [DebuggerStepThrough]
        public static void IsBeetwen(int val, int min, int max, string paramName)
        {
            if(val <min || val > max) throw new ArgumentOutOfRangeException(paramName);
        }

        public static void IsGreaterOrEqualTo(int val, int min, string paramName)
        {
            if (val < min) throw new ArgumentOutOfRangeException(paramName);
        }

        public static void ContainsKey<T,TQ>(IDictionary<T, TQ> dict, T key, string message)
        {
            if(!dict.ContainsKey(key)) throw new ArgumentException(message);
        }
    }
}
