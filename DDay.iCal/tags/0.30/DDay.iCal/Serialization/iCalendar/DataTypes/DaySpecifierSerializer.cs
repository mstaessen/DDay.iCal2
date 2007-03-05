using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class DaySpecifierSerializer : FieldSerializer
    {
        #region Private Fields

        private Recur.DaySpecifier m_DaySpecifier;

        #endregion

        #region Constructors

        public DaySpecifierSerializer(Recur.DaySpecifier byday)
            : base(byday)
        {
            this.m_DaySpecifier = byday;
        }

        #endregion

        #region ISerializable Members

        public override string SerializeToString()
        {
            string value = string.Empty;
            if (m_DaySpecifier.Num != int.MinValue)
                value += m_DaySpecifier.Num;
            value += Enum.GetName(typeof(DayOfWeek), m_DaySpecifier.DayOfWeek).ToUpper().Substring(0, 2);
            return value;
        }

        #endregion
    }
}
