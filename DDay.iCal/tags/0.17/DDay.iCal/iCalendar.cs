using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Configuration;
using System.IO;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Objects;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents an iCalendar object.  To load an iCalendar object, generally a
    /// static LoadFromXXX method is used.
    /// <example>
    ///     For example, use the following code to load an iCalendar object from a URL:
    ///     <code>
    ///        iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
    ///     </code>
    /// </example>
    /// Once created, an iCalendar object can be used to gather relevant information about
    /// events, todos, time zones, journal entries, and free/busy time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The following is an example of loading an iCalendar and displaying a text-based calendar.
    /// 
    /// <code>
    /// //
    /// // The following code loads and displays an iCalendar 
    /// // with US Holidays for 2006.
    /// //
    /// iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://www.applegatehomecare.com/Calendars/USHolidays.ics"));
    /// iCal.Evaluate(
    ///     new Date_Time(2006, 1, 1, "US-Eastern", iCal),
    ///     new Date_Time(2006, 12, 31, "US-Eastern", iCal));
    /// 
    /// Date_Time dt = new Date_Time(2006, 1, 1, "US-Eastern", iCal);
    /// while (dt.Year == 2006)
    /// {
    ///     // First, display the current date we're evaluating
    ///     Console.WriteLine(dt.Local.ToShortDateString());
    /// 
    ///     // Then, iterate through each event in our iCalendar
    ///     foreach (Event evt in iCal.Events)
    ///     {
    ///         // Determine if the event occurs on the specified date
    ///         if (evt.OccursOn(dt))
    ///         {
    ///             // Display the event summary
    ///             Console.Write("\t" + evt.Summary);
    /// 
    ///             // Display the time the event happens (unless it's an all-day event)
    ///             if (evt.Start.HasTime)
    ///             {
    ///                 Console.Write(" (" + evt.Start.Local.ToShortTimeString() + " - " + evt.End.Local.ToShortTimeString());
    ///                 if (evt.Start.TimeZoneInfo != null)
    ///                     Console.Write(" " + evt.Start.TimeZoneInfo.TimeZoneName);
    ///                 Console.Write(")");
    ///             }
    /// 
    ///             Console.Write(Environment.NewLine);
    ///         }
    ///     }
    /// 
    ///     // Move to the next day
    ///     dt = dt.AddDays(1);
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// The following example loads all active to-do items from an iCalendar:
    /// 
    /// <code>
    /// //
    /// // The following code loads and displays active todo items from an iCalendar
    /// // for January 6th, 2006.
    /// //
    /// iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
    /// iCal.Evaluate(
    ///     new Date_Time(2006, 1, 1, "US-Eastern", iCal),
    ///     new Date_Time(2006, 1, 31, "US-Eastern", iCal));
    /// 
    /// Date_Time dt = new Date_Time(2006, 1, 6, "US-Eastern", iCal);
    /// foreach(Todo todo in iCal.Todos)
    /// {
    ///     if (todo.IsActive(dt))
    ///     {
    ///         // Display the todo summary
    ///         Console.WriteLine(todo.Summary);
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public class iCalendar : ComponentBase, IDisposable
    {
        #region Constructors

        /// <summary>
        /// To load an existing an iCalendar object, use one of the provided LoadFromXXX methods.
        /// <example>
        /// For example, use the following code to load an iCalendar object from a URL:
        /// <code>
        ///     iCalendar iCal = iCalendar.LoadFromUri(new Uri("http://somesite.com/calendar.ics"));
        /// </code>
        /// </example>
        /// </summary>
        public iCalendar() : base(null)
        {
            this.Name = "VCALENDAR";
            Events = new UniqueComponentList<Event>(this);
            FreeBusy = new List<FreeBusy>();
            Journals = new UniqueComponentList<Journal>(this);
            TimeZones = new List<DDay.iCal.Components.TimeZone>();
            Todos = new UniqueComponentList<Todo>(this);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Adds an <see cref="iCalObject"/>-based component to the
        /// appropriate collection.  Currently, the iCalendar component
        /// supports the following components:
        ///     <list type="bullet">        
        ///         <item><see cref="Event"/></item>
        ///         <item><see cref="FreeBusy"/></item>
        ///         <item><see cref="Journal"/></item>
        ///         <item><see cref="DDay.iCal.Components.TimeZone"/></item>
        ///         <item><see cref="Todo"/></item>
        ///     </list>
        /// </summary>
        /// <param name="child"></param>
        public override void AddChild(iCalObject child)
        {
            base.AddChild(child);            
            child.Parent = this;

            Type type = child.GetType();
            switch (type.Name)
            {
                case "Event": Events.Add((Event)child); break;
                case "FreeBusy": FreeBusy.Add((FreeBusy)child); break;
                case "Journal": Journals.Add((Journal)child); break;
                case "TimeZone": TimeZones.Add((DDay.iCal.Components.TimeZone)child); break;
                case "Todo": Todos.Add((Todo)child); break;
                default: break;
            }                
        }

        public override void OnLoad(EventArgs e)
        {
            Events.ResolveUIDs();
            Todos.ResolveUIDs();
            Journals.ResolveUIDs();

            base.OnLoad(e);            
        }

        #endregion

        #region Private Fields

        private UniqueComponentList<Event> m_Events;
        private List<FreeBusy> m_FreeBusy;
        private UniqueComponentList<Journal> m_Journal;
        private List<DDay.iCal.Components.TimeZone> m_TimeZone;
        private UniqueComponentList<Todo> m_Todo;

        #endregion

        #region Public Properties

        /// <summary>
        /// A collection of <see cref="Event"/> components in the iCalendar.
        /// </summary>
        public UniqueComponentList<Event> Events
        {
            get { return m_Events; }
            set { m_Events = value; }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Components.FreeBusy"/> components in the iCalendar.
        /// </summary>
        public List<FreeBusy> FreeBusy
        {
            get { return m_FreeBusy; }
            set { m_FreeBusy = value; }
        }
        
        /// <summary>
        /// A collection of <see cref="Journal"/> components in the iCalendar.
        /// </summary>
        public UniqueComponentList<Journal> Journals
        {
            get { return m_Journal; }
            set { m_Journal = value; }
        }

        /// <summary>
        /// A collection of <see cref="DDay.iCal.Components.TimeZone"/> components in the iCalendar.
        /// </summary>
        public List<DDay.iCal.Components.TimeZone> TimeZones
        {
            get { return m_TimeZone; }
            set { m_TimeZone = value; }
        }

        /// <summary>
        /// A collection of <see cref="Todo"/> components in the iCalendar.
        /// </summary>
        public UniqueComponentList<Todo> Todos
        {
            get { return m_Todo; }
            set { m_Todo = value; }
        }

        public Property Version
        {
            get
            {
                if (Properties.ContainsKey("VERSION"))
                    return (Property)Properties["VERSION"];
                return null;
            }
            set
            {                
                Properties["VERSION"] = value;
            }
        }

        public Property ProductID
        {
            get
            {
                if (Properties.ContainsKey("PRODID"))
                    return (Property)Properties["PRODID"];
                return null;
            }
            set
            {                
                Properties["PRODID"] = value;
            }            
        }

        public Property Scale
        {
            get
            {
                if (Properties.ContainsKey("CALSCALE"))
                    return (Property)Properties["CALSCALE"];
                return null;
            }
            set
            {                
                Properties["CALSCALE"] = value;
            }             
        }

        public Property Method
        {
            get
            {
                if (Properties.ContainsKey("METHOD"))
                    return (Property)Properties["METHOD"];
                return null;
            }
            set
            {
                Properties["METHOD"] = value;
            }
        }

        #endregion

        #region Static Public Methods
        
        /// <summary>
        /// Loads an <see cref="iCalendar"/> from the file system.
        /// </summary>
        /// <param name="Filepath">The path to the file to load.</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static public iCalendar LoadFromFile(string Filepath)
        {            
            FileStream fs = new FileStream(Filepath, FileMode.Open);

            iCalendar iCal = LoadFromStream(fs);
            fs.Close();
            return iCal;
        }

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from an open stream.
        /// </summary>
        /// <param name="s">The stream from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static public iCalendar LoadFromStream(Stream s)
        {
            iCalLexer lexer = new iCalLexer(s);
            iCalParser parser = new iCalParser(lexer);
            return parser.icalobject();
        }

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from a given Uri.
        /// </summary>
        /// <param name="url">The Uri from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>An <see cref="iCalendar"/> object</returns>
        static public iCalendar LoadFromUri(Uri uri)
        {
            return LoadFromUri(uri, null, null);            
        }

        /// <summary>
        /// Loads an <see cref="iCalendar"/> from a given Uri, using a 
        /// specified <paramref name="username"/> and <paramref name="password"/>
        /// for credentials.
        /// </summary>
        /// <param name="url">The Uri from which to load the <see cref="iCalendar"/> object</param>
        /// <returns>an <see cref="iCalendar"/> object</returns>
        static public iCalendar LoadFromUri(Uri uri, string username, string password)
        {
            try
            {
                WebClient client = new WebClient();
                if (username != null &&
                    password != null)
                    client.Credentials = new System.Net.NetworkCredential(username, password);

                byte[] bytes = client.DownloadData(uri);
                MemoryStream ms = new MemoryStream();
                ms.SetLength(bytes.Length);
                bytes.CopyTo(ms.GetBuffer(), 0);

                return LoadFromStream(ms);
            }
            catch (System.Net.WebException ex)
            {
                return null;
            }
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the <see cref="DDay.iCal.Components.TimeZone" /> object for the specified
        /// <see cref="TZID"/> (Time Zone Identifier).
        /// </summary>
        /// <param name="tzid">A valid <see cref="TZID"/> object, or a valid <see cref="TZID"/> string.</param>
        /// <returns>A <see cref="TimeZone"/> object for the <see cref="TZID"/>.</returns>
        public DDay.iCal.Components.TimeZone GetTimeZone(TZID tzid)
        {
            foreach (DDay.iCal.Components.TimeZone tz in TimeZones)
            {
                if (tz.TZID.Equals(tzid))
                {
                    return tz;
                }
            }
            return null;
        }

        /// <summary>
        /// Evaluates component recurrences for the given range of time.
        /// <example>
        ///     For example, if you are displaying a month-view for January 2007,
        ///     you would want to evaluate recurrences for Jan. 1, 2007 to Jan. 31, 2007
        ///     to display relevant information for those dates.
        /// </example>
        /// </summary>
        /// <param name="FromDate">The beginning date/time of the range to test.</param>
        /// <param name="ToDate">The end date/time of the range to test.</param>                
        public void Evaluate(Date_Time FromDate, Date_Time ToDate)
        {
            foreach (iCalObject obj in Children)
            {
                if (obj is RecurringComponent)
                    ((RecurringComponent)obj).Evaluate(FromDate, ToDate);
            }
        }

        //public ArrayList GetTodos(string category)
        //{
        //    ArrayList t = new ArrayList();
        //    foreach (Todo todo in Todos)
        //    {
        //        if (todo.Categories != null)
        //        {
        //            foreach (TextCollection cat in todo.Categories)
        //            {
        //                foreach (Text text in cat.Values)
        //                {
        //                    if (text.Value == category)
        //                        t.Add(todo);
        //                }
        //            }
        //        }
        //    }

        //    return t;
        //}

        /// <summary>
        /// Merges the current <see cref="iCalendar"/> with another iCalendar.
        /// <note>
        ///     Since each object is associated with one and only one iCalendar object,
        ///     the <paramref name="iCal"/> that is passed is automatically Disposed
        ///     in the process, because all of its objects are re-assocated with the new iCalendar.
        /// </note>
        /// </summary>
        /// <param name="iCal">The iCalendar to merge with the current <see cref="iCalendar"/></param>
        public void MergeWith(iCalendar iCal)
        {
            if (iCal != null)
            {
                foreach (iCalObject obj in iCal.Children)
                    this.AddChild(obj);
                iCal.Dispose();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Children.Clear();
            Events.Clear();
            FreeBusy.Clear();
            Journals.Clear();
            Todos.Clear();
        }

        #endregion
    }
}
