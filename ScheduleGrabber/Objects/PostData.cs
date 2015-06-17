using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleGrabber
{
    /// <summary>
    /// This is the object containing the data required to create
    /// a post request against the UiA-Schedule website.
    /// </summary>
    public class PostData
    {
        public string __EVENTTARGET { get; set; }
        public string __EVENTARGUMENT { get; set; }
        public string __LASTFOCUS { get; set; }
        public string __VIEWSTATE { get; set; }
        public string __VIEWSTATEGENERATOR { get; set; }
        public string __EVENTVALIDATION { get; set; }
        public string tLinkType { get; set; }
        public string tWildcard { get; set; }
        public string lbWeeks { get; set; }
        public string lbDays { get; set; }
        public string RadioType { get; set; }
        public string bGetTimetable { get; set; }

        public PostData()
        {

        }

        /// <summary>
        /// Encode the object so that is can be passed in a request
        /// </summary>
        /// <param name="department">
        ///     the department as the dlObject
        ///     The dlObject is the department ID from the
        ///     UiA-Schedule website selectbox option ID.
        /// </param>
        /// <returns>FormUrlEncodedContent object used by HttpClient requests</returns>
        public FormUrlEncodedContent UrlEncode(Department department)
        {
            var requestData = new Dictionary<string, string>();
            requestData.Add("dlObject", department.Id);
            foreach (var prop in typeof(PostData).GetProperties())
            {
                requestData.Add(prop.Name, prop.GetValue(this) as string);
            }
            return new FormUrlEncodedContent(requestData);
        }
    }
}
