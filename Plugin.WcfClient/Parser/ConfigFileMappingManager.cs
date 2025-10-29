using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Plugin.WcfClient.Parser
{
	internal class ConfigFileMappingManager
	{//TODO: Is this really needed?
		private static String savedConfigFolder = Path.Combine(ToolingEnvironment.SavedDataBase, "CachedConfig");
		private static String mappingFilePath = Path.Combine(ConfigFileMappingManager.savedConfigFolder, "AddressToConfigMapping.xml");
		private static ConfigFileMappingManager configFileManager;
		private readonly IDictionary<String, String> addressToFileEntries = new Dictionary<String, String>();

		private ConfigFileMappingManager()
			=> this.ReadFromMappingFile();

		public static ConfigFileMappingManager Instance
			=> configFileManager ?? (configFileManager = new ConfigFileMappingManager());

		public void AddConfigFileMapping(String address)
		{
			if(!this.addressToFileEntries.ContainsKey(address))
			{
				String path = Guid.NewGuid().ToString() + ".config";
				String value = Path.Combine(ConfigFileMappingManager.savedConfigFolder, path);
				this.addressToFileEntries.Add(address, value);

				ExceptionUtility.InvokeFSAction(this.WriteToMappingFile);
			}
		}

		public void Clear()
		{
			this.addressToFileEntries.Clear();

			ExceptionUtility.InvokeFSAction(() => Directory.Delete(ConfigFileMappingManager.savedConfigFolder, true));
		}

		public void DeleteConfigFileMapping(String address)
		{
			if(this.addressToFileEntries.ContainsKey(address))
			{
				ExceptionUtility.InvokeFSAction(() =>
				{
					File.Delete(this.addressToFileEntries[address]);
					this.WriteToMappingFile();
				});
				this.addressToFileEntries.Remove(address);
			}
		}

		public Boolean DoesConfigMappingExist(String address)
			=> this.addressToFileEntries.ContainsKey(address) && File.Exists(this.addressToFileEntries[address]);
		
		public String GetSavedConfigPath(String address)
		{
			if(this.addressToFileEntries.ContainsKey(address))
				return this.addressToFileEntries[address];
			return null;
		}

		private void ReadFromMappingFile()
		{
			this.addressToFileEntries.Clear();
			if(!File.Exists(ConfigFileMappingManager.mappingFilePath))
				return;

			XmlDocument xmlDocument = new XmlDocument()
			{
				XmlResolver = null,
			};

			try
			{
				if(!ExceptionUtility.InvokeFSAction(() => xmlDocument.Load(ConfigFileMappingManager.mappingFilePath)))
					return;
			} catch(XmlException)
			{
				return;
			}

			foreach(XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes)
				if(xmlNode.ChildNodes.Count >= 2)
					this.addressToFileEntries.Add(xmlNode.ChildNodes[0].InnerText, xmlNode.ChildNodes[1].InnerText);
		}

		private void WriteToMappingFile()
		{
			XmlDocument xmlDocument = new XmlDocument()
			{
				XmlResolver = null,
			};

			xmlDocument.AppendChild(xmlDocument.CreateElement("Mapping"));
			foreach(KeyValuePair<String, String> current in this.addressToFileEntries)
			{
				XmlElement xmlElement = xmlDocument.CreateElement("Entry");
				XmlElement xmlElement2 = xmlDocument.CreateElement("Address");
				xmlElement2.InnerText = current.Key;
				xmlElement.AppendChild(xmlElement2);
				XmlElement xmlElement3 = xmlDocument.CreateElement("ConfigPath");
				xmlElement3.InnerText = current.Value;
				xmlElement.AppendChild(xmlElement3);
				xmlDocument.DocumentElement.AppendChild(xmlElement);
			}
			if(!Directory.Exists(ConfigFileMappingManager.savedConfigFolder))
				Directory.CreateDirectory(ConfigFileMappingManager.savedConfigFolder);

			xmlDocument.Save(ConfigFileMappingManager.mappingFilePath);
		}
	}
}