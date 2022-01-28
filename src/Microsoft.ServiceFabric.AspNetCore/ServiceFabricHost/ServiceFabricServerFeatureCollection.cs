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
    using Microsoft.AspNetCore.Http.Features;

    internal class ServiceFabricServerFeatureCollection : IFeatureCollection
    {
        private IFeatureCollection innerFeatures;

        public bool IsReadOnly { get => this.innerFeatures.IsReadOnly; }

        public int Revision { get => this.innerFeatures.Revision; }

        public object this[Type key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                return this.innerFeatures[key];
            }

            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                this.innerFeatures[key] = value;
            }
        }

        public TFeature Get<TFeature>()
        {
            return this.innerFeatures.Get<TFeature>();
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return this.innerFeatures.GetEnumerator();
        }

        public void Set<TFeature>(TFeature instance)
        {
            this.innerFeatures.Set(instance);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal void SetFeatures(IFeatureCollection features)
        {
            this.innerFeatures = features;
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
