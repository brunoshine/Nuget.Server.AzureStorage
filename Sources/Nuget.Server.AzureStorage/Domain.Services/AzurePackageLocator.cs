﻿//-----------------------------------------------------------------------
// <copyright file="PackageLocator.cs" company="A-IT">
//     Copyright (c) A-IT. All rights reserved.
// </copyright>
// <author>Szymon M Sasin</author>
//-----------------------------------------------------------------------

namespace Nuget.Server.AzureStorage.Domain.Services
{
    using NuGet;

    internal sealed class AzurePackageLocator : IPackageLocator
    {
        public string GetContainerName(IPackage package)
        {
            return this.GetAzureFriendlyString(package.Id);
        }

        public string GetItemName(IPackage package)
        {
            return this.GetAzureFriendlyString(package.Version.ToString());
        }

        private string GetAzureFriendlyString(string packageId)
        {
            return packageId.ToLower().Replace(".", "-");
        }
    }
}