using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IBinary :
        ICalendarDataType
    {
        IURI Uri { get; set; }
        IText FormatType { get; set; }

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated
        /// at the given <see cref="URI"/>.
        /// </summary>
        void LoadDataFromUri();

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated
        /// at the given <see cref="URI"/>.
        /// </summary>
        /// <param name="username">The username to supply for credentials</param>
        /// <param name="password">The pasword to supply for credentials</param>
        void LoadDataFromUri(string username, string password);

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated
        /// at the given <see cref="URI"/>.
        /// </summary>
        /// <param name="uri">The Uri from which to download the <c>Data</c></param>
        /// <param name="username">The username to supply for credentials</param>
        /// <param name="password">The pasword to supply for credentials</param>
        void LoadDataFromUri(Uri uri, string username, string password);
    }
}
