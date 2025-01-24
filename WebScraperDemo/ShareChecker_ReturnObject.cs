using System.Runtime.Serialization;
using System.Text;

namespace Vishnu_UserModules
{
    /// <summary>
    /// ReturnObject mit aktuellen Werten für eine Aktie.
    /// Die Werte werden von einer Webseite ausgelesen.
    /// </summary>
    /// <remarks>
    /// Autor: Erik Nagel, NetEti
    ///
    /// 21.10.2021 Erik Nagel: erstellt
    /// </remarks>
    [DataContract] //[Serializable()]
    public class ShareChecker_ReturnObject
    {
        #region share-presets

        /// <summary>
        /// Enthält den Langnamen der Aktie.
        /// </summary>
        [DataMember]
        public string? FullName { get; set; }

        /// <summary>
        /// Enthält ein Kürzel für die Aktie.
        /// </summary>
        [DataMember]
        public string? ShortName { get; set; }

        /// <summary>
        /// Enthält die Web-Url der Kursinformationen für die Aktie.
        /// </summary>
        [DataMember]
        public string? Url { get; set; }

        /// <summary>
        /// Enthält die Anzahl Aktien zu Beginn der Anlage.
        /// </summary>
        [DataMember]
        public decimal? StartCount { get; set; }

        /// <summary>
        /// Enthält den Aktienkurs zu Beginn der Anlage.
        /// </summary>
        [DataMember]
        public decimal? StartValue { get; set; }

        /// <summary>
        /// Enthält die Anzahl Aktien zum aktuellen Zeitpunkt.
        /// </summary>
        [DataMember]
        public decimal? CurrentCount { get; set; }

        /// <summary>
        /// Enthält einen möglichen Differenzausgleich in Euro.
        /// </summary>
        [DataMember]
        public decimal? Compensation { get; set; }

        #endregion share-presets

        /// <summary>
        /// Zeitpunkt der letzten Auswertung.
        /// </summary>
        [DataMember]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Aktueller Wert der Aktie.
        /// </summary>
        [DataMember]
        public decimal? CurrentValue { get; set; }

        /// <summary>
        /// Vergleich des aktuellen Wertes mit dem Ausgangswert.
        /// </summary>
        [DataMember]
        public decimal? GainsLosses { get; set; }

        /// <summary>
        /// Standard Konstruktor.
        /// </summary>
        public ShareChecker_ReturnObject() { }

        /// <summary>
        /// Deserialisierungs-Konstruktor.
        /// </summary>
        /// <param name="info">Property-Container.</param>
        /// <param name="context">Übertragungs-Kontext.</param>
        protected ShareChecker_ReturnObject(SerializationInfo info, StreamingContext context)
        {
            this.FullName = info.GetString("FullName");
            this.ShortName = info.GetString("ShortName");
            this.Url = info.GetString("Url");
            this.StartCount = (decimal?)info.GetValue("StartCount", typeof(decimal));
            this.StartValue = (decimal?)info.GetValue("StartValue", typeof(decimal));
            this.CurrentCount = (decimal?)info.GetValue("CurrentCount", typeof(decimal));
            this.Compensation = (decimal?)info.GetValue("Compensation", typeof(decimal));

            this.Timestamp = (DateTime?)info.GetValue("Timestamp", typeof(DateTime));
            this.CurrentValue = (decimal?)info.GetValue("CurrentValue", typeof(decimal));
            this.GainsLosses = (decimal?)info.GetValue("GainsLosses", typeof(decimal));
        }

        /// <summary>
        /// Serialisierungs-Hilfsroutine: holt die Objekt-Properties in den Property-Container.
        /// </summary>
        /// <param name="info">Property-Container.</param>
        /// <param name="context">Serialisierungs-Kontext.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FullName", this.FullName);
            info.AddValue("ShortName", this.ShortName);
            info.AddValue("Url", this.Url);
            info.AddValue("StartCount", this.StartCount);
            info.AddValue("StartValue", this.StartValue);
            info.AddValue("CurrentCount", this.CurrentCount);
            info.AddValue("Compensation", this.Compensation);

            info.AddValue("Timestamp", this.Timestamp);
            info.AddValue("CurrentValue", this.CurrentValue);
            info.AddValue("GainsLosses", this.GainsLosses);
        }

        /// <summary>
        /// Überschriebene ToString()-Methode - stellt alle öffentlichen Properties
        /// als einen (mehrzeiligen) aufbereiteten String zur Verfügung.
        /// </summary>
        /// <returns>Alle öffentlichen Properties als ein String aufbereitet.</returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder(String.Format("{0:dd.MM.yyyy HH:mm} ", this.Timestamp));
            str.Append(this.ShortName + ":");
            str.Append(String.Format(" aktueller Kurs {0:#.00 EUR}", this.CurrentValue));
            str.Append(String.Format(", Gewinn/Verlust {0:#.00 EUR}", this.GainsLosses));
            return str.ToString();
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
    }
}
