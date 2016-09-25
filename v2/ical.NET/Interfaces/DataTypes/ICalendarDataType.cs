using System;
using ical.net.Interfaces.General;
using IServiceProvider = ical.net.Interfaces.General.IServiceProvider;

namespace ical.net.Interfaces.DataTypes
{
    public interface ICalendarDataType : ICalendarParameterCollectionContainer, ICopyable, IServiceProvider
    {
        Type GetValueType();
        void SetValueType(string type);
        ICalendarObject AssociatedObject { get; set; }
        ICalendar Calendar { get; }

        string Language { get; set; }
    }
}