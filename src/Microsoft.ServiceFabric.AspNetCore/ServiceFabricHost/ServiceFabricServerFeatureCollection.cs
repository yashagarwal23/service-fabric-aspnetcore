// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;

    internal class ServiceFabricServerFeatureCollection : IFeatureCollection
    {
        private IFeatureCollection internalFeatures;
        private ServiceFabricServerAddressFeature serverAddressFeature;

        public ServiceFabricServerFeatureCollection()
        {
            this.serverAddressFeature = new ServiceFabricServerAddressFeature();
        }

        public bool IsReadOnly { get => this.internalFeatures.IsReadOnly; }

        public int Revision { get => this.internalFeatures.Revision; }

        public object this[Type key]
        {
            get
            {
                return this.internalFeatures[key];
            }

            set
            {
                this.internalFeatures[key] = value;
            }
        }

        public TFeature Get<TFeature>()
        {
            var feature = this[typeof(TFeature)];
            return feature != null ? (TFeature)feature : default(TFeature);
        }

        public void Set<TFeature>(TFeature instance)
        {
            this[typeof(TFeature)] = instance;
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return this.internalFeatures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal void ConfigureInternalFeatures(IFeatureCollection newFeatures)
        {
            if (this.internalFeatures != null)
            {
                foreach (var feature in this.internalFeatures)
                {
                    if (newFeatures[feature.Key] == null)
                    {
                        newFeatures[feature.Key] = feature.Value;
                    }
                }
            }

            this.serverAddressFeature.ConfigureInternalAddressFeatures(newFeatures.Get<IServerAddressesFeature>());
            newFeatures.Set<IServerAddressesFeature>(this.serverAddressFeature);

            this.internalFeatures = newFeatures;
        }
    }
}
