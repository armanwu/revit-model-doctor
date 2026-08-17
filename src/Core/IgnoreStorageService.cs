using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace ModelDoctor.Core
{
    /// <summary>
    /// Service managing persistent ignored element IDs in Revit Document via ExtensibleStorage.
    /// </summary>
    public static class IgnoreStorageService
    {
        public static readonly Guid SchemaGuid = new Guid("9B1A2C3D-4E5F-6789-ABCD-EF0123456789");
        private const string FieldName = "IgnoredElementIds";

        private static Schema? GetOrCreateSchema()
        {
            Schema schema = Schema.Lookup(SchemaGuid);
            if (schema != null) return schema;

            SchemaBuilder builder = new SchemaBuilder(SchemaGuid);
            builder.SetReadAccessLevel(AccessLevel.Public);
            builder.SetWriteAccessLevel(AccessLevel.Public);
            builder.SetVendorId("ModelDoctor");
            builder.SetSchemaName("ModelDoctorIgnoredElements");

            builder.AddArrayField(FieldName, typeof(long));
            return builder.Finish();
        }

        /// <summary>
        /// Retrieves the set of ignored ElementId values stored in the document.
        /// </summary>
        public static HashSet<long> GetIgnoredElementIds(Document doc)
        {
            var result = new HashSet<long>();
            if (doc == null) return result;

            Element projInfo = doc.ProjectInformation;
            if (projInfo == null) return result;

            Schema? schema = GetOrCreateSchema();
            if (schema == null) return result;

            Entity entity = projInfo.GetEntity(schema);
            if (!entity.IsValid()) return result;

            IList<long> ids = entity.Get<IList<long>>(FieldName);
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    result.Add(id);
                }
            }

            return result;
        }

        /// <summary>
        /// Marks an ElementId as ignored in ExtensibleStorage (Must be called within an active Transaction).
        /// </summary>
        public static void IgnoreElement(Document doc, ElementId elementId)
        {
            if (doc == null || elementId == null || elementId == ElementId.InvalidElementId) return;

            Element projInfo = doc.ProjectInformation;
            if (projInfo == null) return;

            Schema? schema = GetOrCreateSchema();
            if (schema == null) return;

            Entity entity = projInfo.GetEntity(schema);
            if (!entity.IsValid())
            {
                entity = new Entity(schema);
            }

            IList<long> ids = entity.Get<IList<long>>(FieldName) ?? new List<long>();
            long val = elementId.Value;
            if (!ids.Contains(val))
            {
                ids.Add(val);
                entity.Set(FieldName, ids);
                projInfo.SetEntity(entity);
            }
        }

        /// <summary>
        /// Removes an ElementId from the ignored set in ExtensibleStorage (Must be called within an active Transaction).
        /// </summary>
        public static void UnignoreElement(Document doc, ElementId elementId)
        {
            if (doc == null || elementId == null || elementId == ElementId.InvalidElementId) return;

            Element projInfo = doc.ProjectInformation;
            if (projInfo == null) return;

            Schema? schema = GetOrCreateSchema();
            if (schema == null) return;

            Entity entity = projInfo.GetEntity(schema);
            if (!entity.IsValid()) return;

            IList<long> ids = entity.Get<IList<long>>(FieldName);
            if (ids != null && ids.Contains(elementId.Value))
            {
                ids.Remove(elementId.Value);
                entity.Set(FieldName, ids);
                projInfo.SetEntity(entity);
            }
        }
    }
}
