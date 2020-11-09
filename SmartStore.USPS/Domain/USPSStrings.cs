//------------------------------------------------------------------------------
// Contributor(s): RJH 08/07/2009. 
//------------------------------------------------------------------------------

namespace SmartStore.USPS.Domain
{
    /// <summary>
    /// Class for USPS V3 XML rate class 
    /// </summary>
    public class USPSStrings
    {
        /// <summary>
        /// String array field instance.
        /// </summary>
        /// V3 USPS Service must be Express, Express SH, Express Commercial, Express SH Commercial, First Class, Priority, Priority Commercial, Parcel, Library, BPM, Media, ALL or ONLINE;
        /// Comment out the services not needed. 
        string[] _elements = {  // "Express", 
                                    // "Express SH",
                                    // "Express Commercial",
                                    // "Express SH Commercial",
                                    // "First Class",                       /* 13 oz limit */
                                    // "Priority",
                                    // "Priority Commercial",
                                    // "Parcel", 
                                    // "Library",
                                    // "BPM",
                                    // "Media",
                                    "ALL",  //USPSStrings Elements should only have "ALL" uncommented as rates are filtered on response, not request
                                    // "ONLINE" 
                                 };

        /// <summary>
        /// String array property getter.
        /// </summary>
        public string[] Elements => _elements;

        /// <summary>
        /// String array indexer.
        /// </summary>
        public string this[int index] => _elements[index];
    }
}
