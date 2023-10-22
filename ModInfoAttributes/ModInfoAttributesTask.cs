using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Build.Framework; // ITaskItem
using Microsoft.Build.Utilities; // Task

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Vintagestory.API.Common;

namespace ModInfoTask
{
	internal class ModInfoContractResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);

			if (property.DeclaringType == typeof(ModInfo) && property.PropertyName == "TextureSize")
			{
				property.ShouldSerialize = _ => false;
			}

			return property;
		}
	}

	public class ModInfoAttributes: Task
	{
		[Required]
		public string Name                  { get; set; }
		public string Type                  { get; set; }
		public string IconPath              { get; set; }
		public string ModID                 { get; set; }
		public string Version               { get; set; }
		public string NetworkVersion        { get; set; }
		public string Description           { get; set; }
		public string Website               { get; set; }
		public string Authors               { get; set; }
		public string Contributors          { get; set; }
		public string Side                  { get; set; }
		public string RequiredOnClient      { get; set; }
		public string RequiredOnServer      { get; set; }
		public string WorldConfig           { get; set; }
		public ITaskItem[] ModDependency    { get; set; }

		public string OutputPath { get; set; }

		[Output]
		public ITaskItem[] AssemblyAttribute { get; set; }

        private bool SerializeJson()
        {
			ModInfo modInfo = new ModInfo(
                Enum.TryParse(Type, out EnumModType ModType) ? ModType : EnumModType.Code,
				Name,
				ModID,
				Version,
				Description,
				ItemGroupToEnumerable(Authors),
				Contributors != null ? ItemGroupToEnumerable(Contributors) : null,
				Website,
                Enum.TryParse(Side, out EnumAppSide AppSide) ? AppSide : EnumAppSide.Universal,
				RequiredOnClient != null ? Convert.ToBoolean(RequiredOnClient) : true,
				RequiredOnServer != null ? Convert.ToBoolean(RequiredOnServer) : true,
				ModDependency != null ? ModDependency.Select(s => new ModDependency(s.ItemSpec, s.GetMetadata("Version"))) : null
				);

			JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings()
			{
				Formatting = Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new ModInfoContractResolver()
			});

			Directory.CreateDirectory(OutputPath);
			using (StreamWriter writer = new StreamWriter(Path.Combine(OutputPath, "modinfo.json")))
			{
				using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
				{
					serializer.Serialize(jsonWriter, modInfo);
				}
			}

			return !Log.HasLoggedErrors;
        }

        private bool CreateAttributes()
        {
			TaskItem modInfoAttribute = new TaskItem("Vintagestory.API.Common.ModInfoAttribute");
			modInfoAttribute.SetMetadata("_Parameter1", Name);
			modInfoAttribute.SetMetadata("_Parameter2", ModID);

			modInfoAttribute.SetMetadata("Authors", ItemGroupToCSharpArray(Authors));
			modInfoAttribute.SetMetadata("Authors_IsLiteral", "true");

			modInfoAttribute.SetMetadata("Description", Description);
			modInfoAttribute.SetMetadata("Version", Version);
			modInfoAttribute.SetMetadata("IconPath", IconPath);

			if (NetworkVersion != null)
              modInfoAttribute.SetMetadata("NetworkVersion", NetworkVersion);

			if (Website != null)
              modInfoAttribute.SetMetadata("Website", Website);

			if (Contributors != null)
			{
				modInfoAttribute.SetMetadata("Contributors", ItemGroupToCSharpArray(Contributors));
				modInfoAttribute.SetMetadata("Contributors_IsLiteral", "true");
			}

			if (Side != null) modInfoAttribute.SetMetadata("Side", Side);
			if (RequiredOnClient != null)
			{
				modInfoAttribute.SetMetadata("RequiredOnClient", RequiredOnClient);
				modInfoAttribute.SetMetadata("RequiredOnClient_IsLiteral", "true");
			}
			if (RequiredOnServer != null)
			{
				modInfoAttribute.SetMetadata("RequiredOnServer", RequiredOnServer);
				modInfoAttribute.SetMetadata("RequiredOnServer_IsLiteral", "true");
			}

			if (WorldConfig != null)
              modInfoAttribute.SetMetadata("WorldConfig", WorldConfig);

			AssemblyAttribute = new ITaskItem[] { modInfoAttribute };

			return !Log.HasLoggedErrors;
        }

		public override bool Execute()
		{
			if (ModID != null)
			{
				if (!ModInfo.IsValidModID(ModID))
				{
					Log.LogError("Invalid value for ModID ({0}).", ModID);
					return false;
				}
			}
			else
			{
				ModID = ModInfo.ToModID(Name);
			}

			if (Side != null)
			{
				if (!Enum.IsDefined(typeof(EnumAppSide), Side))
				{
					Log.LogError("Invalid value for Side ({0}). Possible values: {1}", Side, string.Join(", ", Enum.GetNames(typeof(EnumAppSide))));
					return false;
				}
			}
			else
			{
				Side = "Universal";
			}

            if (!SerializeJson())
              return false;

            return CreateAttributes();
		}

		private string ItemGroupToCSharpArray(string itemGroup)
		{
			return string.Format("new[] {{{0}}}", string.Join(", ", itemGroup.Split(';').Select(a => "\"" + a + "\"")));
		}

		private IEnumerable<string> ItemGroupToEnumerable(string itemGroup)
		{
			return itemGroup.Split(';').Select(a => a);
		}
	}
}
