using System;
using System.Collections.Generic;
using System.Text;

#if !DATACONTRACT
namespace System.Runtime.Serialization
{
    // Summary:
    //     Describes the source and destination of a given serialized stream, and provides
    //     an additional caller-defined context.
    [Serializable]
    public struct StreamingContext
    {
        #region Private Fields

        private StreamingContextStates m_States;

        #endregion

        //
        // Summary:
        //     Initializes a new instance of the System.Runtime.Serialization.StreamingContext
        //     class with a given context state.
        //
        // Parameters:
        //   state:
        //     A bitwise combination of the System.Runtime.Serialization.StreamingContextStates
        //     values that specify the source or destination context for this System.Runtime.Serialization.StreamingContext.
        public StreamingContext(StreamingContextStates state)
        {
            m_States = state;
        }

        // Summary:
        //     Gets context specified as part of the additional context.
        //
        // Returns:
        //     The context specified as part of the additional context.
        public object Context { get; }
        
        //
        // Summary:
        //     Gets the source or destination of the transmitted data.
        //
        // Returns:
        //     During serialization, the destination of the transmitted data. During deserialization,
        //     the source of the data.
        public StreamingContextStates State { get { return m_States; } }
    }

    // Summary:
    //     Defines a set of flags that specifies the source or destination context for
    //     the stream during serialization.
    [Serializable]
    [Flags]
    public enum StreamingContextStates
    {
        // Summary:
        //     Specifies that the source or destination context is a different process on
        //     the same computer.
        CrossProcess = 1,
        //
        // Summary:
        //     Specifies that the source or destination context is a different computer.
        CrossMachine = 2,
        //
        // Summary:
        //     Specifies that the source or destination context is a file. Users can assume
        //     that files will last longer than the process that created them and not serialize
        //     objects in such a way that deserialization will require accessing any data
        //     from the current process.
        File = 4,
        //
        // Summary:
        //     Specifies that the source or destination context is a persisted store, which
        //     could include databases, files, or other backing stores. Users can assume
        //     that persisted data will last longer than the process that created the data
        //     and not serialize objects so that deserialization will require accessing
        //     any data from the current process.
        Persistence = 8,
        //
        // Summary:
        //     Specifies that the data is remoted to a context in an unknown location. Users
        //     cannot make any assumptions whether this is on the same computer.
        Remoting = 16,
        //
        // Summary:
        //     Specifies that the serialization context is unknown.
        Other = 32,
        //
        // Summary:
        //     Specifies that the object graph is being cloned. Users can assume that the
        //     cloned graph will continue to exist within the same process and be safe to
        //     access handles or other references to unmanaged resources.
        Clone = 64,
        //
        // Summary:
        //     Specifies that the source or destination context is a different AppDomain.
        //     (For a description of AppDomains, see Application Domains).
        CrossAppDomain = 128,
        //
        // Summary:
        //     Specifies that the serialized data can be transmitted to or received from
        //     any of the other contexts.
        All = 255,
    }
}
#endif
