using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Data.Items;
using Sitecore.Data;
using System;
using Sitecore.Data.Managers;

namespace Cheeto.Sc.ContentSearch.ComputedFields
{
    public abstract class AbstractTemplateFilterField : AbstractComputedIndexField
    {
        private readonly List<XmlNode> templateIncludes;
        private readonly List<XmlNode> templateExcludes;
        private List<Item> templateItemIncludes;
        private List<Item> templateItemExcludes;
        protected Item IndexedItem { get; private set; }
        protected IEnumerable<ID> IncludedTemplateIds
        {
            get
            {
                return templateItemIncludes?.Select(t => t.TemplateID);
            }
        }
        protected bool ShouldProcess
        {
            get
            {
                return  IncludedTemplateIds != null && 
                        IncludedTemplateIds.Contains(IndexedItem.TemplateID);
            }
        }

        protected abstract object ExecuteComputeFieldValue(IIndexable indexable);

        public AbstractTemplateFilterField()
                : this((XmlNode)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.ContentSearch.ComputedFields.MediaItemContentExtractor" /> class.
        /// </summary>
        /// <param name="configurationNode">The configuration node.</param>
        public AbstractTemplateFilterField(XmlNode configurationNode)
        {
            this.templateExcludes = new List<XmlNode>();
            this.templateItemExcludes = new List<Item>();
            this.templateIncludes = new List<XmlNode>();
            this.templateItemIncludes = new List<Item>();

            this.Initialize(configurationNode);
        }

        public override object ComputeFieldValue(IIndexable indexable)
        {
            IndexedItem = indexable.ToItem();

            if (ShouldProcess)
                return this.ExecuteComputeFieldValue(indexable);
        }

        protected virtual void Initialize(XmlNode configurationNode)
        {
            if (configurationNode == null)
                return;

            //MediaItemIFilterTextExtractor ifilterTextExtractor = new MediaItemIFilterTextExtractor();
            XmlNode xmlNode1 = (XmlNode)null;
            if (configurationNode.ChildNodes.Count > 0)
            {
                xmlNode1 = configurationNode.SelectSingleNode("templateFilters");
                if (xmlNode1 != null && xmlNode1.Attributes != null && xmlNode1.Attributes["ref"] != null)
                {
                    string xpath = xmlNode1.Attributes["ref"].Value;
                    if (string.IsNullOrEmpty(xpath))
                    {
                        Log.Error("<templateFilters> configuration error: \"ref\" attribute in templateFilters section cannot be empty.", (object)this);
                        return;
                    }
                    xmlNode1 = Factory.GetConfigNode(xpath);
                }
            }
            if (xmlNode1 == null)
            {
                Log.Error("Could not find <templateFilters> node in content search index configuration.", (object)this);
            }
            else
            {
                XmlNode xmlNode2 = xmlNode1.SelectSingleNode("includes");
                if (xmlNode2 == null)
                    Log.Error("<templateFilters> configuration error: \"includes\" node not found.", (object)this);
                else
                    this.templateIncludes.AddRange(Transform<XmlNode>((IEnumerable)xmlNode2.ChildNodes));
                XmlNode xmlNode3 = xmlNode1.SelectSingleNode("excludes");
                if (xmlNode3 == null)
                    Log.Error("<templateFilters> configuration error: \"excludes\" node not found.", (object)this);
                else
                    this.templateExcludes.AddRange(Transform<XmlNode>((IEnumerable)xmlNode3.ChildNodes));


                if (this.templateExcludes.Count == 1)
                {
                    if (this.templateIncludes.First<XmlNode>().InnerText == "*")
                    {
                        foreach (XmlNode templateExclude in this.templateExcludes)
                        {
                            this.AddTemplateExcludeFilter(templateExclude.InnerText);
                        }
                    }
                }
                else
                {
                    foreach (XmlNode templateInclude in this.templateIncludes)
                    {
                        this.AddTemplateIncludeFilter(templateInclude.InnerText);
                    }
                }
            }

        }

        public static IEnumerable<T> Transform<T>(IEnumerable enumerable)
        {
            foreach (object obj in enumerable)
            {
                if (obj is XmlElement)
                    yield return (T)obj;
            }
        }

        protected virtual void AddTemplateExcludeFilter(string templatePath)
        {
            Item item = this.Database.GetItem(templatePath);
            if (item != null)
                templateItemExcludes.Add(item);
        }

        protected virtual void AddTemplateIncludeFilter(string templatePath)
        {
            Item item = this.Database.GetItem(templatePath);
            if (item != null)
                templateItemIncludes.Add(item);
        }

        protected Database Database { get; private set; }

        protected bool TrySetDatabase(IIndexable indexable)
        {
            if (indexable == null)
            {
                Log.Warn(this + " : IIndexable is null", this);
                return false;
            }

            var sitecoreIndexableItem = indexable as SitecoreIndexableItem;
            if (sitecoreIndexableItem == null)
            {
                Log.Warn(this + " : unsupported IIndexable type : " + indexable.GetType(), this);
                return false;
            }

            var item = (Item)sitecoreIndexableItem;
            if (item == null)
            {
                Log.Warn(this + " : SitecoreIndexableItem's Item is null", this);
                return false;
            }

            // Skip items that are in the core database
            if (string.Compare(item.Database.Name, "core", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }

            this.Database = item.Database;

            return true;
        }

        protected Item GetCurrentItem(IIndexable indexable)
        {
            var language = LanguageManager.GetLanguage(indexable.Culture.TwoLetterISOLanguageName);
            return this.Database.GetItem(ID.Parse(indexable.Id), language);
        }

        protected Guid GetGuid(string id)
        {
            Guid guidId;
            Guid.TryParse(id, out guidId);
            return guidId;
        }

        protected Item GetItem(string path)
        {
            return this.Database.GetItem(path);
        }
        protected Item GetItem(ID id)
        {
            return this.Database.GetItem(id);
        }
    }
}
