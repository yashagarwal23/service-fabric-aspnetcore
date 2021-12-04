// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    internal static class ProxyService
    {
        private static readonly AssemblyBuilder assemblyBuilder;
        private static readonly ModuleBuilder moduleBuilder;

        static ProxyService()
        {
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName("Microsoft.ServiceFabric.Services.Communication.AspNetCore"),
                    AssemblyBuilderAccess.Run);

            moduleBuilder = assemblyBuilder.DefineDynamicModule("Root");
        }

        internal static Type CreateProxyStatelessService(Type serviceClass, IServiceProvider serviceProvider)
        {
            // Create Type Builder
            var typeBuilder = moduleBuilder.DefineType(
                    $"{serviceClass.Name}_{Guid.NewGuid():N}_proxy",
                    TypeAttributes.Class | TypeAttributes.Public,
                    serviceClass);

            // create StatelessServiceContext private field builder
            FieldBuilder serviceContextFieldBuilder = typeBuilder.DefineField("serviceContext", typeof(IServiceProvider), FieldAttributes.Private);

            // create IServiceProvider private field builder
            FieldBuilder serviceProviderFieldBuilder = typeBuilder.DefineField("serviceProvider", typeof(IServiceProvider), FieldAttributes.Private);

            // create constructor (StatelessServiceContext, IServiceProvider)
            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { typeof(StatelessServiceContext), typeof(IServiceProvider) });

            var ctoril = ctor.GetILGenerator();

            var baseCtor = serviceClass.GetConstructors()[0];

            // all serviceClass ctor();
            ctoril.Emit(OpCodes.Ldarg_0);
            ctoril.Emit(OpCodes.Ldarg_1);
            ctoril.Emit(OpCodes.Ldarg_2);
            ctoril.Emit(OpCodes.Call, baseCtor);
            ctoril.Emit(OpCodes.Nop);
            ctoril.Emit(OpCodes.Nop);

            // this.serviceContext = serviceContext
            ctoril.Emit(OpCodes.Ldarg_0);
            ctoril.Emit(OpCodes.Ldarg_1);
            ctoril.Emit(OpCodes.Stfld, serviceContextFieldBuilder);

            // this.serviceProvider = serviceProvider
            ctoril.Emit(OpCodes.Ldarg_0);
            ctoril.Emit(OpCodes.Ldarg_2);
            ctoril.Emit(OpCodes.Stfld, serviceProviderFieldBuilder);

            // ServiceRegistrant.Add(serviceContext, serviceProvider);
            ctoril.Emit(OpCodes.Ldarg_1);
            ctoril.Emit(OpCodes.Ldarg_2);
            ctoril.Emit(OpCodes.Call, typeof(ServiceRegistrant).GetMethod("Add"));

            // return
            ctoril.Emit(OpCodes.Ret);

            var listenerFactory = typeBuilder.DefineMethod(
                "Factory",
                MethodAttributes.Private,
                typeof(ICommunicationListener),
                new Type[] { typeof(StatelessServiceContext) });

            var listenerFactoryIl = listenerFactory.GetILGenerator();
            var communicationListener = listenerFactoryIl.DeclareLocal(typeof(ICommunicationListener));
            listenerFactoryIl.Emit(OpCodes.Nop);
            listenerFactoryIl.Emit(OpCodes.Ldarg_0);
            listenerFactoryIl.Emit(OpCodes.Ldfld, serviceProviderFieldBuilder);
            listenerFactoryIl.Emit(OpCodes.Newobj, typeof(CommonWebHostCommunicationListener).GetConstructor(new Type[] { typeof(IServiceProvider) }));
            listenerFactoryIl.Emit(OpCodes.Stloc, communicationListener);
            listenerFactoryIl.Emit(OpCodes.Ldloc, communicationListener);
            listenerFactoryIl.Emit(OpCodes.Ret);

            var instanceListenerMethod = typeBuilder.DefineMethod(
                "CreateServiceInstanceListeners",
                MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(IEnumerable<ServiceInstanceListener>),
                Type.EmptyTypes);

            var baseClassMethod = serviceClass.GetMethod(instanceListenerMethod.Name);

            var methodIl = instanceListenerMethod.GetILGenerator();
            var serviceInstanceListenerList = methodIl.DeclareLocal(typeof(IList<ServiceInstanceListener>));
            var commonServiceInstanceListener = methodIl.DeclareLocal(typeof(ServiceInstanceListener));
            var serviceInstanceListenerEnumerable = methodIl.DeclareLocal(typeof(IEnumerable<ServiceInstanceListener>));
            methodIl.Emit(OpCodes.Nop);
            methodIl.Emit(OpCodes.Ldarg_0);
            methodIl.Emit(OpCodes.Call, baseClassMethod);
            methodIl.Emit(OpCodes.Call, typeof(IEnumerable<ServiceInstanceListener>).GetMethod("toList"));
            methodIl.Emit(OpCodes.Stloc, serviceInstanceListenerList);
            methodIl.Emit(OpCodes.Ldarg_0);
            methodIl.Emit(OpCodes.Ldftn, listenerFactory);
            methodIl.Emit(OpCodes.Newobj, typeof(Func<StatelessServiceContext, ICommunicationListener>).GetConstructors()[0]);
            methodIl.Emit(OpCodes.Ldstr, string.Empty);
            methodIl.Emit(OpCodes.Newobj, typeof(ServiceInstanceListener).GetConstructors()[0]);
            methodIl.Emit(OpCodes.Stloc, commonServiceInstanceListener);
            methodIl.Emit(OpCodes.Ldloc, serviceInstanceListenerList);
            methodIl.Emit(OpCodes.Ldloc, commonServiceInstanceListener);
            methodIl.Emit(OpCodes.Callvirt, typeof(ICollection<ServiceInstanceListener>).GetMethod("Add"));
            methodIl.Emit(OpCodes.Nop);
            methodIl.Emit(OpCodes.Ldloc, serviceInstanceListenerList);
            methodIl.Emit(OpCodes.Stloc, serviceInstanceListenerEnumerable);
            methodIl.Emit(OpCodes.Ldloc, serviceInstanceListenerEnumerable);
            methodIl.Emit(OpCodes.Ret);

            return typeBuilder.CreateType();
        }
    }
}
