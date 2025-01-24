using System.Runtime.Serialization;
using System.Text;

namespace Vishnu_UserModules
{
    /// <summary>
    /// ReturnObject für das Ergebnis des Covid19-UserCheckers.
    /// </summary>
    /// <remarks>
    /// Autor: Erik Nagel, NetEti
    ///
    /// 27.11.2020 Erik Nagel: erstellt
    /// </remarks>
    [DataContract] //[Serializable()]
    public class WebScraperDemoChecker_ReturnObject
    {
        /// <summary>
        /// Wrapper-Klasse um List&lt;SubResult&gt; SubResults.
        /// </summary>
        [DataContract] //[Serializable()]
        public class SubResultListContainer : ISerializable
        {
            /// <summary>
            /// 0 bis n Datensätze bestehend aus einem Detail-Ergebnis (bool?) und Detail-Record (hier: string).
            /// </summary>
            [DataMember]
            public List<SubResult>? SubResults { get; set; }

            /// <summary>
            /// Standard Konstruktor.
            /// </summary>
            public SubResultListContainer()
            {
                this.SubResults = new List<SubResult>();
            }

            /// <summary>
            /// Deserialisierungs-Konstruktor.
            /// </summary>
            /// <param name="info">Property-Container.</param>
            /// <param name="context">Übertragungs-Kontext.</param>
            protected SubResultListContainer(SerializationInfo info, StreamingContext context)
            {
                this.SubResults = (List<SubResult>?)info.GetValue("SubResults", typeof(List<SubResult>));
            }

            /// <summary>
            /// Serialisierungs-Hilfsroutine: holt die Objekt-Properties in den Property-Container.
            /// </summary>
            /// <param name="info">Property-Container.</param>
            /// <param name="context">Serialisierungs-Kontext.</param>
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("SubResults", this.SubResults);
            }

            /// <summary>
            /// Überschriebene ToString()-Methode.
            /// </summary>
            /// <returns>Id des Knoten + ":" + ReturnObject.ToString()</returns>
            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                string delimiter = "";
                if (this.SubResults != null)
                {
                    foreach (SubResult subResult in this.SubResults)
                    {
                        stringBuilder.Append(delimiter + subResult.ToString());
                        delimiter = Environment.NewLine;
                    }
                }
                return stringBuilder.ToString();
            }

            /// <summary>
            /// Vergleicht Dieses Result mit einem übergebenen Result nach Inhalt.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns>True, wenn das übergebene Result inhaltlich (ohne Timestamp) gleich diesem Result ist.</returns>
            public override bool Equals(object? obj)
            {
                if (obj == null || this.GetType() != obj.GetType())
                {
                    return false;
                }
                if (Object.ReferenceEquals(this, obj))
                {
                    return true;
                }
                SubResultListContainer subResultList = (SubResultListContainer)obj;
                if (this.SubResults?.Count != subResultList.SubResults?.Count)
                {
                    return false;
                }
                for (int i = 0; i < this.SubResults?.Count; i++)
                {
                    if (this.SubResults[i] != subResultList.SubResults?[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Erzeugt einen eindeutigen Hashcode für dieses Result.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return (this.ToString()).GetHashCode();
            }
        }

        /// <summary>
        /// Klasse für ein Teilergebnis.
        /// </summary>
        [DataContract] //[Serializable()]
        public class SubResult : ISerializable
        {
            /// <summary>
            /// Das logische Einzelergebnis eines Unterergebnisses.
            /// true, false oder null.
            /// </summary>
            [DataMember]
            public bool? LogicalResult { get; set; }

            /// <summary>
            /// Der Wert einer Detail-Information der Prüfroutine
            /// (i.d.R int).
            ///  </summary>
            [DataMember]
            public string? ResultRecord { get; set; }

            /// <summary>
            /// Standard Konstruktor.
            /// </summary>
            public SubResult() { }

            /// <summary>
            /// Deserialisierungs-Konstruktor.
            /// </summary>
            /// <param name="info">Property-Container.</param>
            /// <param name="context">Übertragungs-Kontext.</param>
            protected SubResult(SerializationInfo info, StreamingContext context)
            {
                this.LogicalResult = (bool?)info.GetValue("LogicalResult", typeof(bool?));
                this.ResultRecord = info.GetString("ResultRecord");
            }

            /// <summary>
            /// Serialisierungs-Hilfsroutine: holt die Objekt-Properties in den Property-Container.
            /// </summary>
            /// <param name="info">Property-Container.</param>
            /// <param name="context">Serialisierungs-Kontext.</param>
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("LogicalResult", this.LogicalResult);
                info.AddValue("ResultRecord", this.ResultRecord);
            }

            /// <summary>
            /// Überschriebene ToString()-Methode.
            /// </summary>
            /// <returns>Id des Knoten + ":" + ReturnObject.ToString()</returns>
            public override string ToString()
            {
                //string resultStr = this.LogicalResult == null ? "null" : this.LogicalResult.ToString();
                //return String.Format("{0}: {1}", resultStr, this.ResultRecord);
                return String.Format("{0}", this.ResultRecord);
            }

            /// <summary>
            /// Vergleicht dieses Result mit einem übergebenen Result nach Inhalt.
            /// Der Timestamp wird bewusst nicht in den Vergleich einbezogen.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns>True, wenn das übergebene Result inhaltlich (ohne Timestamp) gleich diesem Result ist.</returns>
            public override bool Equals(object? obj)
            {
                if (obj == null || this.GetType() != obj.GetType())
                {
                    return false;
                }
                if (Object.ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (this.ToString() != obj.ToString())
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Erzeugt einen eindeutigen Hashcode für dieses Result.
            /// Der Timestamp wird bewusst nicht in den Vergleich einbezogen.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return (this.ToString()).GetHashCode();
            }
        }

        /// <summary>
        /// Wrapper-Klasse um List&lt;SubResult&gt; SubResults.
        /// </summary>
        [DataMember]
        public SubResultListContainer? SubResults { get; set; }

        /// <summary>
        /// Das logische Gesamtergebnis eines Prüfprozesses:
        /// true, false oder null.
        /// </summary>
        [DataMember]
        public bool? LogicalResult { get; set; }

        /// <summary>
        /// Die Anzahl der Treffer, die das Prüfkriterium erfüllen.
        /// </summary>
        [DataMember]
        public long? RecordCount { get; set; }

        /// <summary>
        /// Name der Datei mit den letzten Covid19-Werten.
        /// </summary>
        [DataMember]
        public string? Covid19InfoFile { get; set; }

        /// <summary>
        /// Klartext-Informationen zur Prüfroutine
        /// (was die Routine prüft).
        ///  </summary>
        [DataMember]
        public string? Comment { get; set; }

        /// <summary>
        /// Standard Konstruktor.
        /// </summary>
        public WebScraperDemoChecker_ReturnObject()
        {
            this.SubResults = new SubResultListContainer();
            this.LogicalResult = null;
        }

        /// <summary>
        /// Deserialisierungs-Konstruktor.
        /// </summary>
        /// <param name="info">Property-Container.</param>
        /// <param name="context">Übertragungs-Kontext.</param>
        protected WebScraperDemoChecker_ReturnObject(SerializationInfo info, StreamingContext context)
        {
            this.SubResults = (SubResultListContainer?)info.GetValue("SubResults", typeof(SubResultListContainer));
            this.LogicalResult = (bool?)info.GetValue("LogicalResult", typeof(bool?));
            this.RecordCount = (long?)info.GetValue("RecordCount", typeof(long));
            this.Covid19InfoFile = info.GetString("Covid19InfoFile");
            this.Comment = info.GetString("Comment");
        }

        /// <summary>
        /// Serialisierungs-Hilfsroutine: holt die Objekt-Properties in den Property-Container.
        /// </summary>
        /// <param name="info">Property-Container.</param>
        /// <param name="context">Serialisierungs-Kontext.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SubResults", this.SubResults);
            info.AddValue("LogicalResult", this.LogicalResult);
            info.AddValue("RecordCount", this.RecordCount);
            info.AddValue("Covid19InfoFile", this.Covid19InfoFile);
            info.AddValue("Comment", this.Comment);
        }

        /// <summary>
        /// Überschriebene ToString()-Methode - stellt alle öffentlichen Properties
        /// als einen (zweizeiligen) aufbereiteten String zur Verfügung.
        /// </summary>
        /// <returns>Alle öffentlichen Properties als ein String aufbereitet.</returns>
        public override string ToString()
        {
            string? logicalResultStr = this.LogicalResult.ToString();
            StringBuilder str = new StringBuilder(String.Format("{0} ({1})", logicalResultStr == "" ? "null" : logicalResultStr, this.Comment));
            str.Append(String.Format("\nCovid19InfoFile {0}", this.Covid19InfoFile));
            str.Append(String.Format("\nRecordCount {0}", this.RecordCount));
            str.Append("\nRecords:");
            if (this.SubResults?.SubResults != null)
            {
                foreach (SubResult subResult in this.SubResults.SubResults)
                {
                    str.Append(String.Format("\n    {0}", subResult.ToString()));
                }
            }
            return str.ToString();
        }

        /// <summary>
        /// Vergleicht dieses Result mit einem übergebenen Result nach Inhalt.
        /// Der Timestamp wird bewusst nicht in den Vergleich einbezogen.
        /// </summary>
        /// <param name="obj">Das zu vergleichende Result.</param>
        /// <returns>True, wenn das übergebene Result inhaltlich (ohne Timestamp) gleich diesem Result ist.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (this.ToString() != obj.ToString())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Erzeugt einen eindeutigen Hashcode für dieses Result.
        /// Der Timestamp wird bewusst nicht in den Vergleich einbezogen.
        /// </summary>
        /// <returns>Hashcode (int).</returns>
        public override int GetHashCode()
        {
            return (this.ToString()).GetHashCode();
        }
    }
}
