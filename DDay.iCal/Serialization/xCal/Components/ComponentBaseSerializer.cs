using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.xCal.Components
{
    public class ComponentBaseSerializer : iCalObjectSerializer
    {
        #region Private Fields

        private DDay.iCal.Components.ComponentBase m_Component;

        #endregion

        #region Public Properties

        protected ComponentBase Component
        {
            get { return m_Component; }
            set
            {
                if (!object.Equals(m_Component, value))
                {
                    m_Component = value;
                    base.Object = value;
                }
            }
        }

        #endregion

        #region Constructors

        public ComponentBaseSerializer() { }
        public ComponentBaseSerializer(DDay.iCal.Components.ComponentBase component)
            : base(component)
        {
            Component = component;
        }

        #endregion
    }
}
