using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Utility
{
    public interface ISettingUtility
    {
        Setting GetByKey(string key);
        void Save(Setting setting);
        void SaveAccessToken(string accessToken);
        void SaveRefreshToken(string refreshToken);
        Setting GetAccessToken();
        Setting GetRefreshToken();
    }

    public class SettingUtility : ISettingUtility
    {
        private string _curremtPath;

        public SettingUtility()
        {
            _curremtPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "settings.xml");
        }
        public Setting GetAccessToken()
        {
            return GetByKey("accessToken");
        }

        public Setting GetRefreshToken()
        {
            return GetByKey("refreshToken");
        }

        public void SaveAccessToken(string accessToken)
        {
            var setting = new Setting()
            {
                Key = "accessToken",
                Value = accessToken
            };

            Save(setting);
        }

        public void SaveRefreshToken(string refreshToken)
        {
            var setting = new Setting()
            {
                Key = "refreshToken",
                Value = refreshToken
            };

            Save(setting);
        }

        public Setting GetByKey(string key)
        {
            return ReadFromXmls().FirstOrDefault(x => x.Key == key);
        }

        public void Save(Setting setting)
        {
            var allSettings = ReadFromXmls().ToList();

            var targetSetting = allSettings.FirstOrDefault(x => x.Key == setting.Key);

            if (targetSetting == null)
            {
                allSettings.Add(setting);
            }
            else
            {
                targetSetting.Value = setting.Value;
            }

            WriteToXml(allSettings);
        }

        private void WriteToXml(List<Setting> settings)
        {
            var s = ToXML(settings);
            File.WriteAllText(_curremtPath, s);
        }

        private IEnumerable<Setting> ReadFromXmls()
        {
            if (File.Exists(_curremtPath))
            {
                var s = File.ReadAllText(_curremtPath);
                return FromXML<List<Setting>>(s);
            }

            return new List<Setting>();
        }

        private T FromXML<T>(string xml)
        {
            using (StringReader stringReader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
        }

        private string ToXML<T>(T obj)
        {
            using (StringWriter stringWriter = new StringWriter(new StringBuilder()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }
    }
}
