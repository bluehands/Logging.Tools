using System;
using System.Deployment.Application;
using System.IO;
using System.Xml.Serialization;

namespace Bluehands.Repository.Diagnostics
{
    public class StatePersister<TState> where TState : class, new()
    {
        protected string m_StateFilePath;
        protected bool IsInitialized { get; set; }

        public StatePersister(string stateFilename)
        {
            try
            {
                string directory = null;
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    directory = ApplicationDeployment.CurrentDeployment.DataDirectory;
                }
                if (directory == null)
                {
                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BluehandsLogViewer");
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                m_StateFilePath = Path.Combine(directory, stateFilename);
            }
            catch (Exception)
            {
                m_StateFilePath = null;
            }
        }

        public TState Load()
        {
            return Load(m_StateFilePath);
        }

        static TState Load(string filePath)
        {
            var result = new TState();

            if (string.IsNullOrEmpty(filePath))
            {
                return result;
            }

            try
            {
                if (File.Exists(filePath))
                {
                    var xmlSerializer = new XmlSerializer(typeof(TState));

                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        result = (TState)xmlSerializer.Deserialize(stream);
                    }
                }
            }
            catch (Exception)
            {
                result = new TState();
            }
            return result;
        }

        public void Save(TState state)
        {
            if (string.IsNullOrEmpty(m_StateFilePath))
            {
                return;
            }

            try
            {
                var xmlSerializer = new XmlSerializer(typeof(TState));

                using (var stream = new FileStream(m_StateFilePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    xmlSerializer.Serialize(stream, state);
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
