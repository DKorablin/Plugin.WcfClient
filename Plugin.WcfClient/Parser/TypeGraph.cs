using System;
using System.Collections.Generic;

namespace Plugin.WcfClient.Parser
{
	public class TypeGraph
	{
		private readonly IDictionary<String, IList<StringPair>> adjacencyList;

		private readonly IDictionary<String, IList<String>> knownTypesAdjacencyList;

		internal ICollection<IList<StringPair>> Links => this.adjacencyList.Values;

		internal ICollection<String> Types => this.adjacencyList.Keys;

		public TypeGraph(IDictionary<String, IList<StringPair>> adjacencyList, IDictionary<String, IList<String>> knownTypesAdjacencyList)
		{
			this.adjacencyList = adjacencyList;
			this.knownTypesAdjacencyList = knownTypesAdjacencyList;
		}

		internal IList<StringPair> GetLinksByType(String typeName)
			=> this.adjacencyList[typeName];

		internal void PopulateKnownTypeLinks(IDictionary<String, ServiceTypeWrapper> types)
		{
			foreach(String current in this.knownTypesAdjacencyList.Keys)
				foreach(String current2 in this.knownTypesAdjacencyList[current])
					types[current].AddToSubTypes(types[current2]);
		}

		internal IDictionary<String, IList<String>> Reverse()
		{
			IDictionary<String, IList<String>> dictionary = new Dictionary<String, IList<String>>();
			foreach(String current in this.adjacencyList.Keys)
				dictionary.Add(current, new List<String>());

			foreach(String current2 in this.adjacencyList.Keys)
				foreach(StringPair current3 in this.adjacencyList[current2])
					dictionary[current3.String2].Add(current2);
			return dictionary;
		}
	}
}