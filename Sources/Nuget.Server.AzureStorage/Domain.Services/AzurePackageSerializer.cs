//-----------------------------------------------------------------------
// <copyright file="AzurePackageSerializer.cs" company="A-IT">
//     Copyright (c) A-IT. All rights reserved.
// </copyright>
// <author>Szymon M Sasin</author>
//-----------------------------------------------------------------------

namespace Nuget.Server.AzureStorage.Domain.Services
{
    using AutoMapper;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Nuget.Server.AzureStorage.Doman.Entities;
    using NuGet;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class AzurePackageSerializer : IAzurePackageSerializer
    {
        public AzurePackage ReadFromMetadata(CloudBlockBlob blob)
        {
            if(blob == null)
            {
                return null;
            }

            blob.FetchAttributes();
            var package = new AzurePackage();

            package.Id = blob.Metadata["Id"];
            package.Version = new SemanticVersion(blob.Metadata["Version"]);
            if (blob.Metadata.ContainsKey("Title"))
            {
                package.Title = blob.Metadata["Title"];
            }
            if (blob.Metadata.ContainsKey("Authors"))
            {
                package.Authors = blob.Metadata["Authors"].Split(',');
            }
            if (blob.Metadata.ContainsKey("Owners"))
            {
                package.Owners = blob.Metadata["Owners"].Split(',');
            }
            if (blob.Metadata.ContainsKey("IconUrl"))
            {
                package.IconUrl = new Uri(blob.Metadata["IconUrl"]);
            }
            if (blob.Metadata.ContainsKey("LicenseUrl"))
            {
                package.LicenseUrl = new Uri(blob.Metadata["LicenseUrl"]);
            }
            if (blob.Metadata.ContainsKey("ProjectUrl"))
            {
                package.ProjectUrl = new Uri(blob.Metadata["ProjectUrl"]);
            }
            package.RequireLicenseAcceptance = blob.Metadata["RequireLicenseAcceptance"].ToBool();
            package.DevelopmentDependency = blob.Metadata["DevelopmentDependency"].ToBool();
            if (blob.Metadata.ContainsKey("Description"))
            {
                package.Description = blob.Metadata["Description"];
            }
            if (blob.Metadata.ContainsKey("Summary"))
            {
                package.Summary = blob.Metadata["Summary"];
            }
            if (blob.Metadata.ContainsKey("ReleaseNotes"))
            {
                package.ReleaseNotes = blob.Metadata["ReleaseNotes"];
            }
            if (blob.Metadata.ContainsKey("Tags"))
            {
                package.Tags = blob.Metadata["Tags"];
            }
            var dependencySetContent = blob.Metadata["Dependencies"];
            dependencySetContent = this.Base64Decode(dependencySetContent);
            package.DependencySets = dependencySetContent
                .FromJson<IEnumerable<AzurePackageDependencySet>>()
                .Select(x => new PackageDependencySet(x.TargetFramework, x.Dependencies));
            package.IsAbsoluteLatestVersion = blob.Metadata["IsAbsoluteLatestVersion"].ToBool();
            package.IsLatestVersion = blob.Metadata["IsLatestVersion"].ToBool();
            if (blob.Metadata.ContainsKey("MinClientVersion"))
            {
                package.MinClientVersion = new Version(blob.Metadata["MinClientVersion"]);
            }
            package.Listed = blob.Metadata["Listed"].ToBool();
            package.Published = DateTimeOffset.Parse(blob.Metadata["Published"]);

            return package;
        }

        public void SaveToMetadata(AzurePackage package, CloudBlockBlob blob)
        {
            SetMetadataValue(blob, "Id", package.Id);
            SetMetadataValue(blob,"Version" ,package.Version);
            SetMetadataValue(blob, "Title", package.Title);
            SetMetadataValue(blob,"Authors",string.Join(",", package.Authors));
            SetMetadataValue(blob,"Owners" ,string.Join(",", package.Owners));
            if (package.IconUrl != null)
                SetMetadataValue(blob,"IconUrl",package.IconUrl.AbsoluteUri);
            if (package.LicenseUrl != null)
                SetMetadataValue(blob,"LicenseUrl",package.LicenseUrl.AbsoluteUri);
            if (package.ProjectUrl != null)
                SetMetadataValue(blob,"ProjectUrl",package.ProjectUrl.AbsoluteUri);
            SetMetadataValue(blob,"RequireLicenseAcceptance",package.RequireLicenseAcceptance);
            SetMetadataValue(blob,"DevelopmentDependency",package.DevelopmentDependency);
            SetMetadataValue(blob,"Description",package.Description);
            SetMetadataValue(blob,"Summary",package.Summary);
            SetMetadataValue(blob,"ReleaseNotes",package.ReleaseNotes);
            SetMetadataValue(blob,"Tags",package.Tags);
            SetMetadataValue(blob,"Dependencies",this.Base64Encode(package.DependencySets.Select(Mapper.Map<AzurePackageDependencySet>).ToJson()));
            SetMetadataValue(blob,"IsAbsoluteLatestVersion",package.IsAbsoluteLatestVersion);
            SetMetadataValue(blob,"IsLatestVersion",package.IsLatestVersion);
            SetMetadataValue(blob,"MinClientVersion",package.MinClientVersion);
            SetMetadataValue(blob, "Listed", package.Listed);
            SetMetadataValue(blob, "Published", DateTime.UtcNow);
            try
            {
                blob.SetMetadata();

            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        private void SetMetadataValue(CloudBlockBlob blob, string key, object value)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                blob.Metadata[key] = value.ToString();
            }
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}