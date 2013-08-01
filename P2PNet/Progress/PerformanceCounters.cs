//
// - PerformanceCounters.cs
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
using System.Linq;
using System.Text;

namespace P2PNet
{
    class PerformanceCounters
    {
        private const string CategoryName = "P2PNet";
        private const string IncommingConnectionsCounterName = "# incomming connections";
        private const string OutgoingConnectionsCounterName = "# outgoing connections";
        private const string BufferMemoryInUseCounterName = "buffer memory in use";
        private const string BytesReceivedCounterName = "# bytes received";

        private static readonly Lazy<PerformanceCounter> _incommingConnections = new Lazy<PerformanceCounter>(() => CreateCounter(IncommingConnectionsCounterName));
        private static readonly Lazy<PerformanceCounter> _bufferMemoryUse = new Lazy<PerformanceCounter>(() => CreateCounter(BufferMemoryInUseCounterName));
        private static readonly Lazy<PerformanceCounter> _bytesReceived = new Lazy<PerformanceCounter>(() => CreateCounter(BytesReceivedCounterName));

        private static PerformanceCounter CreateCounter(string name)
        {
            return new PerformanceCounter{
                        CategoryName = CategoryName,
                        CounterName = name,
                        MachineName = ".",
                        ReadOnly = false
                    };            
        }

        internal static PerformanceCounter IncommingConnections
        {
            get { return _incommingConnections.Value; }    
        }

        internal static PerformanceCounter BufferMemoryUsed
        {
            get { return _bufferMemoryUse.Value; }    
        }

        internal static PerformanceCounter BytesReceived
        {
            get { return _bytesReceived.Value; }
        }

        private void RegisterPerformanceCounters()
        {
            if (PerformanceCounterCategory.Exists(CategoryName)) return;

            var counters = new CounterCreationDataCollection{
                new CounterCreationData{
                        CounterName = IncommingConnectionsCounterName,
                        CounterHelp = "Total number of incomming connections",
                        CounterType = PerformanceCounterType.NumberOfItems32
                },
                new CounterCreationData{
                    CounterName = OutgoingConnectionsCounterName,
                    CounterHelp = "Total number of outgoing connections",
                    CounterType = PerformanceCounterType.NumberOfItems32
                },
                new CounterCreationData{
                    CounterName = BufferMemoryInUseCounterName,
                    CounterHelp = "Total number of bytes used by buffers",
                    CounterType = PerformanceCounterType.NumberOfItems32
                },
                new CounterCreationData{
                        CounterName = BytesReceivedCounterName,
                        CounterHelp = "Total number of bytes received",
                        CounterType = PerformanceCounterType.NumberOfItems32
                }
            };

            PerformanceCounterCategory.Create(CategoryName, "P2PNet resources performance", PerformanceCounterCategoryType.SingleInstance, counters);
        }
    }
}
