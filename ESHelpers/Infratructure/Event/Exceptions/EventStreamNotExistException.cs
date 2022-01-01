using System;

namespace ESHelpers.Infratructure.Event.Exceptions
{
    public class EventStreamNotExistException: Exception
    {
        public EventStreamNotExistException(string? message) : base(message)
        {
        }
    }
}