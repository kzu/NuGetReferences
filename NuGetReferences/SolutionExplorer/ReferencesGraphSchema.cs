namespace ClariusLabs.NuGetReferences
{
    using Microsoft.VisualStudio.GraphModel;

    public class ReferencesGraphSchema
    {
        public static readonly GraphSchema Schema = new GraphSchema(Id.For("Schema"));
        public static readonly GraphCategory PackageCategory = Schema.Categories.AddNewCategory(Id.For("Schema_PackageCategory"));
        public static readonly GraphProperty PackageProperty = Schema.Properties.AddNewProperty(Id.For("Schema_Package"), typeof(global::NuGet.VisualStudio.IVsPackageMetadata));
    }
}